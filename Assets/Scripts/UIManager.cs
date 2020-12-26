using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour, IPointerDownHandler
{
    public static event Action OnPlayerTapped;
    public static event Action OnPauseButtonClicked;
    public static event Action OnResumeButtonClicked;
    public static event Action OnQuitButtonClicked;

    [SerializeField] private TextMeshProUGUI tapArea;
    [SerializeField] private TextMeshProUGUI tapToPlayText;
    [SerializeField] private Button quitButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button resumeButton;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI doubleTapText;
    [SerializeField] private List<Image> powerUpImages;

    private const int POWER_UP_IMAGE_COUNT = 4;
    private const float POWER_UP_DURATION = 25f;
    private const int DOUBLE_TAP_COIN_COUNT = 50;
    private LevelState levelState;
    private int activePowerUpCount = 0;
    private bool isGameActive = false;

    private readonly Dictionary<int, Vector3> posDict = new Dictionary<int, Vector3>(POWER_UP_IMAGE_COUNT);
    private readonly Dictionary<ImageColor, int> powerUpImagesPosDict = new Dictionary<ImageColor, int> { { ImageColor.red, 0}, { ImageColor.black, 1 }, { ImageColor.blue, 2 }, { ImageColor.green, 3 } };
    private readonly Dictionary<ImageColor, float> powerUpImagesCountersDict = new Dictionary<ImageColor, float> { { ImageColor.red, 0 },{ ImageColor.black, 0 },{ ImageColor.blue, 0 },{ ImageColor.green, 0 } };

    private readonly Color white = new Color(1f, 1f, 1f);
    private readonly Color black = new Color(0f, 0f, 0f);
    private readonly Color red = new Color(248f/255f, 11f/255f, 11f/255f);
    private readonly Color blue = new Color(0f, 149f/255f, 1f);
    private readonly Color green = new Color(0f, 199f/255f, 34f/255f);

    private enum ImageColor
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

    public void HandleQuitButtonClick()
    {
        OnQuitButtonClicked?.Invoke();
    }

    private IEnumerator FadeColor(ImageColor colorOfImage, Color colorToFade)
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

    private void ShiftPowerUpColors(ImageColor colorOfImage)
    {
        int posIndex = powerUpImagesPosDict[colorOfImage];

        for (int i = 0; i < POWER_UP_IMAGE_COUNT; i++)
        {
            if (powerUpImagesPosDict[(ImageColor)i] > posIndex)
            {
                int newPosIndex = powerUpImagesPosDict[(ImageColor)i] - 1;
                powerUpImages[i].transform.position = posDict[newPosIndex];
                powerUpImagesPosDict[(ImageColor)i]--;
            }
        }
    }

    private void ActivateBlueDoubleScore()
    {
        if (powerUpImagesCountersDict[ImageColor.blue] > 0)
        {
            powerUpImagesCountersDict[ImageColor.blue] = 0;
            return;
        }

        powerUpImages[(int)ImageColor.blue].transform.position = posDict[activePowerUpCount];

        powerUpImagesPosDict[ImageColor.blue] = activePowerUpCount;
        powerUpImages[(int)ImageColor.blue].gameObject.SetActive(true);

        StartCoroutine(FadeColor(ImageColor.blue, blue));
        activePowerUpCount++;
    }

    private void ActivateGreenHighJump()
    {
        if (powerUpImagesCountersDict[ImageColor.green] > 0)
        {
            powerUpImagesCountersDict[ImageColor.green] = 0;
            return;
        }

        powerUpImages[(int)ImageColor.green].transform.position = posDict[activePowerUpCount];

        powerUpImagesPosDict[ImageColor.green] = activePowerUpCount;
        powerUpImages[(int)ImageColor.green].gameObject.SetActive(true);

        StartCoroutine(FadeColor(ImageColor.green, green));
        activePowerUpCount++;
    }

    private void ActivateRedMagnet()
    {
        if (powerUpImagesCountersDict[ImageColor.red] > 0)
        {
            powerUpImagesCountersDict[ImageColor.red] = 0;
            return;
        }

        powerUpImages[(int)ImageColor.red].transform.position = posDict[activePowerUpCount];

        powerUpImagesPosDict[ImageColor.red] = activePowerUpCount;
        powerUpImages[(int)ImageColor.red].gameObject.SetActive(true);

        StartCoroutine(FadeColor(ImageColor.red, red));
        activePowerUpCount++;
    }

    private void ActivateDoubleTap()
    {
        doubleTapText.text = $"-{DOUBLE_TAP_COIN_COUNT} coins";
        doubleTapText.gameObject.SetActive(true);


        powerUpImages[(int)ImageColor.black].transform.position = posDict[activePowerUpCount];
        powerUpImagesPosDict[ImageColor.black] = activePowerUpCount;
        powerUpImages[(int)ImageColor.black].gameObject.SetActive(true);

        StartCoroutine(FadeColor(ImageColor.black, black));
        activePowerUpCount++;
    }

    private void DeactivateDoubleTap()
    {
        doubleTapText.gameObject.SetActive(false);
        powerUpImagesCountersDict[ImageColor.black] = POWER_UP_DURATION;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex != 1) { return; }

        levelState = FindObjectOfType<LevelManager>().StateOfTheLevel;

        isGameActive = true;
    }


    private void LoadGameScreen()
    {
        activePowerUpCount = 0;

        tapArea.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
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
        isGameActive = false;

        foreach (Image image in powerUpImages)
        {
            image.gameObject.SetActive(false);
        }

        for (int i = 0; i < POWER_UP_IMAGE_COUNT; i++)
        {
            powerUpImagesCountersDict[(ImageColor)i] = 0;
        }
        StopAllCoroutines();

        pauseButton.gameObject.SetActive(false);
        tapToPlayText.gameObject.SetActive(true);
        tapArea.gameObject.SetActive(true);
        doubleTapText.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(true);
    }

    private void OnEnable()
    {
        GameManager.OnRestartGame += LoadGameScreen;
        SceneManager.sceneLoaded += OnSceneLoaded; 
        Player.OnPlayerDeath += LoadGameOverScreen;

        Player.OnCoinPickUp += UpdateCoinText;

        Player.OnBlueDoubleScorePickUp += ActivateBlueDoubleScore;
        Player.OnGreenHighJumpPickUp += ActivateGreenHighJump;
        Player.OnRedMagnetPickUp += ActivateRedMagnet;
        LevelManager.OnActivateDoubleTap += ActivateDoubleTap;
        Player.OnDeactivateDoubleTap += DeactivateDoubleTap;  
    }

    private void OnDisable()
    {
        GameManager.OnRestartGame -= LoadGameScreen;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Player.OnPlayerDeath -= LoadGameOverScreen;

        Player.OnCoinPickUp -= UpdateCoinText;

        Player.OnBlueDoubleScorePickUp -= ActivateBlueDoubleScore;
        Player.OnGreenHighJumpPickUp -= ActivateGreenHighJump;
        Player.OnRedMagnetPickUp -= ActivateRedMagnet;
        LevelManager.OnActivateDoubleTap -= ActivateDoubleTap;
        Player.OnDeactivateDoubleTap -= DeactivateDoubleTap;
    }

    private void UpdateCoinText()
    {
        coinText.text = levelState.Coins.ToString();
    }

    private void UpdateScoreText()
    {
        scoreText.text = ((int)levelState.Score).ToString();
    }

    private void Start()
    {
        for (int i = 0; i < POWER_UP_IMAGE_COUNT; i++)
        {
            posDict[i] = powerUpImages[i].transform.position;
        }
    }

    private void Update()
    {
        if (!isGameActive) { return; }

        UpdateScoreText();
        UpdateCoinText();
    }
}
