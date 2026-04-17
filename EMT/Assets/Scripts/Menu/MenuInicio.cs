using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuInicio : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void NivelXativa() 
    {
        Debug.Log("Nivel Xativa");
        SceneManager.LoadScene("Player");
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
}
