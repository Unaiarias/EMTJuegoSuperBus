using TMPro;
using UnityEngine;

public class MenuInicioPaneles : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelMapa; // Panel con los iconos de niveles
    [SerializeField] private GameObject panelXativa; // Panel específico de Xativa
    [SerializeField] private GameObject panelPoble; // Panel específico de Poble
    [SerializeField] private GameObject panelTorres; // Panel específico de Torres 
    [SerializeField] private GameObject panelMercat; // Panel específico de Mercat
    [SerializeField] private GameObject panelEstacion; // Panel específico de Estacion 
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelCreditos;

    [Header("Textos HighScore de cada nivel")]
    [SerializeField] private TextMeshProUGUI highScoreXativaText;
    [SerializeField] private TextMeshProUGUI highScorePobleText;
    [SerializeField] private TextMeshProUGUI highScoreTorresText;
    [SerializeField] private TextMeshProUGUI highScoreMercatText;
    [SerializeField] private TextMeshProUGUI highScoreEstacionText; 

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

        // Cargar highscore de Torres 
        int highScoreTorres = PlayerPrefs.GetInt("HighScore_Torres", 0);
        if (highScoreTorresText != null)
            highScoreTorresText.text = $"Best: {highScoreTorres}";

        // Cargar highscore de Mercat 
        int highScoreMercat = PlayerPrefs.GetInt("HighScore_Mercat", 0);
        if (highScoreMercatText != null)
            highScoreMercatText.text = $"Best: {highScoreMercat}";

        // Cargar highscore de Estacion 
        int highScoreEstacion = PlayerPrefs.GetInt("HighScore_Estacion", 0);
        if (highScoreEstacionText != null)
            highScoreEstacionText.text = $"Best: {highScoreEstacion}";

        Debug.Log($"?? HighScores cargados - Xativa: {highScoreXativa}, Poble: {highScorePoble}, Torres: {highScoreTorres}, Mercat: {highScoreMercat}, Estacion: {highScoreEstacion}");
    }

    // ============ MÉTODOS PARA ABRIR PANELES DE NIVEL ============

    public void AbrirPanelXativa()
    {
        panelMapa.SetActive(false);
        panelXativa.SetActive(true);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);

        int highScore = PlayerPrefs.GetInt("HighScore_Xativa", 0);
        if (highScoreXativaText != null)
            highScoreXativaText.text = $"Best: {highScore}";
    }

    public void AbrirPanelPoble()
    {
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(true);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);

        int highScore = PlayerPrefs.GetInt("HighScore_Poble", 0);
        if (highScorePobleText != null)
            highScorePobleText.text = $"Best: {highScore}";
    }

    public void AbrirPanelTorres()
    {
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(true);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);

        int highScore = PlayerPrefs.GetInt("HighScore_Torres", 0);
        if (highScoreTorresText != null)
            highScoreTorresText.text = $"Best: {highScore}";
    }

    public void AbrirPanelMercat()
    {
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(true);
        panelEstacion.SetActive(false);

        int highScore = PlayerPrefs.GetInt("HighScore_Mercat", 0);
        if (highScoreMercatText != null)
            highScoreMercatText.text = $"Best: {highScore}";
    }

    public void AbrirPanelEstacion() 
    {
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(true);

        int highScore = PlayerPrefs.GetInt("HighScore_Estacion", 0);
        if (highScoreEstacionText != null)
            highScoreEstacionText.text = $"Best: {highScore}";
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

    public void JugarTorres()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
        {
            menuInicio.EmpezarNivelTorres1();
        }
    }

    public void JugarMercat()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
        {
            menuInicio.EmpezarNivelMercat1();
        }
    }

    public void JugarEstacion() // NUEVO
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
        {
            menuInicio.EmpezarNivelEstacion1();
        }
    }

    // ============ MÉTODOS PARA VOLVER ============

    public void VolverAlMapa()
    {
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);
        panelMapa.SetActive(true);

        CargarTodosLosHighScores();
    }

    // ============ CONTROL DE PANELES ============

    public void MostrarPanelPrincipal()
    {
        panelPrincipal.SetActive(true);
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(false);
    }

    public void MostrarPanelOpciones()
    {
        panelPrincipal.SetActive(false);
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);
        panelOpciones.SetActive(true);
        panelCreditos.SetActive(false);
    }

    public void MostrarPanelCreditos()
    {
        panelPrincipal.SetActive(false);
        panelMapa.SetActive(false);
        panelXativa.SetActive(false);
        panelPoble.SetActive(false);
        panelTorres.SetActive(false);
        panelMercat.SetActive(false);
        panelEstacion.SetActive(false);
        panelOpciones.SetActive(false);
        panelCreditos.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        MostrarPanelPrincipal();
    }
}