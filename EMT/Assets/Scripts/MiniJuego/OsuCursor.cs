using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Cursor estilo OSU: círculo brillante con rastro de color.
/// Añade este script a un GameObject vacío (hijo del Canvas del minijuego).
/// Asigna el sprite en el Inspector (puedes usar note_circle.png).
/// El cursor del sistema se oculta automáticamente al activarse.
/// </summary>
public class OsuCursor : MonoBehaviour
{
    [Header("Cursor principal")]
    [SerializeField] private Sprite cursorSprite;
    [SerializeField] private float cursorSize = 42f;
    [SerializeField] private Color cursorColor = new Color(1f, 0.95f, 0.4f, 1f);   // amarillo brillante

    [Header("Anillo exterior (opcional)")]
    [SerializeField] private bool mostrarAnillo = true;
    [SerializeField] private float ringSize = 58f;
    [SerializeField] private Color ringColor = new Color(1f, 1f, 1f, 0.35f);

    [Header("Rastro")]
    [SerializeField] private int trailCount = 22;
    [SerializeField] private float trailSpacing = 5f;        // px mínimos entre puntos del rastro
    [SerializeField] private Color trailColorCerca = new Color(1f, 0.55f, 0.1f, 0.85f);  // naranja
    [SerializeField] private Color trailColorLejos = new Color(0.9f, 0.1f, 0.55f, 0f);   // rosa → transparente

    // ── internos ──────────────────────────────────────────────
    private Canvas       canvas;
    private RectTransform canvasRect;

    private RectTransform mainRT;
    private Image         mainImg;
    private RectTransform ringRT;

    private RectTransform[] trailRT;
    private Image[]          trailImg;
    private Vector2[]        trailPos;

    private Vector2 lastRecorded;

    // ── ciclo de vida ─────────────────────────────────────────
    private void Awake()
    {
        canvas     = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        // Cursor principal
        mainImg = CreateCircle("OsuCursor_Main", cursorSize, cursorColor);
        mainRT  = mainImg.GetComponent<RectTransform>();

        // Anillo exterior
        if (mostrarAnillo && cursorSprite != null)
        {
            var ringImg = CreateCircle("OsuCursor_Ring", ringSize, ringColor);
            ringRT = ringImg.GetComponent<RectTransform>();
            ringImg.type = Image.Type.Simple;
        }

        // Rastro
        trailRT  = new RectTransform[trailCount];
        trailImg = new Image[trailCount];
        trailPos = new Vector2[trailCount];

        for (int i = 0; i < trailCount; i++)
        {
            float t    = (float)i / (trailCount - 1);
            float size = Mathf.Lerp(cursorSize * 0.78f, cursorSize * 0.15f, t);
            Color col  = Color.Lerp(trailColorCerca, trailColorLejos, t);

            trailImg[i] = CreateCircle($"OsuCursor_Trail_{i}", size, col);
            trailRT[i]  = trailImg[i].GetComponent<RectTransform>();
        }

        // Ordenar: rastro al fondo, anillo encima, cursor arriba del todo
        for (int i = 0; i < trailCount; i++)
            trailRT[i].SetAsFirstSibling();
        if (ringRT != null) ringRT.SetAsLastSibling();
        mainRT.SetAsLastSibling();
    }

    private void OnEnable()  => Cursor.visible = false;
    private void OnDisable() => Cursor.visible = true;
    private void OnDestroy() => Cursor.visible = true;

    // ── update ────────────────────────────────────────────────
    private void Update()
    {
        Vector2 mouseScreen = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        Vector2 mouseLocal = ScreenToCanvas(mouseScreen);

        // Mover cursor principal
        mainRT.anchoredPosition = mouseLocal;
        if (ringRT != null) ringRT.anchoredPosition = mouseLocal;

        // Registrar nuevo punto del rastro si el ratón se movió suficiente
        if (Vector2.Distance(mouseLocal, lastRecorded) >= trailSpacing)
        {
            ShiftTrail(mouseLocal);
            lastRecorded = mouseLocal;
        }

        // Actualizar posiciones del rastro
        for (int i = 0; i < trailCount; i++)
            trailRT[i].anchoredPosition = trailPos[i];
    }

    // ── helpers ───────────────────────────────────────────────
    private void ShiftTrail(Vector2 newPos)
    {
        // Desplazar hacia atrás
        for (int i = trailCount - 1; i > 0; i--)
            trailPos[i] = trailPos[i - 1];
        trailPos[0] = newPos;
    }

    private Vector2 ScreenToCanvas(Vector3 screenPos)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 local);
        return local;
    }

    private Image CreateCircle(string goName, float size, Color color)
    {
        var go = new GameObject(goName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot     = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);

        var img = go.GetComponent<Image>();
        img.sprite        = cursorSprite;
        img.color         = color;
        img.raycastTarget = false;
        img.preserveAspect = true;

        return img;
    }
}
