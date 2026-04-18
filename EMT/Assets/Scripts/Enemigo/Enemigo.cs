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
    }

    public void RecibirDanoEnemigo(int cantidadDano)
    {
        vidaActualEnemigo -= cantidadDano;

        if (playerVida != null)
        {
            playerVida.AumentarCombo();
        }

        if (vidaActualEnemigo <= 0)
        {
            MorirEnemigo();
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