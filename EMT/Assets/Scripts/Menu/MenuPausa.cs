using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MenuPausa : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject buttonPause;

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer audioMixer;

    private VolumeController volumeController;
    private bool audioMuted = false;

    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";

    private void Start()
    {
        volumeController = FindFirstObjectByType<VolumeController>();
    }

    public void PausarJuego()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
        buttonPause.SetActive(false);

        // Actualizar los sliders con los valores actuales ANTES de silenciar
        if (volumeController != null)
        {
            // Buscar sliders dentro del menu de pausa (que ahora está activo)
            Slider musicSlider = GameObject.Find("MusicSlider")?.GetComponent<Slider>();
            Slider sfxSlider = GameObject.Find("SFXSlider")?.GetComponent<Slider>();

            if (musicSlider != null) musicSlider.value = volumeController.GetMusicVolume();
            if (sfxSlider != null) sfxSlider.value = volumeController.GetSFXVolume();
        }

        // Silenciar temporalmente
        if (audioMixer != null && !audioMuted)
        {
            audioMixer.SetFloat(MUSIC_PARAM, -80f);
            audioMixer.SetFloat(SFX_PARAM, -80f);
            audioMuted = true;
            Debug.Log("Audio silenciado durante pausa");
        }
    }

    public void ReanudarJuego()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        buttonPause.SetActive(true);

        if (audioMixer != null && audioMuted)
        {
            if (volumeController != null)
            {
                volumeController.OnMusicVolumeChanged(volumeController.GetMusicVolume());
                volumeController.OnSFXVolumeChanged(volumeController.GetSFXVolume());
                Debug.Log("Audio restaurado con valores actuales");
            }
            audioMuted = false;
        }
    }

    public void SalirJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void MenuInicio()
    {
        Time.timeScale = 1;

        if (audioMixer != null && audioMuted && volumeController != null)
        {
            volumeController.OnMusicVolumeChanged(volumeController.GetMusicVolume());
            volumeController.OnSFXVolumeChanged(volumeController.GetSFXVolume());
        }

        SceneManager.LoadScene("MenuInicio");
    }
}