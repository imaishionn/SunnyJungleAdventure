using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameOverManager : MonoBehaviour
{
    [SerializeField] GameObject firstSelected;

    private bool m_isTransitioning = false;
    private Button m_gameOverButton;

    void Start()
    {
        if (firstSelected != null)
        {
            m_gameOverButton = firstSelected.GetComponent<Button>();
            // 注意: このif文はNullチェックですが、ログを消したので、ボタンが見つからない場合も警告は表示されません。
            if (m_gameOverButton == null)
            {
                // Debug.LogWarningは削除
            }

            if (EventSystem.current != null)
            {
                // SetSelectedGameObject(null)は冗長なので削除
                EventSystem.current.SetSelectedGameObject(firstSelected);
                // Debug.Logは削除
            }

        }

    }

    void Update()
    {
        if (m_isTransitioning) return;

        if (Input.GetButtonDown("Submit") && m_gameOverButton != null && m_gameOverButton.interactable)
        {
            // Debug.Logは削除
            OnClickReturnTitle();
        }
    }

    public void OnClickReturnTitle()
    {
        if (m_isTransitioning)
        {
            // Debug.Logは削除
            return;
        }

        m_isTransitioning = true;
        // Debug.Logは削除

        if (m_gameOverButton != null)
        {
            m_gameOverButton.interactable = false;
        }

        Time.timeScale = 1f;

        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.instance.TitleSceneName);
        }
        else
        {
            // このLogErrorは重要な情報なので残しておくことをお勧めします。
            // GameManagerが見つからない場合にゲームがクラッシュするため、エラーログは開発時に役立ちます。
            // SceneManager.LoadScene(GameManager.instance.TitleSceneName); の行は
            // NullReferenceExceptionの原因になるため、以下の修正版を使用してください。
            // Debug.LogError("GameOverManager: GameManager.instanceが見つかりません！TitleSceneを直接ロードします。");
            SceneManager.LoadScene("TitleScene"); // ここを直接シーン名に修正
        }
    }
}