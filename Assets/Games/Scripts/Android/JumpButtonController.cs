using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ジャンプボタンUIのタッチイベントを処理し、プレイヤーのジャンプを呼び出すスクリプト。
/// IPointerDownHandlerインターフェースを実装することで、
/// UnityのEvent Systemからボタンのタップイベントを直接受け取ることができる。
/// </summary>
public class JumpButtonController : MonoBehaviour, IPointerDownHandler {
    // プレイヤーの移動を管理するスクリプトへの参照
    private PlayerMove _playerMove;

    /// <summary>
    /// ジャンプが出来るかどうか
    /// </summary>
    private bool AbleJump => _playerMove != null && _playerMove.gameObject.activeInHierarchy;

    /// <summary>
    /// GameManagerからPlayerMoveの参照を受け取るためのメソッド
    /// 外部（通常はGameManager）からPlayerMoveのインスタンスを設定する
    /// </summary>
    /// <param name="playerMove">PlayerMoveコンポーネント</param>
    public void SetPlayerMove(PlayerMove playerMove) => _playerMove = playerMove;

    /// <summary>
    /// ボタンがタップされたときに呼び出されます。
    /// このメソッドは、UIのEventSystemによって自動的に実行されます。
    /// </summary>
    /// <param name="eventData">ポインター（タップ）イベントに関する情報</param>
    public void OnPointerDown(PointerEventData eventData) {
        // プレイヤーオブジェクトが存在し、かつヒエラルキーでアクティブな場合のみジャンプ処理を実行
        if (AbleJump) {
            // PlayerMoveスクリプトのJump()メソッドを呼び出して、プレイヤーをジャンプさせる
            _playerMove.Jump();
        }
    }
}
