using UnityEngine;

public class BalaEmpujeHaciaEnemigo : MonoBehaviour
{
   

    [Header("Enemy Target")]
    [SerializeField] private Transform enemigoDestino;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerKnockback knock = other.GetComponent<PlayerKnockback>();
        if (knock != null && enemigoDestino != null)
        {
            knock.ApplyKnockbackTowards(enemigoDestino.position);
        }
    }
}

