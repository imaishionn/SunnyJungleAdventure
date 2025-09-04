using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲームオーバー画面のUIとシーン遷移を管理するスクリプトです。
/// </summary>
public class GameOverManager : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("UI設定")]
    [Tooltip("ゲームパッド操作で最初に選択状態にしたいUI要素")]
    [SerializeField] private Selectable firstSelected;
    [Tooltip("タイトルに戻るボタン")]
    [SerializeField] private Button returnTitleButton;
    [Tooltip("もう一度プレイするボタン")]
    [SerializeField] private Button retryButton;

    // ----------------------------------------------------------------------------------------------------
    // シーン設定
    // ----------------------------------------------------------------------------------------------------
    [Header("シーン設定")]
    [Tooltip("タイトルシーンの名前")]
    [SerializeField] private string titleSceneName = "TitleScene";

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private bool m_isTransitioning = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        // UIの初期選択を設定
        if(firstSelected != null) {
            if(EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
            }
        }
        else {
            Debug.LogWarning("GameOverManager: firstSelectedが割り当てられていません。",this);
        }

        // ボタンのクリックイベントにリスナーを登録
        if(returnTitleButton != null) {
            returnTitleButton.onClick.AddListener(OnClickReturnTitle);
        }
        if(retryButton != null) {
            retryButton.onClick.AddListener(OnClickRetry);
        }

        // シーンロード完了時に遷移フラグをリセット
        m_isTransitioning = false;
    }

    private void OnDestroy() {
        // シーンが破棄される前にイベントリスナーを解除する
        if(returnTitleButton != null) {
            returnTitleButton.onClick.RemoveListener(OnClickReturnTitle);
        }
        if(retryButton != null) {
            retryButton.onClick.RemoveListener(OnClickRetry);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// タイトルに戻るボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickReturnTitle() {
        if(m_isTransitioning) return;
        m_isTransitioning = true;
        Time.timeScale = 1f;

        // GameManager.instance を GameManager.Instance に修正
        if(GameManager.Instance != null) {
            // GameManager.instance.LoadSceneWithFade を GameManager.Instance.LoadSceneWithFade に修正
            GameManager.Instance.LoadSceneWithFade(titleSceneName);
        }
        else {
            SceneManager.LoadScene(titleSceneName);
        }
    }

    /// <summary>
    /// もう一度プレイするボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickRetry() {
        if(m_isTransitioning) return;
        m_isTransitioning = true;
        Time.timeScale = 1f;

        // GameManager.instance を GameManager.Instance に修正
        if(GameManager.Instance != null) {
            // ここでゲームデータをリセットする
            // GameManager.instance.ResetGameData() を GameManager.Instance.ResetGameData() に修正
            // GameManager.instance.RetryLastStage() を GameManager.Instance.RetryLastStage() に修正
            GameManager.Instance.ResetGameData();
            GameManager.Instance.RetryLastStage(); // GameManagerのメソッドを呼び出す
        }
        else {
            Debug.LogError("GameOverManager: GameManager.instanceが見つかりません！タイトルシーンへ直接遷移します。",this);
            SceneManager.LoadScene(titleSceneName);
        }
    }
}