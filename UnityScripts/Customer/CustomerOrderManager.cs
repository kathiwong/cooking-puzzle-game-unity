using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class CustomerOrderManager : MonoBehaviour
{
    private const float SpeedIncreaseInterval = 15f; //when speed changes are triggered, very 10 seconds
    private const float SpeedStepMultiplier = 0.9f; //how much speed changes each time, x10 faster
    private const float MinimumSpeedMultiplier = 0.1f; //a minimum cap, 10% of original duration, to prevent orders from becoming impossible to fulfill

    public GameManager gameManager;

    
    [Header("Order Settings")]
    public GameObject orderPrefab;
    public Transform orderParent;
    public int[] possibleValues = { 8, 16, 32, 64, 128 }; // Possible order values
    

    public TileState[] tileStates; // Reference to tile states for icons

    public float orderDuration = 10f; //shrink all in 10 seconds //initial value

    private List<CustomerOrderUI> activeOrders = new List<CustomerOrderUI>();
    private float speedMultiplier = 5.0f;
    private float nextSpeedIncreaseTime;
    private int pendingSpeedChanges;

    private void Start()
    {
        nextSpeedIncreaseTime = Time.time + SpeedIncreaseInterval;
        AddOrder();
        Debug.Log("First order added: " + activeOrders[0].RequestedValue);
    }

    private void Update()
    {
        QueuePendingSpeedChanges();

        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            var order = activeOrders[i];
            order.UpdateTimer(Time.deltaTime);

            if (order.IsExpired)
            {
                Debug.Log("Order expired: " + order.RequestedValue);
                Destroy(order.gameObject);
                activeOrders.RemoveAt(i);

                // Trigger game over
                if (gameManager != null)
                {
                    gameManager.GameOver();
                }
            }

        }
    }

    public void AddOrder()
    {
        int value = possibleValues[Random.Range(0, possibleValues.Length)];

        GameObject obj = Instantiate(orderPrefab, orderParent);
        CustomerOrderUI orderUI = obj.GetComponent<CustomerOrderUI>();
        float customDuration = GetDurationForValue(value);

        Sprite iconSprite = GetSpriteForValue(value);

        // Auto-link the scene customer animation script
        if (orderUI != null && orderUI.customerAnimator == null)
        {
            CustomerAnimatorSpeed animatorRef = FindObjectOfType<CustomerAnimatorSpeed>();

            if (animatorRef != null)
            {
                orderUI.customerAnimator = animatorRef;
                Debug.Log("Customer animator linked successfully.");
            }
            else
            {
                Debug.LogWarning("CustomerOrderManager: No CustomerAnimatorSpeed found in scene.");
            }
        }

        orderUI.Init(value, customDuration, iconSprite);

        activeOrders.Add(orderUI);

        Debug.Log("New order added: " + value + " with duration: " + customDuration);
    }

    private void QueuePendingSpeedChanges()
    {
        while (Time.time >= nextSpeedIncreaseTime)
        {
            pendingSpeedChanges++;
            nextSpeedIncreaseTime += SpeedIncreaseInterval;

            Debug.Log(
                $"Speed change queued at {SpeedIncreaseInterval:F0}s checkpoint. " +
                $"Pending changes: {pendingSpeedChanges}. Current multiplier stays {speedMultiplier:F2} " +
                $"until the active order is completed."
            );
        }
    }

    /*private Sprite GetSpriteForValue(int value)
    {
    // value 8 -> index 0, 16 -> 1, 32 -> 2, 64 -> 3, 128 -> 4
    int index = Mathf.RoundToInt(Mathf.Log(value, 2)) - 3; // Log base 2 of value gives us the exponent, subtracting 3 to align with our index (since 8 is 2^3)
    if (index < 0 || index >= tileStates.Length) 
        {
            Debug.LogWarning("Invalid index for tile state: " + index);
            return null;
        }else 
        {
            Debug.Log("Getting sprite for value: " + value + " at index: " + index);
            return tileStates[index].sprite;
        }
    }
*/
    private Sprite GetSpriteForValue(int value)
    {

        if (tileStates == null || tileStates.Length == 0)
        {
            Debug.LogWarning("tileStates array is empty!");
            return null;
        }

        for (int i = 0; i < tileStates.Length; i++)
        {
            Debug.Log($"Checking TileState index {i}, number={tileStates[i].number}");
            if (tileStates[i] != null && tileStates[i].number == value)
            {
                Debug.Log($"Found sprite for value {value} in tileStates[{i}]");
                return tileStates[i].sprite;
            }
        }

        Debug.LogWarning("No TileState found for value: " + value);
        return null;
    }

    private float GetDurationForValue(int value)
    {
    /*
    switch (value) //custom durations for different orders - easy
        {
        case 8: return 6*4f;
        case 16: return 8*4f;
        case 32: return 12*4f;
        case 64: return 20*4f;
        case 128: return 30*4f;
        default: return orderDuration; // fallback
        }
*/
    float baseDuration;

    switch (value) //custom durations for different orders - middle
        {
        case 8: baseDuration = 12*2f; break;
        case 16: baseDuration = 16*2f; break;
        case 32: baseDuration = 24*2f; break;
        case 64: baseDuration = 40*2f; break;
        case 128: baseDuration = 60*2f; break;
        default: baseDuration = orderDuration; break; // fallback
        }
    return baseDuration * speedMultiplier;
/*
    switch (value) //custom durations for different orders - debug ver
        {
        case 8: return 100f;//12
        case 16: return 100f;//16
        case 32: return 100f;//24
        case 64: return 100f;//40
        default: return orderDuration; // fallback
        }
        */
    }

    public bool TryFulfillOrder(int value)
    {
        for (int i = 0; i < activeOrders.Count; i++)
        {
            if (activeOrders[i].RequestedValue == value)
            {
                Destroy(activeOrders[i].gameObject);
                activeOrders.RemoveAt(i);
                ApplyPendingSpeedChanges();
                AddOrder(); // Replace with a new one
                return true;
            }
        }

        return false;
    }

    public bool HasOrder(int value)
    {
        foreach (var order in activeOrders)
        {
            if (order.RequestedValue == value)
                return true;
        }
        return false;
    }


    public void ResetOrders()
    {
        // Remove all existing orders
        foreach (var order in activeOrders)
        {
            Destroy(order.gameObject);
        }

        activeOrders.Clear();
        speedMultiplier = 1.0f;
        pendingSpeedChanges = 0;
        nextSpeedIncreaseTime = Time.time + SpeedIncreaseInterval;

        // Spawn the first order again
        AddOrder();
    }

    private void ApplyPendingSpeedChanges()
    {
        if (pendingSpeedChanges <= 0)
        {
            return;
        }

        float previousSpeedMultiplier = speedMultiplier;

        for (int i = 0; i < pendingSpeedChanges; i++)
        {
            speedMultiplier = Mathf.Max(speedMultiplier * SpeedStepMultiplier, MinimumSpeedMultiplier);
        }

        Debug.Log(
            $"Order speed changed after order completion. Multiplier: {previousSpeedMultiplier:F2} -> {speedMultiplier:F2}, " +
            $"duration scale now {speedMultiplier:F2}x after applying {pendingSpeedChanges} queued change(s)."
        );

        pendingSpeedChanges = 0;
    }


}
