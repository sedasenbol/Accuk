using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static event Action OnDeactivateRedMagnet;
    public static event Action OnDeactivateGreenHighJump;
    public static event Action OnDeactivateBlueDoubleScore;
    public static event Action OnActivateDoubleTap;
    public static event Action OnDeactivateDoubleTap;
    public static event Action OnRestartGame;

    private GameObject stage;
    private GameState gameState = new GameState();
    public GameState stateOfTheGame => gameState;
    private float redMagnetStartTime;
    private float greenHighJumpStartTime;
    private float blueDoubleScoreStartTime;
    private float doubleTapStartTime;
    private const float RED_MAGNET_DURATION = 25f;
    private const float GREEN_HIGH_JUMP_DURATION = 25f;
    private const float BLUE_DOUBLE_SCORE_DURATION = 25f;
    private const float DOUBLE_TAP_DURATION = 25f;
    private const int DOUBLE_TAP_COIN_COUNT = 1;

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
        gameState.CurrentState = GameState.State.Paused;
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f;
        gameState.CurrentState = GameState.State.OnPlay;
    }

    private void GameOver()
    {
        Time.timeScale = 0f;
        gameState.CurrentState = GameState.State.GameOver;
        gameState.IsAlive = false;
    }

    private void IncreaseCoins()
    {
        gameState.Coins++;
    }

    private void ActivateRedMagnet()
    {
        gameState.IsRedMagnetActive = true;
        redMagnetStartTime = Time.time;
    }

    private void DeactivateRedMagnet()
    {
        if (!gameState.IsRedMagnetActive || Time.time < redMagnetStartTime + RED_MAGNET_DURATION) { return; }

        gameState.IsRedMagnetActive = false;
        OnDeactivateRedMagnet?.Invoke();
    }

    private void ActivateGreenHighJump()
    {
        gameState.IsGreenHighJumpActive = true;
        greenHighJumpStartTime = Time.time;
    }
    private void DeactivateGreenHighJump()
    {
        if (!gameState.IsGreenHighJumpActive || Time.time < greenHighJumpStartTime + GREEN_HIGH_JUMP_DURATION) { return; }

        gameState.IsGreenHighJumpActive = false;
        OnDeactivateGreenHighJump?.Invoke();
    }

    private void ActivateBlueDoubleScore()
    {
        gameState.IsBlueDoubleScoreActive = true;
        blueDoubleScoreStartTime = Time.time;
    }
    private void DeactivateBlueDoubleScore()
    {
        if (!gameState.IsBlueDoubleScoreActive || Time.time < blueDoubleScoreStartTime + BLUE_DOUBLE_SCORE_DURATION) { return; }

        gameState.IsBlueDoubleScoreActive = false;
        OnDeactivateBlueDoubleScore?.Invoke();
    }

    private void ActivateDoubleTap()
    {
        if (gameState.Coins <  DOUBLE_TAP_COIN_COUNT || gameState.IsDoubleTapActive) { return; }

        gameState.IsDoubleTapActive = true;
        doubleTapStartTime = Time.time;
        OnActivateDoubleTap?.Invoke();
        gameState.Coins -= DOUBLE_TAP_COIN_COUNT;
    }

    private void DeactivateDoubleTap()
    {
        if (!gameState.IsDoubleTapActive || Time.time < doubleTapStartTime + DOUBLE_TAP_DURATION) { return; } 

        gameState.IsDoubleTapActive = false;
        OnDeactivateDoubleTap?.Invoke();
    }

    private void OnEnable()
    {
        UIManager.OnPlayerTapped += StartGame;
        UIManager.OnPauseButtonClicked += PauseGame;
        UIManager.OnResumeButtonClicked += ResumeGame;
        Player.OnPlayerDeath += GameOver;
        Player.OnCoinPickUp += IncreaseCoins;
        Player.OnRedMagnetPickUp += ActivateRedMagnet;
        Player.OnGreenHighJumpPickUp += ActivateGreenHighJump;
        Player.OnBlueDoubleScorePickUp += ActivateBlueDoubleScore;
        Player.OnDeactivateDoubleTap += (()=> doubleTapStartTime = Time.time - DOUBLE_TAP_DURATION);
        TouchController.OnDoubleTapMovement += ActivateDoubleTap;
    }

    private void OnDisable()
    {
        UIManager.OnPlayerTapped -= StartGame;
        UIManager.OnPauseButtonClicked -= PauseGame;
        UIManager.OnResumeButtonClicked -= ResumeGame;
        Player.OnPlayerDeath -= GameOver;
        Player.OnCoinPickUp -= IncreaseCoins;
        Player.OnRedMagnetPickUp -= ActivateRedMagnet;
        Player.OnGreenHighJumpPickUp -= ActivateGreenHighJump;
        Player.OnBlueDoubleScorePickUp -= ActivateBlueDoubleScore;
        Player.OnDeactivateDoubleTap -= (() => doubleTapStartTime = Time.time - DOUBLE_TAP_DURATION);
        TouchController.OnDoubleTapMovement -= ActivateDoubleTap;
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

    private void Update()
    {
        DeactivateRedMagnet();
        DeactivateGreenHighJump();
        DeactivateBlueDoubleScore();
        DeactivateDoubleTap();
    }
}
