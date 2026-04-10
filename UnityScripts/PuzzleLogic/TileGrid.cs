using UnityEngine;

public class TileGrid : MonoBehaviour
{
    //checking the rows and cells in the grid
    public TileRow[] rows { get; private set; }
    public TileCell[] cells { get; private set; }

    public int Size => cells.Length;
    public int Height => rows.Length;
    public int Width => Size / Height;

    private void Awake()
    {
        rows = GetComponentsInChildren<TileRow>();
        cells = GetComponentsInChildren<TileCell>();

    }

    private void Start()
    {
        //y = rows, x = cells
        for (int y = 0; y < rows.Length; y++)
        {
            for (int x = 0; x < rows[y].cells.Length; x++)
            {            //loop over the row
                rows[y].cells[x].coordinates = new Vector2Int(x, y);    //loop over the cells in the row and assign coordinates based on x and y positions
            }
        }
    }


    public TileCell GetCell(int x, int y)   //to get a specific cell based on x and y coordinates
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)   //check if the coordinates are out of bounds
        {
            return rows[y].cells[x];   //return the cell at the specified coordinates
        }
        else return null;   //if out of bounds, return null

    }

    public TileCell GetCell(Vector2Int coordinates)   //overload to get cell using Vector2Int
    {
        return GetCell(coordinates.x, coordinates.y);
    }

    public TileCell GetAdjacentCell(TileCell cell, Vector2Int direction)
    {
        Vector2Int coordinates = cell.coordinates;   //calculate the coordinates of the adjacent cell by adding the direction vector to the current cell's coordinates
        coordinates.x += direction.x;   //add because x axis is normal
        coordinates.y -= direction.y;   //subtracting because y axis is inverted in Unity UI

        return GetCell(coordinates);   //return the adjacent cell at the calculated coordinates
    }

    public TileCell GetRandomEmptyCell()    //to find a random empty cell from the grid
    {
        int index = Random.Range(0, cells.Length);   //get a random index within the range of the cells array
        int startingIndex = index;   //store the starting index to avoid infinite loop

        //make sure is not occupied
        while (cells[index].Occupied)   //while the cell at the random index is occupied
        {
            index++;   //increment the index to check the next cell

            if (index >= cells.Length)   //if the index exceeds the length of the cells array
            {
                index = 0;   //wrap around to the beginning of the array
            }

            if (index == startingIndex)   //if we have looped through all cells and returned to the starting index
            {
                return null;   //no empty cells available, return null
            }
        }

        return cells[index];   //return the empty cell found at the random index
    }



}
