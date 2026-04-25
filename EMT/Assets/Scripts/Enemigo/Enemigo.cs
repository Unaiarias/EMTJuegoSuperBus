using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Enemigo : MonoBehaviour
{
    public int vidaActualEnemigo = 100;
    public int vidaEnemigoMaxima = 100;
    public SistemaOleadas spawner;

    public GameObject monedaPrefab;

    [SerializeField] private PlayerVida playerVida;

    [Header("Particle Effects")]
    [SerializeField] private GameObject hitToEnemyParticlePrefab;
    [SerializeField] private Transform hitToEnemySpawnPoint;

    [Header("Death Particle Effect")]
    [SerializeField] private GameObject deathParticlePrefab; // Prefab de partículas al morir
    [SerializeField] private Transform deathSpawnPoint; // Punto donde salen las partículas de muerte

    [Header("Footstep Particles")]
    [SerializeField] private GameObject footstepParticlePrefab; // Prefab de partículas al caminar
    [SerializeField] private Transform footstepSpawnPoint; // Punto donde salen las partículas (ej: pies)
    [SerializeField] private float footstepInterval = 0.5f; // Intervalo entre partículas
    [SerializeField] private float minSpeedForFootsteps = 0.5f; // Velocidad mínima para partículas

    private Rigidbody rb;
    private float footstepTimer;
    private bool wasMoving;
    private Vector3 lastPosition;

    private void Start()
    {
        if (playerVida == null)
        {
            playerVida = FindObjectOfType<PlayerVida>();
        }

        if (playerVida == null)
        {
            Debug.LogWarning("No se encontró PlayerVida en la escena.");
        }

        // Si no se asignó un punto de spawn, usar el transform del enemigo
        if (hitToEnemySpawnPoint == null)
        {
            hitToEnemySpawnPoint = transform;
        }

        // Configurar punto de spawn para partículas de muerte
        if (deathSpawnPoint == null)
        {
            deathSpawnPoint = transform;
        }

        // Configurar punto de spawn para partículas de caminar
        if (footstepSpawnPoint == null)
        {
            footstepSpawnPoint = transform;
        }

        // Obtener Rigidbody
        rb = GetComponent<Rigidbody>();
        lastPosition = transform.position;
        footstepTimer = 0f;
        wasMoving = false;
    }

    private void Update()
    {
        // Manejar partículas al caminar
        HandleFootstepParticles();
    }

    private void HandleFootstepParticles()
    {
        // Calcular si el enemigo se está moviendo
        float currentSpeed = Vector3.Distance(transform.position, lastPosition) / Time.deltaTime;
        lastPosition = transform.position;

        bool isMoving = currentSpeed > minSpeedForFootsteps;

        if (isMoving && footstepParticlePrefab != null)
        {
            footstepTimer -= Time.deltaTime;

            if (footstepTimer <= 0f)
            {
                SpawnFootstepParticle();
                footstepTimer = footstepInterval;
                wasMoving = true;
            }
        }
        else
        {
            if (wasMoving)
            {
                footstepTimer = 0f;
                wasMoving = false;
            }
        }
    }

    private void SpawnFootstepParticle()
    {
        if (footstepParticlePrefab != null && footstepSpawnPoint != null)
        {
            GameObject particleInstance = Instantiate(footstepParticlePrefab, footstepSpawnPoint.position, footstepSpawnPoint.rotation);

            // Auto-destruir el efecto
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
        }
    }

    public void RecibirDanoEnemigo(int cantidadDano)
    {
        vidaActualEnemigo -= cantidadDano;

        // Instanciar partícula de daño
        InstanciarParticulaDano();

        if (playerVida != null)
        {
            playerVida.AumentarCombo();
        }

        if (vidaActualEnemigo <= 0)
        {
            MorirEnemigo();
        }
    }

    private void InstanciarParticulaDano()
    {
        if (hitToEnemyParticlePrefab != null && hitToEnemySpawnPoint != null)
        {
            GameObject particleInstance = Instantiate(hitToEnemyParticlePrefab, hitToEnemySpawnPoint.position, hitToEnemySpawnPoint.rotation);

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

            Debug.Log($"Partícula de daño instanciada en {hitToEnemySpawnPoint.position}");
        }
        else
        {
            if (hitToEnemyParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para el daño al enemigo");
        }
    }

    private void InstanciarParticulaMuerte()
    {
        if (deathParticlePrefab != null && deathSpawnPoint != null)
        {
            GameObject particleInstance = Instantiate(deathParticlePrefab, deathSpawnPoint.position, deathSpawnPoint.rotation);

            // Auto-destruir el efecto después de que termine
            ParticleSystem particleSystem = particleInstance.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration;
                Destroy(particleInstance, duration + 0.5f);
                Debug.Log($"Partícula de muerte instanciada en {deathSpawnPoint.position}");
            }
            else
            {
                // Si no tiene ParticleSystem, destruir después de 3 segundos
                Destroy(particleInstance, 3f);
            }
        }
        else
        {
            if (deathParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para la muerte del enemigo");
        }
    }

    public void MorirEnemigo()
    {
        // Instanciar partícula de muerte ANTES de destruir el enemigo
        InstanciarParticulaMuerte();

        if (spawner != null)
        {
            spawner.NotificarMuerteEnemigo();
        }

        if (monedaPrefab != null)
        {
            Instantiate(monedaPrefab, transform.position + Vector3.up * 1f, transform.rotation);
        }

        GetComponent<WaveEnemy>()?.OnDeath();

        // Destruir el enemigo
        Destroy(gameObject);
        Debug.Log("Enemigo Muerto");
    }
}