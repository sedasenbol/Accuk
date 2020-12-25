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
    [SerializeField] private List<Image> powerUpImages;

    private const int POWER_UP_IMAGE_COUNT = 4;
    private const int DOUBLE_TAP_COIN_COUNT = 50;
    private const float POWER_UP_DURATION = 25f;
    private GameState gameState;
    private int activePowerUpCount = 0;

    private Dictionary<int, Vector3> posDict = new Dictionary<int, Vector3>(POWER_UP_IMAGE_COUNT);
    private Dictionary<imageColor, int> powerUpImagesPosDict = new Dictionary<imageColor, int> { { imageColor.red, 0}, {imageColor.black, 1 }, { imageColor.blue, 2 }, { imageColor.green, 3 } };
    private Dictionary<imageColor, float> powerUpImagesCountersDict = new Dictionary<imageColor, float> { { imageColor.red, 0 },{ imageColor.black, 0 },{ imageColor.blue, 0 },{ imageColor.green, 0 } };

    private readonly Color white = new Color(1f, 1f, 1f);
    private readonly Color black = new Color(0f, 0f, 0f);
    private readonly Color red = new Color(248f/255f, 11f/255f, 11f/255f);
    private readonly Color blue = new Color(0f, 149f/255f, 1f);
    private readonly Color green = new Color(0f, 199f/255f, 34f/255f);

    private enum imageColor
    {
        red = 0,
        black = 1,  
        blue = 2,
        green = 3,
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnPlayerTapped?.Invoke();
        LoadGameScreen();
    }

    private IEnumerator FadeColor(int posIndex, imageColor colorOfImage, Color colorToFade)
    {
        while (powerUpImagesCountersDict[colorOfImage]<POWER_UP_DURATION)
        {
            powerUpImagesCountersDict[colorOfImage] += Time.deltaTime;
            powerUpImages[(int)colorOfImage].color = Color.Lerp(colorToFade, white, powerUpImagesCountersDict[colorOfImage] / POWER_UP_DURATION);
            yield return new WaitForEndOfFrame();
        }

        powerUpImages[(int)colorOfImage].gameObject.SetActive(false);
        activePowerUpCount--;
        powerUpImagesCountersDict[colorOfImage] = 0;
        ShiftPowerUpColors(colorOfImage);
    }

    private void ShiftPowerUpColors(imageColor colorOfImage)
    {
        int posIndex = powerUpImagesPosDict[colorOfImage];
        for (int i = 0; i < powerUpImagesPosDict.Count; i++)
        {
            if (powerUpImagesPosDict[(imageColor)i] > posIndex)
            {
                int newPosIndex = powerUpImagesPosDict[(imageColor)i] - 1;
                powerUpImages[i].transform.position = posDict[newPosIndex];
                powerUpImagesPosDict[(imageColor)i]--;
            }
        }
    }

    private void ActivateBlueDoubleScore()
    {
        if (powerUpImagesCountersDict[imageColor.blue] > 0)
        {
            powerUpImagesCountersDict[imageColor.blue] = 0;
            return;
        }

        powerUpImages[(int)imageColor.blue].transform.position = posDict[activePowerUpCount];

        powerUpImagesPosDict[imageColor.blue] = activePowerUpCount;
        powerUpImages[(int)imageColor.blue].gameObject.SetActive(true);

        StartCoroutine(FadeColor(activePowerUpCount, imageColor.blue, blue));
        activePowerUpCount++;

    }

    private void ActivateGreenHighJump()
    {
        if (powerUpImagesCountersDict[imageColor.green] > 0)
        {
            powerUpImagesCountersDict[imageColor.green] = 0;
            return;
        }

        powerUpImages[(int)imageColor.green].transform.position = posDict[activePowerUpCount];

        powerUpImagesPosDict[imageColor.green] = activePowerUpCount;
        powerUpImages[(int)imageColor.green].gameObject.SetActive(true);

        StartCoroutine(FadeColor(activePowerUpCount, imageColor.green, green));
        activePowerUpCount++;

    }

    private void ActivateRedMagnet()
    {
        if (powerUpImagesCountersDict[imageColor.red] > 0)
        {
            powerUpImagesCountersDict[imageColor.red] = 0;
            return;
        }

        powerUpImages[(int)imageColor.red].transform.position = posDict[activePowerUpCount];

        powerUpImagesPosDict[imageColor.red] = activePowerUpCount;
        powerUpImages[(int)imageColor.red].gameObject.SetActive(true);

        StartCoroutine(FadeColor(activePowerUpCount, imageColor.red, red));
        activePowerUpCount++;

    }

    private void LoadGameScreen()
    {
        gameState = FindObjectOfType<GameManager>().stateOfTheGame;
        activePowerUpCount = 0;

        tapArea.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(false);
        topRunButton.gameObject.SetActive(false);
        pauseButton.gameObject.SetActive(true);
        coinText.gameObject.SetActive(true);
        scoreText.gameObject.SetActive(true);
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
        foreach (Image image in powerUpImages)
        {
            image.gameObject.SetActive(false);
        }
        for (int i = 0; i < POWER_UP_IMAGE_COUNT; i++)
        {
            powerUpImagesCountersDict[(imageColor)i] = 0;
        }
        StopAllCoroutines();

        pauseButton.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(true);
        tapArea.gameObject.SetActive(true);
        doubleTapText.gameObject.SetActive(false);
        topRunButton.gameObject.SetActive(true);
    }

    private void ActivateDoubleTap()
    {
        doubleTapText.text = $"-{DOUBLE_TAP_COIN_COUNT} coins";
        doubleTapText.gameObject.SetActive(true);


        powerUpImages[(int)imageColor.black].transform.position = posDict[activePowerUpCount];
        powerUpImagesPosDict[imageColor.black] = activePowerUpCount;
        powerUpImages[(int)imageColor.black].gameObject.SetActive(true);

        StartCoroutine(FadeColor(activePowerUpCount, imageColor.black, black));
        activePowerUpCount++;

    }

    private void DeactivateDoubleTap()
    {
        doubleTapText.gameObject.SetActive(false);
        powerUpImagesCountersDict[imageColor.black] = POWER_UP_DURATION;
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += LoadGameOverScreen;
        Player.OnCoinPickUp += UpdateCoinText;
        Player.OnBlueDoubleScorePickUp += ActivateBlueDoubleScore;
        Player.OnGreenHighJumpPickUp += ActivateGreenHighJump;
        Player.OnRedMagnetPickUp += ActivateRedMagnet;
        GameManager.OnActivateDoubleTap += ActivateDoubleTap;
        Player.OnDeactivateDoubleTap += DeactivateDoubleTap;  
        GameManager.OnRestartGame += LoadGameScreen;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= LoadGameOverScreen;
        Player.OnCoinPickUp -= UpdateCoinText;
        Player.OnBlueDoubleScorePickUp -= ActivateBlueDoubleScore;
        Player.OnGreenHighJumpPickUp -= ActivateGreenHighJump;
        Player.OnRedMagnetPickUp -= ActivateRedMagnet;
        GameManager.OnActivateDoubleTap -= ActivateDoubleTap;
        Player.OnDeactivateDoubleTap -= DeactivateDoubleTap;
        GameManager.OnRestartGame -= LoadGameScreen;
    }

    private void UpdateCoinText()
    {
        coinText.text = gameState.Coins.ToString();
    }

    private void UpdateScoreText()
    {
        scoreText.text = ((int)gameState.Score).ToString();
    }

    private void Start()
    {
        gameState = FindObjectOfType<GameManager>().stateOfTheGame;


        for (int i = 0; i < POWER_UP_IMAGE_COUNT; i++)
        {
            posDict[i] = powerUpImages[i].transform.position;
        }

    }

    private void Update()
    {
        UpdateScoreText();
        UpdateCoinText();
    }
}
