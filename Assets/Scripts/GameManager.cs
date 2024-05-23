using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static readonly float distanceConversionRate = 0.01f;
    public static readonly float movementUpdateRate = 0.015f;

    public static GameObject Player;
    void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this);
        }
        Player = GameObject.Find("Player");

    }

    void Update()
    {
        
    }

    public void StartGame()
    {
        UIManager.instance.GoToMenu(UIManager.Menu.HUD);
        Time.timeScale = 1.0f;
    }

    public void ResumeGame()
    {
        UIManager.instance.GoToMenu(UIManager.Menu.HUD);
        Time.timeScale = 1.0f;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        UIManager.instance.GoToMenu(UIManager.Menu.Pause);
    }

    public void EndGame()
    {
        Time.timeScale = 0f;
        UIManager.instance.GoToMenu(UIManager.Menu.End);
    }

    public void PlayerDeath()
    {


        EndGame();
    }
}
