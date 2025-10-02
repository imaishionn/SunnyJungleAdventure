using System.Collections;
using UnityEngine;

/// <summary>
/// オブジェクトプールから敵をスポーン（生成）するスクリプト。
/// 指定した数と間隔で、敵を定期的に配置します。
/// </summary>
public class Spawner : MonoBehaviour {

    [Header("オブジェクトプールを管理するクラス"), SerializeField]
    private ObjectPoolManager _objectPoolManager;

    [ Header("敵をスポーンする間隔（秒）"), SerializeField]
    private float _spawnInterval = 3f; 

    [ Header("スポーンする敵の総数"), SerializeField]
    private int _spawnCount = 5;

    /// <summary>
    /// 現在スポーンした敵の数
    /// </summary>
    private int _spawnedCount;

    private void Start() {
        if (_objectPoolManager == null) {
            Debug.LogError("ObjectPoolManagerが割り当てられていません。スポーンは実行されません。", this);
            return;
        }

        _spawnedCount = 0;
        StartCoroutine(SpawnEnemyRoutine());
    }

    private IEnumerator SpawnEnemyRoutine() {
        while (_spawnedCount < _spawnCount) {
            yield return new WaitForSeconds(_spawnInterval);

            GameObject enemy = _objectPoolManager.GetEnemyFromPool();
            if (enemy == null) {
                Debug.LogWarning("プールから敵を取得できませんでした。スポーンをスキップします。");
                continue;
            }

            // スポーン位置をランダムに調整
            float offsetX = UnityEngine.Random.Range(-1f, 1f);
            float offsetY = UnityEngine.Random.Range(-1f, 1f);
            Vector3 spawnPosition = transform.position + new Vector3(offsetX, offsetY, 0);

            enemy.transform.position = spawnPosition;
            _spawnedCount++;
            Debug.Log($"{_spawnedCount}体目の敵をスポーンしました。");
        }

        Debug.Log("すべての敵のスポーンが完了しました。");
    }
}
