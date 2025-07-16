using System.Collections; // コルーチンのために必要
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem を使うために追加
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button クラスを使うために追加
using Debug = UnityEngine.Debug; // ここが重要: Debugの曖昧な参照を解消するため、UnityEngine.Debugを明示的に指定

/// <summary>
/// タイトルシーンのUIボタンのクリックイベントを処理し、
/// GameManagerを通してゲームプレイシーンへ遷移するためのスクリプト。
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    [SerializeField, Header("初期選択ボタン")]
    private Button startButton; // STARTボタンへの参照をInspectorで設定

    // あなたのステージ選択シーン名に設定してください
    [SerializeField, Header("ステージ選択シーン名")]
    private string stageSelectSceneName = "StageSelect"; // 例: "StageSelectScene" や "LevelSelect" など

    void Start()
    {
        // シーンロード時に初期ボタンを選択状態にする
        if (EventSystem.current != null && startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
        else if (EventSystem.current == null)
        {
            Debug.LogError("TitleSceneManager: EventSystem.current が見つかりません。GameManagerがEventSystemを作成しているか確認してください。");
        }
        else if (startButton == null)
        {
            Debug.LogWarning("TitleSceneManager: startButton がInspectorで割り当てられていません。");
        }
    }

    /// <summary>
    /// STARTボタンがクリックされたときに呼び出されます。
    /// ステージ選択シーンへ遷移します。
    /// </summary>
    public void OnStartButtonClicked()
    {
        // 現在選択されているUI要素をクリア
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // コルーチンを開始してGameManagerの準備を待つ
        StartCoroutine(LoadStageSelectSceneWhenReady());
    }

    /// <summary>
    /// GameManagerのインスタンスが準備できるまで待機し、その後ステージ選択シーン遷移を行うコルーチン。
    /// </summary>
    private IEnumerator LoadStageSelectSceneWhenReady()
    {
        // GameManager.instanceがnullでなくなるまで待機
        while (GameManager.instance == null)
        {
            yield return null; // 1フレーム待つ
        }

        // ステージ選択シーンへ遷移
        GameManager.instance.LoadSceneWithFade(stageSelectSceneName);
    }
}