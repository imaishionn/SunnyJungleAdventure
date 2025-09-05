using UnityEngine;

/// <summary>
/// プレイヤーが敵を踏みつけたことを判定するスクリプトです。
/// プレイヤーキャラクターの足元の子オブジェクトにアタッチして使用します。
/// </summary>
public class StompCheck : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // プライベート変数 (Inspectorで設定する必要はありません)
    // ----------------------------------------------------------------------------------------------------
    [Header("コンポーネント")]
    private PlayerMove _playerMove;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake() {
        // 親オブジェクトからPlayerMoveコンポーネントの参照を取得
        _playerMove = transform.parent.GetComponent<PlayerMove>();

        // 参照が取得できなかった場合、警告を出す
        if(_playerMove == null) {
            Debug.LogWarning("StompCheck: 親オブジェクトにPlayerMoveコンポーネントが見つかりません。",this);
        }
    }

    /// <summary>
    /// このコライダーが他の2Dコライダーと接触したときに呼び出されます。
    /// </summary>
    /// <param name="other">接触した他のコライダー</param>
    private void OnTriggerEnter2D(Collider2D other) {
        // 敵のタグを持つオブジェクトと接触したかチェック
        if(other.CompareTag("Enemy")) {
            // PlayerMoveコンポーネントが存在する場合、敵を踏みつける処理を呼び出す
            if(_playerMove != null) {
                _playerMove.StompEnemy(other.gameObject);
            }
        }
    }
}
