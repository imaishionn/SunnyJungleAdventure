using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Buttonコンポーネントの参照に必要
using Debug = UnityEngine.Debug;

/// <summary>
/// ゲームクリア後のシーンで、UIやシーン遷移を制御するスクリプトです。
/// </summary>
public class ClearSceneController : MonoBehaviour
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
    private bool m_isTransitioning = false; // シーン遷移中フラグ

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start()
    {
        // UIの初期選択を設定
        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
        else
        {
            Debug.LogWarning("ClearSceneController: firstSelectedが割り当てられていません。", this);
        }

        // シーンロード完了時に遷移フラグをリセット
        m_isTransitioning = false;
    }

    private void Update()
    {
        // シーン遷移中でなければ入力を受け付ける
        if (m_isTransitioning) return;

        // "Submit"ボタンが押されたらタイトルに戻る
        if (Input.GetButtonDown("Submit"))
        {
            OnClickReturnTitle();
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

        m_isTransitioning = true; // 遷移中フラグを立てる
        Time.timeScale = 1f;      // ゲームの時間を通常の速度に戻す

        // GameManagerを通してタイトルシーンへ遷移
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(titleSceneName);
        }
        else
        {
            Debug.LogError("ClearSceneController: GameManager インスタンスが見つかりません！直接シーンをロードします。", this);
            SceneManager.LoadScene(titleSceneName);
        }
    }
}