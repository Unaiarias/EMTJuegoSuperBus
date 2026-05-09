using TMPro;
using UnityEngine;

public class MenuInicioPaneles : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelMapa; // Panel con los iconos de niveles
    [SerializeField] private GameObject panelXativa; // Panel específico de Xativa
    [SerializeField] private GameObject panelPoble; // Panel específico de Poble
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelCreditos;

    [Header("Textos HighScore de cada nivel")]
    [SerializeField] private TextMeshProUGUI highScoreXativaText;
    [SerializeField] private TextMeshProUGUI highScorePobleText;

    void Start()
    {
        MostrarPanelPrincipal();
        CargarTodosLosHighScores();
    }

    private void CargarTodosLosHighScores()
    {
        // Cargar highscore de Xativa
        int highScoreXativa = PlayerPrefs.GetInt("HighScore_Xativa", 0);
        if (highScoreXativaText != null)
            highScoreXativaText.text = $"Best: {highScoreXativa}";

        // Cargar highscore de Poble
        int highScorePoble = PlayerPrefs.GetInt("HighScore_Poble", 0);
        if (highScorePobleText != null)
            highScorePobleText.text = $"Best: {highScorePoble}";

        Debug.Log($"?? HighScores cargados - Xativa: {highScoreXativa}, Poble: {highScorePoble}");
    }

    // ============ MÉTODOS PARA ABRIR PANELES DE NIVEL ============

    public void AbrirPanelXativa()
    {
        panelMapa.SetActive(false);
        panelXativa.SetActive(true);

        // Asegurar que el highscore se muestre correctamente
        int highScore = PlayerPrefs.GetInt("HighScore_Xativa", 0);
        if (highScoreXativaText != null)
            highScoreXativaText.text = $"Best: {highScore}";
    }

    public void AbrirPanelPoble()
    {
        panelMapa.SetActive(false);
        panelPoble.SetActive(true);

        // Asegurar que el highscore se muestre correctamente
        int highScore = PlayerPrefs.GetInt("HighScore_Poble", 0);
        if (highScorePobleText != null)
            highScorePobleText.text = $"Best: {highScore}";
    }

    // ============ MÉTODOS PARA JUGAR ============

    public void JugarXativa()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
        {
            menuInicio.EmpezarNivelXativa1();
        }
    }

    public void JugarPoble()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
        {
            menuInicio.EmpezarNivelPoble1();
        }
    }

    // ============ MÉTODOS PARA VOLVER ============

    public void VolverAlMapa()
    {
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelMapa.SetActive(true);

        // Recargar highscores por si cambiaron
        CargarTodosLosHighScores();
    }

    // ============ CONTROL DE PANELES ============

    public void MostrarPanelPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);
    }

    public void MostrarPanelOpciones()
    {
        panelPrincipal.SetActive(false);
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelOpciones.SetActive(true);
        panelCreditos.SetActive(false);
    }

    public void MostrarPanelCreditos()
    {
        panelPrincipal.SetActive(false);
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        MostrarPanelPrincipal();
    }
}