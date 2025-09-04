using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// オブジェクトプールを管理
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    // シングルトンインスタンス
    public static ObjectPoolManager Instance { get; private set; }

    [Header("プール設定")]
    [Tooltip("プールするゲームオブジェクトのPrefab")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("事前に生成しておくオブジェクトの数")]
    [SerializeField] private int poolSize = 10;

    private List<GameObject> enemyPool;

    private void Awake()
    {
        // シングルトンパターンの実装（DontDestroyOnLoadを削除）
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        InitializePool();
    }

    private void OnDestroy()
    {
        // オブジェクトが破棄される際に、シングルトンインスタンスをクリア
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void InitializePool()
    {
        enemyPool = new List<GameObject>();
        if (enemyPrefab == null)
        {
            Debug.LogError("ObjectPoolManager: enemyPrefabが設定されていません。プールの初期化を中止します。", this);
            return;
        }
        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            if (enemy == null)
            {
                Debug.LogError($"ObjectPoolManager: {i + 1}番目の敵の生成に失敗しました。Prefabが壊れている可能性があります。", this);
                continue;
            }
            enemy.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    public GameObject GetEnemyFromPool()
    {
        foreach (GameObject enemy in enemyPool)
        {
            if (enemy != null && !enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                return enemy;
            }
        }

        if (enemyPrefab == null)
        {
            Debug.LogError("ObjectPoolManager: enemyPrefabが設定されていません。", this);
            return null;
        }
        GameObject newEnemy = Instantiate(enemyPrefab);
        if (newEnemy == null)
        {
            Debug.LogError("ObjectPoolManager: 新しい敵の生成に失敗しました。", this);
            return null;
        }
        enemyPool.Add(newEnemy);
        Debug.LogWarning("ObjectPoolManager: プールが枯渇しました。新しくオブジェクトを生成しました。", this);
        return newEnemy;
    }
}