using UnityEngine;
using TMPro;

public class VersusPlayerController : MonoBehaviour
{
    [Header("Player")]
    public int playerId;

    [Header("References")]
    public TileBoard board;
    public TextMeshProUGUI scoreText;
    public IncomingAttackUI incomingAttackUI;
    public VersusMatchManager matchManager;

    [Header("Movement Keys")]
    public KeyCode moveUp;
    public KeyCode moveDown;
    public KeyCode moveLeft;
    public KeyCode moveRight;

    [Header("Give Keys")]
    public KeyCode give8Key;
    public KeyCode give16Key;
    public KeyCode give32Key;
    public KeyCode give64Key;
    public KeyCode give128Key;

    [Header("CPU Mode")]
    public bool isCPU = false;
    public CPUDifficulty cpuDifficulty = CPUDifficulty.Easy;
    public float minThinkDelay = 0.4f;
    public float maxThinkDelay = 0.9f;
    private float nextCPUActionTime = 0f;
    private int strongMoveStep = 0;

    public enum CPUDifficulty
{
    Easy = 0,
    Medium = 1,
    Strong = 2
}


    private int score;
    private int pendingAttackCount;

    public int Score => score;
    public int PendingAttackCount => pendingAttackCount;

    private void Start()
    {
        PauseMenu.GameIsPaused = false;
        Time.timeScale = 1f;
        
        // Player 2 is CPU depending on menu choice
        if (playerId == 2)
        {
            int gameModeCpu = PlayerPrefs.GetInt("GAME_MODE_CPU", 0);
            isCPU = (gameModeCpu == 1);
        }

        if (board != null)
        {
            board.useBuiltInKeyboardInput = false;

            board.ClearBoard();
            board.CreateTile();
            board.CreateTile();
        }

        if (isCPU)
        {
            int savedDifficulty = PlayerPrefs.GetInt("CPU_DIFFICULTY", 0);

            //Assign the difficulty
            cpuDifficulty = (CPUDifficulty)savedDifficulty;

            Debug.Log("CPU Difficulty set to: " + cpuDifficulty);

            //Apply speed according to the difficulty
            ApplyCpuDifficultySettings();
        }

        UpdateScoreUI();
        UpdateIncomingUI();
    }

    private void Update()
    {
        if (PauseMenu.GameIsPaused)
        {
            return;
        }

        if (matchManager != null && matchManager.MatchEnded)
        {
            return;
        }

        if (board == null || board.IsResolving)
        {
            return;
        }

        if (isCPU)
        {
            HandleCPUInput();
        }
        else
        {
            HandleHumanInput();
        }
    }

    private void HandleHumanInput()
    {
        bool moved = false;
        bool attemptedMove = false;

        if (Input.GetKeyDown(moveUp))
        {
            attemptedMove = true;
            moved = board.Move(Vector2Int.up);
        }
        else if (Input.GetKeyDown(moveDown))
        {
            attemptedMove = true;
            moved = board.Move(Vector2Int.down);
        }
        else if (Input.GetKeyDown(moveLeft))
        {
            attemptedMove = true;
            moved = board.Move(Vector2Int.left);
        }
        else if (Input.GetKeyDown(moveRight))
        {
            attemptedMove = true;
            moved = board.Move(Vector2Int.right);
        }

        if (moved && matchManager != null)
        {
            matchManager.ResolvePendingAttackOnMove(this);
        }
        else if (attemptedMove && matchManager != null && !board.IsResolving)
        {
            matchManager.EvaluateWinner();
        }

        if (Input.GetKeyDown(give8Key))
        {
            TryGive(8);
        }
        if (Input.GetKeyDown(give16Key))
        {
            TryGive(16);
        }
        if (Input.GetKeyDown(give32Key))
        {
            TryGive(32);
        }
        if (Input.GetKeyDown(give64Key))
        {
            TryGive(64);
        }
        if (Input.GetKeyDown(give128Key))
        {
            TryGive(128);
        }
    }

    private void HandleCPUInput()
    {
        if (Time.time < nextCPUActionTime)
        {
            return;
        }

        nextCPUActionTime = Time.time + GetCpuThinkDelay();

        if (matchManager == null || matchManager.sharedOrder == null || board == null)
        {
            return;
        }

        int targetValue = matchManager.sharedOrder.CurrentValue;

        if (board.HasTileWithValue(targetValue))
        {
            TryGive(targetValue);
            return;
        }

        switch (cpuDifficulty)
        {
            case CPUDifficulty.Medium:
                TryCpuMoveMedium();
                break;

            case CPUDifficulty.Strong:
                TryCpuMoveStrong();
                break;

            case CPUDifficulty.Easy:
            default:
                TryCpuMoveEasy();
                break;
        }
    }

