using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    // public static bool GameIsOver = false;
    public GameObject GameOverUI;
    public GameObject PauseMenuUI;
    public PlayerInput player;

    private void Awake(){
        player = GameObject.FindObjectOfType<PlayerController>().GetComponent<PlayerInput>();
    }

    public void CheckPause(InputAction.CallbackContext ctx)
    {
        if (GameIsPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void ResumeGame()
    {
        player.ActivateInput();
        PauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    public void PauseGame()
    {
        player.DeactivateInput();
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void GameOver () 
    {
        player.DeactivateInput();
        GameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    public void RestartGame() 
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name);
    }

    public void ExitGame()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
