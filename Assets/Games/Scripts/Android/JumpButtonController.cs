using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// ジャンプボタンUIのタッチイベントを処理し、プレイヤーのジャンプを呼び出します。
/// </summary>
public class JumpButtonController : MonoBehaviour, IPointerDownHandler {
    /// <summary>
    /// プレイヤーの移動スクリプトへの参照
    /// </summary>
    private PlayerMove _playerMove;

    /// <summary>
    /// シーン内のプレイヤーを探すもの
    /// </summary>
    private void Start() => FindPlayer();

    /// <summary>
    ///  オブジェクトが有効になったときやシーンロード後に呼ばれPlayerMoveへの参照を確実に取り直します。
    /// </summary>
    private void OnEnable() => FindPlayer();

    private void FindPlayer() {
        if (_playerMove == null) {
            // シーン内に存在する唯一のPlayerMoveを探す
            _playerMove = FindObjectOfType<PlayerMove>();
        }
        if (_playerMove == null) {
            // シーンロード直後などでPlayerMoveがまだ生成されていない可能性があるため、
            // ログレベルをWarningに下げて、深刻なエラーとしない
            
        }
    }

    /// <summary>
    /// ボタンがタップされたときに呼び出されます。
    /// </summary>
    /// <param name="eventData">ポインターイベントデータ</param>
    public void OnPointerDown(PointerEventData eventData) {
        // ボタンが押されたこと自体は前回確認済みのため、ログは削除
        // Debug.Log("Jump Button OnPointerDown called."); 

        if (_playerMove != null) {
            // PlayerMoveのメソッドを呼び出す
            _playerMove.OnMobileJumpButtonPressed();
        }
        else {
            // プレイヤーへの参照が切れている場合、OnPointerDownイベント内で再度探し、即座に実行を試みる
            FindPlayer();

            if (_playerMove != null) {
                _playerMove.OnMobileJumpButtonPressed();
            }
            else {
                Debug.LogError("PlayerMoveへの参照が見つかりません！ジャンプできませんでした。");
            }
        }
    }
}
