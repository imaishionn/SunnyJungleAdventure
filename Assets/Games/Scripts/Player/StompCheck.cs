using UnityEngine;

/// <summary>
/// プレイヤーが敵を踏みつけたことを判定するスクリプトです。
/// プレイヤーキャラクターの足元の子オブジェクトにアタッチして使用します。
/// </summary>
public class StompCheck : MonoBehaviour {
    [Header("コンポーネント")]
    private PlayerMove _playerMove;

    private void Awake() {
        _playerMove = GetComponentInParent<PlayerMove>();

        if (_playerMove == null) {
            Debug.LogWarning("StompCheck: 親オブジェクトにPlayerMoveコンポーネントが見つかりません。", this);
        }
    }

    /// <summary>
    /// このコライダーが他の2Dコライダーと接触したときに呼び出されます。
    /// </summary>
    /// <param name="other">接触した他のコライダー</param>
    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Enemy")) {
            if (_playerMove != null) {
                _playerMove.StompEnemy(other.gameObject);
            }
        }
    }
}
