using UnityEngine;

public class BombaEnemigo : MonoBehaviour
{
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
                Destroy(gameObject); // Destruir la bomba después de causar daño
            
        }
    }
}
