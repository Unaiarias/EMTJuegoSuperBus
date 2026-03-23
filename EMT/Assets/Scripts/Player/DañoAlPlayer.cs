using UnityEngine;
using UnityEngine.SceneManagement;

public class DañoAlPlayer : MonoBehaviour
{
    
    public int danoPorGolpe = 30;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Daño al player");

            //enemigo.comboCount = 0; // Reiniciar el contador de combo del enemigo
            PlayerVida playerVida = other.GetComponent<PlayerVida>(); //Cogemos la referencia del script del player
            PlayerAtaque playerAtaque = other.GetComponent<PlayerAtaque>();
            
            if (playerAtaque != null && playerAtaque.isBarrera)
            {
                Debug.Log("El player tiene la barrera activa, no recibe daño");
                //danoPorGolpe = 0;
                return; // Si el player tiene la barrera activa, no recibe daño
            }

            if (playerVida != null)
            {
                playerVida.RecibirDanoPlayer(danoPorGolpe);
                
            }
        }
    }

}
