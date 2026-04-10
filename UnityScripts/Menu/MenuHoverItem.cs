using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuHoverItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Shared")]
    public RectTransform selectorArrow;
    public MenuHoverItem[] allItems;

    [Header("Per Button")]
    public RectTransform arrowTarget;
    public TextMeshProUGUI label;

    [Header("Colors")]
    public Color normalColor = new Color32(60, 74, 60, 255);
    public Color highlightColor = new Color32(179, 58, 58, 255);

    private bool isHovered;

    private void Start()
    {
        ResetVisual();

        if (selectorArrow != null)
        {
            selectorArrow.gameObject.SetActive(false);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovered = true;
        SelectThis();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovered = false;

        bool anyHovered = false;

        if (allItems != null)
        {
            foreach (MenuHoverItem item in allItems)
            {
                if (item != null && item.isHovered)
                {
                    anyHovered = true;
                    break;
                }
            }

            if (!anyHovered)
            {
                foreach (MenuHoverItem item in allItems)
                {
                    if (item != null)
                    {
                        item.ResetVisual();
                    }
                }

                if (selectorArrow != null)
                {
                    selectorArrow.gameObject.SetActive(false);
                }
            }
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        SelectThis();
    }

    public void SelectThis()
    {
        if (allItems != null)
        {
            foreach (MenuHoverItem item in allItems)
            {
                if (item != null)
                {
                    item.ResetVisual();
                }
            }
        }

        if (selectorArrow != null && arrowTarget != null)
        {
            selectorArrow.gameObject.SetActive(true);
            selectorArrow.position = arrowTarget.position;
        }

        if (label != null)
        {
            label.color = highlightColor;
        }
    }

    public void ResetVisual()
    {
        if (label != null)
        {
            label.color = normalColor;
        }
    }
}