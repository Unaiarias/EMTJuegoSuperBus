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
    private int highScore = 0;
    private int comboActual = 0;
    private float ultimoTiempoPuntuacion = 0f;
    private Coroutine corrutinaReinicioCombo;

    public static SistemaPuntuacion Instance;

    public System.Action<int> OnScoreChanged;
    public System.Action<int> OnComboChanged;

    // Referencias para textos del menú (solo se usan en MenuInicio)
    private TextMeshProUGUI menuScoreText;
    private TextMeshProUGUI menuHighScoreText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            CargarHighScore();
            Debug.Log("? SistemaPuntuacion inicializado");
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    // Método para asignar textos del menú (solo llamado desde MenuInicio)
    public void AsignarTextosMenu(TextMeshProUGUI scoreText, TextMeshProUGUI highScoreText)
    {
        menuScoreText = scoreText;
        menuHighScoreText = highScoreText;
        ActualizarTextosMenu();
        Debug.Log("?? Textos del menú asignados a SistemaPuntuacion");
    }

    // Actualizar solo los textos del menú
    private void ActualizarTextosMenu()
    {
        if (menuScoreText != null)
            menuScoreText.text = $"SCORE: {scoreActual}";

        if (menuHighScoreText != null)
            menuHighScoreText.text = $"BEST: {highScore}";
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

        // Notificar cambios
        OnScoreChanged?.Invoke(scoreActual);
        ActualizarTextosMenu();

        Debug.Log($"+{puntosConCombo} puntos ({tipo}) (Combo x{multiplicadorCombo:F1}) | Score total: {scoreActual}");
    }

    public void AumentarCombo()
    {
        comboActual++;
        // comboActual = Mathf.Min(comboActual, 30); // LÍMITE ELIMINADO - Ahora puede subir infinitamente
        OnComboChanged?.Invoke(comboActual);
        Debug.Log($"Combo aumentado a x{comboActual}");
    }

    public void ReiniciarCombo()
    {
        if (comboActual > 0)
        {
            comboActual = 0;
            OnComboChanged?.Invoke(0);
            Debug.Log("Combo reiniciado!");
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

    // Guardar score al completar un nivel (actualiza highscore si es mayor)
    public void GuardarScoreNivel()
    {
        if (scoreActual > highScore)
        {
            highScore = scoreActual;
            GuardarHighScore();
            Debug.Log($"¡NUEVO RÉCORD! {highScore}");
        }
        else
        {
            Debug.Log($"Score final: {scoreActual} | HighScore actual: {highScore}");
        }

        ActualizarTextosMenu();
    }

    // Reiniciar score para un nivel NUEVO (desde el menú)
    public void ReiniciarScore()
    {
        Debug.Log($"?? REINICIANDO SCORE!");
        scoreActual = 0;
        ReiniciarCombo();
        OnScoreChanged?.Invoke(scoreActual);
        ActualizarTextosMenu();
    }

    private void GuardarHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    private void CargarHighScore()
    {
        highScore = PlayerPrefs.GetInt("HighScore", 0);
        Debug.Log($"HighScore cargado: {highScore}");
    }

    public int GetScoreActual() => scoreActual;
    public int GetHighScore() => highScore;
    public int GetComboActual() => comboActual;

    public void ResetTotal()
    {
        Debug.Log($"?? RESET TOTAL");
        scoreActual = 0;
        highScore = 0;
        ReiniciarCombo();
        OnScoreChanged?.Invoke(scoreActual);
        ActualizarTextosMenu();
        GuardarHighScore();
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