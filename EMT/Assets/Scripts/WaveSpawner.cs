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
    public class SpawnArea
    {
        public BoxCollider area;
    }

    public List<EnemyType> enemyTypes = new List<EnemyType>();
    public List<SpawnArea> spawnAreas = new List<SpawnArea>();

    public int maxEnemiesOnScreen = 4;
    public int totalEnemiesToSpawn = 20;

    public Transform player;

    [Tooltip("Distancia mínima entre spawns dentro de la misma área")]
    public float minDistanceBetweenSpawns = 1.5f;

    private int enemiesSpawnedCount = 0;
    private int enemiesAliveCount = 0;
    private readonly List<Vector3> recentSpawnPositions = new List<Vector3>();

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private IEnumerator SpawnLoop()
    {
        yield return null; // así el frame se completa antes de empezar a esperar

        while (enemiesSpawnedCount < totalEnemiesToSpawn)
        {
            if (enemiesAliveCount >= maxEnemiesOnScreen)
                yield return new WaitUntil(() => enemiesAliveCount < maxEnemiesOnScreen);

            SpawnEnemy();
            yield return null;
        }
        OleadaFinalizada();
    }

    private void SpawnEnemy()
    {
        if (enemiesSpawnedCount >= totalEnemiesToSpawn || spawnAreas.Count == 0)
            return;

        GameObject prefab = GetRandomEnemyPrefab();
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

        enemiesSpawnedCount++;
        enemiesAliveCount++;
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
        enemiesAliveCount = Mathf.Max(0, enemiesAliveCount - 1);
    }

    public void OleadaFinalizada()
    {
        Debug.Log("¡Oleada finalizada!");
    }
}