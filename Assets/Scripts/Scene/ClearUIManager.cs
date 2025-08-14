using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClearUIManager : MonoBehaviour
{
    [SerializeField] private Button returnToTitleButton;

    void Start()
    {
        if (GameManager.instance == null)
        {
            UnityEngine.Debug.LogError("ClearUIManager: GameManagerのインスタンスが見つかりません。シーン遷移が機能しない可能性があります。", this);
            return;
        }

        if (returnToTitleButton != null)
        {
            returnToTitleButton.onClick.AddListener(ReturnToTitle);
        }
        else
        {
            UnityEngine.Debug.LogWarning("ClearUIManager: Return To Title Button が割り当てられていません。", this);
        }
    }

    public void ReturnToTitle()
    {
        UnityEngine.Debug.Log("ClearUIManager: タイトルシーンへ戻ります。");
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.instance.TitleSceneName);
        }
        else
        {
            // GameManagerがない場合のフォールバックだが、GameManagerのインスタンスがnullの場合は、
            // TitleSceneNameへのアクセスも問題になるため、このelseブロック自体は
            // 通常発生しないようにGameManagerの初期化を確認することが望ましい。
            // GameManagerが存在しない場合は、TitleSceneNameが取得できないため、
            // ここは静的な文字列でシーン名を指定するか、ビルド設定に依存する形になる。
            // 現在の設計（GameManagerにシーン名が集約されている）を維持するため、
            // この行も instance 経由でアクセスするのが適切だが、論理的にはここには到達しないはず。
            // もしGameManager.instanceが本当にnullになりうるなら、ここも修正が必要。
            // 例: SceneManager.LoadScene("TitleScene"); // 具体的なシーン名を直接指定
            SceneManager.LoadScene(GameManager.instance.TitleSceneName); // ここも instance 経由で統一
        }
    }
}