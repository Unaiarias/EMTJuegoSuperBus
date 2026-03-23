using UnityEngine;

public class RetrocesoAlPlayer : MonoBehaviour
{
    public int danoPorGolpe = 30;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Daño al player");

            PlayerVida playerVida = other.GetComponent<PlayerVida>();
            PlayerAtaque playerAtaque = other.GetComponent<PlayerAtaque>();
            PlayerKnockback knock = other.GetComponent<PlayerKnockback>();

            if (playerAtaque != null && playerAtaque.isBarrera)
            {
                Debug.Log("El player tiene la barrera activa, no recibe daño");
                return;
            }

            if (playerVida != null)
            {
                playerVida.RecibirDanoPlayer(danoPorGolpe);
            }

            if (knock != null)
            {
                // Dirección hacia ATRÁS del enemigo
                Vector3 knockDir = transform.forward;   // adelante del enemigo
                knockDir.y = 0f;
                knockDir.Normalize();

                // EMPUJAR al player hacia el ATRÁS del enemigo
                // es decir: dirección del enemigo, SIN invertirla aquí
                knock.ApplyKnockback(knockDir);
            }
        }
    }
}



