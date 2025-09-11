using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーがゴールに到達した際のシーン遷移を制御します。
/// </summary>
public class GoalFlag : MonoBehaviour {
    [Header("シーン遷移設定"), SerializeField]
    private string _gameClearSceneName = "ClearScene"; // ゲームクリア後に遷移するシーンの名前

    private bool _isTriggered = false; // ゴールが既にトリガーされたかどうかのフラグ

    /// <summary>
    /// プレイヤーがゴールに触れたときに呼び出される
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        Goal(other);
    }

    /// <summary>
    /// ゴール
    /// </summary>
    private void Goal(Collider2D collider2D) {
        if (_isTriggered || !collider2D.CompareTag("Player")) {
            return;
        }

        _isTriggered = true;

        // GameManagerのインスタンスがあれば、GameClearメソッドを呼び出す
        if (GameManager.Instance != null) {
            GameManager.Instance.GameClear();
        }
        else {
            Debug.LogWarning("GoalFlag: GameManagerのインスタンスが見つかりません。直接シーンをロードします。", this);
            SceneManager.LoadScene(_gameClearSceneName);
        }
    }
}
