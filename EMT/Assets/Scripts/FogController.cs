using UnityEngine;

public class FogController : MonoBehaviour
{
    [Header("Configuración de Niebla")]
    [SerializeField] private float densidadInicial = 0.03f;
    [SerializeField] private float densidadFinal = 0f;

    [Header("Referencias")]
    [SerializeField] private WaveSpawner waveSpawner;

    // Variables internas
    private float densidadActual;
    private float densidadPorEnemigo;
    private int enemigosTotales;
    private int enemigosRestantes;
    private bool nieblaActivada = true;

    void Start()
    {
        // Activar la niebla si no está activada
        if (!RenderSettings.fog)
        {
            RenderSettings.fog = true;
            Debug.Log("Niebla activada por FogController");
        }

        // Buscar WaveSpawner si no está asignado
        if (waveSpawner == null)
        {
            waveSpawner = FindObjectOfType<WaveSpawner>();
            if (waveSpawner == null)
            {
                Debug.LogWarning("No se encontró WaveSpawner. La niebla no se actualizará automáticamente.");
                return;
            }
        }

        // Obtener el total de enemigos
        enemigosTotales = waveSpawner.totalEnemiesToSpawn;

        // Calcular cuánto baja la densidad por cada enemigo muerto
        densidadPorEnemigo = (densidadInicial - densidadFinal) / enemigosTotales;

        // Establecer densidad inicial
        densidadActual = densidadInicial;
        RenderSettings.fogDensity = densidadActual;

        Debug.Log($"Niebla inicializada: Density={densidadActual}, Enemigos totales={enemigosTotales}");
    }

    void Update()
    {
        if (waveSpawner == null) return;

        // Calcular enemigos restantes (vivos + los que faltan por spawnear)
        int enemigosSpawned = waveSpawner.enemiesSpawnedCount;
        int enemigosVivos = waveSpawner.enemiesAliveCount;

        // Enemigos que han muerto = spawnados - vivos
        int enemigosMuertos = enemigosSpawned - enemigosVivos;

        // Calcular la densidad actual basada en enemigos muertos
        float nuevaDensidad = densidadInicial - (enemigosMuertos * densidadPorEnemigo);
        nuevaDensidad = Mathf.Clamp(nuevaDensidad, densidadFinal, densidadInicial);

        // Actualizar si ha cambiado
        if (Mathf.Abs(nuevaDensidad - densidadActual) > 0.0001f)
        {
            densidadActual = nuevaDensidad;
            RenderSettings.fogDensity = densidadActual;
            // Debug.Log($"Niebla actualizada: {densidadActual} (Enemigos muertos: {enemigosMuertos}/{enemigosTotales})");
        }
    }

    // Método público para cambiar la densidad manualmente
    public void SetDensidad(float nuevaDensidad)
    {
        densidadActual = Mathf.Clamp(nuevaDensidad, densidadFinal, densidadInicial);
        RenderSettings.fogDensity = densidadActual;
    }

    // Método público para resetear la niebla
    public void ResetNiebla()
    {
        densidadActual = densidadInicial;
        RenderSettings.fogDensity = densidadActual;
    }

    // Método para desactivar la niebla completamente
    public void DesactivarNiebla()
    {
        RenderSettings.fog = false;
        nieblaActivada = false;
    }

    // Método para activar la niebla
    public void ActivarNiebla()
    {
        RenderSettings.fog = true;
        nieblaActivada = true;
        RenderSettings.fogDensity = densidadActual;
    }

    // Método para obtener la densidad actual
    public float GetDensidadActual()
    {
        return densidadActual;
    }
}