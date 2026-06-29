using TMPro;
using UnityEngine;

public class MenuInicioPaneles : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelMapa; // Panel con los iconos de niveles
    [SerializeField] private GameObject panelHistoria;
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

    // Textos Título de cada nivel
    [Header("Textos Título de cada nivel")]
    [SerializeField] private TextMeshProUGUI tituloXativaText;
    [SerializeField] private TextMeshProUGUI tituloPobleText;
    [SerializeField] private TextMeshProUGUI tituloTorresText;
    [SerializeField] private TextMeshProUGUI tituloMercatText;
    [SerializeField] private TextMeshProUGUI tituloEstacionText;

    // Colores para los títulos 
    [Header("Colores")]
    [SerializeField] private Color colorCompletado = Color.green;
    [SerializeField] private Color colorNoCompletado = Color.white;


    void Start()
    {
        MostrarPanelPrincipal();
        CargarTodosLosHighScores();
        ActualizarColoresNivelesCompletados();
        
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

    // Método público para actualizar todos los colores
    public void ActualizarColoresNivelesCompletados()
    {
        ActualizarColorTitulo(tituloXativaText, "NivelCompletado_Xativa");
        ActualizarColorTitulo(tituloPobleText, "NivelCompletado_Poble");
        ActualizarColorTitulo(tituloTorresText, "NivelCompletado_Torres");
        ActualizarColorTitulo(tituloMercatText, "NivelCompletado_Mercat");
        ActualizarColorTitulo(tituloEstacionText, "NivelCompletado_Estacion");
    }

    // Método privado para actualizar un título individual
    private void ActualizarColorTitulo(TextMeshProUGUI texto, string clavePlayerPrefs)
    {
        if (texto != null)
        {
            bool completado = PlayerPrefs.GetInt(clavePlayerPrefs, 0) == 1;
            texto.color = completado ? colorCompletado : colorNoCompletado;
        }
    }

    // Método estático para marcar un nivel como completado (se llama desde MenuInicio)
    public static void MarcarNivelComoCompletado(string nombreNivel)
    {
        string clave = $"NivelCompletado_{nombreNivel}";
        PlayerPrefs.SetInt(clave, 1);
        PlayerPrefs.Save();
        Debug.Log($"?? Nivel {nombreNivel} marcado como completado en PlayerPrefs");
    }

    // Método estático para reiniciar todos los niveles (opcional, para pruebas)
    public static void ReiniciarTodosLosNiveles()
    {
        PlayerPrefs.DeleteKey("NivelCompletado_Xativa");
        PlayerPrefs.DeleteKey("NivelCompletado_Poble");
        PlayerPrefs.DeleteKey("NivelCompletado_Torres");
        PlayerPrefs.DeleteKey("NivelCompletado_Mercat");
        PlayerPrefs.DeleteKey("NivelCompletado_Estacion");
        PlayerPrefs.Save();
        Debug.Log("?? Todos los niveles reiniciados");
    }
    // ===== FIN NUEVO =====

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

        // ===== NUEVO: Actualizar colores al abrir panel =====
        ActualizarColoresNivelesCompletados();
        // ===== FIN NUEVO =====
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

        // ===== NUEVO: Actualizar colores al abrir panel =====
        ActualizarColoresNivelesCompletados();
        // ===== FIN NUEVO =====
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

        // ===== NUEVO: Actualizar colores al abrir panel =====
        ActualizarColoresNivelesCompletados();
        // ===== FIN NUEVO =====
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

        // ===== NUEVO: Actualizar colores al abrir panel =====
        ActualizarColoresNivelesCompletados();
        // ===== FIN NUEVO =====
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

        // ===== NUEVO: Actualizar colores al abrir panel =====
        ActualizarColoresNivelesCompletados();
        // ===== FIN NUEVO =====
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
        ActualizarColoresNivelesCompletados();
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

        ActualizarColoresNivelesCompletados();
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