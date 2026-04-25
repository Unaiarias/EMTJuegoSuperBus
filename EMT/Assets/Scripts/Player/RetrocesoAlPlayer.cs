using UnityEngine;

public class RetrocesoAlPlayer : MonoBehaviour
{
    public int danoPorGolpe = 30;

    [Header("Particle Effects")]
    [SerializeField] private GameObject damageParticlePrefab; // Prefab de partículas de daño
    [SerializeField] private Transform damageSpawnPoint; // Punto donde salen las partículas

    private void Start()
    {
        // Si no se asignó un punto de spawn, usar el transform del objeto que daña
        if (damageSpawnPoint == null)
        {
            damageSpawnPoint = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Daño al player");

            PlayerVida playerVida = other.GetComponent<PlayerVida>();
            PlayerAtaque playerAtaque = other.GetComponent<PlayerAtaque>();
            PlayerKnockback knock = other.GetComponent<PlayerKnockback>();

            if (playerAtaque != null && playerAtaque.isBarrera)
            {
                Debug.Log("El player tiene la barrera activa, no recibe daño");
                return;
            }

            // Instanciar partícula de daño ANTES de aplicar el daño
            InstanciarParticulaDaño(other.transform);

            if (playerVida != null)
            {
                playerVida.RecibirDanoPlayer(danoPorGolpe);
            }

            if (knock != null)
            {
                // Dirección hacia ATRÁS del enemigo
                Vector3 knockDir = transform.forward;
                knockDir.y = 0f;
                knockDir.Normalize();

                knock.ApplyKnockback(knockDir);
            }
        }
    }

    private void InstanciarParticulaDaño(Transform playerTransform)
    {
        // Determinar el punto de spawn
        Vector3 spawnPosition = damageSpawnPoint != null ? damageSpawnPoint.position : playerTransform.position;
        Quaternion spawnRotation = damageSpawnPoint != null ? damageSpawnPoint.rotation : Quaternion.identity;

        if (damageParticlePrefab != null)
        {
            // Instanciar la partícula de daño
            GameObject particleInstance = Instantiate(damageParticlePrefab, spawnPosition, spawnRotation);

            // Auto-destruir el efecto después de que termine
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleInstance, duration + 0.5f);
            }
            else
            {
                // Si no tiene ParticleSystem, destruir después de 2 segundos
                Destroy(particleInstance, 2f);
            }

            Debug.Log($"Partícula de daño instanciada en {spawnPosition}");
        }
        else
        {
            if (damageParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para el daño al player");
        }
    }
}