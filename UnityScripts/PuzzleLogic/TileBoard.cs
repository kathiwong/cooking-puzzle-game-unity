using System.Collections;
using System;
using System.Collections.Generic;   //to use lists
using UnityEngine;

public class TileBoard : MonoBehaviour
{
    public event Action<TileBoard, bool> BoardStateResolved;

    public GameManager gameManager;  //reference to the game manager to notify when game over occurs
    public Tile tilePrefab;     //reference to the tile prefab to create copy of it every time we add a new tile //camnn only put tile prefab in the inspector as only it has the tile script attached 
    public TileState[] tileStates;   //array to hold different tile states

    //VS mode
    [Header("Input")]
    public bool useBuiltInKeyboardInput = true;

    [Header("Versus / Obstacles")]
    public TileState obstacleState;   // assign a grey obstacle TileState in Inspector


    private TileGrid grid;   //take reference to the TileGrid component
    private List<Tile> tiles;

    private const int MaxTileValue = 128; // Maximum tile value for the game

    private bool waiting;  //to prevent user input while tiles are moving

    public bool IsResolving => waiting;

    private void Awake()
    {
        grid = GetComponentInChildren<TileGrid>();   //get the TileGrid component that is a child of this object
        tiles = new List<Tile>(16);    //initialize the tiles list //e.g. 16, but don't need to set size as list can grow dynamically
    }

    public void ClearBoard()
    {
        foreach (var cell in grid.cells)
        {
            cell.tile = null;   //unassign any tiles from the cells
        }
        foreach (Tile tile in tiles)   //loop through all tiles in the tiles list
        {
            Destroy(tile.gameObject);   //destroy each tile's game object
        }

        tiles.Clear();   //clear the tiles list
    }

    public void CreateTile()
    {
        Tile tile = Instantiate(tilePrefab, grid.transform);   //create a new instance of the tile prefab as a child of the grid transform
        tile.SetState(tileStates[0], 2);   //set the state of the new tile to the first tile state (e.g., for number 2)
        tile.Spawn(grid.GetRandomEmptyCell());   //spawn the tile in a random empty cell on the grid
        tiles.Add(tile);   //add the new tile to the tiles list
    }

