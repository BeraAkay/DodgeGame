using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static readonly float distanceConversionRate = 0.01f;
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
