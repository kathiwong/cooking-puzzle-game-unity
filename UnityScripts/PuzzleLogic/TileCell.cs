using UnityEngine;

public class TileCell : MonoBehaviour
{
    public Vector2Int coordinates { get; set; }     //grid telling the cell's position by coordinates, 2 int = x and y
    public Tile tile { get; set; } //the tile that is currently in this cell (as reference)

    public bool Empty => tile == null;  //if there is no tile in the cell, it is empty
    public bool Occupied => tile != null;   //if there is a tile in the cell, it is occupied
}
