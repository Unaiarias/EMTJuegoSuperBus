using UnityEngine;

public class RecogerMonedas : MonoBehaviour
{
    
    public PlayerVida playerVida;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Moneda"))
        {
            // Incrementar el contador de monedas
            playerVida.IncrementarMonedas();
            // Destruir la moneda recogida
            Destroy(other.gameObject);

           
        }
    }

    

}
