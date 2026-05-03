// Assets/Editor/AudioTrimmer.cs
// ─────────────────────────────────────────────────────────────────────────────
// MENÚ: Tools > SuperBus > Recortar Músicas a 30 s
//
// Lee los 4 MP3 ya importados en Assets/Sounds/MiniJuego/,
// los recorta a 30 segundos con fade-out y los guarda como WAV
// en la misma carpeta. Los BeatMaps ya tienen las canciones asignadas
// por GUID así que no hay que hacer nada más.
// ─────────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class AudioTrimmer
{
    private const float  TARGET_DURATION = 30f;
    private const string FOLDER          = "Assets/Sounds/MiniJuego/";

    private static readonly (string mp3Asset, string wavOut)[] Tracks =
    {
        ("BGM_BeatMap1_Tranquilo.mp3", "BGM_BeatMap1_Tranquilo.wav"),
        ("BGM_BeatMap2_Medio.mp3",     "BGM_BeatMap2_Medio.wav"),
        ("BGM_BeatMap3_Rapido.mp3",    "BGM_BeatMap3_Rapido.wav"),
        ("BGM_BeatMap4_Final.mp3",     "BGM_BeatMap4_Final.wav"),
    };

    [MenuItem("Tools/SuperBus/Recortar Músicas a 30 s")]
    public static void TrimAll()
    {
        bool anyError = false;

        foreach (var (mp3, wav) in Tracks)
        {
            string srcAsset  = FOLDER + mp3;
            string destAsset = FOLDER + wav;

            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(srcAsset);
            if (clip == null)
            {
                Debug.LogError($"[AudioTrimmer] No encontrado en Assets: {srcAsset}");
                anyError = true;
                continue;
            }

            if (!TrimAndSaveWav(clip, destAsset))
            {
                anyError = true;
                continue;
            }

            Debug.Log($"[AudioTrimmer] ✓ {wav} listo");
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!anyError)
            EditorUtility.DisplayDialog(
                "¡Músicas recortadas!",
                "Los 4 WAV de 30 s están en Assets/Sounds/MiniJuego/.\n" +
                "Los BeatMaps ya los tienen asignados automáticamente.",
                "OK");
        else
            EditorUtility.DisplayDialog(
                "Recorte con errores",
                "Revisa la Consola. Asegúrate de que Unity importó los MP3 " +
                "(debes ver los iconos en Assets/Sounds/MiniJuego/).",
                "OK");
    }

    // ─── Trim + WAV writer ────────────────────────────────────────────────────

    private static bool TrimAndSaveWav(AudioClip src, string destAsset)
    {
        int channels   = src.channels;
        int sampleRate = src.frequency;
        int totalSamples = src.samples;

        int wantedSamples = Mathf.Min(
            Mathf.RoundToInt(TARGET_DURATION * sampleRate),
            totalSamples);

        float[] allData = new float[totalSamples * channels];
        src.GetData(allData, 0);

        float[] trimmed = new float[wantedSamples * channels];
        Array.Copy(allData, trimmed, trimmed.Length);

        // Fade-out en el último segundo
        int fadeLen = Mathf.Min(sampleRate * channels, trimmed.Length);
        for (int i = 0; i < fadeLen; i++)
        {
            float t = 1f - (float)i / fadeLen;
            trimmed[trimmed.Length - fadeLen + i] *= t;
        }

        string fullPath = Path.GetFullPath(destAsset);
        try
        {
            WriteWav(fullPath, trimmed, channels, sampleRate);
            AssetDatabase.ImportAsset(destAsset);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AudioTrimmer] Error al escribir {destAsset}: {ex.Message}");
            return false;
        }
    }

    private static void WriteWav(string path, float[] data, int channels, int sampleRate)
    {
        int bitsPerSample  = 16;
        int byteRate       = sampleRate * channels * bitsPerSample / 8;
        int blockAlign     = channels * bitsPerSample / 8;
        int dataByteLength = data.Length * 2;

        using var fs = new FileStream(path, FileMode.Create);
        using var bw = new BinaryWriter(fs);

        bw.Write(new char[] { 'R', 'I', 'F', 'F' });
        bw.Write(36 + dataByteLength);
        bw.Write(new char[] { 'W', 'A', 'V', 'E' });
        bw.Write(new char[] { 'f', 'm', 't', ' ' });
        bw.Write(16);
        bw.Write((short)1);
        bw.Write((short)channels);
        bw.Write(sampleRate);
        bw.Write(byteRate);
        bw.Write((short)blockAlign);
        bw.Write((short)bitsPerSample);
        bw.Write(new char[] { 'd', 'a', 't', 'a' });
        bw.Write(dataByteLength);

        foreach (float s in data)
        {
            short v = (short)Mathf.Clamp(Mathf.RoundToInt(s * 32767f),
                                          short.MinValue, short.MaxValue);
            bw.Write(v);
        }
    }
}
#endif
