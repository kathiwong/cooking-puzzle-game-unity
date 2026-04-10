using UnityEngine;

public class VersusOrderManager : MonoBehaviour
{
    public event System.Action<int> OrderExpiredUnfulfilled;

    private const float SpeedIncreaseInterval = 15f;
    private const float SpeedStepMultiplier = 0.9f;
    private const float MinimumSpeedMultiplier = 0.1f;

    [Header("Prefab UI")]
    public GameObject orderPrefab;
    public Transform orderParent;

    [Header("Order Data")]
    public int[] possibleValues = { 8, 16, 32, 64, 128 };
    public TileState[] tileStates;
    public float orderDuration = 10f;

    [Header("Match State")]
    public bool matchEnded;
    public int winningPlayerID;
    public bool MatchEnded => matchEnded; //public getter for match ended state

    private int currentValue;
    private CustomerOrderUI currentOrderUI;
    private float speedMultiplier = 5.0f;
    private float nextSpeedIncreaseTime;
    private int pendingSpeedChanges;
    private bool hasLoggedExpiredOrder;

    public int CurrentValue => currentValue;

    private void Start()
    {
        nextSpeedIncreaseTime = Time.time + SpeedIncreaseInterval;
        CreateOrderUIIfNeeded();
        NewOrder();
    }

    private void Update()
    {
        if (matchEnded)
        {
            return;
        }

        QueuePendingSpeedChanges();

        if (currentOrderUI == null) return;

        currentOrderUI.UpdateTimer(Time.deltaTime);

        if (currentOrderUI.IsExpired && !hasLoggedExpiredOrder)
        {
            Debug.Log($"[VersusOrderManager] Order expired without being served: {currentValue}");
            hasLoggedExpiredOrder = true;
            OrderExpiredUnfulfilled?.Invoke(currentValue);
        }
    }

    private void CreateOrderUIIfNeeded()
    {
        if (currentOrderUI != null) return;

        if (orderPrefab == null || orderParent == null)
        {
            Debug.LogWarning("VersusOrderManager: orderPrefab or orderParent is not assigned.");
            return;
        }

        GameObject obj = Instantiate(orderPrefab, orderParent);
        currentOrderUI = obj.GetComponent<CustomerOrderUI>();

        if (currentOrderUI == null)
        {
            Debug.LogWarning("VersusOrderManager: spawned prefab has no CustomerOrderUI component.");
        }
    }

    public void NewOrder()
    {
        if (possibleValues == null || possibleValues.Length == 0)
        {
            Debug.LogWarning("VersusOrderManager: No possibleValues configured.");
            return;
        }

        CreateOrderUIIfNeeded();

        if (currentOrderUI == null)
        {
            Debug.LogWarning("VersusOrderManager: currentOrderUI is missing.");
            return;
        }

        ApplyPendingSpeedChanges();

        int nextValue = possibleValues[Random.Range(0, possibleValues.Length)];

        // Prevent 128 from appearing twice in a row while keeping other repeats allowed.
        if (currentValue == 128 && nextValue == 128)
        {
            bool foundNon128 = false;

            for (int i = 0; i < possibleValues.Length; i++)
            {
                if (possibleValues[i] != 128)
                {
                    foundNon128 = true;
                    break;
                }
            }

            if (foundNon128)
            {
                do
                {
                    nextValue = possibleValues[Random.Range(0, possibleValues.Length)];
                }
                while (nextValue == 128);
            }
        }

        currentValue = nextValue;

        Sprite iconSprite = GetSpriteForValue(currentValue);
        float duration = GetDurationForValue(currentValue);
        currentOrderUI.Init(currentValue, duration, iconSprite);
        hasLoggedExpiredOrder = false;

        Debug.Log($"[VersusOrderManager] New order: {currentValue} with duration: {duration}");
    }

    private Sprite GetSpriteForValue(int value)
    {
        if (tileStates == null || tileStates.Length == 0)
        {
            Debug.LogWarning("VersusOrderManager: tileStates is empty.");
            return null;
        }

        for (int i = 0; i < tileStates.Length; i++)
        {
            if (tileStates[i] != null && tileStates[i].number == value)
            {
                return tileStates[i].sprite;
            }
        }

        Debug.LogWarning($"VersusOrderManager: No sprite found for value {value}.");
        return null;
    }

    public bool Matches(int value)
    {
        return currentValue == value;
    }

    private void QueuePendingSpeedChanges()
    {
        while (Time.time >= nextSpeedIncreaseTime)
        {
            pendingSpeedChanges++;
            nextSpeedIncreaseTime += SpeedIncreaseInterval;

            Debug.Log(
                $"[VersusOrderManager] Speed change queued at {SpeedIncreaseInterval:F0}s checkpoint. " +
                $"Pending changes: {pendingSpeedChanges}. Current multiplier stays {speedMultiplier:F2} " +
                "until the active order is completed."
            );
        }
    }

    private float GetDurationForValue(int value)
    {
        float baseDuration;

        switch (value)
        {
            case 8: baseDuration = 12f; break;
            case 16: baseDuration = 16f; break;
            case 32: baseDuration = 24f; break;
            case 64: baseDuration = 40f; break;
            case 128: baseDuration = 60f; break;
            default: baseDuration = orderDuration; break;
        }

        return baseDuration * speedMultiplier;
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
            $"[VersusOrderManager] Order speed changed after order completion. Multiplier: {previousSpeedMultiplier:F2} -> {speedMultiplier:F2}, " +
            $"duration scale now {speedMultiplier:F2}x after applying {pendingSpeedChanges} queued change(s)."
        );

        pendingSpeedChanges = 0;
    }

    public void SetMatchEnded(int winnerId)
    {
        matchEnded = true;
        winningPlayerID = winnerId;
    }
}
