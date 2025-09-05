using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour {
    [Header("シーン遷移設定")]
    [SerializeField] private string _gameClearSceneName = "ClearScene";

    // 画像は常に表示されるため、スクリプトでの制御は行わない

    private bool _isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other) {
        if(_isTriggered || !other.CompareTag("Player")) {
            return;
        }

        _isTriggered = true;

        // GameManager.instance を GameManager.Instance に修正
        if(GameManager.Instance != null) {
            // GameManager.instance.GameClear() を GameManager.Instance.GameClear() に修正
            GameManager.Instance.GameClear();
        }
        else {
            Debug.LogWarning("Goal: GameManagerのインスタンスが見つかりません。直接シーンをロードします。",this);
            SceneManager.LoadScene(_gameClearSceneName);
        }
    }
}
