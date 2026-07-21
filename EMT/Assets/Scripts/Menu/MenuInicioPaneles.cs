using TMPro;
using UnityEngine;

public class MenuInicioPaneles : MonoBehaviour
{
    [Header("Paneles")]
    [SerializeField] private GameObject panelPrincipal;
    [SerializeField] private GameObject panelMapa;
    [SerializeField] private GameObject panelHistoria;
    [SerializeField] private GameObject panelXativa;
    [SerializeField] private GameObject panelPoble;
    [SerializeField] private GameObject panelTorres;
    [SerializeField] private GameObject panelMercat;
    [SerializeField] private GameObject panelEstacion;
    [SerializeField] private GameObject panelOpciones;
    [SerializeField] private GameObject panelCreditos;

    [Header("Textos HighScore")]
    [SerializeField] private TextMeshProUGUI highScoreXativaText;
    [SerializeField] private TextMeshProUGUI highScorePobleText;
    [SerializeField] private TextMeshProUGUI highScoreTorresText;
    [SerializeField] private TextMeshProUGUI highScoreMercatText;
    [SerializeField] private TextMeshProUGUI highScoreEstacionText;

    [Header("Textos Título")]
    [SerializeField] private TextMeshProUGUI tituloXativaText;
    [SerializeField] private TextMeshProUGUI tituloPobleText;
    [SerializeField] private TextMeshProUGUI tituloTorresText;
    [SerializeField] private TextMeshProUGUI tituloMercatText;
    [SerializeField] private TextMeshProUGUI tituloEstacionText;

    [Header("Colores")]
    [SerializeField] private Color colorCompletado = Color.green;
    [SerializeField] private Color colorNoCompletado = Color.white;

    void Start()
    {
        MostrarPanelPrincipal();
        CargarTodosLosHighScores();
        ActualizarColoresNivelesCompletados();
    }

