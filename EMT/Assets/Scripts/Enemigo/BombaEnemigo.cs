using UnityEngine;

public class BombaEnemigo : MonoBehaviour
{
    

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Enemigo>()?.MorirEnemigo();
            
            
        }
    }
}
