using UnityEngine;
using System.Collections.Generic;
using Debug = UnityEngine.Debug;

/// <summary>
/// オブジェクトプールを管理するシングルトンクラスです。
/// 敵や弾丸など、頻繁に生成・破棄されるオブジェクトのパフォーマンスを向上させます。
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // シングルトンインスタンス
    // ----------------------------------------------------------------------------------------------------
    public static ObjectPoolManager Instance { get; private set; }

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プール設定")]
    [Tooltip("プールするゲームオブジェクトのPrefab")]
    [SerializeField] private GameObject enemyPrefab;
    [Tooltip("事前に生成しておくオブジェクトの数")]
    [SerializeField] private int poolSize = 10;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private List<GameObject> enemyPool;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake()
    {
        // シングルトンパターンの実装
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this.gameObject); // シーンをまたいで存在させる

        InitializePool();
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// オブジェクトプールを初期化し、指定された数のオブジェクトを生成します。
    /// </summary>
    private void InitializePool()
    {
        // プールリストを初期化
        enemyPool = new List<GameObject>();

        // 指定された数だけオブジェクトを生成し、プールに追加
        for (int i = 0; i < poolSize; i++)
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("ObjectPoolManager: enemyPrefabが設定されていません。", this);
                return;
            }
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.SetActive(false);
            enemyPool.Add(enemy);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プールから利用可能なオブジェクトを取得します。
    /// 利用可能なオブジェクトがない場合、新しく生成してプールに追加します。
    /// </summary>
    /// <returns>プールから取得したGameObject</returns>
    public GameObject GetEnemyFromPool()
    {
        // プール内のオブジェクトを探索
        foreach (GameObject enemy in enemyPool)
        {
            // オブジェクトがアクティブでなければそれを返す
            if (enemy != null && !enemy.activeInHierarchy)
            {
                enemy.SetActive(true);
                return enemy;
            }
        }

        // プールに利用可能なオブジェクトがない場合、新しく生成
        if (enemyPrefab == null)
        {
            Debug.LogError("ObjectPoolManager: enemyPrefabが設定されていません。", this);
            return null;
        }
        GameObject newEnemy = Instantiate(enemyPrefab);
        enemyPool.Add(newEnemy);
        Debug.LogWarning("ObjectPoolManager: プールが枯渇しました。新しくオブジェクトを生成しました。", this);
        return newEnemy;
    }
}