using UnityEngine;

public class BombaEnemigo : MonoBehaviour
{
    [Header("Particle Effects")]
    [SerializeField] private GameObject explosionParticlePrefab;
    [SerializeField] private Transform explosionSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip explosionSound;
    [SerializeField][Range(0f, 1f)] private float volumenExplosion = 0.7f;

    private bool explotando = false;

    private void Start()
    {
        if (explosionSpawnPoint == null)
        {
            explosionSpawnPoint = transform;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (explotando) return;

        if (other.CompareTag("Player"))
        {
            Enemigo enemigo = GetComponent<Enemigo>();

            // Verificar si la bomba ya está muerta
            if (enemigo != null && enemigo.vidaActualEnemigo <= 0)
            {
                Debug.Log("Bomba ya está muerta, no explota");
                return;
            }

            explotando = true;

            // Instanciar partícula de explosión
            InstanciarParticulaExplosion();

            // Reproducir sonido de explosión
            ReproducirSonidoExplosion();

            // Aplicar daño a la bomba (para que el jugador pueda matarla)
            if (enemigo != null)
            {
                PlayerVida playerVida = other.GetComponent<PlayerVida>();
                if (playerVida != null)
                {
                    int dañoJugador = playerVida.DanoActual;
                    Debug.Log($"Bomba recibe {dañoJugador} de daño del jugador");
                    enemigo.RecibirDanoEnemigo(dañoJugador);
                }
            }

            // NO aplicar daño al jugador aquí - Lo hará el script DañoAlPlayer
            // Solo destruir la bomba después de un pequeño delay
            Invoke(nameof(DestruirBomba), 0.1f);
        }
    }

    private void DestruirBomba()
    {
        Enemigo enemigo = GetComponent<Enemigo>();
        if (enemigo != null)
        {
            enemigo.MorirEnemigo();
        }
        else
        {
            Destroy(gameObject);
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
            AudioManager.Instance.PlaySFX(explosionSound, transform.position, volumenExplosion);
            Debug.Log($"Reproduciendo sonido de explosión con volumen: {volumenExplosion}");
        }
        else if (explosionSound != null)
        {
            AudioSource.PlayClipAtPoint(explosionSound, transform.position, volumenExplosion);
            Debug.Log($"Reproduciendo sonido de explosión con PlayClipAtPoint (volumen: {volumenExplosion})");
        }
        else
        {
            Debug.LogWarning("No se asignó el clip de sonido de explosión");
        }
    }
}