using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class PauseManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] GameObject pausePanel;

    //[Header("Audio")]
    //[SerializeField] AudioMixer audioMixer;
    //[SerializeField] Slider musicSlider;
    //[SerializeField] Slider sfxSlider;

    [Header("Cursor")]
    [Tooltip("Activá esto si tu juego bloquea el cursor durante el gameplay (típico en juegos 3D).")]
    [SerializeField] bool lockCursorDuringGameplay = true;


    [Header("Posiciones")]
    public Vector2 posicionInicial;
    [Tooltip("Cuántos píxeles baja desde la posición inicial.")]
    [SerializeField] float distanciaBajada = 200f;

    [Header("Tiempos")]
    [SerializeField] float tiempoDeEntrada = 0.5f; // cuánto tarda en bajar
    [SerializeField] float tiempoDeSalida = 0.5f; // cuánto tarda en volver

    RectTransform rectTransform;

    bool isPaused = false;
    public bool IsPaused { get { return isPaused; } }

    bool canPause = false;

    InputSystem_Actions action;

    private void Awake()
    {
        action = new InputSystem_Actions();

        rectTransform = GetComponent<RectTransform>();
        // Si no se asignó posición inicial en el Inspector, usamos la posición actual
        if (posicionInicial == Vector2.zero)
            posicionInicial = rectTransform.anchoredPosition;
    }
    

    private void OnEnable()
    {
        action.Player.Enable();
       
    }

    IEnumerator HabilitarPausa()
    {
        yield return new WaitForSecondsRealtime(0.2f);

        // Esperar además a que la tecla Escape esté físicamente suelta
        // Esto evita que un input "fantasma" del frame anterior active el menú
        while (Keyboard.current != null && Keyboard.current.escapeKey.isPressed)
            yield return null;

        canPause = true;
    }

    private void OnDisable()
    {
        action.Player.Pause.started -= OnPausePressed;
        action.Player.Disable();
        canPause = false;
    }
    private void Start()
    {
        // Resetear estado por si acaso viene de un reinicio
        isPaused = false;
        canPause = false;
        rectTransform.anchoredPosition = posicionInicial; // asegurar posición inicial

        // Suscribir el input acá, con delay
        action.Player.Pause.started += OnPausePressed;
        StartCoroutine(HabilitarPausa());

        // Cargar volumen guardado (default 1 = volumen completo, no silencio)
        float musicVal = PlayerPrefs.GetFloat("Music", 1f);
        float sfxVal = PlayerPrefs.GetFloat("Sfx", 1f);

        //// Aplicar al slider (0 a 1)
        //musicSlider.value = musicVal;
        //sfxSlider.value = sfxVal;

        //// Aplicar al mixer (convertido a dB)
        //audioMixer.SetFloat("Music", LinearTodB(musicVal));
        //audioMixer.SetFloat("Sfx", LinearTodB(sfxVal));

        //// Eventos sliders
        //musicSlider.onValueChanged.AddListener(SetMusicVolume);
        //sfxSlider.onValueChanged.AddListener(SetSFXVolume);

        // Estado inicial del cursor
        if (lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void OnPausePressed(InputAction.CallbackContext context)
    {
        if (!canPause) return; // ignorar si todavía no está listo
        if (isPaused) ResumeGame();
        else PauseGame();
    }
    public void PauseGame()
    {
        isPaused = true;
        //pausePanel.SetActive(true);
        MostrarMenu();
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
        //pausePanel.SetActive(false);
        OcultarMenu();
        Time.timeScale = 1f;

        if (lockCursorDuringGameplay)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public void RestartLevel()
    {
        StopAllCoroutines();
        isPaused = false;
        canPause = false;
        rectTransform.anchoredPosition = posicionInicial; // resetear posición del panel
        Time.timeScale = 1f;
        Scene current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Inicio");
    }

    //public void SetMusicVolume(float value)
    //{
    //    audioMixer.SetFloat("Music", LinearTodB(value));
    //    PlayerPrefs.SetFloat("Music", value); // Guardamos 0-1, no dB
    //}

    //public void SetSFXVolume(float value)
    //{
    //    audioMixer.SetFloat("Sfx", LinearTodB(value));
    //    PlayerPrefs.SetFloat("Sfx", value);
    //}

    // Convierte un valor lineal (0 a 1) a decibeles (-80 a 0)
    // que es el rango que entiende el AudioMixer de Unity
    float LinearTodB(float linear)
    {
        return linear > 0.0001f ? Mathf.Log10(linear) * 20f : -80f;
    }


    // Llamá este método desde un botón, un evento o desde otro script
    public void MostrarMenu()
    {
       
        StartCoroutine(AnimarPista());
    }

    IEnumerator AnimarPista()
    {
        Vector2 posicionVisible = posicionInicial + Vector2.down * distanciaBajada;

        // ── Bajar ─────────────────────────────────────────────
        yield return Mover(posicionInicial, posicionVisible, tiempoDeEntrada);

    }

    IEnumerator Mover(Vector2 desde, Vector2 hasta, float duracion)
    {
        float tiempo = 0f;

        while (tiempo < duracion)
        {
            tiempo += Time.unscaledDeltaTime;
            float t = tiempo / duracion;

            // EaseInOut: el movimiento arranca suave, acelera y frena suave
            t = t * t * (3f - 2f * t);

            rectTransform.anchoredPosition = Vector2.Lerp(desde, hasta, t);
            yield return null;
        }

        rectTransform.anchoredPosition = hasta; // asegurar posición exacta al final
    }

    IEnumerator OcultarAnimacion()
    {
        Vector2 posicionVisible = posicionInicial + Vector2.down * distanciaBajada;

        yield return Mover(posicionVisible, posicionInicial, tiempoDeSalida);
    }


    public void OcultarMenu()
    {
        
        StartCoroutine(OcultarAnimacion());
    }
}

