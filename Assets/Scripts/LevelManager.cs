using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class LevelManager : MonoBehaviour
{
    public static event Action OnDeactivateRedMagnet;
    public static event Action OnDeactivateGreenHighJump;
    public static event Action OnDeactivateBlueDoubleScore;
    public static event Action OnActivateDoubleTap;
    public static event Action OnDeactivateDoubleTap;

    public LevelState StateOfTheLevel => levelState;
    private LevelState levelState = new LevelState();

    private const float RED_MAGNET_DURATION = 25f;
    private const float GREEN_HIGH_JUMP_DURATION = 25f;
    private const float BLUE_DOUBLE_SCORE_DURATION = 25f;
    private const float DOUBLE_TAP_DURATION = 25f;
    private const int DOUBLE_TAP_COIN_COUNT = 50;

    private float redMagnetStartTime;
    private float greenHighJumpStartTime;
    private float blueDoubleScoreStartTime;
    private float doubleTapStartTime;

    private void IncreaseCoins()
    {
        levelState.Coins++;
    }

    private void IncreaseScore()
    {
        if (!levelState.IsAlive) { return; }

        levelState.Score += (levelState.IsBlueDoubleScoreActive) ? 20 * Time.deltaTime : 10 * Time.deltaTime;
    }

    private void ActivateRedMagnet()
    {
        levelState.IsRedMagnetActive = true;
        redMagnetStartTime = Time.time;
    }

    private void DeactivateRedMagnet()
    {
        if (!levelState.IsRedMagnetActive || Time.time < redMagnetStartTime + RED_MAGNET_DURATION) { return; }

        levelState.IsRedMagnetActive = false;
        OnDeactivateRedMagnet?.Invoke();
    }

    private void ActivateGreenHighJump()
    {
        levelState.IsGreenHighJumpActive = true;
        greenHighJumpStartTime = Time.time;
    }
    private void DeactivateGreenHighJump()
    {
        if (!levelState.IsGreenHighJumpActive || Time.time < greenHighJumpStartTime + GREEN_HIGH_JUMP_DURATION) { return; }

        levelState.IsGreenHighJumpActive = false;
        OnDeactivateGreenHighJump?.Invoke();
    }

    private void ActivateBlueDoubleScore()
    {
        levelState.IsBlueDoubleScoreActive = true;
        blueDoubleScoreStartTime = Time.time;
    }
    private void DeactivateBlueDoubleScore()
    {
        if (!levelState.IsBlueDoubleScoreActive || Time.time < blueDoubleScoreStartTime + BLUE_DOUBLE_SCORE_DURATION) { return; }

        levelState.IsBlueDoubleScoreActive = false;
        OnDeactivateBlueDoubleScore?.Invoke();
    }

    private void ActivateDoubleTap()
    {
        if (levelState.IsDoubleTapActive || levelState.Coins < DOUBLE_TAP_COIN_COUNT) { return; }

        levelState.Coins -= DOUBLE_TAP_COIN_COUNT;

        levelState.IsDoubleTapActive = true;
        doubleTapStartTime = Time.time;
        OnActivateDoubleTap?.Invoke();
    }

    private void DeactivateDoubleTap()
    {
        if (!levelState.IsDoubleTapActive || Time.time < doubleTapStartTime + DOUBLE_TAP_DURATION) { return; }

        levelState.IsDoubleTapActive = false;
        OnDeactivateDoubleTap?.Invoke();
    }

    private void OnEnable()
    {
        Player.OnCoinPickUp += IncreaseCoins;

        Player.OnRedMagnetPickUp += ActivateRedMagnet;
        Player.OnGreenHighJumpPickUp += ActivateGreenHighJump;
        Player.OnBlueDoubleScorePickUp += ActivateBlueDoubleScore;

        Player.OnDeactivateDoubleTap += (()=> doubleTapStartTime = Time.time - DOUBLE_TAP_DURATION);
        TouchController.OnDoubleTapMovement += ActivateDoubleTap;

        UIManager.OnPauseButtonClicked += (()=> levelState.IsAlive = false);
        UIManager.OnResumeButtonClicked += (() => levelState.IsAlive = true);

        Player.OnPlayerDeath += (() => levelState.IsAlive = false);
    }

    private void OnDisable()
    {
        Player.OnCoinPickUp -= IncreaseCoins;

        Player.OnRedMagnetPickUp -= ActivateRedMagnet;
        Player.OnGreenHighJumpPickUp -= ActivateGreenHighJump;
        Player.OnBlueDoubleScorePickUp -= ActivateBlueDoubleScore;

        Player.OnDeactivateDoubleTap -= (() => doubleTapStartTime = Time.time - DOUBLE_TAP_DURATION);
        TouchController.OnDoubleTapMovement -= ActivateDoubleTap;

        UIManager.OnPauseButtonClicked -= (() => levelState.IsAlive = false);
        UIManager.OnResumeButtonClicked -= (() => levelState.IsAlive = true);

        Player.OnPlayerDeath -= (() => levelState.IsAlive = false);
    }

    private void Update()
    {
        IncreaseScore();

        DeactivateRedMagnet();
        DeactivateGreenHighJump();
        DeactivateBlueDoubleScore();
        DeactivateDoubleTap();
    }
}
