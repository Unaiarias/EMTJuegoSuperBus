using TMPro;
using UnityEngine;

public class MenuInicioPaneles : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelSeleccionNivel;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelCreditos;

    [Header("Score UI (dentro del panel de selección de nivel)")]
    [SerializeField] private TextMeshProUGUI scorePreviewText;
    [SerializeField] private TextMeshProUGUI highScorePreviewText;

    void Start()
    {
        MostrarPanelPrincipal();

        // Asignar textos del menú al sistema de puntuación
        if (SistemaPuntuacion.Instance != null && scorePreviewText != null && highScorePreviewText != null)
        {
            SistemaPuntuacion.Instance.AsignarTextosMenu(scorePreviewText, highScorePreviewText);
        }

        ActualizarVistaPreviaScore();
    }

    private void OnEnable()
    {
        ActualizarVistaPreviaScore();
    }

    private void ActualizarVistaPreviaScore()
    {
        if (SistemaPuntuacion.Instance != null)
        {
            if (scorePreviewText != null)
                scorePreviewText.text = $"Score: {SistemaPuntuacion.Instance.GetScoreActual()}";

            if (highScorePreviewText != null)
                highScorePreviewText.text = $"Best: {SistemaPuntuacion.Instance.GetHighScore()}";
        }
    }

    // ============ CONTROL DE PANELES ============

    public void MostrarPanelPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelSeleccionNivel.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);
    }

    public void MostrarPanelSeleccionNivel()
    {
        panelPrincipal.SetActive(false);
        panelSeleccionNivel.SetActive(true);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);
        ActualizarVistaPreviaScore();
    }

    public void MostrarPanelOpciones()
    {
        panelPrincipal.SetActive(false);
        panelSeleccionNivel.SetActive(false);
        panelOpciones.SetActive(true);
        panelCreditos.SetActive(false);
    }

    public void MostrarPanelCreditos()
    {
        panelPrincipal.SetActive(false);
        panelSeleccionNivel.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(true);
    }
}