using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ステージ選択シーン（2ページ目）のUIとイベントを管理するスクリプトです。
/// 各ステージボタンのクリックイベントを制御し、ステージクリア情報を表示します。
/// </summary>
public class StageSelectManager2 : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("UI設定")]
    [Tooltip("ステージ選択シーンのメインCanvas")]
    [SerializeField] private GameObject _stageSelectCanvas;
    [Tooltip("各ステージボタンの配列")]
    [SerializeField] private Button[] _stageButtons;
    [Tooltip("次のページに進むボタン")]
    [SerializeField] private Button _nextButton;
    [Tooltip("次のページに遷移するステージ選択シーン名")]
    [SerializeField] private string _nextStageSelectSceneName = "StageSelect3Scene";

    [Header("クリア表示UI")]
    [Tooltip("各ステージに対応する「クリア」表示のゲームオブジェクト")]
    [SerializeField] private GameObject[] _clearIndicators;

    private const int START_STAGE_INDEX = 3;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        // 必要なUI要素がすべて割り当てられているかチェック
        if (_stageSelectCanvas == null || _stageButtons == null || _nextButton == null || _clearIndicators == null) {
            Debug.LogError("StageSelectManager2: 必要なUI要素がすべて割り当てられていません！インスペクターを確認してください。", this);
            return;
        }

        // キャンバスを有効化
        _stageSelectCanvas.SetActive(true);

        // 各ボタンにクリックリスナーを追加
        for (int i = 0; i < _stageButtons.Length; i++) {
            int stageIndex = i;
            if (_stageButtons[i] != null) {
                _stageButtons[i].onClick.AddListener(() => OnStageButtonClicked(stageIndex));
            }
        }

        // 次へボタンにリスナーを追加
        if (_nextButton != null) {
            _nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        UpdateStageClearIndicators();

        // 初期選択を設定
        if (EventSystem.current != null && _stageButtons.Length > 0 && _stageButtons[0] != null) {
            EventSystem.current.SetSelectedGameObject(_stageButtons[0].gameObject);
        }
    }

    private void OnDestroy() {
        // シーン破棄時にリスナーをクリーンアップ
        if (_stageButtons != null) {
            foreach (Button button in _stageButtons) {
                if (button != null) {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
        if (_nextButton != null) {
            _nextButton.onClick.RemoveAllListeners();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    private void UpdateStageClearIndicators() {
        if (GameManager.Instance == null) {
            Debug.LogError("UpdateStageClearIndicators: GameManagerが見つかりません。", this);
            return;
        }

        if (_clearIndicators.Length != _stageButtons.Length) {
            Debug.LogWarning("UpdateStageClearIndicators: クリア表示とステージボタンの配列の数が一致しません。正しく動作しない可能性があります。", this);
        }

        // すべてのステージボタンを常に表示する
        for (int i = 0; i < _stageButtons.Length; i++) {
            if (_stageButtons[i] != null) {
                _stageButtons[i].gameObject.SetActive(true);
            }
        }

        // すべてのクリア表示を非アクティブにする
        foreach (GameObject indicator in _clearIndicators) {
            if (indicator != null) {
                indicator.SetActive(false);
            }
        }

        // ステージクリア情報を確認し、クリアしたステージのクリア表示を有効にする
        for (int i = 0; i < _clearIndicators.Length; i++) {
            bool isClear = GameManager.Instance.IsStageClear(i + START_STAGE_INDEX);
            if (_clearIndicators[i] != null) {
                _clearIndicators[i].SetActive(isClear);
            }
        }

        // NEXTボタンは常に表示する
        if (_nextButton != null) {
            _nextButton.gameObject.SetActive(true);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    private void OnStageButtonClicked(int index) {
        int gameManagerIndex = index + START_STAGE_INDEX;

        if (GameManager.Instance != null) {
            if (gameManagerIndex >= 0 && gameManagerIndex < GameManager.Instance.StageSceneNames.Length) {
                GameManager.Instance.CurrentStageIndex = gameManagerIndex;
                GameManager.Instance.LoadSceneWithFade(GameManager.Instance.StageSceneNames[gameManagerIndex]);
            }
            else {
                Debug.LogError($"StageSelectManager2: 無効なステージインデックスが渡されました: {gameManagerIndex}", this);
            }
        }
        else {
            Debug.LogError("StageSelectManager2: GameManagerが見つかりません！シーン遷移できません。", this);
        }
    }

    private void OnNextButtonClicked() {
        if (GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(_nextStageSelectSceneName);
        }
        else {
            Debug.LogError("StageSelectManager2: GameManagerが見つかりません！フェードなしで次のステージ選択画面に遷移します。", this);
            SceneManager.LoadScene(_nextStageSelectSceneName);
        }
    }
}
