using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject stage;
    private readonly GameState gameState = new GameState();

    private void StartGame()
    {
        SceneManager.LoadScene(1, LoadSceneMode.Additive);
        Destroy(stage);

        gameState.CurrentScene = GameState.Scene.Game;
        gameState.CurrentState = GameState.State.OnPlay;
        gameState.IsAlive = true;
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        gameState.CurrentState = GameState.State.Paused;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        gameState.CurrentState = GameState.State.OnPlay;
    }

    private void OnEnable()
    {
        UIManager.OnPlayerTapped += StartGame;
        UIManager.OnPauseButtonClicked += PauseGame;
        UIManager.OnResumeButtonClicked += ResumeGame;
    }

    private void OnDisable()
    {
        UIManager.OnPlayerTapped -= StartGame;
        UIManager.OnPauseButtonClicked -= PauseGame;
        UIManager.OnResumeButtonClicked -= ResumeGame;
    }

    private void Start()
    {

    }

    private void Update()
    {
        
    }
}
