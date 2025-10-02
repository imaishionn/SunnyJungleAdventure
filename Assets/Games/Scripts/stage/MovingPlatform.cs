using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 設定された座標間を往復移動する足場を制御するスクリプトです。
/// プレイヤーが乗ると、プレイヤーも一緒に移動します。
/// </summary>
[RequireComponent(typeof(BoxCollider2D))] 
public class MovingPlatform : MonoBehaviour {
    [Header("プラットフォームの移動速度"), SerializeField]
    private float _moveSpeed = 2.0f; 

    [Header("各終点での待機時間"), SerializeField]
    private float _waitTimeAtPoint = 1.0f; 

    [Header("X軸方向への移動距離"), SerializeField]
    private float _moveDistanceX = 0f; 

    [Header("Y軸方向への移動距離"), SerializeField]
    private float _moveDistanceY = 0f;

    /// <summary>
    /// 足場の初期位置を管理。
    /// </summary>
    private Vector3 _initialPosition; // 足場の初期位置

    /// <summary>
    /// 足場の終点位置を管理。
    /// </summary>
    private Vector3 _endPosition;      // 足場の終点位置

    /// <summary>
    /// 足場の現在の目標位置を管理。
    /// </summary>
    private Vector3 _targetPosition;   // 現在の目標位置

    /// <summary>
    /// 移動を制御するコルーチンの参照
    /// </summary>
    private Coroutine _movementCoroutine;


    private void Start() {
        _initialPosition = transform.position;
        _endPosition = _initialPosition + new Vector3(_moveDistanceX, _moveDistanceY, 0f);

        if (_moveDistanceX == 0f && _moveDistanceY == 0f) {
            Debug.LogWarning("MovingPlatform: 移動距離が0のため、プラットフォームは移動しません。", this);
            return;
        }

        _targetPosition = _endPosition;
        _movementCoroutine = StartCoroutine(MovePlatform());
    }

    private void OnDestroy() {
        // コルーチンが実行中であれば停止
        if (_movementCoroutine != null) {
            StopCoroutine(_movementCoroutine);
        }
    }

    /// <summary>
    /// プラットフォームの移動を制御するコルーチン
    /// </summary>
    private IEnumerator MovePlatform() {
        while (true) {
            // 目標位置まで滑らかに移動
            while (Vector3.Distance(transform.position, _targetPosition) > 0.01f) {
                transform.position = Vector3.MoveTowards(transform.position, _targetPosition, _moveSpeed * Time.deltaTime);
                yield return null;
            }

            // 目標位置に到達したら待機
            yield return new WaitForSeconds(_waitTimeAtPoint);

            // 次の目標位置を切り替える
            _targetPosition = (_targetPosition == _endPosition) ? _initialPosition : _endPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            // プレイヤーをプラットフォームの子オブジェクトにする
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            // プレイヤーの親オブジェクトを解除
            other.transform.SetParent(null);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// エディター上でプラットフォームの移動範囲を可視化します。
    /// </summary>
    private void OnDrawGizmos() {
        Vector3 gizmoInitialPos = EditorApplication.isPlayingOrWillChangePlaymode ? _initialPosition : transform.position;
        Vector3 gizmoEndPos = gizmoInitialPos + new Vector3(_moveDistanceX, _moveDistanceY, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gizmoInitialPos, gizmoEndPos);
        Gizmos.DrawWireSphere(gizmoInitialPos, 0.2f);
        Gizmos.DrawWireSphere(gizmoEndPos, 0.2f);
    }
#endif
}
