using UnityEngine;
using UnityEngine.SceneManagement; // シーン管理のために必要
using UnityEngine.EventSystems;    // UIイベントシステムのために必要
using Debug = UnityEngine.Debug;   // Debugの曖昧な参照を解消するため

public class ClearSceneController : MonoBehaviour
{
    // ゲームパッド操作で最初に選択状態にしたいUI要素（ボタンなど）
    [SerializeField] GameObject firstSelected;

    // シーン遷移中フラグ
    private bool m_isTransitioning = false;

    // スクリプトが有効になった最初のフレームで呼び出される
    void Start()
    {
        // Debug.Log("ClearSceneController: Startが呼び出されました。"); // デバッグログ削除
        // ゲームパッドでのUI操作のため、指定されたUI要素にフォーカスを設定する
        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // 一旦現在の選択を解除
            EventSystem.current.SetSelectedGameObject(firstSelected); // 指定されたオブジェクトを選択状態にする
            // Debug.Log($"ClearSceneController: UIの初期選択を '{firstSelected.name}' に設定しました。"); // デバッグログ削除
        }
        else
        {
            Debug.LogWarning("ClearSceneController: firstSelectedが割り当てられていません。");
        }
        m_isTransitioning = false; // シーンロード完了時に遷移フラグをリセット
    }

    // 毎フレーム呼び出される
    void Update()
    {
        // シーン遷移中でない場合のみ入力を受け付ける
        if (m_isTransitioning) return;

        // Submitボタン（Aボタン/Enterキー/Spaceキーなど）が押されたらタイトルに戻る処理を実行
        if (Input.GetButtonDown("Submit"))
        {
            // Debug.Log("ClearSceneController: Submitボタンが押されました。"); // デバッグログ削除
            OnClickReturnTitle();
        }
    }

    // UIボタンのOnClickイベントからも直接呼び出せる公共メソッド
    public void OnClickReturnTitle()
    {
        // 既に遷移中であれば何もしない
        if (m_isTransitioning)
        {
            // Debug.Log("ClearSceneController: 既にシーン遷移中のため、処理をスキップします。"); // デバッグログ削除
            return;
        }

        m_isTransitioning = true; // 遷移中フラグを立てる
        // Debug.Log("ClearSceneController: タイトルに戻る処理を開始します。"); // デバッグログ削除

        Time.timeScale = 1f; // 念のため、ゲームの時間を通常の速度に戻す（ポーズ解除）

        // GameManagerを通してタイトルシーンへフェードアウトしながら遷移する
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade("TitleScene");
            // UnityEngine.Debug.Log("ClearSceneController: タイトルシーンへフェード遷移を開始します。"); // デバッグログ削除
        }
        else
        {
            // GameManagerがない場合のフォールバックとして、直接シーンをロード
            Debug.LogError("ClearSceneController: GameManager インスタンスが見つかりません！直接シーンをロードします。");
            SceneManager.LoadScene("TitleScene");
        }
    }
}
