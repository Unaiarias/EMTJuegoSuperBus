using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RhythmGameManager : MonoBehaviour
{
    [Header("Beatmap")]
    [SerializeField] private BeatMapSO beatMap;
    [SerializeField] private AudioSource audioSource;

    [Header("Spawn / Visual")]
    [Tooltip("Cuánto antes aparece una nota normal (Tap/Drag) antes de tener que pulsarla.")]
    [SerializeField] private float leadTime = 1.8f;

    [Tooltip("Prefab de la nota.")]
    [SerializeField] private NoteView notePrefab;

    [Tooltip("Fallback si desactivas random.")]
    [SerializeField] private RectTransform[] laneHitPoints = new RectTransform[4];

    [Header("UI")]
    [SerializeField] private TMPro.TMP_Text comboText;

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
    [Tooltip("Segundos de espera antes de mostrar el menú de victoria")]
    [SerializeField] private float winMenuDelay = 1.5f;

    [Header("Combo Text Positioning")]
    [Tooltip("Panel donde quieres mover el texto al finalizar")]
    [SerializeField] private RectTransform comboTextTargetParent;
    [Tooltip("Posición relativa al nuevo padre")]
    [SerializeField] private Vector2 comboTextTargetPosition;
    private Transform comboTextOriginalParent; // Padre original
    private Vector2 comboTextOriginalPosition; // Posición original
    private RectTransform comboTextRect; // Referencia al RectTransform del texto


    [Header("Optimization")]
    [SerializeField] private int poolSize = 64;
    [SerializeField] private RectTransform notesParent;

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
    }

    public void StartSong()
    {
        botonStart.SetActive(false);
        botonPausaMinijuego.SetActive(false);

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

            bool shouldSpawn =
                note.type == NoteType.InstantTap
                ? (noteTime - instantTapSpawnLeadTime) <= songTime
                : (noteTime - leadTime <= songTime);

            if (shouldSpawn)
            {
                Spawn(note);
                nextNoteIndex++;
            }
            else break;
        }

        // 2) Actualizar visuals solo para Tap/Drag
        for (int i = 0; i < activeNotes.Count; i++)
        {
            var n = activeNotes[i];
            if (!n.active) continue;
            if (n.Type == NoteType.InstantTap) continue;

            float t = 1f - (float)((n.hitDspTime - now) / leadTime);
            t = Mathf.Clamp01(t);

            n.SetApproach(t);
        }

        // 3) Miss automático / expiración
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var n = activeNotes[i];
            if (!n.active)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            // InstantTap: desaparece tras su tiempo de vida
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

            // Drag: si se armó y expira, miss
            if (n.Type == NoteType.Drag && n.IsDragExpired())
            {
                RegisterMiss();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
                continue;
            }

            // Tap/Drag: si ya se pasó por tarde, miss
            double errorLate = now - n.hitDspTime;
            if (errorLate > goodLateWindow)
            {
                RegisterMiss();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.25f);
            }
        }

        // 4) Fin (sin audio también)
        if (nextNoteIndex >= beatMap.notes.Count && activeNotes.Count == 0 && playing)
        {
            playing = false;
            OnSongFinished(); // Llamar a tu método personalizado
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

        // Si es Drag, deja espacio extra en la dirección del target
        if (note.type == NoteType.Drag)
        {
            float dragDist = (note.dragDistancePx <= 0f) ? 180f : note.dragDistancePx;

            switch (note.dragDirection)
            {
                case DragDirection.Left:
                    leftPad += dragDist;
                    break;
                case DragDirection.Right:
                    rightPad += dragDist;
                    break;
                case DragDirection.Up:
                    topPad += dragDist;
                    break;
                case DragDirection.Down:
                    bottomPad += dragDist;
                    break;
                case DragDirection.Any:
                    rightPad += dragDist;
                    break;
            }
        }

        float minX = bottomLeft.x + leftPad;
        float maxX = topRight.x - rightPad;
        float minY = bottomLeft.y + bottomPad;
        float maxY = topRight.y - topPad;

        if (minX > maxX || minY > maxY)
        {
            Debug.LogWarning("RandomPlayArea es demasiado pequeña para los márgenes actuales.");
            return randomPlayArea.anchoredPosition;
        }

        Vector2 candidate = Vector2.zero;

        for (int attempt = 0; attempt < randomPositionMaxAttempts; attempt++)
        {
            candidate = new Vector2(
                Random.Range(minX, maxX),
                Random.Range(minY, maxY)
            );

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

    private void RegisterHit(int points)
    {
        combo++;
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

    // ---------------- TAP NORMAL ----------------
    public void TryHitNote(NoteView note)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.Tap) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime;

        // Muy temprano: feedback sin castigo
        if (signedError < -goodEarlyWindow)
        {
            note.ShowJudgement("EARLY");
            return;
        }

        // PERFECT
        if (signedError >= -perfectWindow && signedError <= perfectWindow)
        {
            RegisterHit(300);
            note.ShowJudgement("PERFECT");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        // GOOD con más margen por early
        if (signedError >= -goodEarlyWindow && signedError <= goodLateWindow)
        {
            RegisterHit(250);
            note.ShowJudgement("GOOD");
            activeNotes.Remove(note);
            note.DespawnAfter(0.25f);
            return;
        }

        // Muy tarde -> MISS
        if (signedError > goodLateWindow)
        {
            RegisterMiss();
            note.ShowJudgement("MISS");
        }
    }

    // ---------------- INSTANT TAP ----------------
    public void TryHitInstantTap(NoteView note)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.InstantTap) return;

        RegisterHit(200);
        note.ShowJudgement("HIT");
        activeNotes.Remove(note);
        note.DespawnAfter(0.15f);
    }

    // ---------------- DRAG ----------------
    public void TryStartDrag(NoteView note, int pointerId, Vector2 screenPos)
    {
        if (!playing || note == null || !note.active) return;
        if (note.Type != NoteType.Drag) return;

        double now = AudioSettings.dspTime;
        double signedError = now - note.hitDspTime;

        // Muy temprano: feedback sin castigo
        if (signedError < -goodEarlyWindow)
        {
            note.ShowJudgement("EARLY");
            return;
        }

        // Muy tarde -> miss
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

        RegisterHit(250);
        note.ShowJudgement("GOOD");
        activeNotes.Remove(note);
        note.DespawnAfter(0.25f);
    }

    // (Opcional) modo viejo por lanes, lo puedes dejar o borrar
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

    //Acaba el minijuego
    private void OnSongFinished()
    {
        Debug.Log("¡Canción finalizada!");
        StartCoroutine(ShowWinMenuWithDelay());
    }

    private IEnumerator ShowWinMenuWithDelay()
    {
        // Esperar el tiempo configurado antes de mostrar el menú
        yield return new WaitForSeconds(winMenuDelay);

        // Mover el comboText a otro padre
        if (comboText != null && comboTextTargetParent != null)
        {
            comboTextRect.SetParent(comboTextTargetParent, false);
            comboTextRect.anchoredPosition = comboTextTargetPosition;
        }

        // Mostrar el menú de victoria después del desfase
        menuHasGanadoMinijuego.SetActive(true);
    }

    //Boton Reinicio Nivel
    public void ReiniciarMinijuegoLimpieza()
    {
        // Detener la música
        if (audioSource != null)
        {
            audioSource.Stop();
        }

        // Reiniciar el estado del juego
        playing = false;

        // Limpiar notas activas
        foreach (var note in activeNotes)
        {
            if (note != null)
            {
                note.gameObject.SetActive(false);
                pool.Enqueue(note);
            }
        }
        activeNotes.Clear();

        // Resetear índices
        nextNoteIndex = 0;

        // Restaurar padre y posición original del comboText
        if (comboText != null && comboTextOriginalParent != null)
        {
            comboTextRect.SetParent(comboTextOriginalParent, false);
            comboTextRect.anchoredPosition = comboTextOriginalPosition;
        }

        // Resetear UI
        menuHasGanadoMinijuego.SetActive(false);
        botonStart.SetActive(true);
        botonPausaMinijuego.SetActive(true);

        //Resetear combo y score 
        combo = 0;
        score = 0;
        if (comboTextRect) comboText.text = $"Combo: {combo}";

        //Reinicio Musica
        audioSource.time = 0f; // Reinicia el tiempo del audio a 0
    }

}