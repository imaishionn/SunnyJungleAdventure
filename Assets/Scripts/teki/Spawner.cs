using UnityEngine;
using System.Collections;

public class Spawner : MonoBehaviour
{
    public ObjectPoolManager objectPoolManager;
    public float spawnInterval = 3f;
    public int spawnCount = 5;

    private int spawnedCount = 0;

    void Start()
    {
        spawnedCount = 0;

        InvokeRepeating("SpawnEnemy", 0f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (spawnedCount >= spawnCount)
        {
            CancelInvoke("SpawnEnemy");
            return;
        }

        GameObject bat = objectPoolManager.GetEnemyFromPool();
        if (bat != null)
        {
            // Random‚Ì‘O‚ÉUnityEngine.‚ð•t‚¯‚Ü‚µ‚½
            float offsetX = UnityEngine.Random.Range(-1f, 1f);
            float offsetY = UnityEngine.Random.Range(-1f, 1f);
            Vector3 spawnPosition = transform.position + new Vector3(offsetX, offsetY, 0);

            bat.transform.position = spawnPosition;
            spawnedCount++;
        }
    }
}