using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager activeInstance;

    //customer
    public CustomerOrderManager orderManager;
    
    public TileBoard board;
    public CanvasGroup gameOver;
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI infoMessage;


    private int score;
    private float giveCooldownUntil = 0f;


    //give buttons
    public Button give8Button;
    public Button give16Button;
    public Button give32Button;
    public Button give64Button;
    public Button give128Button;
    private void Awake()
    {
        if (activeInstance != null && activeInstance != this)
        {
            Debug.LogWarning(
                $"Duplicate GameManager on '{gameObject.name}' in scene '{gameObject.scene.name}'. " +
                $"Keeping '{activeInstance.gameObject.name}' and disabling this component."
            );
            enabled = false;
            return;
        }

        activeInstance = this;
    }

    private void OnDestroy()
    {
        if (activeInstance == this)
        {
            activeInstance = null;
        }
    }

    private void Start()
    {
        Debug.Log($"GameManager START: name={gameObject.name}, id={GetInstanceID()}, active={gameObject.activeInHierarchy}, scene={gameObject.scene.name}");
        NewGame();

        // Remove any old listeners to avoid double-calls if the scene reloads
        //if (give8Button != null)  { give8Button.onClick.RemoveAllListeners();  give8Button.onClick.AddListener(() => OnGivePressed(8)); }
        //if (give16Button != null)  { give16Button.onClick.RemoveAllListeners();  give16Button.onClick.AddListener(() => OnGivePressed(16)); }
        //if (give32Button != null)  { give32Button.onClick.RemoveAllListeners();  give32Button.onClick.AddListener(() => OnGivePressed(32)); }
        //if (give64Button != null)  { give64Button.onClick.RemoveAllListeners();  give64Button.onClick.AddListener(() => OnGivePressed(64)); }
        //if (give128Button != null) { give128Button.onClick.RemoveAllListeners(); give128Button.onClick.AddListener(() => OnGivePressed(128)); }

        // Initialize score UI
        if (scoreText != null) scoreText.text = score.ToString();
    }

    private void Update()
    {
        //number keys for giving tiles
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1))
            {
                Debug.Log("Key1 detected by GameManager id=" + GetInstanceID());
                OnGivePressed(8);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2))
            {
                Debug.Log("Key2 detected by GameManager id=" + GetInstanceID());
                OnGivePressed(16);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3))
            {
                Debug.Log("Key3 detected by GameManager id=" + GetInstanceID());
                OnGivePressed(32);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4))
            {
                Debug.Log("Key4 detected by GameManager id=" + GetInstanceID());
                OnGivePressed(64);
            }
            if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            {
                Debug.Log("Key5 detected by GameManager id=" + GetInstanceID());
                OnGivePressed(128);
            }
    }
    public void NewGame()
    {
        SetScore(0);

        if (highScoreText != null)
        highScoreText.text = LoadHiscore().ToString();
        else
        Debug.LogError("GameManager: highScoreText is not assigned!");

        int highScore = LoadHiscore();
        if (gameOver != null)
        {
        gameOver.alpha = 0f;
        gameOver.interactable = false;
        }
        else Debug.LogError("GameManager: gameOver CanvasGroup not assigned!");

        if (board == null) { Debug.LogError("GameManager: board not assigned!"); return; }

        board.ClearBoard();
        board.CreateTile();
        board.CreateTile();
        board.enabled = true;

        if (orderManager != null) orderManager.ResetOrders();
        else Debug.LogError("GameManager: orderManager not assigned!");
    }

    public void GameOver()
    {
        board.enabled = false;
        gameOver.interactable = true;

        StartCoroutine((Fade(gameOver, 1f, 1f)));
    }

    private IEnumerator Fade(CanvasGroup canvasGroup, float to, float delay)
    {
        yield return new WaitForSeconds(delay);

        float elapsed = 0f;
        float duration = 0.5f;
        float from = canvasGroup.alpha;

        while (elapsed < duration) //loop until the elapsed time reaches the duration
        {
            canvasGroup.alpha = Mathf.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        canvasGroup.alpha = to; //ensure final alpha is set
    }

    public void IncreaseScore(int points)
    {
        SetScore(score + points);
    }

    private void SetScore(int score)
    {
    this.score = score;

    if (scoreText != null)
        scoreText.text = score.ToString();
    else
        Debug.LogError("GameManager: scoreText is not assigned!");

    SaveHighScore();
    }


    private void SaveHighScore()
    {
        int highScore = LoadHiscore();

        if (score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
        }
    }

    private int LoadHiscore()
    {
        return PlayerPrefs.GetInt("highScore", 0); //default to 0 if no high score saved
    }

    public IEnumerator ShowMessage(string text, float duration = 1.5f)
    {
        infoMessage.text = text;
        yield return new WaitForSeconds(duration);
        infoMessage.text = "";
    }

    private string GetTileDisplayName(int value)
    {
        return value switch
        {
            8 => "siu mai",
            16 => "pineapple bun",
            32 => "french toast",
            64 => "lemon tea",
            128 => "milk tea",
            _ => value.ToString()
        };
    }


    //Give buttons methods
    public void OnGivePressed(int value)
    {
        if (Time.unscaledTime < giveCooldownUntil) return;
        giveCooldownUntil = Time.unscaledTime + 0.12f;

        Debug.Log($"Give button pressed for value: {value}");
        // Check if any customer wants this value
        if (!orderManager.HasOrder(value))
        {
            StartCoroutine(ShowMessage($"No customer wants {GetTileDisplayName(value)}!"));
            Debug.Log($"Attempted to give {value}, but no order requests it.");
            return; // stop here
        }

        // Ask the board to remove ONE tile of that value
        bool removed = board.TryGiveOneTile(value);

        if (removed)
        {
            Debug.Log($"Removed a {value} tile.");
            bool matched = orderManager.TryFulfillOrder(value);
            if (matched)
            {
                IncreaseScore(value); // add exactly the value you gave
            }
            else
            {
                StartCoroutine(ShowMessage($"Gave a {value} tile!"));
            }
        }
        else
        {
            StartCoroutine(ShowMessage($"No {GetTileDisplayName(value)} tile!"));
            Debug.Log($"No tile with number {value} found to give!");
        }
    }

}
