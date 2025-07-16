using UnityEngine;
using UnityEngine.SceneManagement; // SceneManagerを使用するため
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

// using System.Diagnostics; // この行は削除します。

public class Goal : MonoBehaviour
{
    [SerializeField, Header("次のシーン名")]
    private string nextSceneName = "GameClear"; // デフォルトはGameClear

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Goal OnTriggerEnter2D: {collision.gameObject.name} と衝突しました。");

        // プレイヤーとの衝突か確認（プレイヤーに"Player"タグが付いていることを前提とします）
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Goal: プレイヤーがゴールに到達しました！");

            // プレイヤーの動きを止める（例：PlayerMoveスクリプトを無効化）
            var playerMove = collision.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.enabled = false;
                Debug.Log("Goal: プレイヤーの動きを停止しました。");
            }
            else
            {
                Debug.LogWarning("Goal: プレイヤーにPlayerMoveスクリプトが見つかりませんでした。");
            }

            // GameManagerのインスタンスが存在するか確認し、シーン遷移を要求
            if (GameManager.instance != null)
            {
                Debug.Log($"Goal: {nextSceneName} シーンへ遷移します。");
                GameManager.instance.LoadSceneWithFade(nextSceneName);
            }
            else
            {
                Debug.LogError("Goal: GameManager.instanceが見つかりません！シーン遷移できません。直接シーンをロードします。");
                // GameManagerが見つからない場合のフォールバック（デバッグ用）
                SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
        }
    }
}
