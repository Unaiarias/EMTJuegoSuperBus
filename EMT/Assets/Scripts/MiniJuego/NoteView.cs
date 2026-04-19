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
    [SerializeField] private GameObject dragIcon;
    [SerializeField] private RectTransform dragTarget;

    [SerializeField] private RawImage circleGraphic;
    [SerializeField] private RawImage ringGraphic;
    [SerializeField] private RawImage hitMarkerGraphic;

    [Header("Hit Marker Colors")]
    [SerializeField] private Color hitMarkerTapColor = new Color(1f, 1f, 1f, 0.22f);
    [SerializeField] private Color hitMarkerDragColor = new Color(1f, 0.85f, 0.2f, 0.35f);
    [SerializeField] private Color hitMarkerArmedColor = new Color(0.2f, 1f, 0.6f, 0.45f);
    [SerializeField] private Color instantTapColor = new Color(0.15f, 0.95f, 1f, 1f);
    //[SerializeField] private Color instantTapRingColor = new Color(0.15f, 0.95f, 1f, 0.95f);
    //[SerializeField] private Color hitMarkerInstantTapColor = new Color(0.15f, 0.95f, 1f, 0.35f);

    [Header("OSU Feel")]
    [SerializeField] private float startScale = 3f;
    [SerializeField] private float endScale = 1f;

    [Header("Judgement UI")]
    [SerializeField] private float judgementDuration = 0.4f;

    [Header("Drag")]
    [SerializeField] private NoteType noteType = NoteType.Tap;
    [SerializeField] private DragDirection dragDirection = DragDirection.Any;
    [SerializeField] private float dragDistancePx = 180f;
    [SerializeField] private float dragTimeLimit = 0.8f;
    [SerializeField] private float dragTargetReachThreshold = 60f;

    [Header("Colors")]
    [SerializeField] private Color tapColor = Color.white;
    [SerializeField] private Color dragColor = new Color(1f, 0.85f, 0.2f, 1f);
    [SerializeField] private Color dragArmedColor = new Color(0.2f, 1f, 0.6f, 1f);

    [HideInInspector] public int lane;
    [HideInInspector] public double hitDspTime;
    [HideInInspector] public bool active;

    private RhythmGameManager manager;

    private float judgementHideTime;
    private bool despawnScheduled;
    public double InstantTapExpireDspTime { get; private set; }

    // Drag runtime
    private bool dragArmed;
    private float dragDeadline;
    private Vector2 dragStartScreenPos;
    private int dragPointerId = int.MinValue;
    private RectTransform selfRect;
    private Vector2 originalAnchoredPos;
    private bool pointerHeld;

    public NoteType Type => noteType;

    public void Init(RhythmGameManager mgr, int laneIndex, double dspTime, float leadTimeSeconds)
    {
        manager = mgr;
        lane = laneIndex;
        hitDspTime = dspTime;
        active = true;

        StopAllCoroutines();
        despawnScheduled = false;

        gameObject.SetActive(true);

        if (selfRect == null)
            selfRect = (RectTransform)transform;

        originalAnchoredPos = selfRect.anchoredPosition;
        pointerHeld = false;

        if (approachRing != null)
            approachRing.localScale = Vector3.one * startScale;

        if (judgementText != null)
            judgementText.text = "";

        judgementHideTime = 0f;

        // El target debe quedar como hermano de la nota para que NO se mueva con ella
        if (dragTarget != null)
        {
            if (dragTarget.parent != selfRect.parent)
                dragTarget.SetParent(selfRect.parent, false);

            dragTarget.gameObject.SetActive(false);
            dragTarget.localScale = Vector3.one;
            dragTarget.localRotation = Quaternion.identity;
        }

        if (hitMarkerGraphic != null)
            hitMarkerGraphic.gameObject.SetActive(true);

        InstantTapExpireDspTime = 0;

        if (approachRing != null)
            approachRing.gameObject.SetActive(true);


        CancelDrag();
    }

    public void ConfigureTap()
    {
        noteType = NoteType.Tap;

        if (dragIcon != null) dragIcon.SetActive(false);
        if (dragTarget != null) dragTarget.gameObject.SetActive(false);

        if (hitMarkerGraphic != null) hitMarkerGraphic.gameObject.SetActive(true);
        if (approachRing != null) approachRing.gameObject.SetActive(true);

        if (circleGraphic != null) circleGraphic.color = tapColor;
        if (ringGraphic != null) ringGraphic.color = tapColor;
        if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerTapColor;

        CancelDrag();
    }

    public void ConfigureDrag(DragDirection dir, float distancePx, float timeLimit)
    {
        noteType = NoteType.Drag;
        dragDirection = dir;
        dragDistancePx = distancePx;
        dragTimeLimit = timeLimit;

        if (dragIcon != null) dragIcon.SetActive(true);
        if (hitMarkerGraphic != null) hitMarkerGraphic.gameObject.SetActive(true);
        if (approachRing != null) approachRing.gameObject.SetActive(true);

        if (dragTarget != null)
        {
            dragTarget.gameObject.SetActive(true);
            dragTarget.SetAsLastSibling();
        }

        if (circleGraphic != null) circleGraphic.color = dragColor;
        if (ringGraphic != null) ringGraphic.color = dragColor;
        if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerDragColor;

        CancelDrag();
        SetupDragTargetPosition();
    }

    public void ConfigureInstantTap(double expireDspTime)
    {
        noteType = NoteType.InstantTap;
        InstantTapExpireDspTime = expireDspTime;

        if (dragIcon != null) dragIcon.SetActive(false);
        if (dragTarget != null) dragTarget.gameObject.SetActive(false);

        // IMPORTANTE: esta nota NO usa base ni ring
        if (hitMarkerGraphic != null) hitMarkerGraphic.gameObject.SetActive(false);
        if (approachRing != null) approachRing.gameObject.SetActive(false);

        if (circleGraphic != null)
        {
            circleGraphic.color = instantTapColor;
            circleGraphic.rectTransform.localScale = Vector3.one;
        }

        if (ringGraphic != null)
            ringGraphic.color = instantTapColor;

        CancelDrag();
    }

    private void SetupDragTargetPosition()
    {
        if (dragTarget == null) return;

        Vector2 offset = Vector2.zero;

        switch (dragDirection)
        {
            case DragDirection.Left:
                offset = Vector2.left * dragDistancePx;
                break;
            case DragDirection.Right:
                offset = Vector2.right * dragDistancePx;
                break;
            case DragDirection.Up:
                offset = Vector2.up * dragDistancePx;
                break;
            case DragDirection.Down:
                offset = Vector2.down * dragDistancePx;
                break;
            case DragDirection.Any:
                offset = Vector2.right * dragDistancePx;
                break;
        }

        // Como el target ya no es hijo de la nota, su posición es absoluta dentro del mismo parent
        dragTarget.anchoredPosition = originalAnchoredPos + offset;
        dragTarget.localScale = Vector3.one;
        dragTarget.localRotation = Quaternion.identity;
    }

    public void ArmDrag(int pointerId, Vector2 startScreenPos)
    {
        dragArmed = true;
        dragPointerId = pointerId;
        dragStartScreenPos = startScreenPos;
        dragDeadline = Time.unscaledTime + dragTimeLimit;
        pointerHeld = true;

        if (circleGraphic != null) circleGraphic.color = dragArmedColor;
        if (ringGraphic != null) ringGraphic.color = dragArmedColor;
        if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerArmedColor;
    }

    public bool IsDragExpired()
    {
        return dragArmed && Time.unscaledTime > dragDeadline;
    }

    public void CancelDrag()
    {
        dragArmed = false;
        dragPointerId = int.MinValue;
        dragDeadline = 0f;
        pointerHeld = false;

        if (selfRect != null)
            selfRect.anchoredPosition = originalAnchoredPos;

        if (noteType == NoteType.Drag)
        {
            if (circleGraphic != null) circleGraphic.color = dragColor;
            if (ringGraphic != null) ringGraphic.color = dragColor;
            if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerDragColor;
        }
        else
        {
            if (circleGraphic != null) circleGraphic.color = tapColor;
            if (ringGraphic != null) ringGraphic.color = tapColor;
            if (hitMarkerGraphic != null) hitMarkerGraphic.color = hitMarkerTapColor;
        }
    }

    public void SetApproach(float t01)
    {
        if (approachRing == null) return;

        float s = Mathf.Lerp(startScale, endScale, t01);
        approachRing.localScale = Vector3.one * s;
    }

    public void ShowJudgement(string msg)
    {
        if (judgementText == null) return;

        judgementText.text = msg;
        judgementHideTime = Time.unscaledTime + judgementDuration;
    }

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

    private void Update()
    {
        if (judgementHideTime > 0f && Time.unscaledTime >= judgementHideTime)
        {
            if (judgementText != null)
                judgementText.text = "";

            judgementHideTime = 0f;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!active || manager == null) return;

        if (noteType == NoteType.Tap)
        {
            manager.TryHitNote(this);
        }
        else if (noteType == NoteType.Drag)
        {
            manager.TryStartDrag(this, eventData.pointerId, eventData.position);
        }
        else if (noteType == NoteType.InstantTap)
        {
            manager.TryHitInstantTap(this);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // No hace falta lógica aquí por ahora
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!active || manager == null) return;
        if (noteType != NoteType.Drag) return;
        if (!dragArmed || !pointerHeld) return;
        if (eventData.pointerId != dragPointerId) return;
        if (selfRect == null) selfRect = (RectTransform)transform;

        RectTransform parentRect = selfRect.parent as RectTransform;
        if (parentRect == null) return;

        Vector2 localPoint;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            selfRect.anchoredPosition = localPoint;
        }

        if (dragTarget == null) return;

        float distToTarget = Vector2.Distance(selfRect.anchoredPosition, dragTarget.anchoredPosition);

        // Visual extra: cuanto más cerca, más se cierra el ring
        float pct = 1f - Mathf.Clamp01(distToTarget / dragDistancePx);
        if (approachRing != null)
        {
            float visualT = Mathf.Lerp(0.7f, 1f, pct);
            float s = Mathf.Lerp(startScale, endScale, visualT);
            approachRing.localScale = Vector3.one * s;
        }

        if (distToTarget <= dragTargetReachThreshold)
        {
            dragArmed = false;
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

    public void Despawn()
    {
        active = false;
        gameObject.SetActive(false);

        if (dragTarget != null)
            dragTarget.gameObject.SetActive(false);

        CancelDrag();

        if (manager != null)
            manager.ReturnToPool(this);
    }
}