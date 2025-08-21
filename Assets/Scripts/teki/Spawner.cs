using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

/// <summary>
/// オブジェクトプールから敵をスポーン（生成）するスクリプトです。
/// 指定した数と間隔で、敵を定期的に配置します。
/// </summary>
public class Spawner : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("スポーン設定")]
    [Tooltip("利用するObjectPoolManagerの参照")]
    [SerializeField] private ObjectPoolManager objectPoolManager;
    [Tooltip("敵をスポーンする間隔 (秒)")]
    [SerializeField] private float spawnInterval = 3f;
    [Tooltip("スポーンする敵の総数")]
    [SerializeField] private int spawnCount = 5;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private int spawnedCount = 0;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start()
    {
        // 必要なコンポーネントが割り当てられているか確認
        if (objectPoolManager == null)
        {
            Debug.LogError("Spawner: ObjectPoolManagerが割り当てられていません！", this);
            // 代わりにシングルトンインスタンスを試す
            objectPoolManager = ObjectPoolManager.Instance;
            if (objectPoolManager == null)
            {
                Debug.LogError("Spawner: シングルトンインスタンスも見つかりません。スポーンを中止します。");
                return;
            }
        }

        // コルーチンを開始
        StartCoroutine(SpawnEnemyRoutine());
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 指定された間隔で敵を生成するコルーチンです。
    /// </summary>
    private IEnumerator SpawnEnemyRoutine()
    {
        while (spawnedCount < spawnCount)
        {
            // 次の生成まで待機
            yield return new WaitForSeconds(spawnInterval);

            // オブジェクトプールから敵を取得
            GameObject enemy = objectPoolManager.GetEnemyFromPool();
            if (enemy != null)
            {
                // スポナーの周囲にランダムな位置を生成
                float offsetX = UnityEngine.Random.Range(-1f, 1f);
                float offsetY = UnityEngine.Random.Range(-1f, 1f);
                Vector3 spawnPosition = transform.position + new Vector3(offsetX, offsetY, 0);

                // 敵を配置
                enemy.transform.position = spawnPosition;
                spawnedCount++;
                Debug.Log($"Spawner: 敵をスポーンしました。現在 {spawnedCount} 体 / {spawnCount} 体");
            }
        }
        Debug.Log("Spawner: 指定された数の敵をすべてスポーンしました。");
    }
}