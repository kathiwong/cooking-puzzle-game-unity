using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class VersusMatchManager : MonoBehaviour
{
    [System.Serializable]
    public struct ServedValueAmountConfig
    {
        public int servedValue;
        public int amount;
    }

    public VersusPlayerController player1;
    public VersusPlayerController player2;
    public VersusOrderManager sharedOrder;

    [Header("Result UI")]
    public CanvasGroup resultPanel;
    public TextMeshProUGUI resultText;
    public TextMeshProUGUI player1ScoreText;
    public TextMeshProUGUI player2ScoreText;

    [Header("Match State")]
    [SerializeField] private bool matchEnded;
    [SerializeField] private int winningPlayerId;

    public bool MatchEnded => matchEnded;
    public int WinningPlayerId => winningPlayerId;

    private void OnEnable()
    {
        SubscribeToBoardEvents();
        SubscribeToOrderEvents();
    }

    private void Start()
    {
        matchEnded = false;
        winningPlayerId = 0;
        HideResultPanel();

        if (sharedOrder != null)
        {
            sharedOrder.matchEnded = false;
            sharedOrder.winningPlayerID = 0;
        }
    }

    private void OnDisable()
    {
        UnsubscribeFromBoardEvents();
        UnsubscribeFromOrderEvents();
    }

    [Header("Attack Rules")]
    public int defaultAttackAmountPerServe = 1;
    public ServedValueAmountConfig[] serveAttackConfigs =
    {
        new ServedValueAmountConfig { servedValue = 8, amount = 1 },
        new ServedValueAmountConfig { servedValue = 16, amount = 1 },
        new ServedValueAmountConfig { servedValue = 32, amount = 1 },
        new ServedValueAmountConfig { servedValue = 64, amount = 2 },
        new ServedValueAmountConfig { servedValue = 128, amount = 2 },
    };

    [Header("Serve Clear Rules")]
    public bool clearIncomingAttacksFirst = true;
    public bool allowClearSpillToObstacles = true;
    public ServedValueAmountConfig[] serveClearConfigs =
    {
        new ServedValueAmountConfig { servedValue = 8, amount = 1 },
        new ServedValueAmountConfig { servedValue = 16, amount = 1 },
        new ServedValueAmountConfig { servedValue = 32, amount = 2 },
        new ServedValueAmountConfig { servedValue = 64, amount = 2 },
        new ServedValueAmountConfig { servedValue = 128, amount = 3 },
    };

    public void PlayerTryGive(VersusPlayerController player, int value)
    {
        if (matchEnded)
        {
            return;
        }

        if (player == null)
        {
            Debug.LogWarning("VersusMatchManager.PlayerTryGive called with a null player.");
            return;
        }

        if (player.board == null)
        {
            Debug.LogWarning($"VersusMatchManager: Player {player.playerId} has no board assigned.");
            return;
        }

        if (sharedOrder == null)
        {
            Debug.LogWarning("VersusMatchManager: sharedOrder is not assigned.");
            return;
        }

        Debug.Log($"[VersusMatchManager] Player {player.playerId} tried {value}, current order = {sharedOrder.CurrentValue}");

        if (!sharedOrder.Matches(value))
        {
            Debug.Log($"VersusMatchManager: Player {player.playerId} tried to give {value}, but the current order is {sharedOrder.CurrentValue}.");
            return;
        }

        bool removed = player.board.TryGiveOneTile(value);
        if (!removed)
        {
            Debug.Log($"VersusMatchManager: Player {player.playerId} matched the order with {value}, but no tile with that value was found on the board.");
            return;
        }

        player.AddScore(value);
        ApplyServeClear(player, value);

        VersusPlayerController victim = (player == player1) ? player2 : player1;
        if (victim == null)
        {
            Debug.LogWarning($"VersusMatchManager: Could not find a victim player reference for Player {player.playerId}.");
            return;
        }

        int attackAmount = GetConfiguredAmount(serveAttackConfigs, value, defaultAttackAmountPerServe);
        victim.AddPendingAttack(attackAmount);
        Debug.Log($"VersusMatchManager: Player {player.playerId} served {value}. Added {attackAmount} pending attack to Player {victim.playerId}.");

        sharedOrder.NewOrder();
    }

    public void ResolvePendingAttackOnMove(VersusPlayerController victim)
    {
        if (victim == null)
        {
            Debug.LogWarning("VersusMatchManager.ResolvePendingAttackOnMove called with a null victim.");
            return;
        }

        if (victim.board == null)
        {
            Debug.LogWarning($"VersusMatchManager: Player {victim.playerId} has no board assigned for pending attack resolution.");
            return;
        }

        if (!victim.HasPendingAttack()) return;

        int amount = victim.ConsumePendingAttack();
        victim.board.SpawnObstacleTiles(amount);
        Debug.Log($"VersusMatchManager: Spawned {amount} obstacle tiles on Player {victim.playerId}'s board.");
    }

    private void ApplyServeClear(VersusPlayerController player, int servedValue)
    {
        int configuredClearAmount = GetConfiguredAmount(serveClearConfigs, servedValue, 0);
        int remainingClearAmount = configuredClearAmount;
        int incomingCleared = 0;
        int obstaclesCleared = 0;
        int attackAmount = GetConfiguredAmount(serveAttackConfigs, servedValue, defaultAttackAmountPerServe);

        if (remainingClearAmount <= 0)
        {
            Debug.Log($"VersusMatchManager: Player {player.playerId} served {servedValue}. Configured self-clear amount is 0.");
            return;
        }

        if (clearIncomingAttacksFirst)
        {
            incomingCleared = ClearIncomingAttacks(player, remainingClearAmount);
            remainingClearAmount -= incomingCleared;

            if (allowClearSpillToObstacles && remainingClearAmount > 0)
            {
                obstaclesCleared = player.board.RemoveObstacleTiles(remainingClearAmount);
            }
        }
        else
        {
            obstaclesCleared = player.board.RemoveObstacleTiles(remainingClearAmount);
            remainingClearAmount -= obstaclesCleared;

            if (allowClearSpillToObstacles && remainingClearAmount > 0)
            {
                incomingCleared = ClearIncomingAttacks(player, remainingClearAmount);
            }
        }

        Debug.Log(
            $"VersusMatchManager: Player {player.playerId} served {servedValue}. " +
            $"Configured clear={configuredClearAmount}, incoming cleared={incomingCleared}, " +
            $"obstacles cleared={obstaclesCleared}, attack sent={attackAmount}."
        );
    }

    private int ClearIncomingAttacks(VersusPlayerController player, int amount)
    {
        if (amount <= 0 || !player.HasPendingAttack())
        {
            return 0;
        }

        int cleared = Mathf.Min(player.PendingAttackCount, amount);
        player.ReducePendingAttack(cleared);
        return cleared;
    }

    private int GetConfiguredAmount(ServedValueAmountConfig[] configs, int value, int defaultAmount)
    {
        if (configs == null)
        {
            return Mathf.Max(0, defaultAmount);
        }

        for (int i = 0; i < configs.Length; i++)
        {
            if (configs[i].servedValue == value)
            {
                return Mathf.Max(0, configs[i].amount);
            }
        }

        return Mathf.Max(0, defaultAmount);
    }


//local VS mode
    public void EvaluateWinner()
    {
        if (matchEnded) return;

        if (player1 == null || player2 == null)
        {
            Debug.LogWarning("VersusMatchManager: player1 or player2 is missing.");
            return;
        }

        if (player1.board == null || player2.board == null)
        {
            Debug.LogWarning("VersusMatchManager: one of the player boards is missing.");
            return;
        }

        bool p1Lost = player1.board.IsGameOver();
        bool p2Lost = player2.board.IsGameOver();

        if (p1Lost && p2Lost)
        {
            EndMatch(ResolveWinnerByScore());
        }
        else if (p1Lost)
        {
            EndMatch(2); // player 2 wins
        }
        else if (p2Lost)
        {
            EndMatch(1); // player 1 wins
        }
    }

    public void EndMatch(int winnerId)
    {
        if (matchEnded) return;

        matchEnded = true;
        winningPlayerId = winnerId;
        ShowResultPanel();

        if (sharedOrder != null)
        {
            sharedOrder.SetMatchEnded(winnerId);
        }

        if (winnerId == 0)
        {
            Debug.Log("VersusMatchManager: Match ended in a draw.");
        }
        else
        {
            Debug.Log($"VersusMatchManager: Match ended. Player {winnerId} wins.");
        }
    }

    private void SubscribeToBoardEvents()
    {
        Subscribe(player1);
        Subscribe(player2);
    }

    private void SubscribeToOrderEvents()
    {
        if (sharedOrder == null)
        {
            return;
        }

        sharedOrder.OrderExpiredUnfulfilled -= HandleOrderExpiredUnfulfilled;
        sharedOrder.OrderExpiredUnfulfilled += HandleOrderExpiredUnfulfilled;
    }

    private void UnsubscribeFromBoardEvents()
    {
        Unsubscribe(player1);
        Unsubscribe(player2);
    }

    private void UnsubscribeFromOrderEvents()
    {
        if (sharedOrder == null)
        {
            return;
        }

        sharedOrder.OrderExpiredUnfulfilled -= HandleOrderExpiredUnfulfilled;
    }

    private void Subscribe(VersusPlayerController player)
    {
        if (player == null || player.board == null)
        {
            return;
        }

        player.board.BoardStateResolved -= HandleBoardStateResolved;
        player.board.BoardStateResolved += HandleBoardStateResolved;
    }

    private void Unsubscribe(VersusPlayerController player)
    {
        if (player == null || player.board == null)
        {
            return;
        }

        player.board.BoardStateResolved -= HandleBoardStateResolved;
    }

    private void HandleBoardStateResolved(TileBoard board, bool isGameOver)
    {
        EvaluateWinner();
    }

    private void HandleOrderExpiredUnfulfilled(int expiredValue)
    {
        if (matchEnded)
        {
            return;
        }

        Debug.Log($"VersusMatchManager: Order {expiredValue} expired unfulfilled. Resolving winner by score.");
        EndMatch(ResolveWinnerByScore());
    }

    private int ResolveWinnerByScore()
    {
        int player1Score = player1 != null ? player1.Score : 0;
        int player2Score = player2 != null ? player2.Score : 0;

        if (player1Score > player2Score)
        {
            return 1;
        }

        if (player2Score > player1Score)
        {
            return 2;
        }

        return 0;
    }

    private void HideResultPanel()
    {
        if (resultPanel == null)
        {
            return;
        }

        resultPanel.alpha = 0f;
        resultPanel.interactable = false;
        resultPanel.blocksRaycasts = false;
    }

    private void ShowResultPanel()
    {
        if (resultText != null)
        {
            resultText.text = GetResultLabel();
        }

        if (player1ScoreText != null)
        {
            string p1Label = (player1 != null && player1.isCPU) ? "CPU" : "Player";
            player1ScoreText.text = $"{p1Label} score: {(player1 != null ? player1.Score : 0)}";
        }

        if (player2ScoreText != null)
        {
            string p2Label = (player2 != null && player2.isCPU) ? "CPU" : "Player";
            player2ScoreText.text = $"{p2Label} score: {(player2 != null ? player2.Score : 0)}";
        }

        if (resultPanel != null)
        {
            resultPanel.alpha = 1f;
            resultPanel.interactable = true;
            resultPanel.blocksRaycasts = true;
        }
    }

    private string GetResultLabel()
    {
        if (winningPlayerId == 0)
        {
            return "Draw";
        }

        VersusPlayerController winner = null;

        if (winningPlayerId == 1)
        {
            winner = player1;
        }
        else if (winningPlayerId == 2)
        {
            winner = player2;
        }

        if (winner == null)
        {
            return $"Player {winningPlayerId} Wins";
        }

        if (winner.isCPU)
        {
            return "CPU Wins";
        }

        return $"Player {winningPlayerId} Wins";
    }

    public void RestartMatch()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

}
