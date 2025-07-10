using System.Collections; // コルーチンのために必要
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem を使うために追加
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button クラスを使うために追加
using Debug = UnityEngine.Debug; // ★ここが重要: Debugの曖昧な参照を解消するため、UnityEngine.Debugを明示的に指定★

/// <summary>
/// タイトルシーンのUIボタンのクリックイベントを処理し、
/// GameManagerを通してゲームプレイシーンへ遷移するためのスクリプト。
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    [SerializeField, Header("初期選択ボタン")]
    private Button startButton; // STARTボタンへの参照をInspectorで設定

    void Start()
    {
        // シーンロード時に初期ボタンを選択状態にする
        // GameManagerがDontDestroyOnLoadでEventSystemを作成しているため、
        // ここでEventSystem.currentが利用可能になっているはず
        if (EventSystem.current != null && startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            Debug.Log("TitleSceneManager: 初期ボタン「" + startButton.name + "」を選択状態にしました。");
        }
        else if (EventSystem.current == null)
        {
            Debug.LogError("TitleSceneManager: EventSystem.current が見つかりません。GameManagerがEventSystemを作成しているか確認してください。");
        }
        else if (startButton == null)
        {
            Debug.LogWarning("TitleSceneManager: startButton がInspectorで割り当てられていません。"); // ★この警告が出ています★
        }
    }

    /// <summary>
    /// STARTボタンがクリックされたときに呼び出されます。
    /// ゲームプレイシーンへ遷移します。
    /// </summary>
    public void OnStartButtonClicked()
    {
        UnityEngine.Debug.Log("TitleSceneManager: OnStartButtonClickedが呼び出されました！"); // ボタンが押されたことを確認

        // 現在選択されているUI要素をクリア
        // これにより、新しいシーンがロードされたときに、そのシーンのデフォルトの選択可能なUI要素が自動的に選択されるようにします。
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // コルーチンを開始してGameManagerの準備を待つ
        StartCoroutine(LoadGameSceneWhenReady());
    }

    /// <summary>
    /// GameManagerのインスタンスが準備できるまで待機し、その後シーン遷移を行うコルーチン。
    /// </summary>
    private IEnumerator LoadGameSceneWhenReady()
    {
        UnityEngine.Debug.Log("TitleSceneManager: GameManagerの準備を待機中...");

        // GameManager.instanceがnullでなくなるまで待機
        while (GameManager.instance == null)
        {
            UnityEngine.Debug.Log("TitleSceneManager: GameManager.instanceはまだnullです。1フレーム待機します。");
            yield return null; // 1フレーム待つ
        }

        UnityEngine.Debug.Log("TitleSceneManager: GameManager.instanceが見つかりました！シーン遷移を開始します。");
        GameManager.instance.LoadSceneWithFade("Demo_tileset"); // "Demo_tileset" はあなたのメインゲームシーンの名前
    }
}
