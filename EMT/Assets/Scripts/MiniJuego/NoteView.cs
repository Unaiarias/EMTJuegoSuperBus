using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;

public class NoteView : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("References")]
    [SerializeField] private RectTransform approachRing;
    [SerializeField] private TMP_Text judgementText;
    [SerializeField] private GameObject dragIcon; // opcional

    [HideInInspector] public int lane;
    [HideInInspector] public double hitDspTime;
    [HideInInspector] public bool active;

    private RhythmGameManager manager;

    [Header("OSU Feel")]
    [SerializeField] private float startScale = 2.2f;
    [SerializeField] private float endScale = 1.0f;

    [Header("Judgement UI")]
    [SerializeField] private float judgementDuration = 0.4f;
    private float judgementHideTime;

    // --- Drag ---
    [Header("Drag")]
    [SerializeField] private NoteType noteType = NoteType.Tap;
    [SerializeField] private DragDirection dragDirection = DragDirection.Any;
    [SerializeField] private float dragDistancePx = 180f;
    [SerializeField] private float dragTimeLimit = 0.8f;

    private bool dragArmed;
    private float dragDeadline;
    private Vector2 dragStartScreenPos;
    private int dragPointerId = int.MinValue;

    private bool despawnScheduled;

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

        if (approachRing != null)
            approachRing.localScale = Vector3.one * startScale;

        if (judgementText != null)
            judgementText.text = "";

        judgementHideTime = 0f;

        CancelDrag();
    }

    // Configura tipo y parámetros desde el beatmap
    public void ConfigureTap()
    {
        noteType = NoteType.Tap;
        if (dragIcon) dragIcon.SetActive(false);
        CancelDrag();
    }

    public void ConfigureDrag(DragDirection dir, float distancePx, float timeLimit)
    {
        noteType = NoteType.Drag;
        dragDirection = dir;
        dragDistancePx = distancePx;
        dragTimeLimit = timeLimit;

        if (dragIcon) dragIcon.SetActive(true);
        CancelDrag();
    }

    public void ArmDrag(int pointerId, Vector2 startScreenPos)
    {
        dragArmed = true;
        dragPointerId = pointerId;
        dragStartScreenPos = startScreenPos;
        dragDeadline = Time.unscaledTime + dragTimeLimit;
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
        if (!dragArmed) return;
        if (eventData.pointerId != dragPointerId) return;

        Vector2 delta = eventData.position - dragStartScreenPos;
        float dist = delta.magnitude;

        bool dirOk = (dragDirection == DragDirection.Any) ? true : CheckDirection(delta);

        if (dirOk && dist >= dragDistancePx)
            manager.TryCompleteDrag(this);
    }

    public void OnEndDrag(PointerEventData eventData) { }

    private bool CheckDirection(Vector2 delta)
    {
        float ax = Mathf.Abs(delta.x);
        float ay = Mathf.Abs(delta.y);

        switch (dragDirection)
        {
            case DragDirection.Left: return delta.x < 0f && ax >= ay;
            case DragDirection.Right: return delta.x > 0f && ax >= ay;
            case DragDirection.Up: return delta.y > 0f && ay >= ax;
            case DragDirection.Down: return delta.y < 0f && ay >= ax;
            default: return true;
        }
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