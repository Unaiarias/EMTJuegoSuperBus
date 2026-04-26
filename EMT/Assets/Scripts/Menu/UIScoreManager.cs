using UnityEngine;
using TMPro;

public class UIScoreManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI comboText;

    // Referencia al RectTransform para poder moverlo si es necesario
    private RectTransform scoreTextRect;
    private Transform scoreTextOriginalParent;
    private Vector2 scoreTextOriginalPosition;

    private void Start()
    {
        // Guardar posición original del scoreText
        if (scoreText != null)
        {
            scoreTextRect = scoreText.GetComponent<RectTransform>();
            scoreTextOriginalParent = scoreTextRect.parent;
            scoreTextOriginalPosition = scoreTextRect.anchoredPosition;
        }

        ActualizarUI();
    }

    private void OnEnable()
    {
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.OnScoreChanged += OnScoreChanged;
            SistemaPuntuacion.Instance.OnComboChanged += OnComboChanged;
        }
        ActualizarUI();
    }

    private void OnDisable()
    {
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.OnScoreChanged -= OnScoreChanged;
            SistemaPuntuacion.Instance.OnComboChanged -= OnComboChanged;
        }
    }

    private void OnScoreChanged(int newScore)
    {
        if (scoreText != null)
            scoreText.text = $"Score: {newScore}";
    }

    private void OnComboChanged(int newCombo)
    {
        if (comboText != null)
            comboText.text = newCombo > 0 ? $"x{newCombo}" : "";
    }

    private void ActualizarUI()
    {
        if (SistemaPuntuacion.Instance != null)
        {
            if (scoreText != null)
                scoreText.text = $"Score: {SistemaPuntuacion.Instance.GetScoreActual()}";

            if (comboText != null)
                comboText.text = SistemaPuntuacion.Instance.GetComboActual() > 0 ? $"x{SistemaPuntuacion.Instance.GetComboActual()}" : "";
        }
    }

    // Método para ocultar todo el UIScoreManager
    public void Ocultar()
    {
        gameObject.SetActive(false);
    }

    // Método para mostrar todo el UIScoreManager
    public void Mostrar()
    {
        gameObject.SetActive(true);
    }

    // Método para mover el scoreText a otro padre (para el panel de victoria)
    public void MoverScoreAPanel(RectTransform nuevoPadre, Vector2 nuevaPosicion)
    {
        if (scoreTextRect != null && nuevoPadre != null)
        {
            scoreTextRect.SetParent(nuevoPadre, false);
            scoreTextRect.anchoredPosition = nuevaPosicion;
            scoreTextRect.localScale = Vector3.one;
        }
    }

    // Método para restaurar el scoreText a su posición original
    public void RestaurarScoreOriginal()
    {
        if (scoreTextRect != null && scoreTextOriginalParent != null)
        {
            scoreTextRect.SetParent(scoreTextOriginalParent, false);
            scoreTextRect.anchoredPosition = scoreTextOriginalPosition;
            scoreTextRect.localScale = Vector3.one;
        }
    }
}