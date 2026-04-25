using UnityEngine;

public class BombaEnemigo : MonoBehaviour
{
    [Header("Particle Effects")]
    [SerializeField] private GameObject explosionParticlePrefab;
    [SerializeField] private Transform explosionSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;

    private void Start()
    {
        if (explosionSpawnPoint == null)
        {
            explosionSpawnPoint = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            InstanciarParticulaExplosion();
            ReproducirSonidoExplosion();
            GetComponent<Enemigo>()?.MorirEnemigo();
        }
    }

    private void InstanciarParticulaExplosion()
    {
        if (explosionParticlePrefab != null && explosionSpawnPoint != null)
        {
            GameObject particleInstance = Instantiate(explosionParticlePrefab, explosionSpawnPoint.position, explosionSpawnPoint.rotation);

            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                Destroy(particleInstance, particleSystem.main.duration + 0.5f);
            }
            else
            {
                Destroy(particleInstance, 3f);
            }
        }
    }

    private void ReproducirSonidoExplosion()
    {
        if (explosionSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(explosionSound, transform.position);
            Debug.Log("Reproduciendo sonido de explosión");
        }
    }
}