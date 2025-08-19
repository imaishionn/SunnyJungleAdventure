using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// ゲームオーバー画面のUIとシーン遷移を管理するスクリプトです。
/// </summary>
public class GameOverManager : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("UI設定")]
    [Tooltip("ゲームパッド操作で最初に選択状態にしたいUI要素")]
    [SerializeField] private Selectable firstSelected;

    [Header("シーン設定")]
    [Tooltip("タイトルシーンの名前")]
    [SerializeField] private string titleSceneName = "TitleScene";

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private bool m_isTransitioning = false;
    private Button m_gameOverButton;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start()
    {
        // UIの初期選択を設定
        if (firstSelected != null)
        {
            m_gameOverButton = firstSelected.GetComponent<Button>();

            // firstSelectedにButtonコンポーネントが見つからない場合は警告を出す
            if (m_gameOverButton == null)
            {
                Debug.LogWarning("GameOverManager: firstSelectedにButtonコンポーネントが見つかりません。", this);
            }

            // UIの初期選択を強制的に設定
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
            }
        }
        else
        {
            Debug.LogWarning("GameOverManager: firstSelectedが割り当てられていません。", this);
        }

        // シーンロード完了時に遷移フラグをリセット
        m_isTransitioning = false;
    }

    private void Update()
    {
        // シーン遷移中であれば入力を受け付けない
        if (m_isTransitioning) return;

        // "Submit"ボタンが押されたら処理を実行
        if (Input.GetButtonDown("Submit"))
        {
            // ボタンが設定されていて、かつインタラクト可能かを確認
            if (m_gameOverButton != null && m_gameOverButton.interactable)
            {
                OnClickReturnTitle();
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// タイトルに戻るボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickReturnTitle()
    {
        // 既に遷移中であれば何もしない
        if (m_isTransitioning)
        {
            return;
        }

        m_isTransitioning = true;
        Time.timeScale = 1f;

        // ボタンを無効化し、多重クリックを防ぐ
        if (m_gameOverButton != null)
        {
            m_gameOverButton.interactable = false;
        }

        // GameManagerを通してタイトルシーンへ遷移
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(titleSceneName);
        }
        else
        {
            // GameManagerが見つからない場合のフォールバック
            Debug.LogError("GameOverManager: GameManager.instanceが見つかりません！タイトルシーンを直接ロードします。", this);
            SceneManager.LoadScene(titleSceneName);
        }
    }
}