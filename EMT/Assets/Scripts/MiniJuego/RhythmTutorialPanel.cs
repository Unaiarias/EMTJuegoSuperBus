using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pantalla de tutorial para el minijuego de ritmo.
///
/// Setup:
///   1. GameObject vacío hijo del Canvas → añadir este script.
///   2. Inspector:
///      · Note Sprite            → note_circle (Sprite)
///      · Boton Start            → botonStart del RhythmGameManager
///      · Boton Sprite           → Boton_nuevo
///      · Ocultar Durante Tutorial → BotonEscape, BotonSaltar, etc.
///
/// Para bloquear el escape por teclado en otro script:
///      if (RhythmTutorialPanel.Activo) return;
/// </summary>
public class RhythmTutorialPanel : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Sprite     noteSprite;
    [SerializeField] private GameObject botonStart;

    [Header("Botón Entendido")]
    [SerializeField] private Sprite botonSprite;   // Boton_nuevo

    [Header("Objetos a ocultar durante el tutorial")]
    [Tooltip("Arrastra aquí: BotonEscape, BotonSaltar, BotonPausa, etc.")]
    [SerializeField] private GameObject[] objetosAOcultar;

    // colores fijos por tipo de nota
    private static readonly Color ColorTap     = new Color(1f,    1f,    1f,    1f);
    private static readonly Color ColorDrag    = new Color(1f,    0.85f, 0.2f,  1f);
    private static readonly Color ColorInstant = new Color(1f,    0.18f, 0.18f, 1f);

    /// <summary>True mientras el tutorial esté abierto. Úsalo en otros scripts para bloquear el escape.</summary>
    public static bool Activo { get; private set; }

    // refs internas
    private GameObject    panelRoot;
    private Image         tapRingImg;
    private RectTransform dragArrowRT;
    private RectTransform instanteNoteRT;
    private bool          animando = false;

    // ═══════════════════════════════════════════════════
    private void Awake()
    {
        Activo = false;

        // Pantalla completa
        var rt = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;

        // Canvas propio sortingOrder 200 → por encima de la escena
        var cv = gameObject.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder    = 200;
        gameObject.AddComponent<GraphicRaycaster>();
    }

    private void Start()
    {
        // Construir la UI pero mantenerla oculta hasta que el jugador pulse TUTORIAL
        BuildUI();
        panelRoot.SetActive(false);
    }

    /// <summary>
    /// Llama a este método desde el botón TUTORIAL de la escena.
    /// </summary>
    public void MostrarTutorial()
    {
        Activo   = true;
        animando = true;

        foreach (var obj in objetosAOcultar)
            if (obj != null) obj.SetActive(false);

        if (botonStart != null) botonStart.SetActive(false);

        panelRoot.SetActive(true);

        StartCoroutine(AnimateTapRing());
        StartCoroutine(AnimateDragArrow());
        StartCoroutine(AnimateInstante());
    }

    // ═══════════════════════════════════════════════════
    //  BUILD UI
    // ═══════════════════════════════════════════════════
    private void BuildUI()
    {
        // Fondo negro puro OPACO
        panelRoot = CreateStretch("TutorialRoot", transform);
        var bg = panelRoot.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 1f);
        bg.raycastTarget = true;

        Transform p = panelRoot.transform;

        // ── Título ───────────────────────────────────────────
        MakeText("Titulo", p, 0f, 270f, 1100f, 85f,
            "¿Cómo se juega?", 68, FontStyles.Bold,
            new Color(1f, 0.92f, 0.3f, 1f));

        MakeText("Subtitulo", p, 0f, 195f, 980f, 46f,
            "Hay tres tipos de notas que tienes que completar:", 26,
            FontStyles.Normal, new Color(0.68f, 0.68f, 0.68f, 1f));

        // ── Cards ────────────────────────────────────────────
        // H grande para que el icono, título y descripción no se pisen
        const float W = 320f, H = 390f;
        const float cardY = -55f, gap = 355f;

        var tapRefs  = BuildCard(p, -gap, cardY, W, H,
            ColorTap,     new Color(1f,1f,1f,0.55f), drawRing:true,  arrow:"",
            "TAP",        "Pulsa el círculo cuando\nel anillo se haya cerrado");

        var dragRefs = BuildCard(p,     0, cardY, W, H,
            ColorDrag,    ColorDrag,                  drawRing:true,  arrow:"→",
            "ARRASTRE",   "Pulsa y arrastra\nen la dirección\nde la flecha");

        var instRefs = BuildCard(p, +gap, cardY, W, H,
            ColorInstant, Color.clear,                drawRing:false, arrow:"",
            "¡INSTANTE!",  "¡Pulsa en cuanto aparezca!\nSe va muy rápido");

        tapRingImg     = tapRefs.ring;
        dragArrowRT    = dragRefs.arrowRT;
        instanteNoteRT = instRefs.noteRT;

        BuildBoton(p, cardY, H);
    }

    // ═══════════════════════════════════════════════════
    //  CARD
    // ═══════════════════════════════════════════════════
    private struct CardRefs
    {
        public Image ring;
        public RectTransform arrowRT, noteRT;
    }

    private CardRefs BuildCard(Transform p, float cx, float cy, float w, float h,
        Color noteColor, Color ringColor, bool drawRing, string arrow,
        string titulo, string desc)
    {
        var refs = new CardRefs();

        // Fondo card — un poco más visible
        MakeRect("Card_bg", p, cx, cy, w, h).AddComponent<Image>().color =
            new Color(1f, 1f, 1f, 0.10f);

        // El icono ocupa el tercio superior de la card
        float noteSize = 88f;
        float noteY    = cy + h * 0.5f - 100f;   // bien dentro del borde superior

        // Anillo approach — más pequeño para no comerse el título
        if (drawRing && noteSprite != null)
        {
            float rs = noteSize * 1.65f;          // 1.65x en vez de 2.15x
            var ri = MakeRect("Ring", p, cx, noteY, rs, rs).AddComponent<Image>();
            ri.sprite = noteSprite; ri.preserveAspect = true; ri.raycastTarget = false;
            ri.color  = new Color(ringColor.r, ringColor.g, ringColor.b, 0.50f);
            refs.ring = ri;
        }

        // Círculo nota
        var nGo = MakeRect("Note", p, cx, noteY, noteSize, noteSize);
        var ni  = nGo.AddComponent<Image>();
        ni.sprite = noteSprite; ni.color = noteColor;
        ni.preserveAspect = true; ni.raycastTarget = false;
        refs.noteRT = nGo.GetComponent<RectTransform>();

        // Flecha (solo DRAG)
        if (!string.IsNullOrEmpty(arrow))
        {
            var aGo = MakeRect("Arrow", p, cx, noteY, noteSize, noteSize);
            var at  = aGo.AddComponent<TextMeshProUGUI>();
            at.text = arrow; at.fontSize = 58; at.fontStyle = FontStyles.Bold;
            at.color = Color.white; at.alignment = TextAlignmentOptions.Center;
            at.raycastTarget = false;
            refs.arrowRT = aGo.GetComponent<RectTransform>();
        }

        // Título card — separado del icono (más abajo)
        float titleY = cy - 20f;
        MakeText("CardTitle", p, cx, titleY, w - 12f, 46f,
            titulo, 28, FontStyles.Bold, noteColor);

        // Descripción — en el tercio inferior de la card
        float descY = cy - h * 0.5f + 85f;
        MakeText("CardDesc", p, cx, descY, w - 24f, 110f,
            desc, 21, FontStyles.Normal, new Color(0.80f, 0.80f, 0.80f, 1f));

        return refs;
    }

    // ═══════════════════════════════════════════════════
    //  BOTÓN ENTENDIDO
    // ═══════════════════════════════════════════════════
    private void BuildBoton(Transform p, float cardY, float cardH)
    {
        float bW = 420f, bH = 85f;
        float bY  = cardY - cardH * 0.5f - 58f;  // debajo de las cards con margen generoso

        var btnGo = MakeRect("BotonEntendido", p, 0f, bY, bW, bH);

        var img = btnGo.AddComponent<Image>();
        if (botonSprite != null)
        {
            img.sprite = botonSprite;
            img.type   = Image.Type.Sliced;
            img.color  = Color.white;
        }
        else
        {
            img.color = new Color(0.2f, 0.78f, 0.35f, 1f);
        }

        var btn = btnGo.AddComponent<Button>();
        var cols = btn.colors;
        cols.highlightedColor = new Color(0.88f, 1f, 0.88f, 1f);
        cols.pressedColor     = new Color(0.55f, 0.88f, 0.55f, 1f);
        btn.colors = cols;
        btn.onClick.AddListener(OnEntendido);

        MakeText("Label", btnGo.transform, 0f, 0f, bW, bH,
            "¡Entendido!", 32, FontStyles.Bold,
            botonSprite != null ? Color.black : Color.white,
            stretch: true);
    }

    // ═══════════════════════════════════════════════════
    //  CERRAR
    // ═══════════════════════════════════════════════════
    private void OnEntendido()
    {
        Activo   = false;
        animando = false;
        StopAllCoroutines();

        foreach (var obj in objetosAOcultar)
            if (obj != null) obj.SetActive(true);

        if (botonStart != null) botonStart.SetActive(true);
        if (panelRoot  != null) panelRoot.SetActive(false);
    }

    // ═══════════════════════════════════════════════════
    //  ANIMACIONES
    // ═══════════════════════════════════════════════════

    private IEnumerator AnimateTapRing()
    {
        if (tapRingImg == null) yield break;
        var rt = tapRingImg.GetComponent<RectTransform>();
        float big = rt.sizeDelta.x, small = big * 0.42f;
        while (animando)
        {
            float e = 0f, d = 1.3f;
            while (e < d)
            {
                e += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(e / d);
                rt.sizeDelta = Vector2.one * Mathf.Lerp(big, small, t);
                var c = tapRingImg.color; c.a = Mathf.Lerp(0.18f, 0.88f, t); tapRingImg.color = c;
                yield return null;
            }
            yield return new WaitForSecondsRealtime(0.1f);
            rt.sizeDelta = Vector2.one * big;
            var c2 = tapRingImg.color; c2.a = 0.12f; tapRingImg.color = c2;
            yield return new WaitForSecondsRealtime(0.08f);
        }
    }

    private IEnumerator AnimateDragArrow()
    {
        if (dragArrowRT == null) yield break;
        Vector2 orig = dragArrowRT.anchoredPosition;
        while (animando)
        {
            float e = 0f, d = 0.3f;
            while (e < d) { e += Time.unscaledDeltaTime; dragArrowRT.anchoredPosition = orig + new Vector2(Mathf.Lerp(0,22,EaseOut(e/d)), 0); yield return null; }
            yield return new WaitForSecondsRealtime(0.07f);
            e = 0f; d *= 0.5f;
            while (e < d) { e += Time.unscaledDeltaTime; dragArrowRT.anchoredPosition = orig + new Vector2(Mathf.Lerp(22,0,EaseIn(e/d)), 0); yield return null; }
            dragArrowRT.anchoredPosition = orig;
            yield return new WaitForSecondsRealtime(0.55f);
        }
    }

    private IEnumerator AnimateInstante()
    {
        if (instanteNoteRT == null) yield break;
        while (animando)
        {
            float e = 0f, d = 0.17f;
            while (e < d) { e += Time.unscaledDeltaTime; instanteNoteRT.localScale = Vector3.one * Mathf.Lerp(1f, 1.4f, EaseOut(e/d)); yield return null; }
            e = 0f;
            while (e < d) { e += Time.unscaledDeltaTime; instanteNoteRT.localScale = Vector3.one * Mathf.Lerp(1.4f, 1f, EaseIn(e/d)); yield return null; }
            yield return new WaitForSecondsRealtime(0.5f);
        }
    }

    // ═══════════════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════════════

    private GameObject MakeRect(string name, Transform parent, float cx, float cy, float w, float h)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(cx, cy);
        rt.sizeDelta        = new Vector2(w, h);
        return go;
    }

    private GameObject CreateStretch(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return go;
    }

    private TMP_Text MakeText(string name, Transform parent,
        float cx, float cy, float w, float h,
        string text, int fontSize, FontStyles style, Color color, bool stretch = false)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        if (stretch)
        {
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        }
        else
        {
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = new Vector2(cx, cy);
            rt.sizeDelta        = new Vector2(w, h);
        }
        var tmp = go.GetComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = fontSize; tmp.fontStyle = style;
        tmp.color = color; tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false; tmp.enableWordWrapping = true;
        return tmp;
    }

    private static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseIn(float t)  => t * t;
}
