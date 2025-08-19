using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug; // 曖昧な参照を解消するため

/// <summary>
/// ゴールオブジェクトのスクリプト。
/// プレイヤーが接触した際にゲームクリア処理を呼び出します。
/// </summary>
public class Goal : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("シーン遷移設定")]
    [Tooltip("ゲームクリア時に遷移するシーンの名前")]
    [SerializeField] private string gameClearSceneName = "GameClearScene";

    // ----------------------------------------------------------------------------------------------------
    // トリガーイベントメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// このコライダーが他の2Dコライダーと接触したときに呼び出されます。
    /// </summary>
    /// <param name="other">接触した他のコライダー</param>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 接触したオブジェクトがプレイヤーかチェック
        if (other.CompareTag("Player"))
        {
            Debug.Log("Goal: プレイヤーがゴールに到達しました！", this);

            // GameManagerが存在するか確認
            if (GameManager.instance != null)
            {
                // ゲームの状態が「プレイ中」の場合のみ処理を実行
                if (GameManager.instance.GetCurrentGameState() == GameManager.GameState.Gameplay)
                {
                    Debug.Log("Goal: GameManagerにゲームクリアを通知します。", this);
                    GameManager.instance.SetState(GameManager.GameState.StageClear);
                    GameManager.instance.LoadSceneWithFade(gameClearSceneName);
                }
                else
                {
                    Debug.Log("Goal: ゲームがプレイ状態ではないため、ゲームクリア処理をスキップしました。", this);
                }
            }
            else
            {
                // GameManagerが見つからない場合のフォールバック処理
                Debug.LogWarning("Goal: GameManagerのインスタンスが見つかりません。直接シーンをロードします。", this);
                SceneManager.LoadScene(gameClearSceneName);
            }
        }
    }
}