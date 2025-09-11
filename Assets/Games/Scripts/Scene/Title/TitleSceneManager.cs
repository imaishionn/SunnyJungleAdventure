using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// タイトルシーンのUIとイベントを管理するスクリプトです。
/// ボタンクリックによるシーン遷移を制御します。
/// </summary>
public class TitleSceneManager : MonoBehaviour {
    [Header("ゲーム開始ボタン"), SerializeField]
    private Button _startButton; 

    [Header("タイトルシーンのメインCanvas"), SerializeField]
    private GameObject _titleCanvas; 

    /// <summary>
    /// シーン開始時に一度だけ呼び出され、UIの初期設定を行います。
    /// </summary>
    private void Start() {
        if (_titleCanvas != null) {
            // Canvasをアクティブにする。フェードインはGameManagerが担当。
            _titleCanvas.SetActive(true);
        }
        else {
            Debug.LogError("TitleSceneManager: Title Canvas が割り当てられていません！", this);
        }

        if (_startButton != null) {
            // ボタンがクリックされたときのイベントを登録
            _startButton.onClick.AddListener(OnStartButtonClicked);

            // ゲームパッドやキーボード操作のために、初期選択をStartボタンに設定
            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(_startButton.gameObject);
            }
            else {
                Debug.LogWarning("TitleSceneManager: EventSystemが見つかりません。ボタンの初期選択に失敗しました。", this);
            }
        }
        else {
            Debug.LogError("TitleSceneManager: Startボタンが割り当てられていません！", this);
        }
    }

    /// <summary>
    /// コンポーネントが破棄されるときに呼ばれ、イベントリスナーを解除します。
    /// </summary>
    private void OnDestroy() {
        // リスナー解除前にボタンが破棄されていないか確認
        if (_startButton != null) {
            _startButton.onClick.RemoveListener(OnStartButtonClicked);
        }
    }

    /// <summary>
    /// スタートボタンがクリックされたときに呼ばれ、ステージ選択シーンへ遷移します。
    /// </summary>
    private void OnStartButtonClicked() {
        if (GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.StageSelectSceneName);
        }
        else {
            // GameManager.Instanceがnullのため、エラーログを出力
            Debug.LogError("TitleSceneManager: GameManagerが見つかりません！シーン遷移できません。", this);
        }
    }
}
