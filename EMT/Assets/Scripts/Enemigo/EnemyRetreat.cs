using UnityEngine;

public class EnemyRetreat : MonoBehaviour
{
    [Header("Retreat Settings")]
    public float retreatDistance = 3f;
    public float retreatSpeed = 4f;
    public float retreatDuration = 0.5f; // Tiempo que dura la retirada
    public float retreatCooldown = 2f;   // Tiempo que debe esperar antes de otra retirada

    [Header("Detection")]
    public float retreatTriggerDistance = 2f; // Distancia para activar retirada

    private Transform target;
    private bool isRetreating = false;
    private bool onCooldown = false;
    private float retreatTimer = 0f;
    private float cooldownTimer = 0f;
    private Vector3 retreatDirection;
    private float initialDistance;

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

    void Update()
    {
        // Manejar el cooldown
        if (onCooldown)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                onCooldown = false;
            }
        }

        // Manejar la retirada activa
        if (isRetreating)
        {
            retreatTimer -= Time.deltaTime;

            if (retreatTimer > 0)
            {
                // Movimiento de retirada
                transform.position += retreatDirection * retreatSpeed * Time.deltaTime;
            }
            else
            {
                // Terminar retirada
                isRetreating = false;
            }
        }
    }

    public bool TryStartRetreat(Transform newTarget)
    {
        // No se puede retirar si ya está en retirada o en cooldown
        if (isRetreating || onCooldown) return false;

        target = newTarget;

        // Verificar si realmente está muy cerca
        float distanceToTarget = Vector3.Distance(transform.position, target.position);
        if (distanceToTarget > retreatTriggerDistance) return false;

        // Iniciar retirada
        isRetreating = true;
        retreatTimer = retreatDuration;

        // Calcular dirección de retirada (alejarse del jugador)
        retreatDirection = (transform.position - target.position).normalized;

        // Iniciar cooldown
        onCooldown = true;
        cooldownTimer = retreatCooldown;

        return true;
    }

    public bool IsRetreatFinished => !isRetreating;
    public bool IsOnCooldown => onCooldown;

    // Método opcional para forzar el fin de la retirada
    public void ForceStopRetreat()
    {
        isRetreating = false;
    }
}