using UnityEngine;

public class MonedaScript : MonoBehaviour
{
    private float timer = 0f; // Timer para la destrucción
    public float tiempoDes = 10f; // Tiempo en segundos para destruir la moneda

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Timer para destrucción
        if (timer < tiempoDes)
        {
            timer += Time.deltaTime;
        }
        if (timer >= tiempoDes)
        {
            Destroy(gameObject);
        }
    }
}
