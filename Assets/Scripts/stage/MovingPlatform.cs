using System.Collections; // Coroutineのために必要
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
    [Header("移動設定")]
    [Tooltip("移動速度")]
    [SerializeField] private float moveSpeed = 2.0f;

    [Tooltip("目的地に到達してから次の移動を開始するまでの待機時間")]
    [SerializeField] private float waitTimeAtPoint = 1.0f;

    [Tooltip("X軸方向への移動距離 (正の値で右、負の値で左)")]
    [SerializeField] private float moveDistanceX = 0f; // 例: 5.0f

    [Tooltip("Y軸方向への移動距離 (正の値で上、負の値で下)")]
    [SerializeField] private float moveDistanceY = 0f; // 例: 3.0f

    private Vector3 initialPosition; // 足場の初期位置
    private Vector3 targetPosition;  // 現在の目標地点 (初期位置または終点)
    private Vector3 endOffsetPosition; // 初期位置からのオフセットを加算した終点

    private bool movingToEnd = true; // trueならオフセット地点へ、falseなら初期位置へ移動中

    void Start()
    {
        initialPosition = transform.position; // スクリプト開始時の位置を初期位置とする
        endOffsetPosition = initialPosition + new Vector3(moveDistanceX, moveDistanceY, 0f); // 終点を計算

        targetPosition = endOffsetPosition; // 最初は終点へ向かう
        StartCoroutine(MovePlatform()); // コルーチンで移動を開始
    }

    // 足場の移動を制御するコルーチン
    private IEnumerator MovePlatform()
    {
        while (true) // 無限ループで往復移動を続ける
        {
            // 目標地点に向かって移動
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null; // 1フレーム待機
            }

            // 目標地点に到達したら待機
            yield return new WaitForSeconds(waitTimeAtPoint);

            // 次の目標地点を設定（移動方向を反転）
            movingToEnd = !movingToEnd;
            if (movingToEnd)
            {
                targetPosition = endOffsetPosition;
            }
            else
            {
                targetPosition = initialPosition;
            }
        }
    }

    // プレイヤーが足場に乗った時の処理
    private void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーのタグをチェック (例: "Player")
        if (other.CompareTag("Player"))
        {
            // プレイヤーを足場の子オブジェクトにする
            // これにより、足場が移動するとプレイヤーも一緒に移動する
            other.transform.SetParent(transform);
        }
    }

    // プレイヤーが足場から離れた時の処理
    private void OnTriggerExit2D(Collider2D other)
    {
        // プレイヤーのタグをチェック (例: "Player")
        if (other.CompareTag("Player"))
        {
            // プレイヤーの親を解除する
            // nullを設定すると、シーンのルートに戻る
            other.transform.SetParent(null);
        }
    }

    // Sceneビューで移動経路を可視化する
    void OnDrawGizmos()
    {
        // エディタ実行中でない場合、または初期位置がまだ設定されていない場合は描画しない
        if (!Application.isPlaying)
        {
            initialPosition = transform.position;
            endOffsetPosition = initialPosition + new Vector3(moveDistanceX, moveDistanceY, 0f);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(initialPosition, endOffsetPosition);
        Gizmos.DrawWireSphere(initialPosition, 0.2f); // 開始地点
        Gizmos.DrawWireSphere(endOffsetPosition, 0.2f); // 終了地点
    }
}
