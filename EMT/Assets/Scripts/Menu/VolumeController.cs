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

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const string SFX_VOLUME_KEY = "SFXVolume";
    private const string MUSIC_PARAM = "Music";
    private const string SFX_PARAM = "SFX";

    private float currentMusicVolume;
    private float currentSFXVolume;
    private string currentSceneName = "";
    private bool valoresInicialesCargados = false;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (!valoresInicialesCargados)
        {
            CargarValoresGuardados();
            valoresInicialesCargados = true;
        }

        AplicarVolumenesAlMixer();
        Debug.Log($"?? VolumeController inicializado - Music: {currentMusicVolume}, SFX: {currentSFXVolume}");
    }

    private void Start()
    {
        AplicarVolumenesAlMixer();
    }

    private void OnEnable()
    {
        AplicarVolumenesAlMixer();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void CargarValoresGuardados()
    {
        if (guardarPreferencias)
        {
            currentMusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, volumenPorDefectoMusic);
            currentSFXVolume = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, volumenPorDefectoSFX);
            Debug.Log($"?? Valores cargados de PlayerPrefs - Music: {currentMusicVolume}, SFX: {currentSFXVolume}");
        }
        else
        {
            currentMusicVolume = volumenPorDefectoMusic;
            currentSFXVolume = volumenPorDefectoSFX;
            Debug.Log($"?? Usando valores por defecto - Music: {currentMusicVolume}, SFX: {currentSFXVolume}");
        }
    }

    public void RecargarValoresDesdePlayerPrefs()
    {
        if (guardarPreferencias)
        {
            float musicVol = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, volumenPorDefectoMusic);
            float sfxVol = PlayerPrefs.GetFloat(SFX_VOLUME_KEY, volumenPorDefectoSFX);

            if (Mathf.Abs(currentMusicVolume - musicVol) > 0.0001f)
            {
                currentMusicVolume = musicVol;
                currentSFXVolume = sfxVol;
                AplicarVolumenesAlMixer();
                Debug.Log($"?? Volumenes recargados desde PlayerPrefs - Music: {currentMusicVolume}, SFX: {currentSFXVolume}");
            }
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;
        Debug.Log($"?? Escena cargada: {currentSceneName}");

        // Si es el menú de inicio, recargar valores desde PlayerPrefs
        if (scene.name == "MenuInicio")
        {
            RecargarValoresDesdePlayerPrefs();
        }

        AplicarVolumenesAlMixer();
        BuscarYConfigurarSliders();

        if (scene.name.Contains("Nivel") || scene.name == "Nivel1" || scene.name.Contains("Level"))
        {
            SincronizarSlidersEnEscena();
        }
    }

    private void BuscarYConfigurarSliders()
    {
        Slider[] sliders = Resources.FindObjectsOfTypeAll<Slider>();

        foreach (Slider slider in sliders)
        {
            if (slider.gameObject.scene.name != currentSceneName) continue;
            if (slider.gameObject.scene.name == null) continue;

            if (slider.gameObject.name == "MusicSlider")
            {
                if (musicSlider != null)
                {
                    musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
                }
                musicSlider = slider;
                ConfigurarSliderMusic();
                Debug.Log($"? MusicSlider encontrado y configurado");
            }
            else if (slider.gameObject.name == "SFXSlider")
            {
                if (sfxSlider != null)
                {
                    sfxSlider.onValueChanged.RemoveListener(OnSFXVolumeChanged);
                }
                sfxSlider = slider;
                ConfigurarSliderSFX();
                Debug.Log($"? SFXSlider encontrado y configurado");
            }
        }
    }

    public void SincronizarSlidersEnEscena()
    {
        Slider[] sliders = Resources.FindObjectsOfTypeAll<Slider>();
        string sceneName = SceneManager.GetActiveScene().name;

        Debug.Log($"?? Sincronizando sliders en escena: {sceneName}");

        foreach (Slider slider in sliders)
        {
            if (slider.gameObject.scene.name != sceneName) continue;

            if (slider.gameObject.name == "MusicSlider")
            {
                float valorMostrar = currentMusicVolume <= 0.0002f ? 0f : currentMusicVolume;
                slider.SetValueWithoutNotify(valorMostrar);
                Debug.Log($"?? Slider Music sincronizado a: {valorMostrar} (real: {currentMusicVolume})");
            }
            else if (slider.gameObject.name == "SFXSlider")
            {
                float valorMostrar = currentSFXVolume <= 0.0002f ? 0f : currentSFXVolume;
                slider.SetValueWithoutNotify(valorMostrar);
                Debug.Log($"?? Slider SFX sincronizado a: {valorMostrar} (real: {currentSFXVolume})");
            }
        }
    }

    private void ConfigurarSliderMusic()
    {
        if (musicSlider != null)
        {
            musicSlider.onValueChanged.RemoveAllListeners();
            musicSlider.minValue = 0.0001f;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);

            float valorMostrar = currentMusicVolume <= 0.0002f ? 0f : currentMusicVolume;
            musicSlider.SetValueWithoutNotify(valorMostrar);
        }
    }

    private void ConfigurarSliderSFX()
    {
        if (sfxSlider != null)
        {
            sfxSlider.onValueChanged.RemoveAllListeners();
            sfxSlider.minValue = 0.0001f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);

            float valorMostrar = currentSFXVolume <= 0.0002f ? 0f : currentSFXVolume;
            sfxSlider.SetValueWithoutNotify(valorMostrar);
        }
    }

    private void AplicarVolumenesAlMixer()
    {
        if (audioMixer != null)
        {
            float musicDB = LinearToDecibels(currentMusicVolume);
            float sfxDB = LinearToDecibels(currentSFXVolume);

            audioMixer.SetFloat(MUSIC_PARAM, musicDB);
            audioMixer.SetFloat(SFX_PARAM, sfxDB);

            Debug.Log($"?? Volúmenes aplicados - Music: {currentMusicVolume} ({musicDB} dB), SFX: {currentSFXVolume} ({sfxDB} dB)");
        }
    }

    private float LinearToDecibels(float linearValue)
    {
        linearValue = Mathf.Max(linearValue, 0.0001f);
        return Mathf.Log10(linearValue) * 20f;
    }

    public void OnMusicVolumeChanged(float value)
    {
        float realValue = value <= 0.0002f ? 0.0001f : value;
        currentMusicVolume = realValue;

        if (audioMixer != null)
        {
            float dB = LinearToDecibels(realValue);
            audioMixer.SetFloat(MUSIC_PARAM, dB);
            Debug.Log($"?? Music Volume cambiado: {realValue} -> {dB} dB");
        }

        if (guardarPreferencias)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, realValue);
            PlayerPrefs.Save();
        }
    }

    public void OnSFXVolumeChanged(float value)
    {
        float realValue = value <= 0.0002f ? 0.0001f : value;
        currentSFXVolume = realValue;

        if (audioMixer != null)
        {
            float dB = LinearToDecibels(realValue);
            audioMixer.SetFloat(SFX_PARAM, dB);
            Debug.Log($"?? SFX Volume cambiado: {realValue} -> {dB} dB");
        }

        if (guardarPreferencias)
        {
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, realValue);
            PlayerPrefs.Save();
        }
    }

    public void RestaurarValoresPorDefecto()
    {
        currentMusicVolume = volumenPorDefectoMusic;
        currentSFXVolume = volumenPorDefectoSFX;

        if (musicSlider != null)
        {
            musicSlider.SetValueWithoutNotify(currentMusicVolume);
        }
        if (sfxSlider != null)
        {
            sfxSlider.SetValueWithoutNotify(currentSFXVolume);
        }

        AplicarVolumenesAlMixer();

        if (guardarPreferencias)
        {
            PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, currentMusicVolume);
            PlayerPrefs.SetFloat(SFX_VOLUME_KEY, currentSFXVolume);
            PlayerPrefs.Save();
        }

        Debug.Log($"?? Volúmenes restaurados a valores por defecto");
    }

    public float GetMusicVolume() => currentMusicVolume;
    public float GetSFXVolume() => currentSFXVolume;

    public void ForzarBusquedaSliders()
    {
        BuscarYConfigurarSliders();
    }
}