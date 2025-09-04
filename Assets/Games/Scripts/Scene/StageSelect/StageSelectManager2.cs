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
    [SerializeField] private GameObject stageSelectCanvas;
    [Tooltip("各ステージボタンの配列")]
    [SerializeField] private Button[] stageButtons;
    [Tooltip("前のページに戻るボタン")]
    [SerializeField] private Button backButton;
    [Tooltip("戻るボタンで遷移する前のステージ選択シーン名")]
    [SerializeField] private string previousStageSelectSceneName = "StageSelect";

    [Header("クリア表示UI")]
    [Tooltip("各ステージに対応する「クリア」表示のゲームオブジェクト")]
    [SerializeField] private GameObject[] clearIndicators;

    private const int START_STAGE_INDEX = 3;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        if(stageSelectCanvas != null) {
            stageSelectCanvas.SetActive(true);
        }
        else {
            Debug.LogError("StageSelectManager2: StageSelect Canvas が割り当てられていません！",this);
        }

        if(stageButtons != null && stageButtons.Length > 0) {
            for(int i = 0;i < stageButtons.Length;i++) {
                int stageIndex = i;
                if(stageButtons[i] != null) {
                    stageButtons[i].onClick.AddListener(() => OnStageButtonClicked(stageIndex));
                }
            }

            // 初期選択を設定
            if(EventSystem.current != null && stageButtons[0] != null) {
                EventSystem.current.SetSelectedGameObject(stageButtons[0].gameObject);
            }
        }
        else {
            Debug.LogWarning("StageSelectManager2: ステージボタンが割り当てられていません！",this);
        }

        if(backButton != null) {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        UpdateStageClearIndicators();
    }

    private void OnDestroy() {
        if(stageButtons != null) {
            foreach(var button in stageButtons) {
                if(button != null) {
                    button.onClick.RemoveAllListeners();
                }
            }
        }
        if(backButton != null) {
            backButton.onClick.RemoveAllListeners();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    private void UpdateStageClearIndicators() {
        if(GameManager.Instance == null) {
            Debug.LogError("UpdateStageClearIndicators: GameManagerが見つかりません。",this);
            return;
        }

        if(clearIndicators == null || stageButtons == null || clearIndicators.Length != stageButtons.Length) {
            Debug.LogWarning("UpdateStageClearIndicators: clearIndicators配列とstageButtons配列の数が一致しません。または設定されていません。",this);
            return;
        }

        for(int i = 0;i < stageButtons.Length;i++) {
            bool isClear = GameManager.Instance.IsStageClear(i + START_STAGE_INDEX);
            if(clearIndicators[i] != null) {
                clearIndicators[i].SetActive(isClear);
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    private void OnStageButtonClicked(int index) {
        int gameManagerIndex = index + START_STAGE_INDEX;

        if(GameManager.Instance != null) {
            if(gameManagerIndex >= 0 && gameManagerIndex < GameManager.Instance.stageSceneNames.Length) {
                GameManager.Instance.currentStageIndex = gameManagerIndex;
                GameManager.Instance.LoadSceneWithFade(GameManager.Instance.stageSceneNames[gameManagerIndex]);
            }
            else {
                Debug.LogError($"StageSelectManager2: 無効なステージインデックスが渡されました: {gameManagerIndex}",this);
            }
        }
        else {
            Debug.LogError("StageSelectManager2: GameManagerが見つかりません！フェードなしで遷移します。",this);
            // GameManagerがない場合、ステージ名がわからないため遷移できない
            // 処理を中止
        }
    }

    private void OnBackButtonClicked() {
        if(GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(previousStageSelectSceneName);
        }
        else {
            Debug.LogError("StageSelectManager2: GameManagerが見つかりません！フェードなしで前のステージ選択画面に遷移します。",this);
            SceneManager.LoadScene(previousStageSelectSceneName);
        }
    }
}
