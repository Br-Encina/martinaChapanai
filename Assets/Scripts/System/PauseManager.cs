using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject pausePanel;

    [Header("Audio")]
    [SerializeField] AudioMixer audioMixer;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;

    [Header("Cursor")]
    [Tooltip("Activá esto si tu juego bloquea el cursor durante el gameplay (típico en juegos 3D).")]
    [SerializeField] bool lockCursorDuringGameplay = true;

    bool isPaused = false;
    public bool IsPaused { get { return isPaused; } }

    InputSystem_Actions action;

    private void Awake()
    {
        action = new InputSystem_Actions();
    }

    private void OnEnable()
    {
        action.Player.Enable();
        action.Player.Pause.started += OnPausePressed;
    }

    private void OnDisable()
    {
        action.Player.Pause.started -= OnPausePressed;
        action.Player.Disable();
    }

    private void Start()
    {
        pausePanel.SetActive(false);

        // Cargar volumen guardado (default 1 = volumen completo, no silencio)
        float musicVal = PlayerPrefs.GetFloat("Music", 1f);
        float sfxVal = PlayerPrefs.GetFloat("Sfx", 1f);

        // Aplicar al slider (0 a 1)
        musicSlider.value = musicVal;
        sfxSlider.value = sfxVal;

        // Aplicar al mixer (convertido a dB)
        audioMixer.SetFloat("Music", LinearTodB(musicVal));
        audioMixer.SetFloat("Sfx", LinearTodB(sfxVal));

        // Eventos sliders
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // Estado inicial del cursor
        if (lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnPausePressed(InputAction.CallbackContext context)
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f;

        if (lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f;

        if (lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("Music", LinearTodB(value));
        PlayerPrefs.SetFloat("Music", value); // Guardamos 0-1, no dB
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("Sfx", LinearTodB(value));
        PlayerPrefs.SetFloat("Sfx", value);
    }

    // Convierte un valor lineal (0 a 1) a decibeles (-80 a 0)
    // que es el rango que entiende el AudioMixer de Unity
    float LinearTodB(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }
}
