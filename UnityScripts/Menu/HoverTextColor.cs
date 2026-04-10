using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class HoverTextColor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public TextMeshProUGUI targetText;
    public Color normalColor = new Color32(75, 90, 70, 255);
    public Color hoverColor = new Color32(179, 58, 58, 255);

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetText != null)
            targetText.color = normalColor;
    }
}