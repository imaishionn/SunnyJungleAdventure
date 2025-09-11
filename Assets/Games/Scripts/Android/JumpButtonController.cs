using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ジャンプボタンUIのタッチイベントを処理し、プレイヤーのジャンプを呼び出します。
/// </summary>
public class JumpButtonController : MonoBehaviour, IPointerDownHandler {
    private PlayerMove _playerMove;

    private void Start() => FindPlayer();

    private void FindPlayer() {
        if (_playerMove == null) {
            _playerMove = FindObjectOfType<PlayerMove>();
        }
        if (_playerMove == null) {
            Debug.LogError("JumpButtonController: シーン内にPlayerMoveコンポーネントが見つかりません。", this);
        }
    }

    /// <summary>
    /// ボタンがタップされたときに呼び出されます。
    /// </summary>
    /// <param name="eventData">ポインターイベントデータ</param>
    public void OnPointerDown(PointerEventData eventData) {
        // デバッグログを追加して、ボタンが押されているかを確認する
        Debug.Log("Jump Button OnPointerDown called.");

        if (_playerMove != null) {
            // PlayerMoveのメソッドを呼び出す
            _playerMove.OnMobileJumpButtonPressed();
        }
        else {
            // プレイヤーが見つからない場合もログを出す
            Debug.LogError("PlayerMoveへの参照が見つかりません！ジャンプできませんでした。");
        }
    }
}
