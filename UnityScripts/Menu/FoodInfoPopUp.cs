using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FoodInfoPopup : MonoBehaviour
{
    public GameObject popup;
    public Image foodImage;
    public TextMeshProUGUI titleChineseText;
    public TextMeshProUGUI titleEnglishText;
    public TextMeshProUGUI descriptionText;

    public void ShowFood(Sprite sprite, string chineseTitle, string englishTitle, string description)
    {
        popup.SetActive(true);

        foodImage.sprite = sprite;
        titleChineseText.text = chineseTitle;
        titleEnglishText.text = englishTitle;
        descriptionText.text = description;
    }

    public void HidePopup()
    {
        popup.SetActive(false);
    }
}