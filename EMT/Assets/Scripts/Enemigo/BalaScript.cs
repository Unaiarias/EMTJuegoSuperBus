using UnityEngine;

public class BalaScript : MonoBehaviour
{
    private float timer = 0;
    private float tiempoDes = 5;

    [Header("Particle Effects")]
    [SerializeField] private GameObject impactoPlayerParticlePrefab; // Prefab de partículas al impactar con el player
    [SerializeField] private Transform impactoPlayerSpawnPoint; // Punto donde salen las partículas

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip impactoPlayerSound; // Sonido de impacto al player

    private void Start()
    {
        // Si no se asignó un punto de spawn, usar el transform de la bala
        if (impactoPlayerSpawnPoint == null)
        {
            impactoPlayerSpawnPoint = transform;
        }

        // Configurar AudioSource si no existe
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
    }

    private void Update()
    {
        if (timer < tiempoDes)
        {
            timer += Time.deltaTime;
        }
        if (timer >= tiempoDes)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Impactar con suelo (layer 6)
        if (other.gameObject.layer == 6)
        {
            Destroy(gameObject);
        }

        // Impactar con el player
        if (other.gameObject.tag == "Player")
        {
            InstanciarParticulaImpacto();
            ReproducirSonidoImpacto();
            Destroy(gameObject);
        }
    }

    private void InstanciarParticulaImpacto()
    {
        if (impactoPlayerParticlePrefab != null && impactoPlayerSpawnPoint != null)
        {
            // Instanciar la partícula en la posición del punto de spawn
            GameObject particleInstance = Instantiate(impactoPlayerParticlePrefab, impactoPlayerSpawnPoint.position, impactoPlayerSpawnPoint.rotation);

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

            Debug.Log($"Partícula de impacto instanciada en {impactoPlayerSpawnPoint.position}");
        }
        else
        {
            if (impactoPlayerParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para el impacto al player");
        }
    }

    private void ReproducirSonidoImpacto()
    {
        if (impactoPlayerSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(impactoPlayerSound);
            Debug.Log("Reproduciendo sonido de impacto al player");
        }
    }
}