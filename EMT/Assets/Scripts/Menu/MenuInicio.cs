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

    public void Nivel() 
    {
        Debug.Log("Nivel");
        SceneManager.LoadScene("Player");
    }


    public void Salir()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
    }
}
