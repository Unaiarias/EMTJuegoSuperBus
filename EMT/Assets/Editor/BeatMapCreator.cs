// Assets/Editor/BeatMapCreator.cs
// ─────────────────────────────────────────────────────────────────────────────
// MENÚ: Tools > SuperBus > Crear 4 BeatMaps
//
// Crea los cuatro activos BeatMapSO en Assets/MiniJuego/:
//   BeatMap1_Tranquilo.asset   (~80 BPM, 38 s, notas simples)
//   BeatMap2_Medio.asset       (~100 BPM, 44 s, mix taps + drags)
//   BeatMap3_Rapido.asset      (~120 BPM, 42 s, más InstantTaps)
//   BeatMap4_Final.asset       (~140 BPM, 48 s, todo mezclado)
//
// ¡IMPORTANTE! Asigna el AudioClip de cada beatmap en el Inspector
// una vez que tengas las 4 músicas descargadas.
// ─────────────────────────────────────────────────────────────────────────────

#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class BeatMapCreator
{
    private const string SAVE_PATH = "Assets/MiniJuego/";

    [MenuItem("Tools/SuperBus/Crear 4 BeatMaps")]
    public static void CreateAllBeatMaps()
    {
        // BeatMap1 NO se toca — ya tiene sus notas originales y funciona bien
        CreateAndSave(BuildBeatMap2(), "BeatMap2_Medio");
        CreateAndSave(BuildBeatMap3(), "BeatMap3_Rapido");
        CreateAndSave(BuildBeatMap4(), "BeatMap4_Final");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[BeatMapCreator] ✓ 4 BeatMaps creados en Assets/MiniJuego/. " +
                  "Asigna el AudioClip de cada uno en el Inspector.");
        EditorUtility.DisplayDialog(
            "¡BeatMaps creados!",
            "Se han generado 4 BeatMaps en Assets/MiniJuego/.\n\n" +
            "Recuerda asignar el AudioClip (canción) de cada uno en el Inspector " +
            "cuando tengas las músicas.",
            "OK");
    }

    // ─── BEATMAP 1 – TRANQUILO (~80 BPM, predominan Taps) ────────────────────
    private static BeatMapSO BuildBeatMap1()
    {
        var bm = ScriptableObject.CreateInstance<BeatMapSO>();
        bm.offsetSeconds = 0f;
        // song → null: asignar en Inspector

        // 80 BPM → beat cada 0.75 s
        // Patrón: taps sueltos con algún drag puntual, muy asequible
        var n = new List<BeatMapSO.NoteData>
        {
            // ── Intro suave ──────────────────────────────────────────
            T(1.5f,  0), T(2.25f, 2), T(3.0f,  1), T(3.75f, 3),
            T(4.5f,  0), T(5.25f, 1),

            // ── Motivo A (cada beat) ─────────────────────────────────
            T(6.0f,  2), T(6.75f, 0), T(7.5f,  3), T(8.25f, 1),
            T(9.0f,  2), T(9.75f, 0),

            // ── Drag suave ───────────────────────────────────────────
            D(10.5f, 1, DragDirection.Right, 160f, 0.9f),
            T(11.5f, 3), T(12.25f, 0),

            // ── Motivo B (doble tap) ─────────────────────────────────
            T(13.0f, 0), T(13.4f,  2),
            T(14.25f,1), T(14.65f, 3),
            T(15.5f, 0), T(16.25f, 2),

            // ── Drag + tap ───────────────────────────────────────────
            D(17.0f, 0, DragDirection.Up,   150f, 0.9f),
            T(18.0f, 3), T(18.75f, 1),

            // ── Puente tranquilo ─────────────────────────────────────
            T(19.5f, 2), T(20.25f, 0), T(21.0f,  1), T(21.75f, 3),

            // ── Sección final ────────────────────────────────────────
            T(22.5f, 0), T(23.25f, 2),
            D(24.0f, 1, DragDirection.Left,  160f, 0.9f),
            T(25.0f, 3), T(25.75f, 0),
            T(26.5f, 2), T(27.25f, 1),
            T(28.0f, 3), T(28.75f, 0),
            D(29.5f, 2, DragDirection.Down,  160f, 0.9f),
            T(30.5f, 1), T(31.25f, 3),
            T(32.0f, 0), T(32.75f, 2),
            T(33.5f, 1), T(34.25f, 3),
            T(35.0f, 0), T(35.75f, 2),
            T(36.5f, 1),
        };

        bm.notes = n;
        return bm;
    }

    // ─── BEATMAP 2 – MEDIO (~100 BPM, mix taps + drags, ≤ 30 s) ─────────────
    private static BeatMapSO BuildBeatMap2()
    {
        var bm = ScriptableObject.CreateInstance<BeatMapSO>();
        bm.offsetSeconds = 0f;

        // 100 BPM → beat cada 0.6 s · canción de 30 s
        var n = new List<BeatMapSO.NoteData>
        {
            // ── Arranque ─────────────────────────────────────────────
            T(1.0f,  0), T(1.6f,  2), T(2.2f,  1), T(2.8f,  3),
            T(3.4f,  0), T(4.0f,  2),

            // ── Primera oleada de drags ───────────────────────────────
            D(4.6f,  1, DragDirection.Right, 170f, 0.8f),
            T(5.6f,  3), T(6.2f,  0),
            D(6.8f,  2, DragDirection.Left,  170f, 0.8f),
            T(7.8f,  1), T(8.4f,  3),

            // ── Patrón cruzado ───────────────────────────────────────
            T(9.0f,  0), T(9.4f,  3),
            T(10.0f, 1), T(10.4f, 2),
            T(11.0f, 0), T(11.4f, 3),

            // ── Drag hacia arriba ────────────────────────────────────
            D(12.0f, 1, DragDirection.Up,   160f, 0.85f),
            T(13.0f, 2), T(13.6f, 0),

            // ── Trío rápido ──────────────────────────────────────────
            T(14.2f, 3), T(14.6f, 1), T(15.0f, 2),
            T(15.6f, 0), T(16.0f, 3), T(16.4f, 1),

            // ── Sección drag + tap ───────────────────────────────────
            D(17.0f, 2, DragDirection.Down,  170f, 0.85f),
            T(18.0f, 0), T(18.6f, 3),
            D(19.2f, 1, DragDirection.Right, 170f, 0.8f),
            T(20.2f, 2), T(20.8f, 0),

            // ── Escalada de intensidad ───────────────────────────────
            T(21.4f, 3), T(21.8f, 1),
            T(22.4f, 2), T(22.8f, 0),
            T(23.4f, 3), T(23.8f, 2),

            // ── Clímax ───────────────────────────────────────────────
            D(24.4f, 0, DragDirection.Right, 180f, 0.75f),
            T(25.4f, 3), T(26.0f, 1),
            D(26.6f, 2, DragDirection.Left,  180f, 0.75f),
            T(27.6f, 0), T(28.2f, 3),

            // ── Cadencia final ────────────────────────────────────────
            T(28.8f, 1), T(29.3f, 2),
        };

        bm.notes = n;
        return bm;
    }

    // ─── BEATMAP 3 – RÁPIDO (~120 BPM, muchos InstantTaps, ≤ 30 s) ──────────
    private static BeatMapSO BuildBeatMap3()
    {
        var bm = ScriptableObject.CreateInstance<BeatMapSO>();
        bm.offsetSeconds = 0f;

        // 120 BPM → beat cada 0.5 s · canción de 30 s
        var n = new List<BeatMapSO.NoteData>
        {
            // ── Arranque veloz ────────────────────────────────────────
            // ── Intro suave (~0.9 s entre notas) ─────────────────────
            T(1.5f, 0), T(2.4f, 2), T(3.3f, 1), T(4.2f, 3),

            // ── Primer InstantTap (uno solo, claro) ───────────────────
            I(5.2f, 0),
            T(6.2f, 2), T(7.1f, 3),

            // ── Drag con tiempo de sobra ──────────────────────────────
            D(8.1f, 1, DragDirection.Right, 160f, 1.0f),
            T(9.5f, 0), T(10.4f, 3),

            // ── Taps alternados (~0.8 s) ──────────────────────────────
            T(11.3f, 2), T(12.1f, 0),
            T(13.0f, 3), T(13.9f, 1),

            // ── InstantTap + tap seguidos ─────────────────────────────
            I(14.8f, 2),
            T(15.8f, 0), T(16.7f, 3),

            // ── Drag hacia arriba ─────────────────────────────────────
            D(17.6f, 1, DragDirection.Up, 160f, 1.0f),
            T(19.0f, 2), T(19.9f, 0),

            // ── Patrón cruzado moderado ───────────────────────────────
            T(20.8f, 3), I(21.7f, 1),
            T(22.6f, 0), T(23.5f, 2),

            // ── Drag + taps finales ───────────────────────────────────
            D(24.4f, 3, DragDirection.Left, 160f, 1.0f),
            T(25.8f, 1), I(26.7f, 0),
            T(27.6f, 2), T(28.5f, 3),
            T(29.4f, 0),
        };

        bm.notes = n;
        return bm;
    }

    // ─── BEATMAP 4 – FINAL (~140 BPM, máxima dificultad, ≤ 30 s) ───────────
    private static BeatMapSO BuildBeatMap4()
    {
        var bm = ScriptableObject.CreateInstance<BeatMapSO>();
        bm.offsetSeconds = 0f;

        // 140 BPM → beat cada ~0.43 s · canción de 30 s
        var n = new List<BeatMapSO.NoteData>
        {
            // ── Arranque moderado (~0.7 s entre notas) ────────────────
            T(1.2f, 0), T(1.9f, 2), T(2.6f, 1), T(3.3f, 3),

            // ── Primer drag con flecha clara ──────────────────────────
            D(4.2f, 0, DragDirection.Right, 160f, 1.0f),
            T(5.5f, 2), T(6.2f, 3),

            // ── InstantTap solo ───────────────────────────────────────
            I(7.1f, 1),
            T(8.0f, 0), T(8.7f, 2),

            // ── Taps alternados sin agobio ────────────────────────────
            T(9.5f, 3), T(10.2f, 1),
            T(10.9f, 0), T(11.6f, 2),

            // ── Drag + pausa ──────────────────────────────────────────
            D(12.5f, 3, DragDirection.Left, 160f, 1.0f),
            T(13.9f, 1), I(14.7f, 0),
            T(15.5f, 2), T(16.2f, 3),

            // ── Subida de intensidad ──────────────────────────────────
            T(17.0f, 0), T(17.7f, 2),
            I(18.4f, 1), T(19.1f, 3),

            // ── Drag hacia arriba ─────────────────────────────────────
            D(20.0f, 2, DragDirection.Up, 160f, 1.0f),
            T(21.3f, 0), T(22.0f, 3),

            // ── Combo tap-instant-tap ─────────────────────────────────
            T(22.7f, 1), I(23.4f, 2), T(24.1f, 0),

            // ── Drag final + taps de cierre ───────────────────────────
            D(25.0f, 3, DragDirection.Down, 160f, 1.0f),
            T(26.3f, 1), T(27.0f, 2),
            I(27.7f, 0), T(28.5f, 3),
            T(29.2f, 1),
        };

        bm.notes = n;
        return bm;
    }

    // ─── Helpers ─────────────────────────────────────────────────────────────

    private static BeatMapSO.NoteData T(float t, int lane) =>
        BeatMapSO.NoteData.MakeTap(t, lane);

    private static BeatMapSO.NoteData D(float t, int lane, DragDirection dir,
                                         float dist = 170f, float limit = 0.8f) =>
        BeatMapSO.NoteData.MakeDrag(t, lane, dir, dist, limit);

    private static BeatMapSO.NoteData I(float t, int lane) =>
        new BeatMapSO.NoteData
        {
            time           = t,
            lane           = lane,
            type           = NoteType.InstantTap,
            dragDirection  = DragDirection.Right,
            dragDistancePx = 180f,
            dragTimeLimit  = 0.8f
        };

    // ─── Crear y guardar asset ────────────────────────────────────────────────

    private static void CreateAndSave(BeatMapSO bm, string assetName)
    {
        string path = SAVE_PATH + assetName + ".asset";

        // Si ya existe, sobreescribir con los datos nuevos en lugar de duplicar
        var existing = AssetDatabase.LoadAssetAtPath<BeatMapSO>(path);
        if (existing != null)
        {
            existing.notes         = bm.notes;
            existing.offsetSeconds = bm.offsetSeconds;
            // Conservamos el AudioClip que tuviera asignado
            EditorUtility.SetDirty(existing);
            Object.DestroyImmediate(bm);
            Debug.Log($"[BeatMapCreator] Actualizado: {path}");
        }
        else
        {
            AssetDatabase.CreateAsset(bm, path);
            Debug.Log($"[BeatMapCreator] Creado: {path}");
        }
    }
}
#endif
