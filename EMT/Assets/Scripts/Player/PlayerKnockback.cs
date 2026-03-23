using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackUp = 0.5f;      // subida inicial
    public float knockbackDuration = 0.25f;

    private Vector3 knockbackDir;
    private float knockbackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // direction = ya es hacia atrás del enemigo (transform.forward)
    public void ApplyKnockback(Vector3 direction)
    {
        // mantenemos la dirección hacia atrás
        Vector3 dir = direction;

        // añadir un poco hacia arriba para que suba en diagonal
        dir += Vector3.up * knockbackUp;
        dir.Normalize();

        knockbackDir = dir;
        knockbackTimer = knockbackDuration;
    }

    public bool IsKnockbackActive() => knockbackTimer > 0f;

    private void FixedUpdate()
    {
        if (knockbackTimer > 0f)
        {
            // empuje suave durante todo el tiempo
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.VelocityChange);

            // justo al final del empuje, forzamos que baje un poco
            if (knockbackTimer < 0.05f)
            {
                // resetear la velocidad vertical a 0 (o ligeramente negativa)
                Vector3 v = rb.linearVelocity;
                v.y = -1f; // ligeramente hacia abajo para que caiga rápido
                rb.linearVelocity = v;
            }

            knockbackTimer -= Time.fixedDeltaTime;
        }
    }
}
