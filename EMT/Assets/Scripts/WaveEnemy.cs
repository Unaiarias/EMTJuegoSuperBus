using UnityEngine;

public class WaveEnemy : MonoBehaviour
{
    private WaveSpawner spawner;

    public void Setup(WaveSpawner s)
    {
        spawner = s;
    }

    public void OnDeath()
    {
        if (spawner != null)
            spawner.EnemyKilled();

        Destroy(gameObject);
    }
}