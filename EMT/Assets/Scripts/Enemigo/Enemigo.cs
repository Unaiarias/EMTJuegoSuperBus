using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    public int vidaActualEnemigo = 100;
    public int vidaEnemigoMaxima = 100;
    public SistemaOleadas spawner;
    //[Header("Interfaz Vida")]
    //public Image barraSalud; // Arrastra aqu� la imagen "Fill" del canvas hijo
    //public TextMeshProUGUI vidaText; // Opcional

    [SerializeField] private PlayerVida playerVida;

    private void Start()
    {
        //vidaActualEnemigo = vidaEnemigoMaxima;
        //ActualizarInterfazVida();
    }

    public void RecibirDanoEnemigo(int cantidadDano)
    {
        vidaActualEnemigo -= cantidadDano;
        playerVida.AumentarCombo();
        //ActualizarInterfazVida();

        if (vidaActualEnemigo <= 0)
        {
            MorirEnemigo();
        }
    }
    
    //void ActualizarInterfazVida()
    //{
    //    if (barraSalud != null)
    //        barraSalud.fillAmount = (float)vidaActualEnemigo / vidaEnemigoMaxima;

    //    if (vidaText != null)
    //        vidaText.text = vidaActualEnemigo.ToString();
    //}

    public void MorirEnemigo()
    {
        if (spawner != null)
        {
            spawner.NotificarMuerteEnemigo();
        }

        Destroy(gameObject);
        Debug.Log("Enemigo Muerto");
    }
}