using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Beatmap")]
    [SerializeField] private BeatMapSO beatMap;
    [SerializeField] private AudioSource audioSource;

    [Header("Spawn/Visual")]
    [Tooltip("Cuánto antes aparece la nota en pantalla (segundos).")]
    [SerializeField] private float leadTime = 1.2f;

    [Tooltip("Prefab de la nota (Tap/Drag).")]
    [SerializeField] private NoteView notePrefab;

    [Tooltip("Puntos de hit por lane (UI).")]
    [SerializeField] private RectTransform[] laneHitPoints = new RectTransform[4];

    [Header("UI")]
    [SerializeField] private TMPro.TMP_Text comboText;

    [Header("Judgement Windows (seconds)")]
    [SerializeField] private float perfectWindow = 0.10f;
    [SerializeField] private float goodWindow = 0.22f;

    [Header("Optimization")]
    [SerializeField] private int poolSize = 64;
    [SerializeField] private RectTransform notesParent; // Canvas o contenedor dentro del Canvas

    // runtime
    private readonly Queue<NoteView> pool = new();
    private readonly List<NoteView> activeNotes = new();
    private int nextNoteIndex;
    private double songStartDsp;
    private bool playing;

    // score
    public int combo { get; private set; }
    public int score { get; private set; }

    private void Awake()
    {
        // Pool (prewarm)
        for (int i = 0; i < poolSize; i++)
        {
            var n = Instantiate(notePrefab, notesParent);
            n.gameObject.SetActive(false);
            pool.Enqueue(n);
        }

        if (beatMap != null)
            beatMap.notes.Sort((a, b) => a.time.CompareTo(b.time));
    }

    public void StartSong()
    {
        nextNoteIndex = 0;
        activeNotes.Clear();
        combo = 0;
        score = 0;

        if (comboText) comboText.text = $"Combo: {combo}";

        songStartDsp = AudioSettings.dspTime + 0.1;
        playing = true;

        if (beatMap != null && beatMap.song != null && audioSource != null)
        {
            audioSource.clip = beatMap.song;
            audioSource.PlayScheduled(songStartDsp);
        }
    }

    private void Update()
    {
        if (beatMap == null || !playing) return;

        double now = AudioSettings.dspTime;
        double songTime = now - songStartDsp;

        // 1) Spawn
        while (nextNoteIndex < beatMap.notes.Count)
        {
            var note = beatMap.notes[nextNoteIndex];
            double noteTime = note.time + beatMap.offsetSeconds;

            if (noteTime - leadTime <= songTime)
            {
                Spawn(note);
                nextNoteIndex++;
            }
            else break;
        }

        // 2) Visual (approach ring)
        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active) continue;

            float t = 1f - (float)((n.hitDspTime - now) / leadTime);
            t = Mathf.Clamp01(t);

            n.SetApproach(t);
        }

        // 3) Miss automático + drag timeout
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];
            if (!n.active)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            // Drag: expira si no completa en el tiempo límite tras armarse
            if (n.Type == NoteType.Drag && n.IsDragExpired())
            {
                RegisterMiss();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
                continue;
            }

            // Tap/Drag: si ya pasó la ventana de Good (tarde) -> MISS
            double errorLate = (now - n.hitDspTime);
            if (errorLate > goodWindow)
            {
                RegisterMiss();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
            }
        }

        // 4) Fin (sin audio también)
        if (nextNoteIndex >= beatMap.notes.Count && activeNotes.Count == 0)
            playing = false;
    }

    private void Spawn(BeatMapSO.NoteData note)
    {
        int lane = note.lane;
        if (lane < 0 || lane >= laneHitPoints.Length) return;
        if (pool.Count == 0) return;

        var n = pool.Dequeue();

        RectTransform nrt = (RectTransform)n.transform;
        if (notesParent != null)
            nrt.SetParent(notesParent, false);

        nrt.anchoredPosition = laneHitPoints[lane].anchoredPosition;
        nrt.localRotation = Quaternion.identity;
        nrt.localScale = Vector3.one;

        double noteTime = note.time + beatMap.offsetSeconds;
        double hitDsp = songStartDsp + noteTime;

        n.Init(this, lane, hitDsp, leadTime);

        // Configurar tipo
        if (note.type == NoteType.Tap)
        {
            n.ConfigureTap();
        }
        else if (note.type == NoteType.Drag)
        {
            float dist = (note.dragDistancePx <= 0f) ? 180f : note.dragDistancePx;
            float limit = (note.dragTimeLimit <= 0f) ? 0.8f : note.dragTimeLimit;
            n.ConfigureDrag(note.dragDirection, dist, limit);
        }

        activeNotes.Add(n);
    }

    private void RegisterHit(int points)
    {
        combo++; // GOOD mantiene combo
        score += points + combo;

        if (comboText) comboText.text = $"Combo: {combo}";
    }

    private void RegisterMiss()
    {
        combo = 0;
        if (comboText) comboText.text = $"Combo: {combo}";
    }

    public void ReturnToPool(NoteView n)
    {
        pool.Enqueue(n);
    }

    // ---------------- TAP ----------------
    public void TryHitNote(NoteView note)
    {
        if (!playing || note == null || !note.active) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime; // <0 temprano, >0 tarde
        double absError = System.Math.Abs(signedError);

        // Muy temprano: feedback sin castigo
        if (signedError < -goodWindow)
        {
            note.ShowJudgement("EARLY");
            return;
        }

        if (absError <= perfectWindow)
        {
            RegisterHit(300);
            note.ShowJudgement("PERFECT");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        if (absError <= goodWindow)
        {
            RegisterHit(250);
            note.ShowJudgement("GOOD");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        // Fuera de ventana: castiga solo si es tarde de verdad
        if (signedError > goodWindow)
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
        }
        else
        {
            note.ShowJudgement("LATE");
        }
    }

    // ---------------- DRAG ----------------
    public void TryStartDrag(NoteView note, int pointerId, Vector2 screenPos)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.Drag) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime;

        // Muy pronto: feedback sin castigo
        if (signedError < -goodWindow)
        {
            note.ShowJudgement("EARLY");
            return;
        }

        // Muy tarde: miss
        if (signedError > goodWindow)
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
            return;
        }

        // Armamos drag dentro de ventana
        note.ShowJudgement("DRAG");
        note.ArmDrag(pointerId, screenPos);
    }

    public void TryCompleteDrag(NoteView note)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.Drag) return;

        if (note.IsDragExpired())
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
            note.CancelDrag();
            return;
        }

        RegisterHit(250); // como GOOD
        note.ShowJudgement("GOOD");
        activeNotes.Remove(note);
        note.DespawnAfter(0.25f);
    }

    // (Opcional) modo fácil por lanes: puedes borrarlo si ya no lo usas
    public void Hit(int lane) { /* mantener si queréis */ }
}