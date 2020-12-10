using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;

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
    public void OnPointerDown(PointerEventData eventData)
    {
        OnPlayerTapped?.Invoke();
        HandlePlayerTap();
    }

    private void HandlePlayerTap()
    {
        tapArea.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(false);
        topRunButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
    }

    public void HandlePauseButtonClick()
    {
        OnPauseButtonClicked?.Invoke();
        pauseButton.gameObject.SetActive(false);
        resumeButton.gameObject.SetActive(true);
    }

    public void HandleResumeButtonClick()
    {
        OnResumeButtonClicked?.Invoke();
        pauseButton.gameObject.SetActive(true);
        resumeButton.gameObject.SetActive(false);
    }

    private void LoadGameOverScreen()
    {
        pauseButton.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(true);
        topRunButton.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += LoadGameOverScreen;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= LoadGameOverScreen;
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
