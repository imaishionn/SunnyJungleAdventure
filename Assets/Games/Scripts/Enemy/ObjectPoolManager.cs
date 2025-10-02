using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// オブジェクトプールを管理するシングルトンクラスです。
/// </summary>
public class ObjectPoolManager : MonoBehaviour {

    /// <summary>
    /// インスタンス
    /// </summary>
    public static ObjectPoolManager Instance { get; private set; }

    [Header("オブジェクトプール（大量のオブジェクト）の設定"), SerializeField]
    private GameObject _enemyPrefab;

    [Header("プールの初期サイズ"), SerializeField]
    private int _poolSize = 10;

    /// <summary>
    /// 敵オブジェクトのプール 
    /// </summary>
    private List<GameObject> _enemyPool;

    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        InitializePool();
    }

    private void OnDestroy() {
        if (Instance == this) {
            Instance = null;
        }
    }

    private void InitializePool() {
        if (_enemyPrefab == null) {
            Debug.LogError("ObjectPoolManager: 敵のプレハブが割り当てられていません。プールの初期化を中止します。", this);
            return;
        }

        _enemyPool = new List<GameObject>();
        for (int i = 0; i < _poolSize; i++) {
            GameObject enemy = Instantiate(_enemyPrefab, transform);
            enemy.SetActive(false);
            _enemyPool.Add(enemy);
        }
    }

    /// <summary>
    /// プールから使用可能な敵オブジェクトを取得します。
    /// </summary>
    /// <returns>使用可能な敵オブジェクト</returns>
    public GameObject GetEnemyFromPool() {
        if (_enemyPrefab == null) {
            Debug.LogError("ObjectPoolManager: 敵のプレハブが割り当てられていません。プールから敵を取得できません。", this);
            return null;
        }

        foreach (GameObject enemy in _enemyPool) {
            if (!enemy.activeInHierarchy) {
                enemy.SetActive(true);
                return enemy;
            }
        }

        // プールが空の場合、新しいオブジェクトを生成してプールに追加
        GameObject newEnemy = Instantiate(_enemyPrefab, transform);
        _enemyPool.Add(newEnemy);
        Debug.LogWarning("ObjectPoolManager: プールが枯渇しました。新しいオブジェクトを作成します。", this);
        return newEnemy;
    }
}
