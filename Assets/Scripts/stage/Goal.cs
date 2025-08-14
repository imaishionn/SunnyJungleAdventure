using UnityEngine;
// using System.Diagnostics; // ★この行があったら削除またはコメントアウト！★

public class Goal : MonoBehaviour
{
    [SerializeField, Header("ゲームクリア時に遷移するシーン名")]
    private string gameClearSceneName = "GameClearScene"; // GameManagerの定数を使用するように推奨

    void OnTriggerEnter2D(Collider2D other)
    {
        // プレイヤーがゴールに触れたら
        if (other.CompareTag("Player"))
        {
            UnityEngine.Debug.Log("Goal: プレイヤーがゴールしました！", this);

            // GameManagerが存在するか確認
            if (GameManager.instance != null)
            {
                // GameManagerの現在の状態がプレイ中（Gameplay）であることを確認
                // これにより、既にゲームオーバーやクリア状態になっている場合は多重処理を防ぐ
                if (GameManager.instance.GetCurrentGameState() == GameManager.GameState.Gameplay)
                {
                    UnityEngine.Debug.Log("Goal: GameManagerにゲームクリアを通知します。", this);
                    GameManager.instance.SetState(GameManager.GameState.StageClear);
                    GameManager.instance.LoadSceneWithFade(gameClearSceneName);
                }
                else
                {
                    // 修正点: ここに閉じ丸かっこ`)`を追加
                    UnityEngine.Debug.Log("Goal: ゲームが既にプレイ状態でないため、ゲームクリア処理をスキップしました。", this);
                }
            }
            else
            {
                // GameManagerが存在しない場合のフォールバック（望ましくないが、エラー回避のため）
                UnityEngine.Debug.LogWarning("Goal: GameManagerのインスタンスが見つかりません。直接シーンをロードします。", this);
                UnityEngine.Debug.Log($"Goal: シーン'{gameClearSceneName}'をロードします。", this);
                UnityEngine.SceneManagement.SceneManager.LoadScene(gameClearSceneName);
            }
        }
    }
}