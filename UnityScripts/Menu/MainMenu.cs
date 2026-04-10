using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_Dropdown maxTileDropdown;
    private readonly int[] maxTileOptions = { 64, 128, 256, 512, 1024, 2048 };

    [Header("Difficulty Popup")]
    public GameObject difficultyPopup;
    public string versusSceneName = "LocalVersus";

    public void ChangeScene(string sceneName)
    {
        Debug.Log("Change scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit();
    }

    // Called by LocalVersus button
    public void OpenDifficultyPopup()
    {
        if (difficultyPopup != null)
        {
            difficultyPopup.SetActive(true);
        }
    }

    public void CloseDifficultyPopup()
    {
        if (difficultyPopup != null)
        {
            difficultyPopup.SetActive(false);
        }
    }

    public void StartLocalVS()
    {
        PlayerPrefs.SetInt("GAME_MODE_CPU", 0);
        PlayerPrefs.Save();

        Debug.Log("Start PvP Local Versus");
        SceneManager.LoadScene(versusSceneName);
    }

    public void StartPVCWithDifficulty(int difficulty)
    {
        PlayerPrefs.SetInt("GAME_MODE_CPU", 1);
        PlayerPrefs.SetInt("CPU_DIFFICULTY", difficulty);
        PlayerPrefs.Save();

        Debug.Log($"Start PvC difficulty {difficulty}");
        SceneManager.LoadScene(versusSceneName);
    }
}