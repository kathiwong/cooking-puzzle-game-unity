using UnityEngine;

public class TileRow : MonoBehaviour    //to check the cells in each row
{
    public TileCell[] cells { get; private set; }   //array of cells in this row, priv: as only the row need to get access there

    private void Awake()    //calls this function when the game starts
    {
        cells = GetComponentsInChildren<TileCell>();    //get all the TileCell components that are children of this row and store them in the cells array
    }

}
