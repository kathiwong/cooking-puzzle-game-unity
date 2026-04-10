using UnityEngine;
using TMPro;
using System.Collections;

public class CPUPlayerController : MonoBehaviour
{
    [Header("References")]
    public int playerId = 2;
    public TileBoard board;
    public TextMeshProUGUI scoreText;
    public IncomingAttackUI incomingAttackUI;
    public VersusMatchManager matchManager;

    [Header("Bot Timing")]
    public float minThinkDelay = 0.4f;
    public float maxThinkDelay = 0.9f;
    public float giveAttemptExtraDelay = 0.15f;

    private int score;
    private int pendingAttackCount;
    private bool takingTurn;

    public int Score => score;
    public int PendingAttackCount => pendingAttackCount;

    private void Start()
    {
        if (board != null)
        {
            board.useBuiltInKeyboardInput = false;
            board.ClearBoard();
            board.CreateTile();
            board.CreateTile();
        }

        UpdateScoreUI();
        UpdateIncomingUI();

        StartCoroutine(BotLoop());
    }

    private IEnumerator BotLoop()
    {
        while (true)
        {
            if (PauseMenu.GameIsPaused)
            {
                yield return null;
                continue;
            }

            if (matchManager != null && matchManager.MatchEnded)
            {
                yield break;
            }

            if (board == null || matchManager == null || matchManager.sharedOrder == null)
            {
                yield return null;
                continue;
            }

            if (takingTurn || board.IsResolving)
            {
                yield return null;
                continue;
            }

            takingTurn = true;

            float delay = Random.Range(minThinkDelay, maxThinkDelay);
            yield return new WaitForSeconds(delay);

            if (PauseMenu.GameIsPaused)
            {
                takingTurn = false;
                continue;
            }

            if (matchManager != null && matchManager.MatchEnded)
            {
                yield break;
            }

            TakeTurn();

            takingTurn = false;
        }
    }

    private void TakeTurn()
    {
        if (board == null || matchManager == null || matchManager.sharedOrder == null)
        {
            return;
        }

        int targetValue = matchManager.sharedOrder.CurrentValue;

        // Priority 1: if CPU already has the requested tile, try to serve it
        if (board.HasTileWithValue(targetValue))
        {
            matchManager.PlayerTryGive(this.AsVersusProxy(), targetValue);
            return;
        }

        // Priority 2: otherwise make one legal move
        TryBestSimpleMove();
    }

    private void TryBestSimpleMove()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down
        };

        Shuffle(directions);

        bool attemptedMove = false;
        bool moved = false;

        for (int i = 0; i < directions.Length; i++)
        {
            attemptedMove = true;
            if (board.Move(directions[i]))
            {
                moved = true;
                break;
            }
        }

        if (moved && matchManager != null)
        {
            matchManager.ResolvePendingAttackOnMove(this.AsVersusProxy());
        }
        else if (attemptedMove && matchManager != null && !board.IsResolving)
        {
            matchManager.EvaluateWinner();
        }
    }

    private void Shuffle(Vector2Int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = Random.Range(i, array.Length);
            Vector2Int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }

    public void AddScore(int amount)
    {
        score += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString();
        }
    }

    public void AddPendingAttack(int amount)
    {
        pendingAttackCount += amount;
        UpdateIncomingUI();
    }

    public bool HasPendingAttack()
    {
        return pendingAttackCount > 0;
    }

    public int ConsumePendingAttack()
    {
        int amount = pendingAttackCount;
        pendingAttackCount = 0;
        UpdateIncomingUI();
        return amount;
    }

    public void ReducePendingAttack(int amount)
    {
        pendingAttackCount = Mathf.Max(0, pendingAttackCount - amount);
        UpdateIncomingUI();
    }

    private void UpdateIncomingUI()
    {
        if (incomingAttackUI != null)
        {
            incomingAttackUI.SetCount(pendingAttackCount);
        }
    }

    // Temporary adapter helper
    private VersusPlayerController AsVersusProxy()
    {
        return GetComponent<VersusPlayerController>();
    }
}