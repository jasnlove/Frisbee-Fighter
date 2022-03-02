using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    public GameObject PauseMenuUI;
    public PlayerInput player;

    private void Awake(){
        player = GameObject.FindObjectOfType<PlayerController>().GetComponent<PlayerInput>();
    }

    public void CheckPause()
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
        Debug.Log("Button pressed!");
    }

    public void PauseGame()
    {
        player.DeactivateInput();
        PauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }
}
