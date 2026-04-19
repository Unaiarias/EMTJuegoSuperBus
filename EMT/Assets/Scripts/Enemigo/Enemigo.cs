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
            // Instanciar la partícula en la posición del punto de spawn
            GameObject particleInstance = Instantiate(hitToEnemyParticlePrefab, hitToEnemySpawnPoint.position, hitToEnemySpawnPoint.rotation);

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

            Debug.Log($"Partícula de daño instanciada en {hitToEnemySpawnPoint.position}");
        }
        else
        {
            if (hitToEnemyParticlePrefab == null)
                Debug.LogWarning("No se asignó un prefab de partículas para el daño al enemigo");
        }
    }

    public void MorirEnemigo()
    {
        if (spawner != null)
        {
            spawner.NotificarMuerteEnemigo();
        }

        if (monedaPrefab != null)
        {
            Instantiate(monedaPrefab, transform.position + Vector3.up * 1f, transform.rotation);
        }
        GetComponent<WaveEnemy>()?.OnDeath();
        Destroy(gameObject);
        Debug.Log("Enemigo Muerto");
    }
}