using System.Collections;
using TMPro;            //to use text mesh pro
using UnityEngine;
using UnityEngine.UI;   //to establish reference to UI elements, i.e. image

public class Tile : MonoBehaviour
{
    public TileState state { get; private set; }    //the current state of the tile, only want the tile to manage setting
    public TileCell cell { get; private set; }      //reference to the cell the tile is currently in
    public int number { get; private set; }
    public bool locked { get; set; }    //to prevent multiple merges in one move

    [SerializeField] private Image background; //reference to the image component of the tile
    [SerializeField] private TextMeshProUGUI text; //reference to the text component of the tile
    [SerializeField] private Image icon;

    //for number
    //private Image background;   //reference to the image component of the tile
    //private TextMeshProUGUI text;   //reference to the text component of the tile

    private void Awake()
    {
        //Debug.Log($"GameManager AWAKE: name={gameObject.name}, id={GetInstanceID()}, scene={gameObject.scene.name}");
        background = GetComponent<Image>();   //get the image component attached to this game object
        text = GetComponentInChildren<TextMeshProUGUI>();   //get the text component that is a child of this game object
    }

    public void SetState(TileState state, int number)
    {
        this.state = state;
        this.number = number;

        background.color = state.backgroundColor;   //set the background color based on the tile's state
        
        //for number
        //text.color = state.textColor;   //set the text color based on the tile's state
        //text.text = number.ToString();   //set the text to display the tile's number

        icon.sprite = state.sprite; //set the icon sprite based on the tile's state
        icon.enabled = state.sprite != null; //enable the icon if there is a sprite, otherwise disable it
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null)           //make sure we are unassigning from previous cell
        {
            this.cell.tile = null;
        }

        this.cell = cell;   //set the cell reference to the provided cell
        this.cell.tile = this;   //set the tile reference in the cell to this tile

        transform.position = cell.transform.position;   //position the tile at the cell's position
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)           //make sure we are unassigning from previous cell
        {
            this.cell.tile = null;
        }

        this.cell = cell;   //set the cell reference to the provided cell
        this.cell.tile = this;   //set the tile reference in the cell to this tile

        StartCoroutine(Animate(cell.transform.position, false));   //start the animation coroutine to move the tile to the new cell's position
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)           //make sure we are unassigning from previous cell
        {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(Animate(cell.transform.position, true));   //start the animation coroutine to move the tile to the new cell's position
    }


    private IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsed = 0f;   //time elapsed since the start of the animation
        float duration = 0.1f;   //duration of the animation

        Vector3 from = transform.position;   //starting position of the tile

        while (elapsed < duration)   //loop until the elapsed time reaches the duration
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);   //interpolate position between from and to based on elapsed time
            elapsed += Time.deltaTime;   //increment elapsed time by the time since last frame
            yield return null;   //wait for the next frame
        }

        transform.position = to;   //ensure the final position is set to the target position

        if (merging)
        {
            Destroy(gameObject);   //destroy the tile game object after merging
        }

    }

}
