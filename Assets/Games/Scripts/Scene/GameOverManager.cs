using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲームオーバー画面のUIとシーン遷移を管理するスクリプトです。
/// </summary>
public class GameOverManager : MonoBehaviour {
    [Header("UI設定"), SerializeField]
    private Selectable _firstSelected; // ゲームパッド操作で最初に選択状態にしたいUI要素

    [Header("タイトルに戻るボタン"), SerializeField]
    private Button _returnTitleButton;

    [Header("もう一度プレイするボタン"), SerializeField]
    private Button _retryButton; 

    [Header("タイトルシーンの名前"), SerializeField]
    private string _titleSceneName = "TitleScene"; 

    private bool _isTransitioning = false;

    private void Start() {
        // UIの初期選択を設定
        if (_firstSelected != null) {
            if (EventSystem.current != null) {
                EventSystem.current.SetSelectedGameObject(_firstSelected.gameObject);
            }
        }
        else {
            Debug.LogWarning("GameOverManager: firstSelectedが割り当てられていません。", this);
        }

        // ボタンのクリックイベントにリスナーを登録
        if (_returnTitleButton != null) {
            _returnTitleButton.onClick.AddListener(OnClickReturnTitle);
        }
        if (_retryButton != null) {
            _retryButton.onClick.AddListener(OnClickRetry);
        }

        // シーンロード完了時に遷移フラグをリセット
        _isTransitioning = false;
    }

    private void OnDestroy() {
        // シーンが破棄される前にイベントリスナーを解除する
        if (_returnTitleButton != null) {
            _returnTitleButton.onClick.RemoveListener(OnClickReturnTitle);
        }
        if (_retryButton != null) {
            _retryButton.onClick.RemoveListener(OnClickRetry);
        }
    }

    /// <summary>
    /// タイトルに戻るボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickReturnTitle() {
        if (_isTransitioning) {
            return;
        }
        _isTransitioning = true;
        Time.timeScale = 1f;

        if (GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(_titleSceneName);
        }
        else {
            SceneManager.LoadScene(_titleSceneName);
        }
    }

    /// <summary>
    /// もう一度プレイするボタンが押された際に呼び出されます。
    /// </summary>
    public void OnClickRetry() {
        if (_isTransitioning) {
            return;
        }
        _isTransitioning = true;
        Time.timeScale = 1f;

        if (GameManager.Instance != null) {
            // GameManagerにゲームプレイ中は常に出るようにしている情報が保存されているので、それを活用してリトライを行う
            GameManager.Instance.ResetGameData();
            GameManager.Instance.RetryLastStage();
        }
        else {
            Debug.LogError("GameOverManager: GameManagerが見つかりません！タイトルシーンへ直接遷移します。", this);
            SceneManager.LoadScene(_titleSceneName);
        }
    }
}
