using UnityEngine;
using UnityEngine.Audio;

public class AudioDebug : MonoBehaviour
{
    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer audioMixer;

    [ContextMenu("Test Music Volume")]
    public void TestMusicVolume()
    {
        if (audioMixer != null)
        {
            // Probar diferentes valores
            audioMixer.SetFloat("Music", -80f);
            Debug.Log("Music set to -80 dB (silencio)");

            // Después de 2 segundos, restaurar
            Invoke("RestoreMusic", 2f);
        }
    }

    private void RestoreMusic()
    {
        audioMixer.SetFloat("Music", 0f);
        Debug.Log("Music set to 0 dB");
    }

    [ContextMenu("Check Current Values")]
    public void CheckCurrentValues()
    {
        if (audioMixer != null)
        {
            float musicValue;
            float sfxValue;

            if (audioMixer.GetFloat("Music", out musicValue))
            {
                Debug.Log($"Current Music volume: {musicValue} dB");
            }
            else
            {
                Debug.LogError("No se pudo obtener el parámetro 'Music' - ¡Revisa el nombre exacto!");
            }

            if (audioMixer.GetFloat("SFX", out sfxValue))
            {
                Debug.Log($"Current SFX volume: {sfxValue} dB");
            }
            else
            {
                Debug.LogError("No se pudo obtener el parámetro 'SFX' - ¡Revisa el nombre exacto!");
            }
        }
    }
}
