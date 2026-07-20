using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class PanelNivelCompletado : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float tiempoEspera = 3f;

    [Header("Panel")]
    [SerializeField] private GameObject panelCompletado;

    [Header("Texto Puntuación (opcional)")]
    [SerializeField] private TextMeshProUGUI textoPuntuacion;

    void Start()
    {
        // Asegurar que el panel empieza desactivado
        if (panelCompletado != null)
            panelCompletado.SetActive(false);
    }

    // Método público para mostrar el panel
    public void MostrarNivelCompletado()
    {
        if (panelCompletado == null)
        {
            Debug.LogError("PanelCompletado es null");
            IrAlMenu();
            return;
        }

        // ACTIVAR EL PANEL (mantiene el texto que ya tienes escrito en Unity)
        panelCompletado.SetActive(true);
        Debug.Log("Panel activado - mostrando el texto que tienes configurado en Unity");

        // Opcional: Actualizar solo la puntuación si quieres
        if (textoPuntuacion != null && SistemaPuntuacion.Instance != null)
        {
            int scoreFinal = SistemaPuntuacion.Instance.GetScoreActual();
            int highScore = SistemaPuntuacion.Instance.GetHighScore();
            textoPuntuacion.text = $"Puntuación: {scoreFinal}\nMejor: {highScore}";
        }

        // Esperar y volver al menú
        StartCoroutine(EsperarYVolverAlMenu());
    }

    private IEnumerator EsperarYVolverAlMenu()
    {
        // Esperar el tiempo configurado
        yield return new WaitForSecondsRealtime(tiempoEspera);

        Debug.Log("Tiempo completado. Yendo al menú...");
        IrAlMenu();
    }

    private void IrAlMenu()
    {
        // Reiniciar score
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("MenuInicio");
    }
}