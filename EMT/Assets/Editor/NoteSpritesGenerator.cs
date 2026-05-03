// Assets/Editor/NoteSpritesGenerator.cs
// ─────────────────────────────────────────────────────────────────────────────
// MENÚ: Tools > SuperBus > Generar Sprites de Notas
//
// Genera dos PNG en Assets/Art/:
//   note_circle.png  — círculo sólido blanco con borde suave (anti-alias)
//   note_ring.png    — anillo/ring hueco blanco con bordes suaves
//
// Son blancos para que el código de NoteView les aplique color por encima.
// ─────────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

public static class NoteSpritesGenerator
{
    private const int   SIZE        = 512;
    private const float HALF        = SIZE / 2f;
    private const float AA_FALLOFF  = 1.8f;   // píxeles de suavizado en el borde

    // Proporciones del ring
    private const float RING_OUTER  = 0.94f;  // radio exterior como fracción de HALF
    private const float RING_INNER  = 0.68f;  // radio interior (hueco) como fracción de HALF

    [MenuItem("Tools/SuperBus/Generar Sprites de Notas")]
    public static void GenerateAll()
    {
        GenerateCircle();
        GenerateRing();

        AssetDatabase.Refresh();

        Debug.Log("[NoteSpritesGenerator] ✓ note_circle.png y note_ring.png generados en Assets/Art/");
        EditorUtility.DisplayDialog(
            "¡Sprites generados!",
            "Se han creado:\n• Assets/Art/note_circle.png\n• Assets/Art/note_ring.png\n\n" +
            "Asígnalos al prefab de NoteView en los campos Circle Graphic y Ring Graphic.",
            "OK");
    }

    // ─── CÍRCULO SÓLIDO ───────────────────────────────────────────────────────

    private static void GenerateCircle()
    {
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);

        float outerR = HALF * RING_OUTER;

        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                float dx   = x - HALF + 0.5f;
                float dy   = y - HALF + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Alpha: 1 dentro, se suaviza cerca del borde exterior
                float alpha = 1f - Mathf.Clamp01((dist - (outerR - AA_FALLOFF)) / AA_FALLOFF);

                // Gradiente radial muy sutil: centro ligeramente más brillante
                float brightness = Mathf.Lerp(1f, 0.88f, Mathf.Clamp01(dist / outerR));

                tex.SetPixel(x, y, new Color(brightness, brightness, brightness, alpha));
            }
        }

        tex.Apply();
        SavePNG(tex, "Assets/Art/note_circle.png");
        Object.DestroyImmediate(tex);
    }

    // ─── RING / ANILLO ────────────────────────────────────────────────────────

    private static void GenerateRing()
    {
        var tex = new Texture2D(SIZE, SIZE, TextureFormat.RGBA32, false);

        float outerR = HALF * RING_OUTER;
        float innerR = HALF * RING_INNER;

        for (int y = 0; y < SIZE; y++)
        {
            for (int x = 0; x < SIZE; x++)
            {
                float dx   = x - HALF + 0.5f;
                float dy   = y - HALF + 0.5f;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                // Fade suave en borde exterior
                float alphaOuter = 1f - Mathf.Clamp01((dist - (outerR - AA_FALLOFF)) / AA_FALLOFF);

                // Fade suave en borde interior (de fuera hacia dentro se va haciendo transparente)
                float alphaInner = Mathf.Clamp01((dist - innerR) / AA_FALLOFF);

                float alpha = alphaOuter * alphaInner;

                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        tex.Apply();
        SavePNG(tex, "Assets/Art/note_ring.png");
        Object.DestroyImmediate(tex);
    }

    // ─── HELPER: guardar PNG y configurar import settings ────────────────────

    private static void SavePNG(Texture2D tex, string assetPath)
    {
        string fullPath = Path.Combine(
            Path.GetDirectoryName(Application.dataPath)!,
            assetPath.Replace('/', Path.DirectorySeparatorChar));

        File.WriteAllBytes(fullPath, tex.EncodeToPNG());

        // Reimportar y ajustar settings para UI (Sprite)
        AssetDatabase.ImportAsset(assetPath);
        var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
        if (importer != null)
        {
            importer.textureType         = TextureImporterType.Sprite;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled       = false;
            importer.filterMode          = FilterMode.Bilinear;
            importer.maxTextureSize      = 512;
            importer.SaveAndReimport();
        }

        Debug.Log($"[NoteSpritesGenerator] Guardado: {assetPath}");
    }
}
#endif
