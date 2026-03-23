using System;
using System.Collections.Generic;
using UnityEngine;

public enum NoteType
{
    Tap,
    Drag
}

public enum DragDirection
{
    Any,
    Left,
    Right,
    Up,
    Down
}

[CreateAssetMenu(menuName = "Rhythm/Beat Map", fileName = "BeatMap")]
public class BeatMapSO : ScriptableObject
{
    public AudioClip song;

    [Tooltip("Offset en segundos para ajustar sincronía (positivo retrasa notas, negativo las adelanta).")]
    public float offsetSeconds = 0f;

    public List<NoteData> notes = new();

    [Serializable]
    public struct NoteData
    {
        [Tooltip("Segundos desde inicio de la canción (sin offset).")]
        public float time;

        [Tooltip("0..3 (si estás usando lanes fijas).")]
        public int lane;

        public NoteType type;

        [Header("Drag (solo si type = Drag)")]
        public DragDirection dragDirection;

        [Tooltip("Distancia mínima del arrastre en píxeles UI (Canvas). Ej: 160-220 en móvil.")]
        public float dragDistancePx;

        [Tooltip("Tiempo máximo (seg) para completar el drag desde que se inicia correctamente. Ej: 0.6-1.0.")]
        public float dragTimeLimit;

        // Valores por defecto razonables (Unity no llama ctor al editar, pero sirve para código)
        public static NoteData MakeTap(float time, int lane)
        {
            return new NoteData
            {
                time = time,
                lane = lane,
                type = NoteType.Tap,
                dragDirection = DragDirection.Right,
                dragDistancePx = 180f,
                dragTimeLimit = 0.8f
            };
        }

        public static NoteData MakeDrag(float time, int lane, DragDirection dir, float distPx = 180f, float timeLimit = 0.8f)
        {
            return new NoteData
            {
                time = time,
                lane = lane,
                type = NoteType.Drag,
                dragDirection = dir,
                dragDistancePx = distPx,
                dragTimeLimit = timeLimit
            };
        }
    }
}