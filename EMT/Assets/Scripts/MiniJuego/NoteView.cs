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

        CancelDrag();
    }

    public void ConfigureTap()
    {
        noteType = NoteType.Tap;

        if (dragIcon) dragIcon.SetActive(false);
        if (dragTarget) dragTarget.gameObject.SetActive(false);

        if (circleGraphic) circleGraphic.color = tapColor;
        if (ringGraphic) ringGraphic.color = tapColor;

        CancelDrag();
    }

    public void ConfigureDrag(DragDirection dir, float distancePx, float timeLimit)
    {
        noteType = NoteType.Drag;
        dragDirection = dir;
        dragDistancePx = distancePx;
        dragTimeLimit = timeLimit;

        if (dragIcon) dragIcon.SetActive(true);
        if (dragTarget) dragTarget.gameObject.SetActive(true);

        if (circleGraphic) circleGraphic.color = dragColor;
        if (ringGraphic) ringGraphic.color = dragColor;

        CancelDrag();
        SetupDragTargetPosition();
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

        dragTarget.anchoredPosition = offset;
    }

    public void ArmDrag(int pointerId, Vector2 startScreenPos)
    {
        dragArmed = true;
        dragPointerId = pointerId;
        dragStartScreenPos = startScreenPos;
        dragDeadline = Time.unscaledTime + dragTimeLimit;
        pointerHeld = true;

        if (circleGraphic) circleGraphic.color = dragArmedColor;
        if (ringGraphic) ringGraphic.color = dragArmedColor;
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
            if (circleGraphic) circleGraphic.color = dragColor;
            if (ringGraphic) ringGraphic.color = dragColor;
        }
        else
        {
            if (circleGraphic) circleGraphic.color = tapColor;
            if (ringGraphic) ringGraphic.color = tapColor;
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
            if (judgementText != null) judgementText.text = "";
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

        Vector2 targetWorldPos = originalAnchoredPos + dragTarget.anchoredPosition;
        float distToTarget = Vector2.Distance(selfRect.anchoredPosition, targetWorldPos);

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

        CancelDrag();

        if (manager != null)
            manager.ReturnToPool(this);
    }
}