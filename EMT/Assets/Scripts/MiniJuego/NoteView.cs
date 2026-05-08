using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class NoteView : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform approachRing;
    [SerializeField] private TMP_Text judgementText;
    [SerializeField] private TMP_Text dragArrowText;  // texto con la flecha de dirección (←→↑↓)
    [SerializeField] private GameObject dragIcon;
    [SerializeField] private RectTransform dragTarget;

    [SerializeField] private RawImage circleGraphic;
    [SerializeField] private RawImage ringGraphic;
    [SerializeField] private RawImage hitMarkerGraphic;

    [Header("Hit Marker Colors")]
    [SerializeField] private Color hitMarkerTapColor    = new Color(1f, 1f, 1f, 0f);      // invisible por defecto
    [SerializeField] private Color hitMarkerDragColor   = new Color(1f, 0.85f, 0.2f, 0f); // invisible por defecto
    [SerializeField] private Color hitMarkerArmedColor  = new Color(0.2f, 1f, 0.6f, 0.45f);
    [SerializeField] private Color instantTapColor      = new Color(0.15f, 0.95f, 1f, 1f);

    [Header("OSU Feel")]
    [SerializeField] private float startScale = 3f;
    [SerializeField] private float endScale   = 1f;

    [Header("Judgement UI")]
    [SerializeField] private float judgementDuration = 0.55f;

    // ── Nuevas opciones visuales ────────────────────────────────────────────
    [Header("Hit Animation")]
    [SerializeField] private float hitPunchScale     = 1.35f;   // escala máxima al golpear
    [SerializeField] private float hitPunchDuration  = 0.12f;   // duración del punch (subida)
    [SerializeField] private float hitShrinkDuration = 0.18f;   // duración de la vuelta a 1x

    [Header("Miss Animation")]
    [SerializeField] private float missShakeMagnitude = 8f;     // px de sacudida en miss
    [SerializeField] private float missShakeDuration  = 0.25f;  // duración total de la sacudida

    [Header("Judgement Animation")]
    [SerializeField] private float judgementBounceScale = 1.4f; // escala inicial del texto
    [SerializeField] private float judgementFadeDuration = 0.2f; // duración del fade-out

    [Header("Drag")]
    [SerializeField] private NoteType      noteType        = NoteType.Tap;
    [SerializeField] private DragDirection dragDirection   = DragDirection.Any;
    [SerializeField] private float         dragDistancePx  = 180f;
    [SerializeField] private float         dragTimeLimit   = 0.8f;
    [SerializeField] private float         dragTargetReachThreshold = 60f;

    [Header("Colors")]
    [SerializeField] private Color tapColor      = Color.white;
    [SerializeField] private Color dragColor     = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color dragArmedColor = new Color(0.2f, 1f, 0.6f, 1f);

    [HideInInspector] public int    lane;
    [HideInInspector] public double hitDspTime;
    [HideInInspector] public bool   active;

    private RhythmGameManager manager;

    private float judgementHideTime;
    private bool  despawnScheduled;
    public double InstantTapExpireDspTime { get; private set; }

    // Drag runtime
    private bool        dragArmed;
    private float       dragDeadline;
    private Vector2     dragStartScreenPos;
    private int         dragPointerId = int.MinValue;
    private RectTransform selfRect;
    private Vector2     originalAnchoredPos;
    private bool        pointerHeld;

    public NoteType Type => noteType;

    // ── Coroutines activas (para poder cancelar) ────────────────────────────
    private Coroutine punchRoutine;
    private Coroutine shakeRoutine;
    private Coroutine judgementRoutine;

    // ═══════════════════════════════════════════════════════════════════════
    //  INIT
    // ═══════════════════════════════════════════════════════════════════════

    public void Init(RhythmGameManager mgr, int laneIndex, double dspTime, float leadTimeSeconds)
    {
        manager    = mgr;
        lane       = laneIndex;
        hitDspTime = dspTime;
        active     = true;

        StopAllCoroutines();
        despawnScheduled = false;

        gameObject.SetActive(true);

        if (selfRect == null)
            selfRect = (RectTransform)transform;

        // Restablecer escala por si quedó de un punch anterior
        selfRect.localScale = Vector3.one;

        originalAnchoredPos = selfRect.anchoredPosition;
        pointerHeld = false;

        if (approachRing != null)
            approachRing.localScale = Vector3.one * startScale;

        if (judgementText != null)
        {
            judgementText.text  = "";
            judgementText.alpha = 1f;
            judgementText.rectTransform.localScale = Vector3.one;
        }

        judgementHideTime = 0f;

        if (dragTarget != null)
        {
            if (dragTarget.parent != selfRect.parent)
                dragTarget.SetParent(selfRect.parent, false);

            dragTarget.gameObject.SetActive(false);
            dragTarget.localScale    = Vector3.one;
            dragTarget.localRotation = Quaternion.identity;
        }

        if (hitMarkerGraphic != null)
            hitMarkerGraphic.gameObject.SetActive(true);

        InstantTapExpireDspTime = 0;

        if (approachRing != null)
            approachRing.gameObject.SetActive(true);

        CancelDrag();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  CONFIGURE
    // ═══════════════════════════════════════════════════════════════════════

    public void ConfigureTap()
    {
        noteType = NoteType.Tap;

        if (dragIcon      != null) dragIcon.SetActive(false);
        if (dragTarget    != null) dragTarget.gameObject.SetActive(false);
        if (dragArrowText != null) dragArrowText.gameObject.SetActive(false);

        if (hitMarkerGraphic != null) hitMarkerGraphic.gameObject.SetActive(true);
        if (approachRing     != null) approachRing.gameObject.SetActive(true);

        if (circleGraphic    != null) circleGraphic.color    = tapColor;
        if (ringGraphic      != null) ringGraphic.color      = tapColor;
        if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerTapColor;

        CancelDrag();
    }

    public void ConfigureDrag(DragDirection dir, float distancePx, float timeLimit)
    {
        noteType      = NoteType.Drag;
        dragDirection  = dir;
        dragDistancePx = distancePx;
        dragTimeLimit  = timeLimit;

        if (dragIcon     != null) dragIcon.SetActive(true);
        if (hitMarkerGraphic != null) hitMarkerGraphic.gameObject.SetActive(true);
        if (approachRing     != null) approachRing.gameObject.SetActive(true);

        if (dragTarget != null)
        {
            dragTarget.gameObject.SetActive(true);
            dragTarget.SetAsLastSibling();
        }

        if (circleGraphic    != null) circleGraphic.color    = dragColor;
        if (ringGraphic      != null) ringGraphic.color      = dragColor;
        if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerDragColor;

        // Flecha grande que muestra la dirección del drag
        if (dragArrowText != null)
        {
            dragArrowText.gameObject.SetActive(true);
            dragArrowText.color = new Color(1f, 0.95f, 0.1f, 1f);
            dragArrowText.text = dir switch
            {
                DragDirection.Left  => "←",
                DragDirection.Right => "→",
                DragDirection.Up    => "↑",
                DragDirection.Down  => "↓",
                _                   => "↔"
            };
        }

        CancelDrag();
        SetupDragTargetPosition();
    }

    public void ConfigureInstantTap(double expireDspTime)
    {
        noteType = NoteType.InstantTap;
        InstantTapExpireDspTime = expireDspTime;

        if (dragIcon   != null) dragIcon.SetActive(false);
        if (dragTarget != null) dragTarget.gameObject.SetActive(false);

        if (hitMarkerGraphic != null) hitMarkerGraphic.gameObject.SetActive(false);
        if (approachRing     != null) approachRing.gameObject.SetActive(false);
        if (dragArrowText    != null) dragArrowText.gameObject.SetActive(false);

        if (circleGraphic != null)
        {
            circleGraphic.color = instantTapColor;
            circleGraphic.rectTransform.localScale = Vector3.one;
        }

        if (ringGraphic != null)
            ringGraphic.color = instantTapColor;

        CancelDrag();
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  DRAG SETUP
    // ═══════════════════════════════════════════════════════════════════════

    private void SetupDragTargetPosition()
    {
        if (dragTarget == null) return;

        Vector2 offset = Vector2.zero;
        switch (dragDirection)
        {
            case DragDirection.Left:  offset = Vector2.left  * dragDistancePx; break;
            case DragDirection.Right: offset = Vector2.right * dragDistancePx; break;
            case DragDirection.Up:    offset = Vector2.up    * dragDistancePx; break;
            case DragDirection.Down:  offset = Vector2.down  * dragDistancePx; break;
            case DragDirection.Any:   offset = Vector2.right * dragDistancePx; break;
        }

        dragTarget.anchoredPosition = originalAnchoredPos + offset;
        dragTarget.localScale       = Vector3.one;
        dragTarget.localRotation    = Quaternion.identity;
    }

    public void ArmDrag(int pointerId, Vector2 startScreenPos)
    {
        dragArmed          = true;
        dragPointerId      = pointerId;
        dragStartScreenPos = startScreenPos;
        dragDeadline       = Time.unscaledTime + dragTimeLimit;
        pointerHeld        = true;

        if (circleGraphic    != null) circleGraphic.color    = dragArmedColor;
        if (ringGraphic      != null) ringGraphic.color      = dragArmedColor;
        if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerArmedColor;
    }

    public bool IsDragExpired() => dragArmed && Time.unscaledTime > dragDeadline;

    public void CancelDrag()
    {
        dragArmed     = false;
        dragPointerId = int.MinValue;
        dragDeadline  = 0f;
        pointerHeld   = false;

        if (selfRect != null)
            selfRect.anchoredPosition = originalAnchoredPos;

        if (noteType == NoteType.Drag)
        {
            if (circleGraphic    != null) circleGraphic.color    = dragColor;
            if (ringGraphic      != null) ringGraphic.color      = dragColor;
            if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerDragColor;
        }
        else
        {
            if (circleGraphic    != null) circleGraphic.color    = tapColor;
            if (ringGraphic      != null) ringGraphic.color      = tapColor;
            if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerTapColor;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  APPROACH
    // ═══════════════════════════════════════════════════════════════════════

    public void SetApproach(float t01)
    {
        if (approachRing == null) return;
        float s = Mathf.Lerp(startScale, endScale, t01);
        approachRing.localScale = Vector3.one * s;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  JUDGEMENT FEEDBACK
    // ═══════════════════════════════════════════════════════════════════════

    public void ShowJudgement(string msg)
    {
        if (judgementText == null) return;

        // Cancelar animación previa si la hay
        if (judgementRoutine != null) StopCoroutine(judgementRoutine);

        judgementText.text  = msg;
        judgementText.alpha = 1f;
        judgementHideTime   = 0f; // la rutina controla el timing

        judgementRoutine = StartCoroutine(JudgementAnimation(msg));
    }

    private IEnumerator JudgementAnimation(string msg)
    {
        if (judgementText == null) yield break;

        // Color del texto según resultado
        judgementText.color = msg switch
        {
            "PERFECT" => new Color(1f, 0.95f, 0.2f),   // amarillo dorado
            "GOOD"    => new Color(0.2f, 1f, 0.4f),    // verde
            "HIT"     => new Color(0.2f, 0.9f, 1f),    // cyan (InstantTap)
            "DRAG"    => new Color(1f, 0.85f, 0.2f),   // naranja
            "MISS"    => new Color(1f, 0.2f, 0.2f),    // rojo
            "EARLY"   => new Color(0.8f, 0.8f, 1f),    // lavanda
            _         => Color.white
        };

        // Bounce: escala de bounceScale → 1 en hitPunchDuration
        float elapsed = 0f;
        float halfDur = judgementDuration * 0.35f;
        RectTransform rt = judgementText.rectTransform;

        while (elapsed < halfDur)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / halfDur);
            float s = Mathf.Lerp(judgementBounceScale, 1f, t);
            rt.localScale = Vector3.one * s;
            yield return null;
        }

        rt.localScale = Vector3.one;

        // Esperar hasta que empiece el fade
        float holdTime = judgementDuration - halfDur - judgementFadeDuration;
        if (holdTime > 0f)
            yield return new WaitForSecondsRealtime(holdTime);

        // Fade out
        elapsed = 0f;
        while (elapsed < judgementFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            judgementText.alpha = Mathf.Lerp(1f, 0f, elapsed / judgementFadeDuration);
            yield return null;
        }

        judgementText.text  = "";
        judgementText.alpha = 1f;
        rt.localScale       = Vector3.one;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  HIT ANIMATION  (llamar antes de DespawnAfter)
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Anima un "punch" de escala al golpear la nota.
    /// isPerfect = true → punch más grande y en color dorado.
    /// </summary>
    public void PlayHitAnimation(bool isPerfect)
    {
        if (punchRoutine != null) StopCoroutine(punchRoutine);
        punchRoutine = StartCoroutine(HitPunch(isPerfect));
    }

    private IEnumerator HitPunch(bool isPerfect)
    {
        if (selfRect == null) yield break;

        float targetScale = isPerfect ? hitPunchScale * 1.1f : hitPunchScale;

        // Flash de color al golpear
        Color flashColor = isPerfect
            ? new Color(1f, 0.95f, 0.3f, 1f)   // dorado brillante
            : new Color(0.5f, 1f, 0.5f, 1f);   // verde suave

        if (circleGraphic    != null) circleGraphic.color    = flashColor;
        if (ringGraphic      != null) ringGraphic.color      = flashColor;

        // Ocultar approachRing al golpear
        if (approachRing != null) approachRing.gameObject.SetActive(false);

        // Subida rápida
        float elapsed = 0f;
        while (elapsed < hitPunchDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / hitPunchDuration);
            float s = Mathf.Lerp(1f, targetScale, EaseOut(t));
            selfRect.localScale = Vector3.one * s;
            yield return null;
        }

        // Bajada suave
        elapsed = 0f;
        while (elapsed < hitShrinkDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / hitShrinkDuration);
            float s = Mathf.Lerp(targetScale, 0f, EaseIn(t));
            selfRect.localScale = Vector3.one * s;
            yield return null;
        }

        selfRect.localScale = Vector3.zero;
    }

    // ─── Animación de MISS: sacudida lateral ─────────────────────────────────

    public void PlayMissAnimation()
    {
        if (shakeRoutine != null) StopCoroutine(shakeRoutine);
        shakeRoutine = StartCoroutine(ShakeNote());
    }

    private IEnumerator ShakeNote()
    {
        if (selfRect == null) yield break;

        // Flash rojo
        if (circleGraphic != null) circleGraphic.color = new Color(1f, 0.2f, 0.2f, 1f);
        if (ringGraphic   != null) ringGraphic.color   = new Color(1f, 0.2f, 0.2f, 1f);

        float elapsed  = 0f;
        float freq     = 30f; // Hz de vibración
        Vector2 origin = selfRect.anchoredPosition;

        while (elapsed < missShakeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = elapsed / missShakeDuration;
            float damping  = 1f - progress;                    // amortiguado
            float x = Mathf.Sin(elapsed * freq) * missShakeMagnitude * damping;
            selfRect.anchoredPosition = origin + new Vector2(x, 0f);
            yield return null;
        }

        selfRect.anchoredPosition = origin;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  SPAWN / DESPAWN
    // ═══════════════════════════════════════════════════════════════════════

    public void DespawnAfter(float seconds)
    {
        if (!active || despawnScheduled) return;
        despawnScheduled = true;
        StartCoroutine(DespawnRoutine(seconds));
    }

    private IEnumerator DespawnRoutine(float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        Despawn();
    }

    public void Despawn()
    {
        active = false;
        gameObject.SetActive(false);

        if (dragTarget != null)
            dragTarget.gameObject.SetActive(false);

        CancelDrag();

        // Restablecer escala para el pool
        if (selfRect != null)
            selfRect.localScale = Vector3.one;

        if (manager != null)
            manager.ReturnToPool(this);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  UPDATE
    // ═══════════════════════════════════════════════════════════════════════

    private void Update()
    {
        // El judgement ya se gestiona vía coroutine; este bloque queda como
        // fallback por si la coroutine se interrumpe
        if (judgementHideTime > 0f && Time.unscaledTime >= judgementHideTime)
        {
            if (judgementText != null) judgementText.text = "";
            judgementHideTime = 0f;
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  POINTER EVENTS
    // ═══════════════════════════════════════════════════════════════════════

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!active || manager == null) return;

        switch (noteType)
        {
            case NoteType.Tap:
                manager.TryHitNote(this);
                break;
            case NoteType.Drag:
                manager.TryStartDrag(this, eventData.pointerId, eventData.position);
                break;
            case NoteType.InstantTap:
                manager.TryHitInstantTap(this);
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        if (!active || manager == null) return;
        if (noteType != NoteType.Drag) return;
        if (!dragArmed || !pointerHeld) return;
        if (eventData.pointerId != dragPointerId) return;
        if (selfRect == null) selfRect = (RectTransform)transform;

        RectTransform parentRect = selfRect.parent as RectTransform;
        if (parentRect == null) return;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
        {
            selfRect.anchoredPosition = localPoint;
        }

        if (dragTarget == null) return;

        float distToTarget = Vector2.Distance(selfRect.anchoredPosition, dragTarget.anchoredPosition);

        float pct = 1f - Mathf.Clamp01(distToTarget / dragDistancePx);
        if (approachRing != null)
        {
            float visualT = Mathf.Lerp(0.7f, 1f, pct);
            float s = Mathf.Lerp(startScale, endScale, visualT);
            approachRing.localScale = Vector3.one * s;
        }

        if (distToTarget <= dragTargetReachThreshold)
        {
            dragArmed   = false;
            pointerHeld = false;
            manager.TryCompleteDrag(this);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (noteType != NoteType.Drag) return;
        pointerHeld = false;

        if (selfRect != null && active)
            selfRect.anchoredPosition = originalAnchoredPos;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  EASING HELPERS
    // ═══════════════════════════════════════════════════════════════════════

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseIn(float t)  => t * t;


    //////IA IA IO
    public void ForzarDespawnInmediato()
    {
        if (!active) return;

        // Detener corutinas pendientes
        StopAllCoroutines();

        // Limpiar estado
        active = false;
        gameObject.SetActive(false);

        if (dragTarget != null)
            dragTarget.gameObject.SetActive(false);

        CancelDrag();

        if (manager != null)
            manager.ReturnToPool(this);
    }
}
