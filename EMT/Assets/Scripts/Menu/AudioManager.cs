using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string sfxGroupName = "SFXVolume"; // Nombre del parámetro SFX en el mixer

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Configurar el AudioSource para que use el grupo SFX del AudioMixer
            ConfigurarAudioSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void ConfigurarAudioSource()
    {
        if (sfxAudioSource == null)
        {
            sfxAudioSource = gameObject.AddComponent<AudioSource>();
        }

        // Buscar el grupo SFX en el AudioMixer
        if (audioMixer != null)
        {
            AudioMixerGroup[] groups = audioMixer.FindMatchingGroups("SFX");
            if (groups.Length > 0)
            {
                sfxAudioSource.outputAudioMixerGroup = groups[0];
                Debug.Log("AudioSource de SFX configurado con el grupo SFX del AudioMixer");
            }
            else
            {
                Debug.LogWarning("No se encontró el grupo 'SFX' en el AudioMixer");
            }
        }
    }

    public void PlaySFX(AudioClip clip, Vector3 position)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.transform.position = position;
            sfxAudioSource.PlayOneShot(clip);
            Debug.Log($"Reproduciendo SFX: {clip.name}");
        }
        else if (clip == null)
        {
            Debug.LogWarning("Clip de audio es null");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, Vector3.zero);
    }
}