    private void ApplyCpuDifficultySettings()
    {
        switch (cpuDifficulty)
        {
            case CPUDifficulty.Medium:
                minThinkDelay = 0.45f;
                maxThinkDelay = 0.8f;
                break;

            case CPUDifficulty.Strong:
                minThinkDelay = 0.45f;
                maxThinkDelay = 0.8f;
                break;

            case CPUDifficulty.Easy:
            default:
                minThinkDelay = 0.8f;
                maxThinkDelay = 1.3f;
                break;
        }
    }


//CPU movement logics
    private void TryCpuMoveEasy()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down
        };

        ShuffleDirections(directions);
        TryMoveFromDirectionList(directions);
    }

    private void TryCpuMoveMedium()
    {
        Vector2Int[] directions =
        {
            Vector2Int.up,
            Vector2Int.left,
            Vector2Int.right,
            Vector2Int.down
        };

        TryMoveFromDirectionList(directions);
    }

    private void TryCpuMoveStrong()
    {
        Vector2Int[] directions = GetStrongMoveDirections();
        Vector2Int movedDirection;
        bool moved = TryMoveFromDirectionList(directions, out movedDirection);
        AdvanceStrongMoveSequence(moved, movedDirection);
    }

    private Vector2Int[] GetStrongMoveDirections()
    {
        switch (strongMoveStep)
        {
            case 0:
                return new Vector2Int[]
                {
                    Vector2Int.left,
                    Vector2Int.down,
                    Vector2Int.right
                };

            case 1:
                return new Vector2Int[]
                {
                    Vector2Int.down,
                    Vector2Int.right,
                    Vector2Int.left
                };

            case 2:
                return new Vector2Int[]
                {
                    Vector2Int.right,
                    Vector2Int.down,
                    Vector2Int.left
                };

            default:
                return new Vector2Int[]
                {
                    Vector2Int.down,
                    Vector2Int.left,
                    Vector2Int.right
                };
        }
    }

    private void AdvanceStrongMoveSequence(bool moved, Vector2Int movedDirection)
    {
        if (!moved)
        {
            strongMoveStep = (strongMoveStep == 0 || strongMoveStep == 1) ? 2 : 0;
            return;
        }

        if (movedDirection == Vector2Int.left)
        {
            strongMoveStep = 1;
        }
        else if (movedDirection == Vector2Int.right)
        {
            strongMoveStep = 3;
        }
        else if (movedDirection == Vector2Int.down)
        {
            strongMoveStep = (strongMoveStep == 0 || strongMoveStep == 1) ? 2 : 0;
        }
    }

    private void ShuffleDirections(Vector2Int[] array) 
    {
        for (int i = 0; i < array.Length; i++)
        {
            int j = Random.Range(i, array.Length);
            Vector2Int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
    private float GetCpuThinkDelay()
    {
        float delay = Random.Range(minThinkDelay, maxThinkDelay);

        if (cpuDifficulty == CPUDifficulty.Strong && HasPendingAttack())
        {
            delay *= 0.6f;
        }

        return delay;
    }

    private bool TryMoveFromDirectionList(Vector2Int[] directions, out Vector2Int movedDirection)
    {
        bool attemptedMove = false;
        bool moved = false;
        movedDirection = Vector2Int.zero;

        for (int i = 0; i < directions.Length; i++)
        {
            attemptedMove = true;

            if (board.Move(directions[i]))
            {
                moved = true;
                movedDirection = directions[i];
                break;
            }
        }

        if (moved && matchManager != null)
        {
            matchManager.ResolvePendingAttackOnMove(this);
        }
        else if (attemptedMove && matchManager != null && !board.IsResolving)
        {
            matchManager.EvaluateWinner();
        }

        return moved;
    }

    private void TryMoveFromDirectionList(Vector2Int[] directions)
    {
        Vector2Int movedDirection;
        TryMoveFromDirectionList(directions, out movedDirection);
    }

    private void TryGive(int value)
    {
        if (matchManager != null)
        {
            matchManager.PlayerTryGive(this, value);
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
            scoreText.text = score.ToString();
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
            incomingAttackUI.SetCount(pendingAttackCount);
    }
}
