using UnityEngine;

public class Spawner : MonoBehaviour
{
    // ObjectPoolManagerのインスタンスをインスペクターで設定
    public ObjectPoolManager objectPoolManager;
    // 敵を出現させる間隔
    public float spawnInterval = 3f;
    // 敵を出現させる回数
    public int spawnCount = 5;

    private int spawnedCount = 0;

    void Start()
    {
        // 敵の出現を一定時間ごとに繰り返す
        InvokeRepeating("SpawnEnemy", 0f, spawnInterval);
    }

    void SpawnEnemy()
    {
        if (spawnedCount >= spawnCount)
        {
            // 指定回数出現させたら終了
            CancelInvoke("SpawnEnemy");
            return;
        }

        // プールから敵を取得
        GameObject bat = objectPoolManager.GetEnemyFromPool();
        if (bat != null)
        {
            // 敵の位置をスポナーの位置に設定
            bat.transform.position = transform.position;
            spawnedCount++;
        }
    }
}