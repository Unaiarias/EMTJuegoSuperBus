// Assets/Scripts/MiniJuego/NoteHitEffect.cs
// ─────────────────────────────────────────────────────────────────────────────
// Efecto visual de onda/ripple cuando se golpea o falla una nota.
//
// CÓMO USARLO:
//  1. Crea un GameObject vacío dentro del Canvas del minijuego. Llámalo "HitEffectSpawner".
//  2. Añádele este componente NoteHitEffect.
//  3. Asigna en el Inspector el prefab del ripple (ver instrucciones abajo) o
//     deja hitRipplePrefab en null y el script crea un ripple básico proceduralmente.
//
// PREFAB RECOMENDADO (si quieres uno bonito):
//  - Crea un Image (círculo, sprite circular blanco con transparencia).
//  - Añade un CanvasGroup para controlar el alpha.
//  - Guárdalo como Prefab en Assets/Prefabs/MiniJuego/HitRipple.prefab.
//  - Asígnalo al campo hitRipplePrefab.
//
// LLAMARLO DESDE RhythmGameManager:
//   NoteHitEffect.Instance?.SpawnHit(note.transform.position, isPerfect);
//   NoteHitEffect.Instance?.SpawnMiss(note.transform.position);
// ─────────────────────────────────────────────────────────────────────────────

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class NoteHitEffect : MonoBehaviour
{
    public static NoteHitEffect Instance { get; private set; }

    [Header("Prefab (opcional)")]
    [Tooltip("Prefab de la onda. Si es null se crea un círculo básico proceduralmente.")]
    [SerializeField] private GameObject hitRipplePrefab;

    [Header("Colores")]
    [SerializeField] private Color colorPerfect = new Color(1f, 0.95f, 0.2f, 0.85f);
    [SerializeField] private Color colorGood    = new Color(0.3f, 1f, 0.4f, 0.75f);
    [SerializeField] private Color colorMiss    = new Color(1f, 0.2f, 0.2f, 0.70f);

    [Header("Animación")]
    [SerializeField] private float rippleStartSize  = 60f;   // tamaño inicial (px)
    [SerializeField] private float rippleEndSize    = 220f;  // tamaño final (px)
    [SerializeField] private float rippleDuration   = 0.40f; // duración en segundos
    [SerializeField] private int   poolSize         = 16;    // pool de ondas

    // ── Pool ────────────────────────────────────────────────────────────────
    private readonly Queue<RippleInstance> pool = new();
    private RectTransform parentRect;

    // ═══════════════════════════════════════════════════════════════════════
    //  LIFECYCLE
    // ═══════════════════════════════════════════════════════════════════════

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance   = this;
        parentRect = (RectTransform)transform;

        // Prellenar el pool
        for (int i = 0; i < poolSize; i++)
            pool.Enqueue(CreateRipple());
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  API PÚBLICA
    // ═══════════════════════════════════════════════════════════════════════

    /// <summary>Dispara un efecto de hit en la posición de pantalla dada.</summary>
    /// <param name="worldOrScreenPos">Posición (world space o screen space).</param>
    /// <param name="isPerfect">True → color dorado; False → color verde.</param>
    public void SpawnHit(Vector3 worldOrScreenPos, bool isPerfect)
    {
        Color c = isPerfect ? colorPerfect : colorGood;
        Spawn(worldOrScreenPos, c);
    }

    /// <summary>Dispara un efecto de miss en la posición de pantalla dada.</summary>
    public void SpawnMiss(Vector3 worldOrScreenPos)
    {
        Spawn(worldOrScreenPos, colorMiss);
    }

    // ─── Sobrecarga para pasar la posición de un RectTransform ─────────────

    public void SpawnHit(RectTransform noteRect, bool isPerfect)
    {
        if (noteRect == null) return;
        SpawnHit(noteRect.position, isPerfect);
    }

    public void SpawnMiss(RectTransform noteRect)
    {
        if (noteRect == null) return;
        SpawnMiss(noteRect.position);
    }

    // ═══════════════════════════════════════════════════════════════════════
    //  INTERNAL
    // ═══════════════════════════════════════════════════════════════════════

    private void Spawn(Vector3 worldPos, Color color)
    {
        if (pool.Count == 0) return;

        var ripple = pool.Dequeue();

        // Convertir posición world → local del parentRect
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect, RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null, out Vector2 localPos))
        {
            ripple.rect.anchoredPosition = localPos;
        }

        ripple.rect.gameObject.SetActive(true);
        StartCoroutine(AnimateRipple(ripple, color));
    }

    private IEnumerator AnimateRipple(RippleInstance ripple, Color color)
    {
        var img = ripple.image;
        if (img != null) img.color = color;

        float elapsed = 0f;

        while (elapsed < rippleDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / rippleDuration);

            // Tamaño: lineal de start a end
            float size = Mathf.Lerp(rippleStartSize, rippleEndSize, t);
            ripple.rect.sizeDelta = Vector2.one * size;

            // Alpha: fade out cuadrático
            float alpha = Mathf.Lerp(color.a, 0f, t * t);
            if (img != null)
            {
                Color c = img.color;
                c.a     = alpha;
                img.color = c;
            }

            yield return null;
        }

        ripple.rect.gameObject.SetActive(false);
        pool.Enqueue(ripple);
    }

    // ─── Crear instancia en el pool ─────────────────────────────────────────

    private RippleInstance CreateRipple()
    {
        GameObject go;

        if (hitRipplePrefab != null)
        {
            go = Instantiate(hitRipplePrefab, parentRect);
        }
        else
        {
            // Círculo procedimental básico
            go = new GameObject("Ripple", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parentRect, false);

            var img    = go.GetComponent<Image>();
            img.color  = Color.white;

            // Intentar usar el sprite Built-in "UISprite" si existe, si no quedará cuadrado
            // (Para que sea círculo perfecto asigna un sprite circular en hitRipplePrefab)
            img.type = Image.Type.Simple;
            img.raycastTarget = false;
        }

        var rect = (RectTransform)go.transform;
        rect.sizeDelta = Vector2.one * rippleStartSize;
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot     = new Vector2(0.5f, 0.5f);

        go.SetActive(false);

        return new RippleInstance
        {
            rect  = rect,
            image = go.GetComponent<Image>()
        };
    }

    // ─── Struct auxiliar ────────────────────────────────────────────────────

    private struct RippleInstance
    {
        public RectTransform rect;
        public Image         image;
    }
}
