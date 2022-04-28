using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class MainMenu : MonoBehaviour
{
    public PlayerInput player;

    public void StartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("Level1");
    }

    public void StartInfiniteLevel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync("LevelInfinite");
    }

    public void Menu()
    {
        SceneManager.LoadSceneAsync("MainMenu");
    }
}
