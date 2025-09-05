using System.Collections;
using UnityEngine;
// Unityエディタ上でのみ使用する名前空間を定義
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 設定された座標間を往復移動する足場を制御するスクリプトです。
/// プレイヤーが乗ると、プレイヤーも一緒に移動します。
/// </summary>
public class MovingPlatform : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("移動設定")]
    [Tooltip("プラットフォームの移動速度")]
    [SerializeField] private float _moveSpeed = 2.0f;
    [Tooltip("各終点での待機時間")]
    [SerializeField] private float _waitTimeAtPoint = 1.0f;
    [Tooltip("X軸方向への移動距離")]
    [SerializeField] private float _moveDistanceX = 0f;
    [Tooltip("Y軸方向への移動距離")]
    [SerializeField] private float _moveDistanceY = 0f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Vector3 _initialPosition; // 足場の初期位置
    private Vector3 _endPosition;     // 足場の終点位置
    private Vector3 _targetPosition;  // 現在の目標位置

    private Coroutine _movementCoroutine;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        _initialPosition = transform.position;
        _endPosition = _initialPosition + new Vector3(_moveDistanceX,_moveDistanceY,0f);

        // 移動距離が0の場合は移動しない
        if(_moveDistanceX == 0f && _moveDistanceY == 0f) {
            Debug.LogWarning("MovingPlatform: 移動距離が0のため、プラットフォームは移動しません。",this);
            return;
        }

        // 移動を開始
        _targetPosition = _endPosition;
        _movementCoroutine = StartCoroutine(MovePlatform());
    }

    private void OnDestroy() {
        // シーンが破棄される前にコルーチンを停止
        if(_movementCoroutine != null) {
            StopCoroutine(_movementCoroutine);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プラットフォームの移動を制御するコルーチン
    /// </summary>
    private IEnumerator MovePlatform() {
        while(true) {
            // 目標位置まで移動
            while(Vector3.Distance(transform.position,_targetPosition) > 0.01f) {
                transform.position = Vector3.MoveTowards(transform.position,_targetPosition,_moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 目標位置に到達したら待機
            yield return new WaitForSeconds(_waitTimeAtPoint);

            // 次の目標位置を切り替える
            _targetPosition = _targetPosition == _endPosition ? _initialPosition : _endPosition;
        }
    }

    /// <summary>
    /// プレイヤーがプラットフォームに乗ったときに呼び出されます。
    /// プレイヤーをプラットフォームの子オブジェクトにします。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            // 既に子オブジェクトになっている場合は処理しない
            if (other.transform.parent == transform) {
                return;
            }

            other.transform.SetParent(transform);
        }
    }

    /// <summary>
    /// プレイヤーがプラットフォームから離れたときに呼び出されます。
    /// プレイヤーの親オブジェクトを解除します。
    /// </summary>
    private void OnTriggerExit2D(Collider2D other) {
        if(other.CompareTag("Player")) {
            // 子オブジェクトになっている場合のみ親を解除
            if(other.transform.parent == transform) {
                other.transform.SetParent(null);
            }
        }
    }

#if UNITY_EDITOR
    // ----------------------------------------------------------------------------------------------------
    // Unityエディター関連
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// エディター上でプラットフォームの移動範囲を可視化します。
    /// </summary>
    private void OnDrawGizmos() {
        // 実行中は initialPosition を、エディター上では transform.position を基準にする
        Vector3 gizmoInitialPos = EditorApplication.isPlayingOrWillChangePlaymode ? _initialPosition : transform.position;
        Vector3 gizmoEndPos = gizmoInitialPos + new Vector3(_moveDistanceX,_moveDistanceY,0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gizmoInitialPos,gizmoEndPos);
        Gizmos.DrawWireSphere(gizmoInitialPos,0.2f);
        Gizmos.DrawWireSphere(gizmoEndPos,0.2f);
    }
#endif
}
