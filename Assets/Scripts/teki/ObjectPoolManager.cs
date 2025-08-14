using UnityEngine;
using System.Collections.Generic;

public class ObjectPoolManager : MonoBehaviour
{
    // プールする敵のプレハブ
    public GameObject enemyPrefab;
    // プールする敵の数
    public int poolSize = 10;

    // プールされた敵を格納するリスト
    private List<GameObject> enemyPool;

    void Awake()
    {
        enemyPool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            // 敵を生成し、非アクティブにする
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    public GameObject GetEnemyFromPool()
    {
        for (int i = 0; i < enemyPool.Count; i++)
        {
            // オブジェクトがnullでないか確認
            if (enemyPool[i] != null && !enemyPool[i].activeInHierarchy)
            {
                enemyPool[i].SetActive(true);
                return enemyPool[i];
            }
        }

        // オプション: プールに利用可能な敵がない場合、新たに生成して返す
        GameObject newEnemy = Instantiate(enemyPrefab);
        enemyPool.Add(newEnemy);
        return newEnemy;
    }
}