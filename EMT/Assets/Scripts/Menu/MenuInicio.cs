using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip sonidoBotonPresionado;
    public AudioClip sonidoBotonSeleccionado;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (SistemaPuntuacion.Instance != null)
        {
            Debug.Log($"?? Escena cargada: {SceneManager.GetActiveScene().name} - Score actual: {SistemaPuntuacion.Instance.GetScoreActual()}, HighScore: {SistemaPuntuacion.Instance.GetHighScore()}");
        }
    }

    // ============ MÉTODOS DE AUDIO ============

    public void BotonClickAudio()
    {
        if (audioSource != null && sonidoBotonPresionado != null)
            audioSource.PlayOneShot(sonidoBotonPresionado);
    }

    public void BotonSeleccionadoAudio()
    {
        if (audioSource != null && sonidoBotonSeleccionado != null)
            audioSource.PlayOneShot(sonidoBotonSeleccionado);
    }

    // ============ ACCIONES DE BOTONES ============

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void VolverAlMenu()
    {
        Debug.Log("Volviendo al Menú de Inicio...");

        if (SistemaPuntuacion.Instance != null)
        {
            // SOLO reiniciamos el score actual, NO el highscore
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("MenuInicio");
    }

    public void ReintentarNivel()
    {
        Debug.Log("Reintentando nivel - Recargando escena...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // ============ MÉTODOS PARA EMPEZAR NIVEL NUEVO (SOLO DESDE EL MENÚ) ============

    public void EmpezarNivelXativa1()
    {
        Debug.Log("?? Empezar Nivel Xativa1 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_Escenario1");
    }

    public void EmpezarNivelXativa2()
    {
        Debug.Log("?? Empezar Nivel Xativa2 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_Escenario2");
    }

    public void EmpezarNivelXativa3()
    {
        Debug.Log("?? Empezar Nivel Xativa3 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_Escenario3");
    }

    public void EmpezarNivelXativa4()
    {
        Debug.Log("?? Empezar Nivel Xativa4 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_Escenario4");
    }

    public void EmpezarMinijuegoLimpieza1()
    {
        Debug.Log("?? Empezar Minijuego Limpieza 1 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza1");
    }

    public void EmpezarMinijuegoLimpieza2()
    {
        Debug.Log("?? Empezar Minijuego Limpieza 2 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza2");
    }

    public void EmpezarMinijuegoLimpieza3()
    {
        Debug.Log("?? Empezar Minijuego Limpieza 3 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza3");
    }

    public void EmpezarMinijuegoLimpieza4()
    {
        Debug.Log("?? Empezar Minijuego Limpieza 4 - NUEVO nivel, score reiniciado a 0");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza4");
    }

    // ============ MÉTODOS PARA CONTINUAR AL SIGUIENTE NIVEL ============

    public void ContinuarAlEscenario2()
    {
        Debug.Log($"?? Continuar al Escenario 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel(); // Actualiza highscore si es mayor
        }

        SceneManager.LoadScene("Nivel1_Escenario2");
    }

    public void ContinuarAlMinijuegoLimpieza1()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza 1 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza1");
    }

    public void ContinuarAlEscenario3()
    {
        Debug.Log($"?? Continuar al Escenario 3 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel1_Escenario3");
    }

    public void ContinuarAlMinijuegoLimpieza2()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza2");
    }

    public void ContinuarAlEscenario4()
    {
        Debug.Log($"?? Continuar al Escenario 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel1_Escenario4");
    }

    public void ContinuarAlMinijuegoLimpieza3()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza 3 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza3");
    }

    public void ContinuarAlMinijuegoLimpieza4()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza4");
    }

    // ============ COMPLETAR TODOS LOS NIVELES ============

    public void CompletarTodosLosNiveles()
    {
        Debug.Log($"?? Todos los niveles completados!");

        if (SistemaPuntuacion.Instance != null)
        {
            // Guardar el score final como highscore (si es mayor)
            SistemaPuntuacion.Instance.GuardarScoreNivel();

            int highScoreFinal = SistemaPuntuacion.Instance.GetHighScore();
            Debug.Log($"?? HighScore final guardado: {highScoreFinal}");

            // IMPORTANTE: Reiniciamos solo el score actual, NO el highscore
            // para que al volver al menú se vea el récord
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("MenuInicio");
    }
}