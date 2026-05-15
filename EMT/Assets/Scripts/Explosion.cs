using UnityEngine;

public class ImpactoProyectil : MonoBehaviour
{
    [Header("Efectos Visuales")]
    [Tooltip("Arrastra aquí tu Prefab de la explosión (VFX Graph)")]
    public GameObject explosionPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }
}