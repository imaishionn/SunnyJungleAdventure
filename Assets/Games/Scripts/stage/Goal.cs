using System.Collections;

using UnityEngine;
using UnityEngine.SceneManagement;

public class Goal : MonoBehaviour
{
    [Header("シーン遷移設定")]
    [SerializeField] private string gameClearSceneName = "ClearScene";

    // 画像は常に表示されるため、スクリプトでの制御は行わない

    private bool m_isTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (m_isTriggered || !other.CompareTag("Player"))
        {
            return;
        }

        m_isTriggered = true;

        if (GameManager.instance != null)
        {
            GameManager.instance.GameClear();
        }
        else
        {
            Debug.LogWarning("Goal: GameManagerのインスタンスが見つかりません。直接シーンをロードします。", this);
            SceneManager.LoadScene(gameClearSceneName);
        }
    }
}