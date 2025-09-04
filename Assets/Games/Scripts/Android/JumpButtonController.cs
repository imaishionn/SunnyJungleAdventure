using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ジャンプボタンUIのタッチイベントを処理し、プレイヤーのジャンプを呼び出すスクリプト。
/// </summary>
// IPointerDownHandlerインターフェースを実装することで、
// UnityのEvent Systemからボタンのタップイベントを直接受け取ることができる。
public class JumpButtonController : MonoBehaviour, IPointerDownHandler {
    // プレイヤーの移動を管理するスクリプトへの参照
    private PlayerMove m_playerMove;

    /// <summary>
    /// GameManagerからPlayerMoveの参照を受け取るためのメソッド
    /// </summary>
    /// <param name="player">PlayerMoveコンポーネント</param>
    public void SetPlayerMove(PlayerMove player) {
        // 外部（通常はGameManager）からPlayerMoveのインスタンスを設定する
        m_playerMove = player;
    }

    /// <summary>
    /// ボタンがタップされたときに呼び出されます。
    /// このメソッドは、UIのEventSystemによって自動的に実行されます。
    /// </summary>
    /// <param name="eventData">ポインター（タップ）イベントに関する情報</param>
    public void OnPointerDown(PointerEventData eventData) {
        // プレイヤーオブジェクトが存在し、かつヒエラルキーでアクティブな場合のみジャンプ処理を実行
        if (m_playerMove != null && m_playerMove.gameObject.activeInHierarchy) {
            // PlayerMoveスクリプトのJump()メソッドを呼び出して、プレイヤーをジャンプさせる
            m_playerMove.Jump();
        }
    }
}
