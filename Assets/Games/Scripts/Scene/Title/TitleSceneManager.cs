using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// タイトルシーンのUIとイベントを管理するスクリプトです。
/// ボタンクリックによるシーン遷移を制御します。
/// </summary>
public class TitleSceneManager : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("UI設定")]
    [Tooltip("ゲーム開始ボタン")]
    [SerializeField] private Button _startButton;
    [Tooltip("タイトルシーンのメインCanvas")]
    [SerializeField] private GameObject _titleCanvas;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        // Canvasの存在チェック
        if(_titleCanvas != null) {
            // Canvasをアクティブにする。フェードインはGameManagerが担当。
            _titleCanvas.SetActive(true);
        }
        else {
            Debug.LogError("TitleSceneManager: Title Canvas が割り当てられていません！",this);
        }

        // スタートボタンの存在チェックとリスナーの登録
        if(_startButton != null) {
            // ボタンがクリックされたときのイベントを登録
            _startButton.onClick.AddListener(OnStartButtonClicked);

            // ゲームパッドやキーボード操作のために、初期選択をStartボタンに設定
            if(EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
            }
            else {
                Debug.LogWarning("TitleSceneManager: EventSystemが見つかりません。ボタンの初期選択に失敗しました。",this);
            }
        }
        else {
            Debug.LogError("TitleSceneManager: Startボタンが割り当てられていません！",this);
        }
    }

    private void OnDestroy() {
        // シーン遷移時にボタンのイベントリスナーを解除する
        if(_startButton != null) {
            _startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// スタートボタンがクリックされたときに呼ばれます。
    /// </summary>
    private void OnStartButtonClicked() {
        if(GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.StageSelectSceneName);
        }
        else {
            // GameManager.Instanceがnullのため、StageSelectSceneNameに直接アクセスせず、エラーログを出力
            // フォールバック処理が必要な場合は、予め設定したシーン名などを使用する
            Debug.LogError("TitleSceneManager: GameManagerが見つかりません！シーン遷移できません。",this);
        }
    }
}
