using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;

public class GameManager : MonoBehaviour
{
    // Creates an instance of GameManager and calls PlayerControls
    public static GameManager Instance { get; private set; }
    public static PlayerControls Input { get; private set; }

    // Calls in PauseGame and creates an instance of IsGameOver
    public static bool PauseGame { get; private set; }
    public static bool IsGameOver => Instance.isGameOver;

    [Header("UI Wave Popup")]
    public GameObject UIWavePopup;
    public TextMeshProUGUI wavePopupText;
    public float wavePopupDuration = 1.5f;
    public float waveFadeDuration = 0.5f;

    [Header("UI Crosshair")]
    public GameObject crosshairUI;

    [Header("UI Game Over")]
    public GameObject UIEndScreen;
    public TextMeshProUGUI endMessageText;
    public float endFadeDuration = 1f;

    [Header("References")]
    public GameObject UIPauseScreen;
    public Button QuitButton;
    public GameObject UIHUD;

    private CanvasGroup waveCG;

    private CanvasGroup endCanvas;
    private bool isGameOver = false;
    private Coroutine pulseRoutine;

    private void Awake()
    {
        // Checks for duplicate instances of GameManager
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        // Enables player input
        Input = new PlayerControls();
        Input.Enable();
    }

    private void Start()
    {
        SetCursorState(false);
        QuitButton.onClick.AddListener(QuitGame);

        // Setup end screen
        if (UIEndScreen != null)
        {
            UIEndScreen.SetActive(true);

            endCanvas = UIEndScreen.GetComponent<CanvasGroup>();
            if (endCanvas == null)
                endCanvas = UIEndScreen.AddComponent<CanvasGroup>();

            endCanvas.alpha = 0f;
            endCanvas.interactable = false;
            endCanvas.blocksRaycasts = false;

            if (endMessageText != null)
            {
                Color c = endMessageText.color;
                endMessageText.color = new Color(c.r, c.g, c.b, 0f);
            }

            UIEndScreen.SetActive(false);
        }

        // Setup wave popup
        if (UIWavePopup != null)
        {
            waveCG = UIWavePopup.GetComponent<CanvasGroup>();
            if (waveCG == null)
                waveCG = UIWavePopup.AddComponent<CanvasGroup>();

            waveCG.alpha = 0f;
            UIWavePopup.SetActive(true); // Must stay active for fading to work
        }
    }

    private void OnDestroy()
    {
        // Disables input if instance is missing
        if (Input != null && Instance == this)
            Input.Disable();
    }

    private void Update()
    {
        // Pauses the game when endscreen activates and allows for game restart when button is pressed
        if (!isGameOver && Input.Menu.PauseGame.triggered)
            SetGamePaused(!PauseGame);

        if (isGameOver && Mouse.current.leftButton.wasPressedThisFrame)
            Restart();
    }

    public static void TriggerGameOver()
    {
        Instance.StartCoroutine(Instance.GameOverRoutine());
    }

    // Wave Popup System
    public void ShowWavePopup(int wave)
    {
        Instance.StartCoroutine(Instance.WavePopupRoutine(wave));
    }

    private IEnumerator WavePopupRoutine(int wave)
    {
        if (UIWavePopup == null || waveCG == null) yield break;

        if (wavePopupText != null)
            wavePopupText.text = "Wave " + wave;

        // Hide crosshair
        if (crosshairUI != null)
            crosshairUI.SetActive(false);

        UIWavePopup.SetActive(true);

        // Fade in
        float t = 0f;
        while (t < waveFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            waveCG.alpha = Mathf.Clamp01(t / waveFadeDuration);
            yield return null;
        }

        waveCG.alpha = 1f;

        // Stay visible
        yield return new WaitForSecondsRealtime(wavePopupDuration);

        // Fade out
        t = 0f;
        while (t < waveFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            waveCG.alpha = 1f - Mathf.Clamp01(t / waveFadeDuration);
            yield return null;
        }

        waveCG.alpha = 0f;

        // Show crosshair again
        if (crosshairUI != null)
            crosshairUI.SetActive(true);
    }

    // Game over system
    private IEnumerator GameOverRoutine()
    {
        isGameOver = true;

        // Stop crosshair
        if (crosshairUI != null)
            crosshairUI.SetActive(true);

        UIHUD.SetActive(false);
        UIPauseScreen.SetActive(false);

        SetCursorState(true);

        UIEndScreen.SetActive(true);
        endCanvas.alpha = 0f;

        float t = 0f;
        while (t < endFadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / endFadeDuration);

            endCanvas.alpha = a;

            if (endMessageText != null)
            {
                Color c = endMessageText.color;
                endMessageText.color = new Color(c.r, c.g, c.b, a);
            }

            yield return null;
        }

        Time.timeScale = 0f;

        endCanvas.interactable = true;
        endCanvas.blocksRaycasts = true;

        if (endMessageText != null)
            pulseRoutine = StartCoroutine(PulseText());
    }

    // Game over pulsating subtext
    private IEnumerator PulseText()
    {
        Color baseColor = endMessageText.color;
        float t = 0f;

        while (true)
        {
            t += Time.unscaledDeltaTime;

            float alpha = Mathf.Lerp(0.35f, 1f, (Mathf.Sin(t * 3f) + 1f) / 2f);
            endMessageText.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }
    }

    private void Restart()
    {
        if (pulseRoutine != null)
            StopCoroutine(pulseRoutine);

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public static void SetGamePaused(bool paused)
    {
        PauseGame = paused;

        Instance.UIHUD.SetActive(!paused);
        Instance.UIPauseScreen.SetActive(paused);

        if (paused)
        {
            SetCursorState(true);
        }
        else
        {
            Instance.UIPauseScreen.SetActive(false);

            if (EventSystem.current != null)
                EventSystem.current.SetSelectedGameObject(null);

            SetCursorState(false);
        }

        Time.timeScale = paused ? 0f : 1f;
    }

    public void QuitGame() => Application.Quit();

    public static void SetCursorState(bool enabled)
    {
        Cursor.lockState = enabled ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = enabled;

        if (!enabled && Mouse.current != null)
            Mouse.current.WarpCursorPosition(new Vector2(Screen.width / 2f, Screen.height / 2f));
    }
}