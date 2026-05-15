using UnityEngine;

public class BalaScript : MonoBehaviour
{
    private float timer = 0;
    private float tiempoDes = 5;

    [Header("Particle Effects")]
    [SerializeField] private GameObject impactoPlayerParticlePrefab;
    [SerializeField] private Transform impactoPlayerSpawnPoint;

    //Partícula para impacto contra el suelo
    [SerializeField] private GameObject impactoGroundParticlePrefab;
    [SerializeField] private Transform impactoGroundSpawnPoint;

    [Header("Audio")]
    [SerializeField] private AudioClip impactoPlayerSound;

    [Header("Ground Settings")]
    [SerializeField] private LayerMask groundLayer; // Asignar la capa "Ground" en el Inspector

 

    private void Start()
    {
        // Si no se asignó un punto de spawn, usar el transform de la bala
        if (impactoPlayerSpawnPoint == null)
        {
            impactoPlayerSpawnPoint = transform;
        }

        // Configurar punto de spawn para impacto de suelo
        if (impactoGroundSpawnPoint == null)
        {
            impactoGroundSpawnPoint = transform;
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
        // Impactar con suelo - Usando Layer (sin Tag)
        if (groundLayer == (groundLayer | (1 << other.gameObject.layer)))
        {
            InstanciarParticulaImpactoSuelo();
            Destroy(gameObject);
        }

        // Impactar con el player
        if (other.CompareTag("Player"))
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
            GameObject particleInstance = Instantiate(impactoPlayerParticlePrefab, impactoPlayerSpawnPoint.position, impactoPlayerSpawnPoint.rotation);

            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleInstance, duration + 0.5f);
            }
            else
            {
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

    //Método para instanciar partícula de impacto contra el suelo
    private void InstanciarParticulaImpactoSuelo()
    {
        if (impactoGroundParticlePrefab != null && impactoGroundSpawnPoint != null)
        {
            GameObject particleInstance = Instantiate(impactoGroundParticlePrefab, impactoGroundSpawnPoint.position, impactoGroundSpawnPoint.rotation);

            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleInstance, duration + 0.5f);
            }
            else
            {
                Destroy(particleInstance, 2f);
            }

            Debug.Log($"Partícula de impacto contra suelo instanciada en {impactoGroundSpawnPoint.position}");
        }
        else
        {
            if (impactoGroundParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para el impacto contra el suelo");
        }
    }

    private void ReproducirSonidoImpacto()
    {
        if (impactoPlayerSound != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(impactoPlayerSound, transform.position);
            Debug.Log("Reproduciendo sonido de impacto al player con AudioManager");
        }
        else if (impactoPlayerSound != null)
        {
            // Fallback si no hay AudioManager
            AudioSource.PlayClipAtPoint(impactoPlayerSound, transform.position, 1f);
            Debug.Log("Reproduciendo sonido de impacto al player con PlayClipAtPoint (fallback)");
        }
        else
        {
            Debug.LogWarning("No se asignó el clip de sonido de impacto al player");
        }
    }
}