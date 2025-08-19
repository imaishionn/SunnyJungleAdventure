using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

/// <summary>
/// ゲームプレイ中のゲームオーバー画面と、それに付随するUIイベントを管理するスクリプトです。
/// </summary>
public class InGameGameOverController : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("UI設定")]
    [Tooltip("ゲームオーバー時に表示するUIパネルのゲームオブジェクト")]
    [SerializeField] private GameObject gameOverUI;
    [Tooltip("リトライボタン")]
    [SerializeField] private UnityEngine.UI.Button retryButton;
    [Tooltip("タイトルへ戻るボタン")]
    [SerializeField] private UnityEngine.UI.Button backToTitleButton;

    [Header("シーン設定")]
    [Tooltip("タイトルシーンの名前")]
    [SerializeField] private string titleSceneName = "TitleScene";

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake()
    {
        // UIパネルの存在チェック
        if (gameOverUI == null)
        {
            Debug.LogError("InGameGameOverController: gameOverUIが割り当てられていません！", this);
        }

        // ボタンのリスナーを登録
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnClickRetryButton);
        }

        if (backToTitleButton != null)
        {
            backToTitleButton.onClick.AddListener(OnClickGoToTitleButton);
        }
    }

    private void Start()
    {
        // ゲーム開始時にゲームオーバーUIを非表示にする
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // シーン遷移時にボタンのイベントリスナーを解除
        if (retryButton != null)
        {
            retryButton.onClick.RemoveAllListeners();
        }

        if (backToTitleButton != null)
        {
            backToTitleButton.onClick.RemoveAllListeners();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッド（外部から呼び出される）
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ゲームオーバーUIを表示し、ゲームを停止します。
    /// GameManagerから呼び出されることを想定しています。
    /// </summary>
    public void ShowGameOverUI()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true);
            Time.timeScale = 0f; // ゲームの時間を停止
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// リトライボタンが押された際に呼び出されます。現在のシーンを再ロードします。
    /// </summary>
    private void OnClickRetryButton()
    {
        Time.timeScale = 1f; // ゲームの時間を再開

        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
        else
        {
            Debug.LogError("InGameGameOverController: GameManagerが見つかりません。直接シーンをロードします。", this);
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    /// <summary>
    /// タイトルへ戻るボタンが押された際に呼び出されます。タイトルシーンへ遷移します。
    /// </summary>
    private void OnClickGoToTitleButton()
    {
        Time.timeScale = 1f; // ゲームの時間を再開

        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(titleSceneName);
        }
        else
        {
            Debug.LogError("InGameGameOverController: GameManagerが見つかりません。直接シーンをロードします。", this);
            SceneManager.LoadScene(titleSceneName);
        }
    }
}