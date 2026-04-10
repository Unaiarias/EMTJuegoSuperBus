using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VolumeController : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [Header("Configuración")]
    [SerializeField] private bool guardarPreferencias = true;
    [SerializeField] private float volumenPorDefectoMusic = 0.7f;
    [SerializeField] private float volumenPorDefectoSFX = 0.9f;

    // Claves para PlayerPrefs
    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_PARAM = "MusicVolume";
    private const string SFX_PARAM = "SFXVolume";

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // No configurar sliders aquí, esperar a que se cargue la escena
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"Escena cargada: {scene.name}, buscando sliders...");

        // Buscar los sliders en la nueva escena (incluyendo objetos desactivados)
        BuscarSlidersEnEscena();

        // Si encontramos sliders, configurarlos
        if (musicSlider != null || sfxSlider != null)
        {
            ConfigurarSliders();
            CargarVolumenes();
            AplicarVolumenes();
        }
        else
        {
            Debug.Log($"No se encontraron sliders en la escena {scene.name}");
        }
    }

    private void BuscarSlidersEnEscena()
    {
        // Buscar sliders por nombre en toda la escena (incluyendo objetos desactivados)
        Slider[] sliders = FindObjectsByType<Slider>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (Slider slider in sliders)
        {
            Debug.Log($"Slider encontrado: {slider.gameObject.name}, activo: {slider.gameObject.activeInHierarchy}");

            if (slider.gameObject.name == "MusicSlider")
            {
                musicSlider = slider;
                Debug.Log("MusicSlider encontrado y asignado");
            }
            else if (slider.gameObject.name == "SFXSlider")
            {
                sfxSlider = slider;
                Debug.Log("SFXSlider encontrado y asignado");
            }
        }

        // Si no se encontraron por nombre, buscar por tag
        if (musicSlider == null)
        {
            GameObject musicSliderObj = GameObject.FindGameObjectWithTag("MusicSlider");
            if (musicSliderObj != null) musicSlider = musicSliderObj.GetComponent<Slider>();
        }

        if (sfxSlider == null)
        {
            GameObject sfxSliderObj = GameObject.FindGameObjectWithTag("SFXSlider");
            if (sfxSliderObj != null) sfxSlider = sfxSliderObj.GetComponent<Slider>();
        }
    }

    private void ConfigurarSliders()
    {
        if (musicSlider != null)
        {
            musicSlider.minValue = 0.0001f;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            Debug.Log("MusicSlider configurado correctamente");
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            Debug.Log("SFXSlider configurado correctamente");
        }
    }

    private void CargarVolumenes()
    {
        if (guardarPreferencias)
        {
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, volumenPorDefectoMusic);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, volumenPorDefectoSFX);

            if (musicSlider != null) musicSlider.value = musicVol;
            if (sfxSlider != null) sfxSlider.value = sfxVol;

            Debug.Log($"Volúmenes cargados - Music: {musicVol}, SFX: {sfxVol}");
        }
    }

    private void AplicarVolumenes()
    {
        if (musicSlider != null) OnMusicVolumeChanged(musicSlider.value);
        if (sfxSlider != null) OnSFXVolumeChanged(sfxSlider.value);
    }

    private float LinearToDecibels(float linearValue)
    {
        linearValue = Mathf.Max(linearValue, 0.0001f);
        return Mathf.Log10(linearValue) * 20f;
    }

    public void OnMusicVolumeChanged(float value)
    {
        if (audioMixer != null)
        {
            float dB = LinearToDecibels(value);
            audioMixer.SetFloat(MUSIC_PARAM, dB);
            Debug.Log($"Music Volume: {value} -> {dB} dB");

            if (guardarPreferencias)
            {
                PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, value);
                PlayerPrefs.Save();
            }
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        if (audioMixer != null)
        {
            float dB = LinearToDecibels(value);
            audioMixer.SetFloat(SFX_PARAM, dB);
            Debug.Log($"SFX Volume: {value} -> {dB} dB");

            if (guardarPreferencias)
            {
                PlayerPrefs.SetFloat(SFX_VOLUME_KEY, value);
                PlayerPrefs.Save();
            }
        }
    }

    public void RestaurarValoresPorDefecto()
    {
        if (musicSlider != null) musicSlider.value = volumenPorDefectoMusic;
        if (sfxSlider != null) sfxSlider.value = volumenPorDefectoSFX;
    }

    public float GetMusicVolume() => musicSlider != null ? musicSlider.value : PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, volumenPorDefectoMusic);
    public float GetSFXVolume() => sfxSlider != null ? sfxSlider.value : PlayerPrefs.GetFloat(SFX_VOLUME_KEY, volumenPorDefectoSFX);
}