    public void Update()
    {
        if (!useBuiltInKeyboardInput) return; 
        if (waiting) return;

        if (IsEmpty())
        {
            StartCoroutine(HandleEmptyBoard());
            return;
        }

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            Move(Vector2Int.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector2Int.right);
        }
    }

    private IEnumerator HandleEmptyBoard()
    {
        waiting = true;

        yield return new WaitForSeconds(0.2f);  //short delay before creating new tiles

        if (gameManager != null)
        {
            yield return StartCoroutine(gameManager.ShowMessage("Board was empty! Created two new tiles.", 1.0f)); //show message for 1 second
        }

        CreateTile();
        CreateTile();

        waiting = false;
    }
    public bool IsEmpty()
    {
        foreach (TileCell cell in grid.cells)
        {
            if (cell.tile != null)
            {
                return false;   //if any cell is occupied, the board is not empty
            }
        }
        return true;   //all cells are empty
    }

    public bool Move(Vector2Int direction) //returns true if any tiles were moved or merged, false if no change occurred
    {
        if (waiting) return false;

        if (IsEmpty())
        {
            StartCoroutine(HandleEmptyBoard());
            return false;
        }

        if (direction == Vector2Int.up)
        {
            return MoveTiles(Vector2Int.up, 0, 1, 1, 1);
        }
        else if (direction == Vector2Int.down)
        {
            return MoveTiles(Vector2Int.down, 0, 1, grid.Height - 2, -1);
        }
        else if (direction == Vector2Int.left)
        {
            return MoveTiles(Vector2Int.left, 1, 1, 0, 1);
        }
        else if (direction == Vector2Int.right)
        {
            return MoveTiles(Vector2Int.right, grid.Width - 2, -1, 0, 1);
        }

        return false;
    }

    private bool MoveTiles(Vector2Int direction, int startX, int incrementX, int startY, int incrementY)   // Logic to move tiles in the specified direction
    {
        bool changed = false;

        //loop through all cells in the grid and move tiles accordingly
        for (int x = startX; x >= 0 && x < grid.Width; x += incrementX)
        {
            for (int y = startY; y >= 0 && y < grid.Height; y += incrementY)
            {
                TileCell cell = grid.GetCell(x, y);

                if (cell.Occupied)
                {
                    changed |= MoveTile(cell.tile, direction);
                }
            }
        }

        if (changed)
        {
            StartCoroutine(WaitForChanges());
        }

        return changed;
    }

    private bool MoveTile(Tile tile, Vector2Int direction)  //returns true if the tile was moved or merged, false if it could not move
    {
        TileCell newCell = null;
        TileCell adjacent = grid.GetAdjacentCell(tile.cell, direction);

        while (adjacent != null)    //if null it is out of bounds, if not null then we try to move to that direction as far as possible
        {
            if (adjacent.Occupied)
            {
                //merge the tiles
                if (CanMerge(tile, adjacent.tile))
                {
                    Merge(tile, adjacent.tile); //merge tile into adjacent tile
                    return true;
                }
                break;

            }

            newCell = adjacent;   //update new cell to the adjacent cell
            adjacent = grid.GetAdjacentCell(adjacent, direction);   //get the next adjacent cell
        }

        if (newCell != null)
        {
            tile.MoveTo(newCell);   //move the tile to the new cell
            return true;
        }

        return false;
    }

    private bool CanMerge(Tile a, Tile b)
    {
        if (IsObstacle(a) || IsObstacle(b))
        {
            return false;
        }

        if (b.number >= MaxTileValue)
        {
            return false;
        }

        return a.number == b.number && !b.locked;
    }

    private void Merge(Tile a, Tile b)
    {
        tiles.Remove(a);
        a.Merge(b.cell);   //merge tile a into tile b's cell

        int index = Mathf.Clamp(IndexOf(b.state) + 1, 0, tileStates.Length - 1);   //get the index of the next tile state for tile b after merging (ensure stays within bounds)
        int number = b.number * 2;   //double the number of tile b after merging

        b.SetState(tileStates[index], number);   //set the new state and number for tile b
    }

    private int IndexOf(TileState state) //to get the index of a specific tile state in the tileStates array
    {
        for (int i = 0; i < tileStates.Length; i++)
        {
            if (tileStates[i] == state) //if the current tile state matches the provided state
            {
                return i;
            }
        }

        return -1;  //state not found but should not happen
    }


    private IEnumerator WaitForChanges()
    {
        waiting = true;

        yield return new WaitForSeconds(0.1f);  //wait for the duration of the tile movement animation

        waiting = false;

        foreach (Tile tile in tiles)   //unlock all tiles after movement
        {
            tile.locked = false;
        }

        //create new tile
        if (tiles.Count != grid.Size)
        {
            CreateTile();
        }

        //check for game over
        bool isGameOver = CheckForGameOver();

        if (isGameOver)
        {
            if (gameManager != null)
            {
                gameManager.GameOver();
            }
        }

        BoardStateResolved?.Invoke(this, isGameOver);

    }

    private bool CheckForGameOver()
    {
        if (tiles.Count != grid.Size)   //if there are still empty cells, game is not over
        {
            return false;
        }

        //check for possible merges
        foreach (var tile in tiles)
        {
            //check adjacent tiles
            TileCell up = grid.GetAdjacentCell(tile.cell, Vector2Int.up);
            TileCell down = grid.GetAdjacentCell(tile.cell, Vector2Int.down);
            TileCell left = grid.GetAdjacentCell(tile.cell, Vector2Int.left);
            TileCell right = grid.GetAdjacentCell(tile.cell, Vector2Int.right);

            //if any adjacent tile can be merged, game is not over
            if (up != null && CanMerge(tile, up.tile))
            {
                return false;
            }
            if (down != null && CanMerge(tile, down.tile))
            {
                return false;
            }
            if (left != null && CanMerge(tile, left.tile))
            {
                return false;
            }
            if (right != null && CanMerge(tile, right.tile))
            {
                return false;
            }
        }

        return true;   //no moves left, game over
    }

    //give button functionality
public bool TryGiveOneTile(int value)
{
    foreach (TileCell cell in grid.cells)
    {
        if (cell.tile != null && cell.tile.number == value)
        {
            // capture the tile reference
            Tile t = cell.tile;

            // free the cell
            cell.tile = null;

            // keep the tiles list consistent
            tiles.Remove(t);

            // destroy the object
            Destroy(t.gameObject);

            return true; // success
        }
    }
    return false; // not found
}


    public bool HasTileWithValue(int value)
    {
        foreach (TileCell cell in grid.cells)
        {
            if (cell.tile != null && cell.tile.number == value)
                return true;
        }
        return false;
    }

    public void RemoveOneTileWithValue(int value)
    {
        foreach (TileCell cell in grid.cells)
        {
            if (cell.tile != null && cell.tile.number == value)
            {
                Destroy(cell.tile.gameObject);
                cell.tile = null;
                return;
            }
        }
    }


    private bool IsObstacle(Tile tile)
    {
        return tile != null && tile.number < 0;
    }

    public void SpawnObstacleTiles(int amount)
    {
        if (obstacleState == null)
        {
            Debug.LogError("TileBoard: obstacleState is not assigned!");
            return;
        }

        for (int i = 0; i < amount; i++)
        {
            TileCell empty = grid.GetRandomEmptyCell();
            if (empty == null) return;

            Tile tile = Instantiate(tilePrefab, grid.transform);
            tile.SetState(obstacleState, -1);
            tile.Spawn(empty);
            tiles.Add(tile);
        }
    }

    public int RemoveObstacleTiles(int amount)
    {
        int removed = 0;

        for (int i = grid.cells.Length - 1; i >= 0; i--)
        {
            TileCell cell = grid.cells[i];

            if (cell.tile != null && IsObstacle(cell.tile))
            {
                Tile t = cell.tile;
                cell.tile = null;
                tiles.Remove(t);
                Destroy(t.gameObject);
                removed++;

                if (removed >= amount)
                    break;
            }
        }

        return removed;
    }

    public bool IsGameOver()
    {
        return CheckForGameOver();
    }

}
