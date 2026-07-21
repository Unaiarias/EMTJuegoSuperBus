using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Behavior;

public class WaveSpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyType
    {
        public GameObject prefab;
        [Range(0f, 100f)] public float weight = 1f;
    }

    [System.Serializable]
    public class MinibossConfig
    {
        public GameObject minibossPrefab;
        public int waveToAppear = 5; // Nivel/oleada en que aparece
        public bool canAppear = true; // Si está habilitado para aparecer
        [Range(0f, 100f)] public float weight = 10f; // Peso menor para que sea más raro
    }

    [System.Serializable]
    public class SpawnArea
    {
        public BoxCollider area;
    }

    public List<EnemyType> enemyTypes = new List<EnemyType>();
    public List<MinibossConfig> minibossConfigs = new List<MinibossConfig>();
    public List<SpawnArea> spawnAreas = new List<SpawnArea>();

    public int maxEnemiesOnScreen = 4;
    public int totalEnemiesToSpawn = 20;

    [Header("Miniboss Settings")]
    public int currentWave = 1; // Nivel/oleada actual
    public bool forceMinibossInWave = false; // Forzar miniboss en esta oleada
    public int minibossCount = 0; // Contador de minibosses spawnados
    public int maxMinibossPerWave = 1; // Máximo de minibosses por oleada

    public Transform player;

    [Tooltip("Distancia mínima entre spawns dentro de la misma área")]
    public float minDistanceBetweenSpawns = 1.5f;

    [Header("Referencia a Fog Controller (NO TOCAR)")]
    public int enemiesSpawnedCount = 0;
    public int enemiesAliveCount = 0;
    private readonly List<Vector3> recentSpawnPositions = new List<Vector3>();
    private bool minibossSpawnedInWave = false;

    [Header("UI")]
    public GameObject player1;
    public GameObject menuHasGanado;
    public GameObject UI_Interfaz;

    [Header("Delay de UI")]
    [SerializeField] private float delayVictoriaPanel = 1f; // Tiempo que tarda en aparecer el panel de victoria
    private Coroutine corutinaVictoria;

    //Bandera para evitar múltiples finalizaciones
    private bool oleadaFinalizada = false;

    //Referencia al FogController =====
    [Header("Niebla")]
    [SerializeField] private FogController fogController;
  
    private void Start()
    {
        // Buscar FogController si no está asignado =====
        if (fogController == null)
        {
            fogController = FindObjectOfType<FogController>();
            if (fogController == null)
            {
                Debug.LogWarning("No se encontró FogController. La niebla no se actualizará.");
            }
        }
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return null;

        while (enemiesSpawnedCount < totalEnemiesToSpawn && !oleadaFinalizada)
        {
            if (enemiesAliveCount >= maxEnemiesOnScreen)
                yield return new WaitUntil(() => enemiesAliveCount < maxEnemiesOnScreen);

            SpawnEnemy();
            yield return null;
        }

        // Esperar a que mueran los últimos enemigos
        yield return new WaitUntil(() => enemiesAliveCount == 0 || oleadaFinalizada);

        //Solo finalizar si el jugador está vivo
        if (!oleadaFinalizada && PlayerVida.IsPlayerAlive)
        {
            OleadaFinalizada();
        }
    }

    private void SpawnEnemy()
    {
        if (enemiesSpawnedCount >= totalEnemiesToSpawn || spawnAreas.Count == 0 || oleadaFinalizada)
            return;

        GameObject prefab = GetEnemyToSpawn();
        if (!prefab)
            return;

        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);

        BehaviorGraphAgent agent = enemy.GetComponent<BehaviorGraphAgent>();
        if (agent != null && player != null)
        {
            agent.SetVariableValue("Target", player.gameObject);
        }

        WaveEnemy we = enemy.GetComponent<WaveEnemy>();
        if (!we)
            we = enemy.AddComponent<WaveEnemy>();

        we.Setup(this);

        // Verificar si es miniboss para etiquetarlo
        if (IsMinibossPrefab(prefab))
        {
            minibossCount++;
            minibossSpawnedInWave = true;
            Debug.Log($"¡Miniboss spawnado! (Oleada {currentWave})");
        }

        enemiesSpawnedCount++;
        enemiesAliveCount++;
    }

    private GameObject GetEnemyToSpawn()
    {
        // Determinar si debemos spawnear un miniboss en esta oleada
        bool shouldSpawnMiniboss = ShouldSpawnMiniboss();

        if (shouldSpawnMiniboss)
        {
            GameObject minibossPrefab = GetAvailableMinibossPrefab();
            if (minibossPrefab != null)
                return minibossPrefab;
        }

        // Si no hay miniboss disponible o no toca, spawnear enemigo normal
        return GetRandomEnemyPrefab();
    }

    private bool ShouldSpawnMiniboss()
    {
        // No spawnear más minibosses si ya alcanzamos el límite por oleada
        if (minibossSpawnedInWave && minibossCount >= maxMinibossPerWave)
            return false;

        // Verificar si algún miniboss está configurado para aparecer en esta oleada
        foreach (var miniboss in minibossConfigs)
        {
            if (miniboss.canAppear && miniboss.waveToAppear == currentWave)
            {
                // Si forzamos miniboss o usamos sistema de peso
                if (forceMinibossInWave)
                    return true;

                // Sistema de probabilidad basado en peso
                float totalWeight = GetTotalEnemyWeight();
                float randomValue = Random.Range(0f, totalWeight);
                float currentWeight = 0f;

                // Primero verificar si toca miniboss segun su peso
                currentWeight += miniboss.weight;
                if (randomValue <= currentWeight)
                    return true;

                break;
            }
        }

        return false;
    }

    private GameObject GetAvailableMinibossPrefab()
    {
        // Filtrar minibosses disponibles para esta oleada
        List<MinibossConfig> availableMinibosses = new List<MinibossConfig>();

        foreach (var miniboss in minibossConfigs)
        {
            if (miniboss.canAppear && miniboss.waveToAppear == currentWave)
            {
                availableMinibosses.Add(miniboss);
            }
        }

        if (availableMinibosses.Count == 0)
            return null;

        // Si hay múltiples minibosses posibles, elegir uno aleatorio
        return availableMinibosses[Random.Range(0, availableMinibosses.Count)].minibossPrefab;
    }

    private GameObject GetRandomEnemyPrefab()
    {
        float totalWeight = 0f;
        foreach (var et in enemyTypes)
            totalWeight += et.weight;

        if (totalWeight <= 0f) return null;

        float r = Random.value * totalWeight;
        float sum = 0f;

        foreach (var et in enemyTypes)
        {
            sum += et.weight;
            if (r <= sum)
                return et.prefab;
        }

        return enemyTypes[0].prefab;
    }

    private float GetTotalEnemyWeight()
    {
        float totalWeight = 0f;

        // Sumar pesos de enemigos normales
        foreach (var et in enemyTypes)
            totalWeight += et.weight;

        // Sumar pesos de minibosses disponibles para esta oleada
        foreach (var miniboss in minibossConfigs)
        {
            if (miniboss.canAppear && miniboss.waveToAppear == currentWave)
                totalWeight += miniboss.weight;
        }

        return totalWeight;
    }

    private bool IsMinibossPrefab(GameObject prefab)
    {
        foreach (var miniboss in minibossConfigs)
        {
            if (miniboss.minibossPrefab == prefab)
                return true;
        }
        return false;
    }

    private Vector3 GetRandomSpawnPosition()
    {
        if (spawnAreas.Count == 0)
            return transform.position;

        for (int i = 0; i < 20; i++)
        {
            SpawnArea selectedArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
            if (selectedArea == null || selectedArea.area == null)
                continue;

            Vector3 pos = GetRandomPointInBoxCollider(selectedArea.area);
            if (IsFarEnoughFromRecentSpawns(pos))
                return pos;
        }

        SpawnArea fallbackArea = spawnAreas[Random.Range(0, spawnAreas.Count)];
        return GetRandomPointInBoxCollider(fallbackArea.area);
    }

    private Vector3 GetRandomPointInBoxCollider(BoxCollider box)
    {
        Vector3 localCenter = box.center;
        Vector3 localSize = box.size;

        Vector3 randomPointLocal = new Vector3(
            Random.Range(-localSize.x * 0.5f, localSize.x * 0.5f),
            Random.Range(-localSize.y * 0.5f, localSize.y * 0.5f),
            Random.Range(-localSize.z * 0.5f, localSize.z * 0.5f)
        ) + localCenter;

        return box.transform.TransformPoint(randomPointLocal);
    }

    private bool IsFarEnoughFromRecentSpawns(Vector3 pos)
    {
        foreach (Vector3 oldPos in recentSpawnPositions)
        {
            if (Vector3.Distance(pos, oldPos) < minDistanceBetweenSpawns)
                return false;
        }
        return true;
    }

    public void EnemyKilled()
    {
        if (!oleadaFinalizada)
        {
            enemiesAliveCount = Mathf.Max(0, enemiesAliveCount - 1);
        }
    }

    public void OleadaFinalizada()
    {
        if (oleadaFinalizada) return;

        oleadaFinalizada = true;
        Debug.Log($"¡Oleada {currentWave} finalizada! Minibosses spawnados: {minibossCount}");

        // Desactivar niebla al ganar 
        if (fogController != null)
        {
            fogController.SetDensidad(0f);
            Debug.Log("Niebla desactivada al ganar el nivel");
        }
  
        //Verificar nuevamente que el jugador está vivo
        if (PlayerVida.IsPlayerAlive)
        {
            // Iniciar corrutina con delay en lugar de mostrar inmediatamente
            if (corutinaVictoria != null)
            {
                StopCoroutine(corutinaVictoria);
            }
            corutinaVictoria = StartCoroutine(MostrarVictoriaConDelay());
        }
        else
        {
            Debug.Log("Jugador muerto, no se muestra menú de victoria");
        }
    }

    private IEnumerator MostrarVictoriaConDelay()
    {
        // Esperar el delay configurado antes de mostrar el panel
        yield return new WaitForSeconds(delayVictoriaPanel);

        // Pausar el tiempo (opcional, igual que en la muerte)
        Time.timeScale = 0f;

        // Mostrar el panel de victoria
        menuHasGanado.SetActive(true);
        UI_Interfaz.SetActive(false);
        player1.SetActive(false);

        Debug.Log($"Victoria mostrada después de {delayVictoriaPanel} segundos");
    }

    //Método para preparar la siguiente oleada
    public void PrepararSiguienteOleada(int siguienteNivel)
    {
        if (corutinaVictoria != null)
        {
            StopCoroutine(corutinaVictoria);
            corutinaVictoria = null;
        }

        currentWave = siguienteNivel;
        enemiesSpawnedCount = 0;
        enemiesAliveCount = 0;
        minibossCount = 0;
        minibossSpawnedInWave = false;
        oleadaFinalizada = false;
        recentSpawnPositions.Clear();

        // Resetear flags de minibosses si es necesario
        foreach (var miniboss in minibossConfigs)
        {
            // Aquí puedes implementar lógica para resetear minibosses específicos
            // Por ejemplo, si quieres que ciertos minibosses solo aparezcan una vez
        }

        StartCoroutine(SpawnLoop());
    }

    //Método para configurar manualmente qué minibosses están activos
    public void SetMinibossActive(int waveNumber, bool active)
    {
        foreach (var miniboss in minibossConfigs)
        {
            if (miniboss.waveToAppear == waveNumber)
            {
                miniboss.canAppear = active;
                Debug.Log($"Miniboss para oleada {waveNumber} {(active ? "activado" : "desactivado")}");
            }
        }
    }

    //Método para forzar miniboss en la oleada actual
    public void ForceMinibossInCurrentWave(bool force)
    {
        forceMinibossInWave = force;
    }

    //Método para detener el spawn si el jugador muere
    public void DetenerSpawnPorMuerteJugador()
    {
        if (corutinaVictoria != null)
        {
            StopCoroutine(corutinaVictoria);
            corutinaVictoria = null;
        }

        oleadaFinalizada = true;
        Debug.Log("Spawn detenido por muerte del jugador");
    }
}