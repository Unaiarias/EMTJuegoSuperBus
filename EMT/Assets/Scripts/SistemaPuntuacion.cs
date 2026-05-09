using UnityEngine;
using TMPro;
using System.Collections;

public class SistemaPuntuacion : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float tiempoReinicioCombo = 3f;

    [Header("Puntuación por Tipo")]
    [SerializeField] private int puntosPorEnemigo = 100;
    [SerializeField] private int puntosPorAciertoRitmoPerfect = 300;
    [SerializeField] private int puntosPorAciertoRitmoGood = 250;
    [SerializeField] private int puntosPorAciertoRitmoDrag = 250;
    [SerializeField] private int puntosPorAciertoRitmoInstant = 200;

    private int scoreActual = 0;
    private int highScoreActual = 0;
    private int comboActual = 0;
    private float ultimoTiempoPuntuacion = 0f;
    private Coroutine corrutinaReinicioCombo;

    public static SistemaPuntuacion Instance;

    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnComboChanged;

    private TextMeshProUGUI menuScoreText;
    private TextMeshProUGUI menuHighScoreText;

    // Nivel actual que se está jugando
    private string nivelActual = "";

    // Claves para PlayerPrefs
    private const string HIGHSCORE_PREFIX = "HighScore_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("? SistemaPuntuacion inicializado");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Establecer el nivel actual (llamar desde los métodos EmpezarNivel)
    public void SetNivelActual(string nombreNivel)
    {
        nivelActual = nombreNivel;
        CargarHighScoreDelNivel();
        Debug.Log($"?? Nivel actual: {nivelActual} - HighScore: {highScoreActual}");
    }

    private void CargarHighScoreDelNivel()
    {
        if (string.IsNullOrEmpty(nivelActual)) return;

        string key = HIGHSCORE_PREFIX + nivelActual;
        highScoreActual = PlayerPrefs.GetInt(key, 0);
        Debug.Log($"?? HighScore cargado para {nivelActual}: {highScoreActual}");
        ActualizarTextosMenu();
    }

    private void GuardarHighScoreDelNivel()
    {
        if (string.IsNullOrEmpty(nivelActual)) return;

        string key = HIGHSCORE_PREFIX + nivelActual;
        PlayerPrefs.SetInt(key, highScoreActual);
        PlayerPrefs.Save();
        Debug.Log($"?? HighScore guardado para {nivelActual}: {highScoreActual}");
    }

    public void AsignarTextosMenu(TextMeshProUGUI scoreText, TextMeshProUGUI highScoreText)
    {
        menuScoreText = scoreText;
        menuHighScoreText = highScoreText;
        ActualizarTextosMenu();
        Debug.Log("? Textos del menú asignados");
    }

    private void ActualizarTextosMenu()
    {
        if (menuScoreText != null)
            menuScoreText.text = $"SCORE: {scoreActual}";

        if (menuHighScoreText != null)
            menuHighScoreText.text = $"BEST: {highScoreActual}";
    }

    public void SumarPuntos(TipoPuntuacion tipo, int puntosBase = 0, int puntosExtraCombo = 0)
    {
        int puntosGanados = puntosBase;

        if (puntosBase == 0)
        {
            switch (tipo)
            {
                case TipoPuntuacion.Enemigo:
                    puntosGanados = puntosPorEnemigo;
                    break;
                case TipoPuntuacion.RitmoPerfect:
                    puntosGanados = puntosPorAciertoRitmoPerfect;
                    break;
                case TipoPuntuacion.RitmoGood:
                    puntosGanados = puntosPorAciertoRitmoGood;
                    break;
                case TipoPuntuacion.RitmoDrag:
                    puntosGanados = puntosPorAciertoRitmoDrag;
                    break;
                case TipoPuntuacion.RitmoInstant:
                    puntosGanados = puntosPorAciertoRitmoInstant;
                    break;
            }
        }

        float multiplicadorCombo = 1f + (comboActual * 0.1f);
        multiplicadorCombo = Mathf.Min(multiplicadorCombo, 3f);

        int puntosConCombo = Mathf.RoundToInt(puntosGanados * multiplicadorCombo) + puntosExtraCombo;

        scoreActual += puntosConCombo;
        ReiniciarTimerCombo();

        OnScoreChanged?.Invoke(scoreActual);
        ActualizarTextosMenu();

        Debug.Log($"+{puntosConCombo} puntos ({tipo}) | Score total: {scoreActual}");
    }

    public void AumentarCombo()
    {
        comboActual++;
        OnComboChanged?.Invoke(comboActual);
        Debug.Log($"?? Combo: x{comboActual}");
    }

    public void ReiniciarCombo()
    {
        if (comboActual > 0)
        {
            comboActual = 0;
            OnComboChanged?.Invoke(0);
            Debug.Log("?? Combo reiniciado!");
        }

        if (corrutinaReinicioCombo != null)
        {
            StopCoroutine(corrutinaReinicioCombo);
            corrutinaReinicioCombo = null;
        }
    }

    private void ReiniciarTimerCombo()
    {
        ultimoTiempoPuntuacion = Time.time;

        if (corrutinaReinicioCombo != null)
            StopCoroutine(corrutinaReinicioCombo);

        corrutinaReinicioCombo = StartCoroutine(VerificarReinicioCombo());
    }

    private IEnumerator VerificarReinicioCombo()
    {
        yield return new WaitForSeconds(tiempoReinicioCombo);

        if (Time.time - ultimoTiempoPuntuacion >= tiempoReinicioCombo && comboActual > 0)
        {
            ReiniciarCombo();
        }

        corrutinaReinicioCombo = null;
    }

    // Guardar score al completar un nivel (actualiza highscore del nivel actual)
    public void GuardarScoreNivel()
    {
        if (scoreActual > highScoreActual)
        {
            highScoreActual = scoreActual;
            GuardarHighScoreDelNivel();
            Debug.Log($"?? ¡NUEVO RÉCORD para {nivelActual}! {highScoreActual}");
        }
        else
        {
            Debug.Log($"Score final: {scoreActual} | HighScore de {nivelActual}: {highScoreActual}");
        }

        ActualizarTextosMenu();
    }

    // Reiniciar score para un nivel NUEVO
    public void ReiniciarScore()
    {
        Debug.Log($"?? Reiniciando score para nivel {nivelActual}");
        scoreActual = 0;
        ReiniciarCombo();
        OnScoreChanged?.Invoke(scoreActual);
        ActualizarTextosMenu();
    }

    public int GetScoreActual() => scoreActual;
    public int GetHighScore() => highScoreActual;
    public int GetComboActual() => comboActual;
    public string GetNivelActual() => nivelActual;

    public void ResetTotal()
    {
        Debug.Log($"?? RESET TOTAL - Nivel: {nivelActual}");
        scoreActual = 0;
        highScoreActual = 0;
        ReiniciarCombo();
        OnScoreChanged?.Invoke(scoreActual);
        ActualizarTextosMenu();

        if (!string.IsNullOrEmpty(nivelActual))
        {
            string key = HIGHSCORE_PREFIX + nivelActual;
            PlayerPrefs.SetInt(key, 0);
            PlayerPrefs.Save();
        }
    }
}

public enum TipoPuntuacion
{
    Enemigo,
    RitmoPerfect,
    RitmoGood,
    RitmoDrag,
    RitmoInstant
}