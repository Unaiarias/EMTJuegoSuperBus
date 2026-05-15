using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    private Rigidbody rb;

    [Header("Knockback")]
    public float knockbackForce = 10f;
    public float knockbackUp = 0.5f;
    public float knockbackDuration = 0.25f;

    private Vector3 knockbackDir;
    private float knockbackTimer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Método que YA usas con el otro enemigo
    public void ApplyKnockback(Vector3 direction)
    {
        Vector3 dir = direction;
        dir += Vector3.up * knockbackUp;
        dir.Normalize();

        knockbackDir = dir;
        knockbackTimer = knockbackDuration;
    }

    // Método nuevo para las balas: empuja hacia el enemigo
    public void ApplyKnockbackTowards(Vector3 targetPosition)
    {
        Vector3 dir = targetPosition - transform.position;
        dir.y = 0f;
        dir.Normalize();

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
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.VelocityChange);

            if (knockbackTimer < 0.05f)
            {
                Vector3 v = rb.linearVelocity;
                v.y = -1f;
                rb.linearVelocity = v;
            }

            knockbackTimer -= Time.fixedDeltaTime;
        }
    }
}