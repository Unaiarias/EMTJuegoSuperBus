using UnityEngine;

public class DisparoEnemigo : MonoBehaviour
{
    public GameObject balaPrefab;
    public float velocidadDisparo = 5f;
    public Transform AlturaArma;
    public float launchAngle = 45f;
    public Transform target;

    void Start()
    {
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            target = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("No se encontró ningún objeto con tag 'Player'.");
        }
    }

    public void InstanciarBala()
    {
        if (balaPrefab != null && AlturaArma != null)
        {
            // Instancia la bala en la posici�n de AlturaArmas
            GameObject bala = Instantiate(balaPrefab, AlturaArma.position, Quaternion.identity);

            // Agregar velocidad a la bala (asumiendo que tiene Rigidbody2D o Rigidbody)
            Rigidbody rb = bala.GetComponent<Rigidbody>();
            //if (rb != null)
            //{
            // // Direcci�n hacia la derecha (asumiendo que el enemigo mira a la izquierda/derecha)
            // // Ajusta la direcci�n seg�n necesites
            // rb.linearVelocity = transform.forward * velocidadDisparo;
            //}
            // Direcci�n al objetivo
            Vector3 dir = target.position - AlturaArma.position;
            float h = dir.y; // diferencia de altura
            dir.y = 0f; // solo horizontal
            float dist = dir.magnitude;
            float g = Physics.gravity.magnitude;

            float rad = launchAngle * Mathf.Deg2Rad;

            // elevamos la direcci�n al �ngulo deseado
            dir.y = dist * Mathf.Tan(rad);
            dist += h / Mathf.Tan(rad);

            // velocidad necesaria para llegar con ese �ngulo
            float vel = Mathf.Sqrt(dist * g / Mathf.Sin(2f * rad));

            Vector3 velVector = dir.normalized * vel;

            rb.useGravity = true;
            rb.linearVelocity = velVector;







        }
    }

    void Vacio()
    {

    }


}



