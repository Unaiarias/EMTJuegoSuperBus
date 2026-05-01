using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string sfxGroupName = "SFXVolume";

    [Header("Audio Sources")]
    [SerializeField] private AudioSource sfxAudioSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
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
        PlaySFX(clip, position, 1f);
    }

    public void PlaySFX(AudioClip clip, Vector3 position, float volume)
    {
        if (clip != null && sfxAudioSource != null)
        {
            sfxAudioSource.transform.position = position;
            sfxAudioSource.PlayOneShot(clip, volume);
            Debug.Log($"Reproduciendo SFX: {clip.name} con volumen: {volume}");
        }
        else if (clip == null)
        {
            Debug.LogWarning("Clip de audio es null");
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        PlaySFX(clip, Vector3.zero, 1f);
    }
}