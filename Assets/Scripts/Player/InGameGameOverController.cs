using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理のために必要

public class InGameGameOverController : MonoBehaviour
{
    // ゲームオーバー時に表示するUIパネル（GameObject）
    [SerializeField] GameObject gameOverUI;

    // スクリプトが有効になった最初のフレームで呼び出される
    void Start()
    {
        // ゲーム開始時にゲームオーバーUIを非表示にする
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    // ゲームオーバーUIを表示する公共メソッド
    public void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // ゲームオーバーUIを表示
            Time.timeScale = 0f;        // ゲームの時間を停止（ポーズ状態にする）
        }
        // else の警告ログは削除（Inspectorで設定を促すため、開発終盤には不要）
    }

    // リトライボタンが押された際に呼び出される公共メソッド
    public void Retry()
    {
        Time.timeScale = 1f; // ゲームの時間を再開

        // 現在のシーンをフェードアウトしながら再ロードする
        // GameManagerを通してシーン遷移を管理する
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
        else
        {
            // GameManagerがない場合のフォールバック（直接シーンロード）
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // タイトルへ戻るボタンが押された際に呼び出される公共メソッド
    public void GoToTitle()
    {
        Time.timeScale = 1f; // ゲームの時間を再開

        // タイトルシーンへフェードアウトしながら遷移する
        // GameManagerを通してシーン遷移を管理する
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade("TitleScene"); // 通常は"TitleScene"へ戻る
        }
        else
        {
            // GameManagerがない場合のフォールバック（直接シーンロード）
            SceneManager.LoadScene("TitleScene"); // ここも"TitleScene"が一般的
        }
    }
}