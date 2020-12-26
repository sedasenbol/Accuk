using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static event Action OnRestartGame;

    private GameObject stage;
    private GameState gameState = new GameState();


    private void StartGame()
    {
        if (gameState.CurrentScene == GameState.Scene.Game)
        {
            OnRestartGame?.Invoke();

            SceneManager.UnloadSceneAsync(1);
            Time.timeScale = 1f;

            gameState = new GameState();
            gameState.CurrentScene = GameState.Scene.MainMenu;
        }
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        Destroy(stage);
        gameState.CurrentScene = GameState.Scene.Game;
        gameState.CurrentState = GameState.State.OnPlay;
        gameState.IsAlive = true;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        gameState.IsAlive = false;
        gameState.CurrentState = GameState.State.Paused;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        gameState.IsAlive = true;
        gameState.CurrentState = GameState.State.OnPlay;
    }

    private void GameOver()
    {
        Time.timeScale = 0f;
        gameState.CurrentState = GameState.State.GameOver;
        gameState.IsAlive = false;
    }

    private void OnEnable()
    {
        UIManager.OnPlayerTapped += StartGame;
        UIManager.OnPauseButtonClicked += PauseGame;
        UIManager.OnResumeButtonClicked += ResumeGame;
        Player.OnPlayerDeath += GameOver;
    }

    private void OnDisable()
    {
        UIManager.OnPlayerTapped -= StartGame;
        UIManager.OnPauseButtonClicked -= PauseGame;
        UIManager.OnResumeButtonClicked -= ResumeGame;
        Player.OnPlayerDeath -= GameOver;
    }

    private void Start()
    {
        if (FindObjectsOfType<GameManager>().Length > 1) 
        {
            Destroy(this.gameObject);
        }

        stage = GameObject.Find("Stage");

        DOTween.defaultTimeScaleIndependent = true;
        DOTween.SetTweensCapacity(500,50);
    }
}
