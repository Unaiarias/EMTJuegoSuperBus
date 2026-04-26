using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Beatmap")]
    [SerializeField] private BeatMapSO beatMap;
    [SerializeField] private AudioSource audioSource;

    [Header("Spawn / Visual")]
    [SerializeField] private float leadTime = 1.8f;
    [SerializeField] private NoteView notePrefab;
    [SerializeField] private RectTransform[] laneHitPoints = new RectTransform[4];

    [Header("UI")]
    [SerializeField] private TMPro.TMP_Text comboText;
    [SerializeField] private TMPro.TMP_Text scoreText;

    [Header("Timing Windows")]
    [SerializeField] private float perfectWindow = 0.10f;
    [SerializeField] private float goodLateWindow = 0.22f;
    [SerializeField] private float goodEarlyWindow = 0.32f;

    [Header("Instant Tap")]
    [SerializeField] private float instantTapLifetime = 1.8f;
    [SerializeField] private float instantTapSpawnLeadTime = 0.15f;

    [Header("Random Positioning")]
    [SerializeField] private bool useRandomPositions = true;
    [SerializeField] private RectTransform randomPlayArea;
    [SerializeField] private float notePadding = 90f;
    [SerializeField] private float minDistanceBetweenNotes = 180f;
    [SerializeField] private int randomPositionMaxAttempts = 25;

    [Header("UI")]
    public GameObject menuHasGanadoMinijuego;
    public GameObject botonStart;
    public GameObject botonPausaMinijuego;
    [SerializeField] private float winMenuDelay = 1.5f;

    [Header("Text Positioning - Combo")]
    [SerializeField] private RectTransform comboTextTargetParent;
    [SerializeField] private Vector2 comboTextTargetPosition;
    private Transform comboTextOriginalParent;
    private Vector2 comboTextOriginalPosition;
    private RectTransform comboTextRect;

    [Header("Text Positioning - Score")]
    [SerializeField] private RectTransform scoreTextTargetParent;
    [SerializeField] private Vector2 scoreTextTargetPosition;
    private Transform scoreTextOriginalParent;
    private Vector2 scoreTextOriginalPosition;
    private RectTransform scoreTextRect;

    [Header("Optimization")]
    [SerializeField] private int poolSize = 64;
    [SerializeField] private RectTransform notesParent;

    // runtime
    private readonly Queue<NoteView> pool = new();
    private readonly List<NoteView> activeNotes = new();
    private int nextNoteIndex;
    private double songStartDsp;
    private bool playing;

    private void Awake()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var n = Instantiate(notePrefab, notesParent);
            n.gameObject.SetActive(false);
            pool.Enqueue(n);
        }

        if (beatMap != null)
            beatMap.notes.Sort((a, b) => a.time.CompareTo(b.time));

        // Guardar referencia y posición original del comboText
        if (comboText != null)
        {
            comboTextRect = comboText.GetComponent<RectTransform>();
            comboTextOriginalParent = comboTextRect.parent;
            comboTextOriginalPosition = comboTextRect.anchoredPosition;
        }

        // Guardar referencia y posición original del scoreText
        if (scoreText != null)
        {
            scoreTextRect = scoreText.GetComponent<RectTransform>();
            scoreTextOriginalParent = scoreTextRect.parent;
            scoreTextOriginalPosition = scoreTextRect.anchoredPosition;
        }
    }

    private void Update()
    {
        // Actualizar el texto del combo desde el sistema global
        if (comboText != null && SistemaPuntuacion.Instance != null)
        {
            int comboGlobal = SistemaPuntuacion.Instance.GetComboActual();
            comboText.text = comboGlobal > 0 ? $"Combo: {comboGlobal}" : "Combo: 0";
        }

        // Actualizar el texto del score desde el sistema global
        if (scoreText != null && SistemaPuntuacion.Instance != null)
        {
            int scoreActual = SistemaPuntuacion.Instance.GetScoreActual();
            scoreText.text = $"Score: {scoreActual}";
        }

        if (beatMap == null || !playing) return;

        double now = AudioSettings.dspTime;
        double songTime = now - songStartDsp;

        while (nextNoteIndex < beatMap.notes.Count)
        {
            var note = beatMap.notes[nextNoteIndex];
            double noteTime = note.time + beatMap.offsetSeconds;

            bool shouldSpawn = note.type == NoteType.InstantTap
                ? (noteTime - instantTapSpawnLeadTime) <= songTime
                : (noteTime - leadTime <= songTime);

            if (shouldSpawn)
            {
                Spawn(note);
                nextNoteIndex++;
            }
            else break;
        }

        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active) continue;
            if (n.Type == NoteType.InstantTap) continue;

            float t = 1f - (float)((n.hitDspTime - now) / leadTime);
            t = Mathf.Clamp01(t);
            n.SetApproach(t);
        }

        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];
            if (!n.active)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            if (n.Type == NoteType.InstantTap)
            {
                if (now > n.InstantTapExpireDspTime)
                {
                    RegisterMiss();
                    n.ShowJudgement("MISS");
                    activeNotes.RemoveAt(i);
                    n.DespawnAfter(0.20f);
                }
                continue;
            }

            if (n.Type == NoteType.Drag && n.IsDragExpired())
            {
                RegisterMiss();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
                continue;
            }

            double errorLate = now - n.hitDspTime;
            if (errorLate > goodLateWindow)
            {
                RegisterMiss();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
            }
        }

        if (nextNoteIndex >= beatMap.notes.Count && activeNotes.Count == 0 && playing)
        {
            playing = false;
            OnSongFinished();
        }
    }

    public void StartSong()
    {
        botonStart.SetActive(false);
        botonPausaMinijuego.SetActive(false);

        nextNoteIndex = 0;
        activeNotes.Clear();

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarCombo();
        }

        songStartDsp = AudioSettings.dspTime + 0.1;
        playing = true;

        if (beatMap != null && beatMap.song != null && audioSource != null)
        {
            audioSource.clip = beatMap.song;
            audioSource.PlayScheduled(songStartDsp);
        }
    }

    private void RegisterHit(int puntosBase, TipoPuntuacion tipo)
    {
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.AumentarCombo();
            SistemaPuntuacion.Instance.SumarPuntos(tipo, puntosBase, 0);
        }
    }

    private void RegisterMiss()
    {
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarCombo();
        }
    }

    public void TryHitNote(NoteView note)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.Tap) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime;

        if (signedError < -goodEarlyWindow)
        {
            note.ShowJudgement("EARLY");
            return;
        }

        if (signedError >= -perfectWindow && signedError <= perfectWindow)
        {
            RegisterHit(300, TipoPuntuacion.RitmoPerfect);
            note.ShowJudgement("PERFECT");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        if (signedError >= -goodEarlyWindow && signedError <= goodLateWindow)
        {
            RegisterHit(250, TipoPuntuacion.RitmoGood);
            note.ShowJudgement("GOOD");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        if (signedError > goodLateWindow)
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
        }
    }

    public void TryHitInstantTap(NoteView note)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.InstantTap) return;

        RegisterHit(200, TipoPuntuacion.RitmoInstant);
        note.ShowJudgement("HIT");
        activeNotes.Remove(note);
        note.DespawnAfter(0.15f);
    }

    public void TryStartDrag(NoteView note, int pointerId, Vector2 screenPos)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.Drag) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime;

        if (signedError < -goodEarlyWindow)
        {
            note.ShowJudgement("EARLY");
            return;
        }

        if (signedError > goodLateWindow)
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
            return;
        }

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

        RegisterHit(250, TipoPuntuacion.RitmoDrag);
        note.ShowJudgement("GOOD");
        activeNotes.Remove(note);
        note.DespawnAfter(0.25f);
    }

    public void Hit(int lane)
    {
        if (!playing) return;

        NoteView best = null;
        double bestAbsError = double.MaxValue;
        double now = AudioSettings.dspTime;

        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active || n.lane != lane) continue;

            double absError = System.Math.Abs(now - n.hitDspTime);
            if (absError < bestAbsError)
            {
                bestAbsError = absError;
                best = n;
            }
        }

        if (best == null)
        {
            RegisterMiss();
            return;
        }

        if (best.Type == NoteType.InstantTap)
        {
            TryHitInstantTap(best);
        }
        else if (best.Type == NoteType.Tap)
        {
            TryHitNote(best);
        }
    }

    private void Spawn(BeatMapSO.NoteData note)
    {
        int lane = note.lane;
        if (!useRandomPositions && (lane < 0 || lane >= laneHitPoints.Length)) return;
        if (pool.Count == 0) return;

        var n = pool.Dequeue();

        RectTransform nrt = (RectTransform)n.transform;
        if (notesParent != null)
            nrt.SetParent(notesParent, false);

        Vector2 spawnPos;
        if (useRandomPositions && randomPlayArea != null)
            spawnPos = GetRandomSafePosition(note);
        else
            spawnPos = laneHitPoints[lane].anchoredPosition;

        nrt.anchoredPosition = spawnPos;
        nrt.localRotation = Quaternion.identity;
        nrt.localScale = Vector3.one;

        double noteTime = note.time + beatMap.offsetSeconds;
        double hitDsp = songStartDsp + noteTime;

        n.Init(this, lane, hitDsp, leadTime);

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
        else if (note.type == NoteType.InstantTap)
        {
            double expireDsp = songStartDsp + noteTime + instantTapLifetime;
            n.ConfigureInstantTap(expireDsp);
        }

        activeNotes.Add(n);
    }

    private Vector2 GetRandomSafePosition(BeatMapSO.NoteData note)
    {
        if (randomPlayArea == null || notesParent == null)
            return Vector2.zero;

        Vector3[] corners = new Vector3[4];
        randomPlayArea.GetWorldCorners(corners);

        Vector2 bottomLeft = notesParent.InverseTransformPoint(corners[0]);
        Vector2 topRight = notesParent.InverseTransformPoint(corners[2]);

        float leftPad = notePadding;
        float rightPad = notePadding;
        float topPad = notePadding;
        float bottomPad = notePadding;

        if (note.type == NoteType.Drag)
        {
            float dragDist = (note.dragDistancePx <= 0f) ? 180f : note.dragDistancePx;
            switch (note.dragDirection)
            {
                case DragDirection.Left: leftPad += dragDist; break;
                case DragDirection.Right: rightPad += dragDist; break;
                case DragDirection.Up: topPad += dragDist; break;
                case DragDirection.Down: bottomPad += dragDist; break;
            }
        }

        float minX = bottomLeft.x + leftPad;
        float maxX = topRight.x - rightPad;
        float minY = bottomLeft.y + bottomPad;
        float maxY = topRight.y - topPad;

        if (minX > maxX || minY > maxY)
        {
            return randomPlayArea.anchoredPosition;
        }

        Vector2 candidate = Vector2.zero;
        for (int attempt = 0; attempt < randomPositionMaxAttempts; attempt++)
        {
            candidate = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
            if (IsPositionFarEnough(candidate))
                return candidate;
        }
        return candidate;
    }

    private bool IsPositionFarEnough(Vector2 candidate)
    {
        for (int i = 0; i < activeNotes.Count; i++)
        {
            var note = activeNotes[i];
            if (note == null || !note.active) continue;
            RectTransform rt = note.transform as RectTransform;
            if (rt == null) continue;
            if (Vector2.Distance(rt.anchoredPosition, candidate) < minDistanceBetweenNotes)
                return false;
        }
        return true;
    }

    public void ReturnToPool(NoteView n)
    {
        pool.Enqueue(n);
    }

    private void OnSongFinished()
    {
        Debug.Log("¡Canción finalizada!");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        StartCoroutine(ShowWinMenuWithDelay());
    }

    private IEnumerator ShowWinMenuWithDelay()
    {
        yield return new WaitForSeconds(winMenuDelay);

        // Mover el texto del combo al panel de victoria
        if (comboText != null && comboTextTargetParent != null)
        {
            comboTextRect.SetParent(comboTextTargetParent, false);
            comboTextRect.anchoredPosition = comboTextTargetPosition;
        }

        // Mover el texto del score al panel de victoria con la posición específica
        if (scoreText != null && scoreTextTargetParent != null)
        {
            // Guardar posición original si no se ha guardado
            if (scoreTextOriginalParent == null)
            {
                scoreTextOriginalParent = scoreTextRect.parent;
                scoreTextOriginalPosition = scoreTextRect.anchoredPosition;
            }

            scoreTextRect.SetParent(scoreTextTargetParent, false);
            scoreTextRect.anchoredPosition = scoreTextTargetPosition;
        }

        menuHasGanadoMinijuego.SetActive(true);
    }

    public void ReiniciarMinijuegoLimpieza()
    {
        if (audioSource != null) audioSource.Stop();

        playing = false;

        foreach (var note in activeNotes)
        {
            if (note != null)
            {
                note.gameObject.SetActive(false);
                pool.Enqueue(note);
            }
        }
        activeNotes.Clear();

        nextNoteIndex = 0;

        // Restaurar el texto del combo a su posición original
        if (comboText != null && comboTextOriginalParent != null)
        {
            comboTextRect.SetParent(comboTextOriginalParent, false);
            comboTextRect.anchoredPosition = comboTextOriginalPosition;
        }

        // Restaurar el texto del score a su posición original
        if (scoreText != null && scoreTextOriginalParent != null)
        {
            scoreTextRect.SetParent(scoreTextOriginalParent, false);
            scoreTextRect.anchoredPosition = scoreTextOriginalPosition;
        }

        menuHasGanadoMinijuego.SetActive(false);
        botonStart.SetActive(true);
        botonPausaMinijuego.SetActive(true);

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarCombo();
        }

        audioSource.time = 0f;
    }
}