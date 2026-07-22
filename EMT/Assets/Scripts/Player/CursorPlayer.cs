using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using System.Collections;

public class CursorPlayer : MonoBehaviour
{
    [Header("Cursor principal")]
    [SerializeField] private Sprite cursorSprite;
    [SerializeField] private float cursorSize = 42f;
    [SerializeField] private Color cursorColorNormal = new Color(1f, 0.95f, 0.4f, 1f);

    [Header("Feedback de Golpeo")]
    [SerializeField] private Color cursorColorHit = new Color(0.2f, 1f, 0.3f, 1f);
    [SerializeField] private Color cursorColorMiss = new Color(1f, 0.2f, 0.2f, 1f);
    [SerializeField] private float feedbackDuration = 0.3f;

    [Header("Anillo exterior")]
    [SerializeField] private bool mostrarAnillo = true;
    [SerializeField] private float ringSize = 58f;
    [SerializeField] private Color ringColorNormal = new Color(1f, 1f, 1f, 0.30f);
    [SerializeField] private Color ringColorHit = new Color(0.2f, 1f, 0.3f, 0.5f);
    [SerializeField] private Color ringColorMiss = new Color(1f, 0.2f, 0.2f, 0.5f);

    [Header("Escala de Feedback")]
    [SerializeField] private float scaleHit = 1.4f;
    [SerializeField] private float scaleMiss = 0.7f;
    [SerializeField] private float scaleDuration = 0.15f;

    // ?? internos ??????????????????????????????????????????????
    private Canvas canvas;
    private RectTransform canvasRect;
    private RectTransform mainRT;
    private RectTransform ringRT;
    private Image mainImage;
    private Image ringImage;

    private Coroutine feedbackCoroutine;
    private Coroutine scaleCoroutine;

    // ?? ciclo de vida ?????????????????????????????????????????
    private void Awake()
    {
        // Ocultar cursor de Windows SIEMPRE
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Debug.Log("Cursor de Windows ocultado en Awake");

        canvas = GetComponentInParent<Canvas>();
        canvasRect = canvas.GetComponent<RectTransform>();

        // Canvas propio con sortingOrder muy alto
        var cv = gameObject.AddComponent<Canvas>();
        cv.overrideSorting = true;
        cv.sortingOrder = 500;
        gameObject.AddComponent<GraphicRaycaster>();

        // Cursor principal
        mainImage = CreateCircle("CursorPlayer_Main", cursorSize, cursorColorNormal);
        mainRT = mainImage.rectTransform;

        // Anillo
        if (mostrarAnillo)
        {
            ringImage = CreateCircle("CursorPlayer_Ring", ringSize, ringColorNormal);
            ringRT = ringImage.rectTransform;
        }

        // Orden Z: anillo atrás, cursor encima
        if (ringRT != null) ringRT.SetAsFirstSibling();
        mainRT.SetAsLastSibling();

        // Suscribirse al evento de ataque del PlayerAtaque
        PlayerAtaque playerAtaque = FindObjectOfType<PlayerAtaque>();
        if (playerAtaque != null)
        {
            playerAtaque.OnAtaqueRealizado += OnAtaqueRealizado;
            Debug.Log("CursorPlayer suscrito a eventos de PlayerAtaque");
        }
        else
        {
            Debug.LogWarning("No se encontró PlayerAtaque en la escena");
        }
    }

    private void OnEnable()
    {
        // Asegurar que el cursor de Windows está oculto SIEMPRE al activar
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
        Debug.Log("CursorPlayer activado - Cursor de Windows oculto");
    }

    private void OnDisable()
    {
        // Cuando el cursor se desactiva, NO mostramos el cursor de Windows
        // Lo dejamos oculto para que no aparezca nunca
        Cursor.visible = false;
        Debug.Log("CursorPlayer desactivado - Cursor de Windows se mantiene oculto");
    }

    private void OnDestroy()
    {
        // Solo al destruir completamente mostramos el cursor
        Cursor.visible = true;

        // Limpiar suscripción
        PlayerAtaque playerAtaque = FindObjectOfType<PlayerAtaque>();
        if (playerAtaque != null)
        {
            playerAtaque.OnAtaqueRealizado -= OnAtaqueRealizado;
        }
    }

    // ?? update ????????????????????????????????????????????????
    private void Update()
    {
        // Asegurar cada frame que el cursor de Windows está oculto
        // (por si algún otro script lo muestra)
        if (Cursor.visible)
        {
            Cursor.visible = false;
        }

        if (Mouse.current == null) return;

        Vector2 mouseLocal = ScreenToCanvas(Mouse.current.position.ReadValue());

        // Cursor principal y anillo
        mainRT.anchoredPosition = mouseLocal;
        if (ringRT != null) ringRT.anchoredPosition = mouseLocal;
    }

    // ?? eventos de PlayerAtaque ??????????????????????????????
    private void OnAtaqueRealizado(bool impacto)
    {
        // Cancelar feedback anterior
        if (feedbackCoroutine != null) StopCoroutine(feedbackCoroutine);
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);

        // Iniciar nuevo feedback
        feedbackCoroutine = StartCoroutine(FeedbackCoroutine(impacto));
        scaleCoroutine = StartCoroutine(ScaleCoroutine(impacto));
    }

    // ?? corutinas de feedback ????????????????????????????????
    private IEnumerator FeedbackCoroutine(bool impacto)
    {
        Color mainFeedback = impacto ? cursorColorHit : cursorColorMiss;
        Color ringFeedback = impacto ? ringColorHit : ringColorMiss;

        mainImage.color = mainFeedback;
        if (ringImage != null) ringImage.color = ringFeedback;

        yield return new WaitForSecondsRealtime(feedbackDuration);

        mainImage.color = cursorColorNormal;
        if (ringImage != null) ringImage.color = ringColorNormal;

        feedbackCoroutine = null;
    }

    private IEnumerator ScaleCoroutine(bool impacto)
    {
        float targetScale = impacto ? scaleHit : scaleMiss;
        float duration = scaleDuration;

        // Animación de escala - Subida
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            float currentScale = Mathf.Lerp(1f, targetScale, smoothT);
            mainRT.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }

        // Animación de escala - Bajada
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / duration;
            float smoothT = t * t * (3f - 2f * t);
            float currentScale = Mathf.Lerp(targetScale, 1f, smoothT);
            mainRT.localScale = new Vector3(currentScale, currentScale, 1f);
            yield return null;
        }

        mainRT.localScale = Vector3.one;
        scaleCoroutine = null;
    }

    // ?? helpers ???????????????????????????????????????????????
    private Vector2 ScreenToCanvas(Vector2 screenPos)
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
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(size, size);

        var img = go.GetComponent<Image>();
        img.sprite = cursorSprite;
        img.color = color;
        img.raycastTarget = false;
        img.preserveAspect = true;

        return img;
    }
}