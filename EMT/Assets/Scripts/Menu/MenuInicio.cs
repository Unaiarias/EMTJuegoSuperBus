using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    private AudioSource audioSource;
    public AudioClip sonidoBotonPresionado;
    public AudioClip sonidoBotonSeleccionado;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void BotonClickAudio()
    {
        audioSource.PlayOneShot(sonidoBotonPresionado);
    }

    public void BotonSeleccionadoAudio()
    {
        audioSource.PlayOneShot(sonidoBotonSeleccionado);
    }

    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }

    public void Opciones()
    {
        Debug.Log("Opciones");
       
    }

    public void Creditos()
    {
        Debug.Log("Creditos");

    }

    public void Volver()
    {
        Debug.Log("Volver");

    }

    public void VolverAlMenu()
    {
        Debug.Log("Volver al Menu");
        SceneManager.LoadScene("MenuInicio");
    }

    //Nivel Xativa
    public void NivelXativa1()
    {
        Debug.Log("Nivel Xativa1");
        SceneManager.LoadScene("Nivel1_Escenario1");
    }

    public void NivelEscenaLimpiezaXativa1()
    {
        Debug.Log("Nivel Limpieza Xativa 1");
        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza1");
    }

    public void NivelXativa2()
    {
        Debug.Log("Nivel Xativa2");
        SceneManager.LoadScene("Nivel1_Escenario2");
    }

    public void NivelEscenaLimpiezaXativa2()
    {
        Debug.Log("Nivel Limpieza Xativa 2");
        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza2");
    }

    public void NivelXativa3()
    {
        Debug.Log("Nivel Xativa3");
        SceneManager.LoadScene("Nivel1_Escenario3");
    }

    public void NivelEscenaLimpiezaXativa3()
    {
        Debug.Log("Nivel Limpieza Xativa 3");
        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza3");
    }

    public void NivelXativa4()
    {
        Debug.Log("Nivel Xativa4");
        SceneManager.LoadScene("Nivel1_Escenario4");
    }

    public void NivelEscenaLimpiezaXativa4()
    {
        Debug.Log("Nivel Limpieza Xativa 4");
        SceneManager.LoadScene("Nivel1_MinijuegoLimpieza4");
    }
}
