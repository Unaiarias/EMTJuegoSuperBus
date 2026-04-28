using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class MenuPausa : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject buttonPause;

    [Header("UI Elements to Hide During Pause")]
    public GameObject[] uiElementsToHide; // Array de elementos UI que se ocultarán durante la pausa

    [Header("Input Settings")]
    [SerializeField] private InputActionReference pausaAction;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;

    private VolumeController volumeController;
    private RhythmGameManager rhythmGameManager;
    private bool audioMuted = false;
    private bool isGamePaused = false;

    private const string MUSIC_PARAM = "Music";
    private const string SFX_PARAM = "SFX";

    private Slider musicSlider;
    private Slider sfxSlider;

    private void OnEnable()
    {
        if (pausaAction != null)
        {
            pausaAction.action.Enable();
            pausaAction.action.performed += OnPausaPressed;
        }
        else
        {
            Debug.LogError("? MenuPausa: No se asignó la acción de pausa en el Inspector!");
        }
    }

    private void OnDisable()
    {
        if (pausaAction != null)
        {
            pausaAction.action.performed -= OnPausaPressed;
            pausaAction.action.Disable();
        }
    }

    private void OnPausaPressed(InputAction.CallbackContext context)
    {
        // Solo procesar si se presionó el botón (performed se ejecuta en el frame que se presiona)
        if (context.performed)
        {
            if (isGamePaused)
            {
                ReanudarJuego();
            }
            else
            {
                PausarJuego();
            }
        }
    }

    private void Start()
    {
        volumeController = FindFirstObjectByType<VolumeController>();

        if (volumeController == null)
        {
            Debug.LogError("? MenuPausa: No se encontró VolumeController en la escena!");
        }
        else
        {
            Debug.Log("? MenuPausa: VolumeController encontrado");
        }

        if (audioMixer == null)
        {
            Debug.LogError("? MenuPausa: No se asignó el AudioMixer en el Inspector!");
        }
        else
        {
            Debug.Log("? MenuPausa: AudioMixer asignado");
        }

        // Buscar el RhythmGameManager en la escena
        rhythmGameManager = FindFirstObjectByType<RhythmGameManager>();
        if (rhythmGameManager == null)
        {
            Debug.LogWarning("? MenuPausa: No se encontró RhythmGameManager en la escena!");
        }
        else
        {
            Debug.Log("? MenuPausa: RhythmGameManager encontrado");
        }

        // Asegurar que el juego comienza despausado
        isGamePaused = false;

        // Asegurar que los elementos UI estén visibles al inicio
        ShowUIElements(true);
    }

    // Método para mostrar u ocultar los elementos UI
    private void ShowUIElements(bool show)
    {
        if (uiElementsToHide == null) return;

        foreach (GameObject uiElement in uiElementsToHide)
        {
            if (uiElement != null)
            {
                uiElement.SetActive(show);
                Debug.Log($"?? Elemento UI {(show ? "mostrado" : "oculto")}: {uiElement.name}");
            }
        }
    }

    public void PausarJuego()
    {
        Debug.Log("=== ?? PausarJuego llamado ===");

        isGamePaused = true;
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        buttonPause.SetActive(false);

        // Ocultar elementos UI adicionales durante la pausa
        ShowUIElements(false);

        // Pausar el RhythmGameManager si existe
        if (rhythmGameManager != null)
        {
            rhythmGameManager.PausarRhythmGame();
        }

        if (volumeController != null)
        {
            // Sincronizar sliders del nivel
            volumeController.SincronizarSlidersEnEscena();

            musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
            sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();

            if (musicSlider != null)
            {
                float currentMusicValue = volumeController.GetMusicVolume();
                musicSlider.onValueChanged.RemoveAllListeners();
                musicSlider.onValueChanged.AddListener((value) => {
                    volumeController.OnMusicVolumeChanged(value);
                    Debug.Log($"?? Slider Music movido a: {value}");
                });

                if (currentMusicValue <= 0.0002f)
                {
                    musicSlider.SetValueWithoutNotify(0f);
                }
                else
                {
                    musicSlider.SetValueWithoutNotify(currentMusicValue);
                }
                Debug.Log($"?? Slider Music sincronizado a: {currentMusicValue}");
            }

            if (sfxSlider != null)
            {
                float currentSFXValue = volumeController.GetSFXVolume();
                sfxSlider.onValueChanged.RemoveAllListeners();
                sfxSlider.onValueChanged.AddListener((value) => {
                    volumeController.OnSFXVolumeChanged(value);
                    Debug.Log($"?? Slider SFX movido a: {value}");
                });

                if (currentSFXValue <= 0.0002f)
                {
                    sfxSlider.SetValueWithoutNotify(0f);
                }
                else
                {
                    sfxSlider.SetValueWithoutNotify(currentSFXValue);
                }
                Debug.Log($"?? Slider SFX sincronizado a: {currentSFXValue}");
            }
        }

        if (audioMixer != null && !audioMuted)
        {
            audioMixer.SetFloat(MUSIC_PARAM, -80f);
            audioMixer.SetFloat(SFX_PARAM, -80f);
            audioMuted = true;
            Debug.Log("?? Audio silenciado durante pausa");
        }
    }

    public void ReanudarJuego()
    {
        Debug.Log("=== ?? ReanudarJuego llamado ===");

        isGamePaused = false;
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        buttonPause.SetActive(true);

        // Mostrar nuevamente los elementos UI ocultos
        ShowUIElements(true);

        // Reanudar el RhythmGameManager si existe
        if (rhythmGameManager != null)
        {
            rhythmGameManager.ReanudarRhythmGame();
        }

        if (audioMixer != null && audioMuted)
        {
            if (volumeController != null)
            {
                float musicVol = volumeController.GetMusicVolume();
                float sfxVol = volumeController.GetSFXVolume();

                float musicDB = Mathf.Log10(Mathf.Max(musicVol, 0.0001f)) * 20f;
                float sfxDB = Mathf.Log10(Mathf.Max(sfxVol, 0.0001f)) * 20f;

                audioMixer.SetFloat(MUSIC_PARAM, musicDB);
                audioMixer.SetFloat(SFX_PARAM, sfxDB);

                Debug.Log($"?? Audio restaurado - Music: {musicVol} ({musicDB} dB), SFX: {sfxVol} ({sfxDB} dB)");
            }
            audioMuted = false;
        }
    }

    public void SalirJuego()
    {
        Debug.Log("?? Saliendo del juego...");
        Application.Quit();
    }

    public void MenuInicio()
    {
        Debug.Log("?? Volviendo al Menú de Inicio...");

        // Guardar valores actuales de audio ANTES de cambiar de escena
        if (volumeController != null)
        {
            float currentMusic = volumeController.GetMusicVolume();
            float currentSFX = volumeController.GetSFXVolume();

            PlayerPrefs.SetFloat("MusicVolume", currentMusic);
            PlayerPrefs.SetFloat("SFXVolume", currentSFX);
            PlayerPrefs.Save();

            Debug.Log($"? Valores guardados - Music: {currentMusic}, SFX: {currentSFX}");
        }

        //Solo reiniciar el score actual, NO el highscore
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore(); // Solo reinicia score, mantiene highscore
            Debug.Log("?? Score reiniciado al volver al menú principal. Highscore se mantiene.");
        }

        // Restaurar el tiempo
        Time.timeScale = 1;
        isGamePaused = false;

        // Cargar la escena del menú
        SceneManager.LoadScene("MenuInicio");
    }
}