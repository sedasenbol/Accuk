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

    private void QuitGame()
    {
        Application.Quit();
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
        SceneManager.sceneLoaded += ((Scene scene, LoadSceneMode mode) => Destroy(stage));

        UIManager.OnPauseButtonClicked += PauseGame;
        UIManager.OnResumeButtonClicked += ResumeGame;
        UIManager.OnQuitButtonClicked += QuitGame;
        
        Player.OnPlayerDeath += GameOver;
    }

    private void OnDisable()
    {
        UIManager.OnPlayerTapped -= StartGame;
        SceneManager.sceneLoaded -= ((Scene scene, LoadSceneMode mode) => Destroy(stage));

        UIManager.OnPauseButtonClicked -= PauseGame;
        UIManager.OnResumeButtonClicked -= ResumeGame;
        UIManager.OnQuitButtonClicked -= QuitGame;
        Player.OnPlayerDeath -= GameOver;
    }

    private void Start()
    {
        stage = GameObject.Find("Stage");

        DOTween.defaultTimeScaleIndependent = true;
        DOTween.SetTweensCapacity(500,50);
    }
}
