using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using DG.Tweening;

public class UIManager : MonoBehaviour, IPointerDownHandler
{
    public static event Action OnPlayerTapped;
    public static event Action OnPauseButtonClicked;
    public static event Action OnResumeButtonClicked;

    [SerializeField] private TextMeshProUGUI tapArea;
    [SerializeField] private TextMeshProUGUI tapToPlayText;
    [SerializeField] private Button topRunButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI doubleTapText;

    private bool isPlayerActive = false;
    private bool isDoubleScoreActive = false;
    private int coinCount = 0;
    private float score = 0;
    private const int DOUBLE_TAP_COIN_COUNT = 1;
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPlayerTapped?.Invoke();
        LoadGameScreen();
    }

    private void LoadGameScreen()
    {
        coinCount = 0;
        UpdateCoinText();
        score = 0;
        isPlayerActive = true;
        tapArea.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(false);
        topRunButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        coinText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
    }

    public void HandlePauseButtonClick()
    {
        isPlayerActive = false;
        OnPauseButtonClicked?.Invoke();
        pauseButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);
    }

    public void HandleResumeButtonClick()
    {
        isPlayerActive = true;
        OnResumeButtonClicked?.Invoke();
        pauseButton.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
    }

    private void LoadGameOverScreen()
    {
        isPlayerActive = false;
        pauseButton.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(true);
        tapToPlayText.enabled = true;
        doubleTapText.gameObject.SetActive(false);
        topRunButton.gameObject.SetActive(true);
    }

    private void ActivateDoubleTap()
    {
        doubleTapText.text = $"-{DOUBLE_TAP_COIN_COUNT.ToString()} coins";
        coinCount -= DOUBLE_TAP_COIN_COUNT;
        doubleTapText.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += LoadGameOverScreen;
        Player.OnCoinPickUp += UpdateCoinText;
        Player.OnBlueDoubleScorePickUp += (() => isDoubleScoreActive = true);
        Player.OnDeactivateDoubleTap += (()=> doubleTapText.gameObject.SetActive(false));
        GameManager.OnDeactivateBlueDoubleScore += (() => isDoubleScoreActive = true);
        GameManager.OnActivateDoubleTap += ActivateDoubleTap;
        GameManager.OnRestartGame += LoadGameScreen;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= LoadGameOverScreen;
        Player.OnCoinPickUp -= UpdateCoinText;
        Player.OnBlueDoubleScorePickUp -= (() => isDoubleScoreActive = true);
        Player.OnDeactivateDoubleTap -= (() => doubleTapText.gameObject.SetActive(false));
        GameManager.OnDeactivateBlueDoubleScore -= (() => isDoubleScoreActive = false);
        GameManager.OnActivateDoubleTap -= ActivateDoubleTap;
        GameManager.OnRestartGame -= LoadGameScreen;
    }

    private void UpdateCoinText()
    {
        coinCount++;
        coinText.text = coinCount.ToString();
    }

    private void UpdateScore()
    {
        if (!isPlayerActive) { return; }

        score += (isDoubleScoreActive) ? 20 * Time.deltaTime : 10 * Time.deltaTime;
        scoreText.text = ((int)score).ToString();
    }

    private void Update()
    {
        UpdateScore();
    }
}
