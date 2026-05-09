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

    // Nivel Xativa (1)
    public void EmpezarNivelXativa1()
    {
        Debug.Log("?? Empezar Nivel Xativa1 - NUEVO nivel");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.SetNivelActual("Xativa");
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel1_Escenario1");
    }

    //Nivel Plaza (2)
    public void EmpezarNivelPoble1()
    {
        Debug.Log("?? Empezar Nivel Poble1 - NUEVO nivel");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.SetNivelActual("Poble");
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel2_Escenario1");
    }

    //Nivel Torres (3)
    public void EmpezarNivelTorres1()
    {
        Debug.Log("?? Empezar Nivel Torres1 - NUEVO nivel");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.SetNivelActual("Torres");
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel3_Escenario1");
    }

    //Nivel Mercat Central (4)
    public void EmpezarNivelMercat1()
    {
        Debug.Log("?? Empezar Nivel Mercat1 - NUEVO nivel");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.SetNivelActual("Mercat");
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel4_Escenario1");
    }

    //Nivel Estacion del Norte (5)
    public void EmpezarNivelEstacion1()
    {
        Debug.Log("?? Empezar Nivel Estacion1 - NUEVO nivel");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.SetNivelActual("Estacion");
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        SceneManager.LoadScene("Nivel5_Escenario1");
    }

    // ============ MÉTODOS PARA CONTINUAR AL SIGUIENTE NIVEL ============

    // Nivel Xativa (1)
    public void ContinuarAlEscenario2()
    {
        Debug.Log($"?? Continuar al Escenario 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            // NO cambiar el nivel, solo guardar
            SistemaPuntuacion.Instance.GuardarScoreNivel();
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

    //Nivel Poble (2)

    public void ContinuarAlEscenarioPoble2()
    {
        Debug.Log($"?? Continuar al Escenario Poble 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_Escenario2");
    }

    public void ContinuarAlMinijuegoLimpiezaPoble1()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Poble 1 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_MinijuegoLimpieza1");
    }

    public void ContinuarAlEscenarioPoble3()
    {
        Debug.Log($"?? Continuar al Escenario Poble 3 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_Escenario3");
    }

    public void ContinuarAlMinijuegoLimpiezaPoble2()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Poble 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_MinijuegoLimpieza2");
    }

    public void ContinuarAlEscenarioPoble4()
    {
        Debug.Log($"?? Continuar al Escenario Poble 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_Escenario4");
    }

    public void ContinuarAlMinijuegoLimpiezaPoble3()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Poble 3 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_MinijuegoLimpieza3");
    }

    public void ContinuarAlMinijuegoLimpiezaPoble4()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Poble 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel2_MinijuegoLimpieza4");
    }

    //Nivel Torres (3)
    public void ContinuarAlEscenarioTorres2()
    {
        Debug.Log($"?? Continuar al Escenario Torres 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }
        SceneManager.LoadScene("Nivel3_Escenario2");
    }

    public void ContinuarAlMinijuegoLimpiezaTorres1()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Torres 1 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel3_MinijuegoLimpieza1");
    }

    public void ContinuarAlEscenarioTorres3()
    {
        Debug.Log($"?? Continuar al Escenario Torres - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel3_Escenario3");
    }

    public void ContinuarAlMinijuegoLimpiezaTorres2()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Torres 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel3_MinijuegoLimpieza2");
    }

    public void ContinuarAlEscenarioTorres4()
    {
        Debug.Log($"?? Continuar al Escenario Torres 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel3_Escenario4");
    }

    public void ContinuarAlMinijuegoLimpiezaTorres3()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Torres - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel3_MinijuegoLimpieza3");
    }

    public void ContinuarAlMinijuegoLimpiezaTorres4()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Torres 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel3_MinijuegoLimpieza4");
    }

    //Nivel Mercat (4)

    public void ContinuarAlEscenarioMercat2()
    {
        Debug.Log($"?? Continuar al Escenario Mercat 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }
        SceneManager.LoadScene("Nivel4_Escenario2");
    }

    public void ContinuarAlMinijuegoLimpiezaMercat1()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Mercat 1 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel4_MinijuegoLimpieza1");
    }

    public void ContinuarAlEscenarioMercat3()
    {
        Debug.Log($"?? Continuar al Escenario Mercat - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel4_Escenario3");
    }

    public void ContinuarAlMinijuegoLimpiezaMercat2()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Mercat 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel4_MinijuegoLimpieza2");
    }

    public void ContinuarAlEscenarioMercat4()
    {
        Debug.Log($"?? Continuar al Escenario Mercat 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel4_Escenario4");
    }

    public void ContinuarAlMinijuegoLimpiezaMercat3()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Mercat - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel4_MinijuegoLimpieza3");
    }

    public void ContinuarAlMinijuegoLimpiezaMercat4()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Mercat 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel4_MinijuegoLimpieza4");
    }

    //Nivel Estacion del Norte (5)
    public void ContinuarAlEscenarioEstacion2()
    {
        Debug.Log($"?? Continuar al Escenario Estacion 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");
        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }
        SceneManager.LoadScene("Nivel5_Escenario2");
    }

    public void ContinuarAlMinijuegoLimpiezaEstacion1()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Estacion 1 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel5_MinijuegoLimpieza1");
    }

    public void ContinuarAlEscenarioEstacion3()
    {
        Debug.Log($"?? Continuar al Escenario Estacion - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel5_Escenario3");
    }

    public void ContinuarAlMinijuegoLimpiezaEstacion2()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Estacion 2 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel5_MinijuegoLimpieza2");
    }

    public void ContinuarAlEscenarioEstacion4()
    {
        Debug.Log($"?? Continuar al Escenario Estacion 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel5_Escenario4");
    }

    public void ContinuarAlMinijuegoLimpiezaEstacion3()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Estacion - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel5_MinijuegoLimpieza3");
    }

    public void ContinuarAlMinijuegoLimpiezaEstacion4()
    {
        Debug.Log($"?? Continuar al Minijuego Limpieza Estacion 4 - Score CONTINÚA: {(SistemaPuntuacion.Instance != null ? SistemaPuntuacion.Instance.GetScoreActual().ToString() : "N/A")}");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
        }

        SceneManager.LoadScene("Nivel5_MinijuegoLimpieza4");
    }

    // ============ COMPLETAR TODOS LOS NIVELES ============

    public void CompletarTodosLosNiveles()
    {
        Debug.Log($"?? Todos los niveles completados!");

        if (SistemaPuntuacion.Instance != null)
        {
            SistemaPuntuacion.Instance.GuardarScoreNivel();
            int highScoreFinal = SistemaPuntuacion.Instance.GetHighScore();
            Debug.Log($"?? HighScore final guardado: {highScoreFinal}");
            SistemaPuntuacion.Instance.ReiniciarScore();
        }

        Time.timeScale = 1;
        SceneManager.LoadScene("MenuInicio");
    }
}