    // ===== MÉTODO PARA ABRIR HISTORIA (LLAMADO DESDE EL BOTÓN) =====
    public void AbrirPanelHistoria()
    {
        Debug.Log("=== ABRIENDO PANEL DE HISTORIA ===");

        // Ocultar todos los paneles
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelMapa != null) panelMapa.SetActive(false);
        if (panelXativa != null) panelXativa.SetActive(false);
        if (panelPoble != null) panelPoble.SetActive(false);
        if (panelTorres != null) panelTorres.SetActive(false);
        if (panelMercat != null) panelMercat.SetActive(false);
        if (panelEstacion != null) panelEstacion.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);

        // Mostrar panel de historia
        if (panelHistoria != null)
        {
            panelHistoria.SetActive(true);
            Debug.Log("Panel de historia ACTIVADO");
        }

        // ===== INICIAR EL DIÁLOGO =====
        DialogoManager dialogo = FindObjectOfType<DialogoManager>();
        if (dialogo != null)
        {
            // Suscribirse al evento de fin de diálogo
            dialogo.OnDialogoTerminado -= MostrarSiguientePanel; // Evitar duplicados
            dialogo.OnDialogoTerminado += MostrarSiguientePanel;

            dialogo.IniciarDialogo();
            Debug.Log("Diálogo INICIADO");
        }
        else
        {
            Debug.LogError("No se encontró DialogoManager en la escena");
        }

        ActualizarColoresNivelesCompletados();
    }

    // Método que se ejecuta cuando el diálogo termina
    private void MostrarSiguientePanel()
    {
        Debug.Log("=== DIÁLOGO TERMINADO - MOSTRANDO SIGUIENTE PANEL ===");

        // Ocultar panel de historia
        if (panelHistoria != null)
            panelHistoria.SetActive(false);

        // Mostrar el siguiente panel (ejemplo: panel de selección de niveles)
        if (panelMapa != null)
            panelMapa.SetActive(true);
        else if (panelPrincipal != null)
            panelPrincipal.SetActive(true);

        // Limpiar el evento para que no se ejecute múltiples veces
        DialogoManager dialogo = FindObjectOfType<DialogoManager>();
        if (dialogo != null)
        {
            dialogo.OnDialogoTerminado -= MostrarSiguientePanel;
        }

        ActualizarColoresNivelesCompletados();
    }

    // ===== RESTO DE MÉTODOS (sin cambios) =====

    private void CargarTodosLosHighScores()
    {
        int highScoreXativa = PlayerPrefs.GetInt("HighScore_Xativa", 0);
        if (highScoreXativaText != null)
            highScoreXativaText.text = $"Best: {highScoreXativa}";

        int highScorePoble = PlayerPrefs.GetInt("HighScore_Poble", 0);
        if (highScorePobleText != null)
            highScorePobleText.text = $"Best: {highScorePoble}";

        int highScoreTorres = PlayerPrefs.GetInt("HighScore_Torres", 0);
        if (highScoreTorresText != null)
            highScoreTorresText.text = $"Best: {highScoreTorres}";

        int highScoreMercat = PlayerPrefs.GetInt("HighScore_Mercat", 0);
        if (highScoreMercatText != null)
            highScoreMercatText.text = $"Best: {highScoreMercat}";

        int highScoreEstacion = PlayerPrefs.GetInt("HighScore_Estacion", 0);
        if (highScoreEstacionText != null)
            highScoreEstacionText.text = $"Best: {highScoreEstacion}";

        Debug.Log($"HighScores cargados");
    }

    public void ActualizarColoresNivelesCompletados()
    {
        ActualizarColorTitulo(tituloXativaText, "NivelCompletado_Xativa");
        ActualizarColorTitulo(tituloPobleText, "NivelCompletado_Poble");
        ActualizarColorTitulo(tituloTorresText, "NivelCompletado_Torres");
        ActualizarColorTitulo(tituloMercatText, "NivelCompletado_Mercat");
        ActualizarColorTitulo(tituloEstacionText, "NivelCompletado_Estacion");
    }

    private void ActualizarColorTitulo(TextMeshProUGUI texto, string clavePlayerPrefs)
    {
        if (texto != null)
        {
            bool completado = PlayerPrefs.GetInt(clavePlayerPrefs, 0) == 1;
            texto.color = completado ? colorCompletado : colorNoCompletado;
        }
    }

    public static void MarcarNivelComoCompletado(string nombreNivel)
    {
        string clave = $"NivelCompletado_{nombreNivel}";
        PlayerPrefs.SetInt(clave, 1);
        PlayerPrefs.Save();
        Debug.Log($"Nivel {nombreNivel} marcado como completado");
    }

    public static void ReiniciarTodosLosNiveles()
    {
        PlayerPrefs.DeleteKey("NivelCompletado_Xativa");
        PlayerPrefs.DeleteKey("NivelCompletado_Poble");
        PlayerPrefs.DeleteKey("NivelCompletado_Torres");
        PlayerPrefs.DeleteKey("NivelCompletado_Mercat");
        PlayerPrefs.DeleteKey("NivelCompletado_Estacion");
        PlayerPrefs.Save();
        Debug.Log("Todos los niveles reiniciados");
    }

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

        ActualizarColoresNivelesCompletados();
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

        ActualizarColoresNivelesCompletados();
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

        ActualizarColoresNivelesCompletados();
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

        ActualizarColoresNivelesCompletados();
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

        ActualizarColoresNivelesCompletados();
    }

    public void JugarXativa()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
            menuInicio.EmpezarNivelXativa1();
    }

    public void JugarPoble()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
            menuInicio.EmpezarNivelPoble1();
    }

    public void JugarTorres()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
            menuInicio.EmpezarNivelTorres1();
    }

    public void JugarMercat()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
            menuInicio.EmpezarNivelMercat1();
    }

    public void JugarEstacion()
    {
        MenuInicio menuInicio = FindObjectOfType<MenuInicio>();
        if (menuInicio != null)
            menuInicio.EmpezarNivelEstacion1();
    }

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

    public void MostrarPanelPrincipal()
    {
        if (panelHistoria != null && panelHistoria.activeSelf)
        {
            DialogoManager dialogo = FindObjectOfType<DialogoManager>();
            if (dialogo != null)
                dialogo.ReiniciarDialogo();
        }

        if (panelPrincipal != null) panelPrincipal.SetActive(true);
        if (panelMapa != null) panelMapa.SetActive(false);
        if (panelXativa != null) panelXativa.SetActive(false);
        if (panelPoble != null) panelPoble.SetActive(false);
        if (panelTorres != null) panelTorres.SetActive(false);
        if (panelMercat != null) panelMercat.SetActive(false);
        if (panelEstacion != null) panelEstacion.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
        if (panelHistoria != null) panelHistoria.SetActive(false);

        ActualizarColoresNivelesCompletados();
    }

    public void MostrarPanelOpciones()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelMapa != null) panelMapa.SetActive(false);
        if (panelXativa != null) panelXativa.SetActive(false);
        if (panelPoble != null) panelPoble.SetActive(false);
        if (panelTorres != null) panelTorres.SetActive(false);
        if (panelMercat != null) panelMercat.SetActive(false);
        if (panelEstacion != null) panelEstacion.SetActive(false);
        if (panelHistoria != null) panelHistoria.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(true);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    public void MostrarPanelCreditos()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelMapa != null) panelMapa.SetActive(false);
        if (panelXativa != null) panelXativa.SetActive(false);
        if (panelPoble != null) panelPoble.SetActive(false);
        if (panelTorres != null) panelTorres.SetActive(false);
        if (panelMercat != null) panelMercat.SetActive(false);
        if (panelEstacion != null) panelEstacion.SetActive(false);
        if (panelHistoria != null) panelHistoria.SetActive(false);
        if (panelOpciones != null) panelOpciones.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(true);
    }

    public void VolverAlMenuPrincipal()
    {
        MostrarPanelPrincipal();
    }
}