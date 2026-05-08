using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

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
    [SerializeField] private float winMenuDelay = 1.5f;

    [Header("Pause Settings")]
    [SerializeField] private string rhythmContainerName = "RhythmGameContainer";

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

    [Header("Sound Effects")]
    [SerializeField] private AudioSource rhythmAudioSource;
    [SerializeField] private AudioClip sonidoHitPerfect;
    [SerializeField] private AudioClip sonidoHitGood;
    [SerializeField] private AudioClip sonidoHitInstant;
    [SerializeField] private AudioClip sonidoMiss;

    [Header("Volume Controls")]
    [Range(0f, 1f)] public float volumenMusicaFondo = 0.7f;
    [Range(0f, 1f)] public float volumenPerfect = 0.8f;
    [Range(0f, 1f)] public float volumenGood = 0.7f;
    [Range(0f, 1f)] public float volumenInstant = 0.6f;
    [Range(0f, 1f)] public float volumenMiss = 0.5f;

    // runtime
    private readonly Queue<NoteView> pool = new();
    private readonly List<NoteView> activeNotes = new();
    private int nextNoteIndex;
    private double songStartDsp;
    private bool playing;
    private bool isPaused = false;
    private GameObject rhythmGameContainer;

    private double pausaTiempoAcumulado = 0f;
    private double tiempoPausaInicioDsp = 0f;

    private void Awake()
    {
        // 1. BUSCAR EL CONTENEDOR POR NOMBRE
        rhythmGameContainer = GameObject.Find(rhythmContainerName);

        if (rhythmGameContainer == null)
        {
            Debug.LogError($"¡Crea un GameObject vacío en el Canvas llamado '{rhythmContainerName}'!");
            return;
        }

        Debug.Log($"Contenedor '{rhythmContainerName}' encontrado");

        // 2. VERIFICAR Y MOVER NOTESPARENT DENTRO DEL CONTENEDOR
        if (notesParent == null)
        {
            Debug.LogError("notesParent no está asignado en el Inspector!");
            return;
        }

        // Si notesParent no está dentro del contenedor, lo movemos
        if (notesParent.parent != rhythmGameContainer.transform)
        {
            Debug.LogWarning($"Moviendo notesParent dentro del contenedor '{rhythmContainerName}'");
            notesParent.SetParent(rhythmGameContainer.transform, false);

            notesParent.anchorMin = Vector2.zero;
            notesParent.anchorMax = Vector2.one;
            notesParent.offsetMin = Vector2.zero;
            notesParent.offsetMax = Vector2.zero;
        }

        // 3. MOVER RANDOMPLAYAREA DENTRO DEL CONTENEDOR SI EXISTE
        if (randomPlayArea != null && randomPlayArea.parent != rhythmGameContainer.transform)
        {
            Debug.Log($"Moviendo randomPlayArea dentro del contenedor '{rhythmContainerName}'");
            randomPlayArea.SetParent(rhythmGameContainer.transform, false);
        }

        // 4. MOVER TEXTOS DENTRO DEL CONTENEDOR
        if (comboText != null)
        {
            comboTextRect = comboText.GetComponent<RectTransform>();
            comboTextOriginalParent = comboTextRect.parent;
            comboTextOriginalPosition = comboTextRect.anchoredPosition;

            if (comboTextRect.parent != rhythmGameContainer.transform)
            {
                Debug.Log($"Moviendo comboText dentro del contenedor '{rhythmContainerName}'");
                comboTextRect.SetParent(rhythmGameContainer.transform, false);
            }
        }

        if (scoreText != null)
        {
            scoreTextRect = scoreText.GetComponent<RectTransform>();
            scoreTextOriginalParent = scoreTextRect.parent;
            scoreTextOriginalPosition = scoreTextRect.anchoredPosition;

            if (scoreTextRect.parent != rhythmGameContainer.transform)
            {
                Debug.Log($"Moviendo scoreText dentro del contenedor '{rhythmContainerName}'");
                scoreTextRect.SetParent(rhythmGameContainer.transform, false);
            }
        }

        // 5. CREAR EL POOL DE NOTAS
        for (int i = 0; i < poolSize; i++)
        {
            var n = Instantiate(notePrefab, notesParent);
            n.gameObject.SetActive(false);
            pool.Enqueue(n);
        }

        // Configurar AudioSource para sonidos de ritmo si no existe
        if (rhythmAudioSource == null)
        {
            rhythmAudioSource = gameObject.AddComponent<AudioSource>();
            rhythmAudioSource.playOnAwake = false;
            Debug.Log("AudioSource para ritmo creado automáticamente");
        }

        // Configurar el volumen inicial de los AudioSources
        ConfigurarVolumenes();

        if (beatMap != null)
            beatMap.notes.Sort((a, b) => a.time.CompareTo(b.time));

        // 6. OCULTAR LAS NOTAS INICIALMENTE
        if (notesParent != null)
        {
            notesParent.gameObject.SetActive(false);
        }

        // 7. EL CONTENEDOR DEBE ESTAR VISIBLE AL INICIO
        if (rhythmGameContainer != null)
        {
            rhythmGameContainer.SetActive(true);
        }

        Debug.Log("RhythmGameManager inicializado correctamente");
        Debug.Log($"Todo el contenido del ritmo está dentro de: {rhythmGameContainer.name}");
    }

    // Método para configurar los volúmenes
    private void ConfigurarVolumenes()
    {
        if (audioSource != null)
        {
            audioSource.volume = volumenMusicaFondo;
        }
    }

    // Métodos públicos para cambiar el volumen de la música de fondo
    public void SetVolumenMusicaFondo(float volumen)
    {
        volumenMusicaFondo = Mathf.Clamp01(volumen);
        if (audioSource != null)
        {
            audioSource.volume = volumenMusicaFondo;
        }
        Debug.Log($"Volumen de música de fondo cambiado a: {volumenMusicaFondo}");
    }

    // Métodos públicos para cambiar el volumen de cada efecto de sonido
    public void SetVolumenPerfect(float volumen)
    {
        volumenPerfect = Mathf.Clamp01(volumen);
        Debug.Log($"Volumen de sonido PERFECT cambiado a: {volumenPerfect}");
    }

    public void SetVolumenGood(float volumen)
    {
        volumenGood = Mathf.Clamp01(volumen);
        Debug.Log($"Volumen de sonido GOOD cambiado a: {volumenGood}");
    }

    public void SetVolumenInstant(float volumen)
    {
        volumenInstant = Mathf.Clamp01(volumen);
        Debug.Log($"Volumen de sonido INSTANT cambiado a: {volumenInstant}");
    }

    public void SetVolumenMiss(float volumen)
    {
        volumenMiss = Mathf.Clamp01(volumen);
        Debug.Log($"Volumen de sonido MISS cambiado a: {volumenMiss}");
    }

    // Getters para obtener los volúmenes actuales
    public float GetVolumenMusicaFondo() => volumenMusicaFondo;
    public float GetVolumenPerfect() => volumenPerfect;
    public float GetVolumenGood() => volumenGood;
    public float GetVolumenInstant() => volumenInstant;
    public float GetVolumenMiss() => volumenMiss;

    private void Update()
    {
        if (isPaused) return;

        // Mostrar combo y score SOLO cuando el juego está en curso
        if (playing)
        {
            if (comboText != null && SistemaPuntuacion.Instance != null)
            {
                int comboGlobal = SistemaPuntuacion.Instance.GetComboActual();
                comboText.text = comboGlobal > 0 ? $"Combo: {comboGlobal}" : "Combo: 0";
            }

            if (scoreText != null && SistemaPuntuacion.Instance != null)
            {
                int scoreActual = SistemaPuntuacion.Instance.GetScoreActual();
                scoreText.text = $"Score: {scoreActual}";
            }
        }
        else
        {
            // Limpiar textos cuando el juego no está activo
            if (comboText != null) comboText.text = "";
            if (scoreText != null) scoreText.text = "";
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
                    ReproducirSonidoMiss();
                    n.PlayMissAnimation();
                    n.ShowJudgement("MISS");
                    activeNotes.RemoveAt(i);
                    n.DespawnAfter(0.30f);
                }
                continue;
            }

            if (n.Type == NoteType.Drag && n.IsDragExpired())
            {
                RegisterMiss();
                ReproducirSonidoMiss();
                n.PlayMissAnimation();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.30f);
                continue;
            }

            double errorLate = now - n.hitDspTime;
            if (errorLate > goodLateWindow)
            {
                RegisterMiss();
                ReproducirSonidoMiss();
                n.PlayMissAnimation();
                n.ShowJudgement("MISS");
                activeNotes.RemoveAt(i);
                n.DespawnAfter(0.30f);
            }
        }

        if (nextNoteIndex >= beatMap.notes.Count && activeNotes.Count == 0 && playing)
        {
            playing = false;
            OnSongFinished();
        }
    }

    // Métodos para reproducir sonidos (cada uno con su propio volumen)
    private void ReproducirSonidoPerfect()
    {
        if (sonidoHitPerfect != null && rhythmAudioSource != null)
        {
            rhythmAudioSource.PlayOneShot(sonidoHitPerfect, volumenPerfect);
        }
    }

    private void ReproducirSonidoGood()
    {
        if (sonidoHitGood != null && rhythmAudioSource != null)
        {
            rhythmAudioSource.PlayOneShot(sonidoHitGood, volumenGood);
        }
    }

    private void ReproducirSonidoInstant()
    {
        if (sonidoHitInstant != null && rhythmAudioSource != null)
        {
            rhythmAudioSource.PlayOneShot(sonidoHitInstant, volumenInstant);
        }
    }

    private void ReproducirSonidoMiss()
    {
        if (sonidoMiss != null && rhythmAudioSource != null)
        {
            rhythmAudioSource.PlayOneShot(sonidoMiss, volumenMiss);
        }
    }

    public void PausarRhythmGame()
    {
        if (isPaused) return; // Evita pausas múltiples

        isPaused = true;
        tiempoPausaInicioDsp = AudioSettings.dspTime;

        if (rhythmGameContainer != null)
        {
            rhythmGameContainer.SetActive(false);
            Debug.Log("Contenedor de ritmo OCULTADO - Las notas ya no se ven");
        }

        if (audioSource != null && audioSource.isPlaying)
        {
            audioSource.Pause();
        }

        // Opcional: Pausar el efecto de hit también
        if (NoteHitEffect.Instance != null)
        {
            // Si NoteHitEffect tiene animaciones, también deberías pausarlas
        }
    }

    public void ReanudarRhythmGame()
    {
        if (!isPaused) return;

        // Calcula cuánto tiempo estuvo pausado
        double tiempoPausado = AudioSettings.dspTime - tiempoPausaInicioDsp;
        pausaTiempoAcumulado += tiempoPausado;

        isPaused = false;

        if (rhythmGameContainer != null)
        {
            rhythmGameContainer.SetActive(true);
            Debug.Log("Contenedor de ritmo MOSTRADO - Las notas vuelven a verse");
        }

        if (audioSource != null && playing && !audioSource.isPlaying)
        {
            audioSource.UnPause();
        }

        // Ajustar los tiempos de todas las notas activas
        AjustarTiemposNotasPorPausa(tiempoPausado);

        LimpiarNotasHuerfanas();
    }

    public void LimpiarNotasHuerfanas()
    {
        // Forzar despawn de notas que deberían haber muerto durante la pausa
        for (int i = activeNotes.Count - 1; i >= 0; i--)
        {
            var note = activeNotes[i];
            if (note == null || !note.active)
            {
                activeNotes.RemoveAt(i);
                continue;
            }

            double now = AudioSettings.dspTime;

            // Si la nota ya pasó su ventana de tiempo + margen, forzar despawn
            double tiempoExcedido = now - note.hitDspTime;
            if (tiempoExcedido > goodLateWindow + 0.5f) // 0.5s de margen extra
            {
                Debug.Log($"Limpiando nota huérfana - tiempo excedido: {tiempoExcedido:F3}s");

                if (note.gameObject.activeSelf)
                {
                    note.gameObject.SetActive(false);
                }

                // Devolver al pool
                if (!pool.Contains(note))
                {
                    pool.Enqueue(note);
                }

                activeNotes.RemoveAt(i);
            }
        }
    }

    private void AjustarTiemposNotasPorPausa(double tiempoPausado)
    {
        // Ajustar el tiempo de inicio de la canción
        songStartDsp += tiempoPausado;

        // Ajustar los tiempos de hit de todas las notas activas
        foreach (var note in activeNotes)
        {
            if (note != null && note.active)
            {
                note.hitDspTime += tiempoPausado;

                // Si es InstantTap, ajustar también su tiempo de expiración
                if (note.Type == NoteType.InstantTap)
                {
                    // Necesitarías agregar un método público en NoteView para ajustar expire time
                    // o acceder mediante reflexión (no recomendado)
                }
            }
        }
    }

    public void StartSong()
    {
        if (notesParent != null)
        {
            notesParent.gameObject.SetActive(true);
        }

        ConfigurarVolumenes();

        if (botonStart != null) botonStart.SetActive(false);

        nextNoteIndex = 0;
        activeNotes.Clear();

        // Reiniciar combo y score antes de empezar la canción
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarCombo();
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        songStartDsp = AudioSettings.dspTime + 0.1;
        playing = true;

        if (beatMap != null && beatMap.song != null && audioSource != null)
        {
            audioSource.clip = beatMap.song;
            audioSource.volume = volumenMusicaFondo;
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
        if (!playing || isPaused || note == null || !note.active) return;
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
            ReproducirSonidoPerfect();
            note.PlayHitAnimation(isPerfect: true);
            note.ShowJudgement("PERFECT");
            NoteHitEffect.Instance?.SpawnHit((RectTransform)note.transform, isPerfect: true);
            activeNotes.Remove(note);
            note.DespawnAfter(0.30f);
            return;
        }

        if (signedError >= -goodEarlyWindow && signedError <= goodLateWindow)
        {
            RegisterHit(250, TipoPuntuacion.RitmoGood);
            ReproducirSonidoGood();
            note.PlayHitAnimation(isPerfect: false);
            note.ShowJudgement("GOOD");
            NoteHitEffect.Instance?.SpawnHit((RectTransform)note.transform, isPerfect: false);
            activeNotes.Remove(note);
            note.DespawnAfter(0.30f);
            return;
        }

        if (signedError > goodLateWindow)
        {
            RegisterMiss();
            ReproducirSonidoMiss();
            note.PlayMissAnimation();
            note.ShowJudgement("MISS");
            NoteHitEffect.Instance?.SpawnMiss((RectTransform)note.transform);
        }
    }

    public void TryHitInstantTap(NoteView note)
    {
        if (!playing || isPaused || note == null || !note.active) return;
        if (note.Type != NoteType.InstantTap) return;

        RegisterHit(200, TipoPuntuacion.RitmoInstant);
        ReproducirSonidoInstant();
        note.PlayHitAnimation(isPerfect: false);
        note.ShowJudgement("HIT");
        NoteHitEffect.Instance?.SpawnHit((RectTransform)note.transform, isPerfect: false);

        // Remover de la lista activa y despawnear
        activeNotes.Remove(note);
        note.DespawnAfter(0.20f);
    }

    public void TryStartDrag(NoteView note, int pointerId, Vector2 screenPos)
    {
        if (!playing || isPaused || note == null || !note.active) return;
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
            ReproducirSonidoMiss();
            note.ShowJudgement("MISS");
            return;
        }

        note.ShowJudgement("DRAG");
        note.ArmDrag(pointerId, screenPos);
    }

    public void TryCompleteDrag(NoteView note)
    {
        if (!playing || isPaused || note == null || !note.active) return;
        if (note.Type != NoteType.Drag) return;

        if (note.IsDragExpired())
        {
            RegisterMiss();
            ReproducirSonidoMiss();
            note.ShowJudgement("MISS");
            note.CancelDrag();
            return;
        }

        RegisterHit(250, TipoPuntuacion.RitmoDrag);
        ReproducirSonidoGood();
        note.PlayHitAnimation(isPerfect: false);
        note.ShowJudgement("GOOD");
        NoteHitEffect.Instance?.SpawnHit((RectTransform)note.transform, isPerfect: false);
        activeNotes.Remove(note);
        note.DespawnAfter(0.30f);
    }

    public void Hit(int lane)
    {
        if (!playing || isPaused) return;

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
            ReproducirSonidoMiss();
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
        else if (!useRandomPositions && lane >= 0 && lane < laneHitPoints.Length && laneHitPoints[lane] != null)
            spawnPos = laneHitPoints[lane].anchoredPosition;
        else
            spawnPos = Vector2.zero;

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
        isPaused = false;

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        StartCoroutine(ShowWinMenuWithDelay());
    }

    private IEnumerator ShowWinMenuWithDelay()
    {
        yield return new WaitForSeconds(winMenuDelay);

        if (comboText != null && comboTextTargetParent != null)
        {
            comboTextRect.SetParent(comboTextTargetParent, false);
            comboTextRect.anchoredPosition = comboTextTargetPosition;
        }

        if (scoreText != null && scoreTextTargetParent != null)
        {
            if (scoreTextOriginalParent == null)
            {
                scoreTextOriginalParent = scoreTextRect.parent;
                scoreTextOriginalPosition = scoreTextRect.anchoredPosition;
            }

            scoreTextRect.SetParent(scoreTextTargetParent, false);
            scoreTextRect.anchoredPosition = scoreTextTargetPosition;
        }

        if (menuHasGanadoMinijuego != null) menuHasGanadoMinijuego.SetActive(true);
    }

    public void ReiniciarMinijuegoLimpieza()
    {
        if (audioSource != null) audioSource.Stop();

        playing = false;
        isPaused = false;

        if (notesParent != null)
        {
            notesParent.gameObject.SetActive(false);
        }

        if (rhythmGameContainer != null)
        {
            rhythmGameContainer.SetActive(true);
        }

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

        if (comboText != null && comboTextOriginalParent != null)
        {
            comboTextRect.SetParent(comboTextOriginalParent, false);
            comboTextRect.anchoredPosition = comboTextOriginalPosition;
        }

        if (scoreText != null && scoreTextOriginalParent != null)
        {
            scoreTextRect.SetParent(scoreTextOriginalParent, false);
            scoreTextRect.anchoredPosition = scoreTextOriginalPosition;
        }

        if (menuHasGanadoMinijuego != null) menuHasGanadoMinijuego.SetActive(false);
        if (botonStart != null) botonStart.SetActive(true);

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarCombo();
        }

        if (audioSource != null) audioSource.time = 0f;
    }
}