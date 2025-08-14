using UnityEngine;
using UnityEngine.EventSystems; // ボタンの初期選択用
using UnityEngine.SceneManagement; // SceneManager を使用 (直接呼び出しは減らすが念のため)
using UnityEngine.UI;

// Debugの曖昧な参照を解消するため明示的に指定
using Debug = UnityEngine.Debug;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private Button startButton; // インスペクターで設定してください
    [SerializeField] private GameObject titleCanvas; // タイトルシーンのメインCanvas（表示/非表示用）

    private void Start()
    {
        // GameManagerからのAwake/OnSceneLoadedで状態は既にTitleに設定されているはず
        // なので、ここでSetStateを呼び出す必要はないが、念のため状態確認はOK
 
        if (titleCanvas != null)
        {
            // 既にフェードイン完了後なので、Startではアクティブにしておく
            // フェードインはGameManagerのOnSceneLoadedで制御されるため、
            // ここでCanvasのActive/Deactiveを直接制御する必要は基本的にない
            titleCanvas.SetActive(true);
  
        }
        else
        {
            Debug.LogError("TitleSceneManager: Title Canvas が割り当てられていません！", this);
        }


        if (startButton != null)
        {
            startButton.onClick.AddListener(OnStartButtonClicked);


            // ゲームパッド/キーボード操作のために初期選択を設定
            // ボタンにButtonSoundEffectが付いている場合、ここでSetSelectedGameObjectを呼ぶと
            // OnSelectが発火してサウンドが鳴る可能性があります。
            // 必要に応じて、サウンド再生ロジックを調整してください。
            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
                // Debug.Log("TitleSceneManager: 初期選択をStartボタンに設定しました。", this); // 削除
            }
        }
        else
        {
            Debug.LogError("TitleSceneManager: Startボタンが割り当てられていません！", this);
        }
    }

    private void OnStartButtonClicked()
    {


        // GameManagerのフェード付きシーンロードを呼び出す
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.instance.StageSelectSceneName);
        }
        else
        {
            Debug.LogError("TitleSceneManager: GameManagerが見つかりません！フェードなしで遷移します。", this);
            SceneManager.LoadScene(GameManager.instance.StageSelectSceneName); // フォールバック
        }
    }
}