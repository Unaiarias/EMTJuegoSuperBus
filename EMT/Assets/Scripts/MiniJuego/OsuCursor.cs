using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Cursor estilo OSU: círculo brillante con rastro que desaparece.
/// Pon este GameObject como hijo del Canvas del minijuego.
/// Asigna note_circle.png (Sprite) en Cursor Sprite.
/// </summary>
public class OsuCursor : MonoBehaviour
{
    [Header("Cursor principal")]
    [SerializeField] private Sprite cursorSprite;
    [SerializeField] private float  cursorSize  = 42f;
    [SerializeField] private Color  cursorColor = new Color(1f, 0.95f, 0.4f, 1f);

    [Header("Anillo exterior")]
    [SerializeField] private bool  mostrarAnillo = true;
    [SerializeField] private float ringSize      = 58f;
    [SerializeField] private Color ringColor     = new Color(1f, 1f, 1f, 0.30f);

    [Header("Rastro")]
    [SerializeField] private int   trailCount    = 40;          // nº de imágenes del rastro
    [SerializeField] private float trailFadeTime = 500f;       // segundos hasta desaparecer
    [SerializeField] private float trailSpacing  = 4f;          // px entre puntos registrados
    [SerializeField] private Color trailColorCerca = new Color(1f, 0.55f, 0.1f, 1f);
    [SerializeField] private Color trailColorLejos = new Color(0.9f, 0.1f, 0.55f, 1f);

    // ── internos ──────────────────────────────────────────────
    private Canvas        canvas;
    private RectTransform canvasRect;

    private RectTransform mainRT;
    private RectTransform ringRT;

    private RectTransform[] trailRT;
    private Image[]          trailImg;

    // historial de posiciones con timestamp
    private Vector2[] trailPos;
    private float[]   trailTime;   // Time.unscaledTime en que se registró cada punto
    private Vector2   lastRecorded;

    // ── ciclo de vida ─────────────────────────────────────────
    private void Awake()
    {
        canvas     = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        // Canvas propio con sortingOrder muy alto → siempre por encima de cualquier panel
        var cv = gameObject.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder    = 500;
        gameObject.AddComponent<GraphicRaycaster>();

        // Cursor principal
        mainRT = CreateCircle("OsuCursor_Main", cursorSize, cursorColor).rectTransform;

        // Anillo
        if (mostrarAnillo)
            ringRT = CreateCircle("OsuCursor_Ring", ringSize, ringColor).rectTransform;

        // Rastro
        trailRT   = new RectTransform[trailCount];
        trailImg  = new Image[trailCount];
        trailPos  = new Vector2[trailCount];
        trailTime = new float[trailCount];

        float now = Time.unscaledTime;
        for (int i = 0; i < trailCount; i++)
        {
            float t    = (float)i / (trailCount - 1);
            float size = Mathf.Lerp(cursorSize * 0.80f, cursorSize * 0.12f, t);
            // color base sin alpha (el alpha lo calculamos cada frame)
            Color col  = Color.Lerp(trailColorCerca, trailColorLejos, t);
            col.a = 0f;

            var img    = CreateCircle($"Trail_{i}", size, col);
            trailImg[i] = img;
            trailRT[i]  = img.rectTransform;
            trailTime[i] = now - trailFadeTime; // empieza invisible
        }

        // Orden Z: rastro al fondo, anillo, cursor encima
        for (int i = trailCount - 1; i >= 0; i--)
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
        if (Mouse.current == null) return;

        Vector2 mouseLocal = ScreenToCanvas(Mouse.current.position.ReadValue());

        // Cursor principal y anillo
        mainRT.anchoredPosition = mouseLocal;
        if (ringRT != null) ringRT.anchoredPosition = mouseLocal;

        // Registrar nuevo punto si el ratón se movió suficiente
        if (Vector2.Distance(mouseLocal, lastRecorded) >= trailSpacing)
        {
            // Desplazar el historial hacia atrás
            for (int i = trailCount - 1; i > 0; i--)
            {
                trailPos[i]  = trailPos[i - 1];
                trailTime[i] = trailTime[i - 1];
            }
            trailPos[0]  = mouseLocal;
            trailTime[0] = Time.unscaledTime;
            lastRecorded = mouseLocal;
        }

        // Actualizar posición y alpha de cada punto del rastro
        float now = Time.unscaledTime;
        for (int i = 0; i < trailCount; i++)
        {
            trailRT[i].anchoredPosition = trailPos[i];

            float age      = now - trailTime[i];
            float alpha    = Mathf.Clamp01(1f - age / trailFadeTime);

            // color base interpolado según posición en el rastro
            float t        = (float)i / (trailCount - 1);
            Color baseColor = Color.Lerp(trailColorCerca, trailColorLejos, t);
            baseColor.a    = alpha * Mathf.Lerp(trailColorCerca.a, trailColorLejos.a, t);

            trailImg[i].color = baseColor;
        }
    }

    // ── helpers ───────────────────────────────────────────────
    private Vector2 ScreenToCanvas(Vector2 screenPos)
    {
        Camera cam = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, cam, out Vector2 local);
        return local;
    }

    private Image CreateCircle(string goName, float size, Color color)
    {
        var go  = new GameObject(goName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(transform, false);

        var rt          = go.GetComponent<RectTransform>();
        rt.anchorMin    = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot        = new Vector2(0.5f, 0.5f);
        rt.sizeDelta    = new Vector2(size, size);

        var img             = go.GetComponent<Image>();
        img.sprite          = cursorSprite;
        img.color           = color;
        img.raycastTarget   = false;
        img.preserveAspect  = true;

        return img;
    }
}
