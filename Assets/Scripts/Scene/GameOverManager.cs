using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理のために必要
using UnityEngine.EventSystems;    // UIイベントシステムのために必要
using UnityEngine.UI;              // Buttonコンポーネントを操作するために必要
using Debug = UnityEngine.Debug;   // Debugの曖昧な参照を解消するため

public class GameOverManager : MonoBehaviour
{
    // ゲームパッド操作で最初に選択状態にしたいUI要素（ボタンなど）
    [SerializeField] GameObject firstSelected;

    // シーン遷移中フラグ
    private bool m_isTransitioning = false;

    // 現在アクティブなゲームオーバーボタン（On Clickイベントに紐付けられている想定）
    private Button m_gameOverButton; // ★追加：ボタンへの参照

    // スクリプトが有効になった最初のフレームで呼び出される
    void Start()
    {
        Debug.Log("GameOverManager: Startが呼び出されました。");

        // firstSelected がボタンであることを期待し、参照を取得する
        if (firstSelected != null)
        {
            m_gameOverButton = firstSelected.GetComponent<Button>();
            if (m_gameOverButton == null)
            {
                Debug.LogWarning("GameOverManager: firstSelectedにButtonコンポーネントが見つかりません。");
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
            Debug.Log($"GameOverManager: UIの初期選択を '{firstSelected.name}' に設定しました。");
        }
        else
        {
            Debug.LogWarning("GameOverManager: firstSelectedが割り当てられていません。");
        }
    }

    // 毎フレーム呼び出される
    void Update()
    {
        // シーン遷移中でない場合のみ入力を受け付ける
        if (m_isTransitioning) return;

        // Submitボタン（Aボタン/Enterキー/Spaceキーなど）が押されたらタイトルに戻る処理を実行
        // ここでは、ボタンがインタラクト可能であることも条件に追加
        if (Input.GetButtonDown("Submit") && m_gameOverButton != null && m_gameOverButton.interactable)
        {
            Debug.Log("GameOverManager: Submitボタンが押されました。");
            OnClickReturnTitle();
        }
    }

    // UIボタンのOnClickイベントからも直接呼び出せる公共メソッド
    public void OnClickReturnTitle()
    {
        // 既に遷移中であれば何もしない
        if (m_isTransitioning)
        {
            Debug.Log("GameOverManager: 既にシーン遷移中のため、処理をスキップします。");
            return;
        }

        m_isTransitioning = true; // 遷移中フラグを立てる
        Debug.Log("GameOverManager: タイトルに戻る処理を開始します。");

        // ボタンが有効な場合、即座にインタラクト不能にする
        if (m_gameOverButton != null)
        {
            m_gameOverButton.interactable = false; // ★追加：ボタンを非活性化
            Debug.Log("GameOverManager: 登録ボタンを非活性化しました。");
        }

        Time.timeScale = 1f; // 念のため、ゲームの時間を通常の速度に戻す（ポーズ解除）

        // GameManagerを通してタイトルシーンへフェードアウトしながら遷移する
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.TitleSceneName); // 定数を使用
        }
        else
        {
            // GameManagerがない場合のフォールバックとして、直接シーンをロード
            Debug.LogError("GameOverManager: GameManager.instanceが見つかりません！直接TitleSceneをロードします。");
            SceneManager.LoadScene(GameManager.TitleSceneName); // 定数を使用
        }
    }
}