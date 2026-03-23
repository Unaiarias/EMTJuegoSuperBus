using UnityEngine;

public class DetectarDaño : MonoBehaviour
{
    private PlayerVida playerVida;

    private void Start()
    {
        // Buscar al jugador al inicio
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerVida = player.GetComponent<PlayerVida>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignorar si el que entra es el propio player o sus partes
        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemigo"))
        {
            // Verificar que tenemos referencia al PlayerVida
            if (playerVida == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerVida = player.GetComponent<PlayerVida>();
                }
                else
                {
                    Debug.LogError("No se encontró el Player");
                    return;
                }
            }

     
            Enemigo enemigo = other.GetComponent<Enemigo>(); //Cogemos la referencia del script del Enemigo
            if (enemigo != null)
            {
                // Pasar el daño actual del jugador
                enemigo.RecibirDanoEnemigo(playerVida.DanoActual);
            }
        }
    }
}