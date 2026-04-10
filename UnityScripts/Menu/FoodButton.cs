using UnityEngine;

public class FoodButton : MonoBehaviour
{
    public FoodInfoPopup popupUI;

    public Sprite sprite;
    public string chineseTitle;
    public string englishTitle;

    [TextArea] public string description;

    public void Show()
    {
        popupUI.ShowFood(sprite, chineseTitle, englishTitle, description);
    }
}