using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ゲームクリア後のシーンで、UIやシーン遷移を制御するスクリプトです。
/// </summary>
public class ClearSceneController : MonoBehaviour
{
    [Header("UI設定")]
    [Tooltip("ゲームパッド操作で最初に選択状態にしたいUI要素")]
    [SerializeField] private Selectable firstSelected;

    [Header("シーン設定")]
    [Tooltip("スコアシーンの名前")]
    [SerializeField] private string ResultsceneName = "Resultscene";

    private bool m_isTransitioning = false;

    private void Start()
    {
        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelected.gameObject);
        }
        else
        {
            Debug.LogWarning("ClearSceneController: firstSelectedが割り当てられていません。", this);
        }

        m_isTransitioning = false;
    }

    private void Update()
    {
        if (m_isTransitioning) return;

        if (Input.GetButtonDown("Submit"))
        {
            OnClickGoToScoreScene();
        }
    }

    /// <summary>
    /// スコア画面へ進むボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickGoToScoreScene()
    {
        if (m_isTransitioning) return;
        m_isTransitioning = true;

        if (GameManager.Instance != null)
        {
            // 変数名を ResultsceneName に修正
            GameManager.Instance.LoadSceneWithFade(ResultsceneName);
        }
        else
        {
            Debug.LogError("ClearSceneController: GameManager インスタンスが見つかりません！直接シーンをロードします。", this);
            // 変数名を ResultsceneName に修正
            SceneManager.LoadScene(ResultsceneName);
        }
    }
}