using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomerOrderUI : MonoBehaviour
{
    public GameManager gameManager;
    public Image orderIcon;
    public TextMeshProUGUI orderText;
    public Image timeBarFill;
    public CustomerAnimatorSpeed customerAnimator;

    public int RequestedValue { get; private set; }
    public bool IsExpired => timeRemaining <= 0f;
    public float TimeRemaining => timeRemaining;

    public float timeRemaining;
    public float totalDuration;

    private Color customGreen = new Color(0.2f, 0.8f, 0.5f);
    private Color customYellow = new Color(1.0f, 0.8f, 0.2f);
    private Color customRed = new Color(0.9f, 0.2f, 0.2f);

    public void Init(int value, float duration, Sprite iconSprite)
    {
        RequestedValue = value;

        if (orderText != null)
            orderText.text = "Serve:";

        totalDuration = duration;
        timeRemaining = duration;

        if (timeBarFill != null)
        {
            timeBarFill.fillAmount = 1f;
            timeBarFill.color = customGreen;
        }

        if (orderIcon != null)
        {
            orderIcon.sprite = iconSprite;
            orderIcon.enabled = iconSprite != null;
        }

        if (customerAnimator != null)
        {
            customerAnimator.SetUrgent(false);
        }
    }

    public void UpdateTimer()
    {
        UpdateTimer(Time.deltaTime);
    }

    public void UpdateTimer(float deltaTime)
    {
        timeRemaining -= deltaTime;
        timeRemaining = Mathf.Max(timeRemaining, 0f);

        float fill = (totalDuration > 0f) ? timeRemaining / totalDuration : 0f;

        if (timeBarFill != null)
        {
            timeBarFill.fillAmount = fill;

            if (fill > 0.5f)
            {
                timeBarFill.color = customGreen;
            }
            else if (fill > 0.25f)
            {
                timeBarFill.color = customYellow;
            }
            else
            {
                timeBarFill.color = customRed;
            }
        }

        if (customerAnimator != null)
        {
            bool isUrgent = fill <= 0.25f;
            customerAnimator.SetUrgent(isUrgent);
        }
    }
}