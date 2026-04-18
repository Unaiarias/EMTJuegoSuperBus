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
        if (balaPrefab != null && AlturaArma != null && target != null)
        {
            GameObject bala = Instantiate(balaPrefab, AlturaArma.position, Quaternion.identity);

            Rigidbody rb = bala.GetComponent<Rigidbody>();

            Vector3 dir = target.position - AlturaArma.position;
            float h = dir.y;
            dir.y = 0f;
            float dist = dir.magnitude;
            float g = Physics.gravity.magnitude;

            float rad = launchAngle * Mathf.Deg2Rad;

            dir.y = dist * Mathf.Tan(rad);
            dist += h / Mathf.Tan(rad);

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




