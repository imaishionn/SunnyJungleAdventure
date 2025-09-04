using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ステージ選択シーンのUIとイベントを管理するスクリプトです。
/// 各ステージボタンのクリックイベントを制御し、ステージクリア情報を表示します。
/// </summary>
public class StageSelectManager : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("UI設定")]
    [Tooltip("ステージ選択シーンのメインCanvas")]
    [SerializeField] private GameObject stageSelectCanvas;
    [Tooltip("各ステージボタンの配列")]
    [SerializeField] private Button[] stageButtons;
    [Tooltip("タイトルシーンに戻るボタン")]
    [SerializeField] private Button backButton;
    [Tooltip("次のページに進むボタン")]
    [SerializeField] private Button nextButton;
    [Tooltip("戻るボタンで遷移するタイトルシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene";
    [Tooltip("次のステージ選択シーン名")]
    [SerializeField] private string nextStageSelectSceneName = "StageSelect2Scene";

    [Header("クリア表示UI")]
    [Tooltip("各ステージに対応する「クリア」表示のゲームオブジェクト")]
    [SerializeField] private GameObject[] clearIndicators;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start() {
        if(stageSelectCanvas != null) {
            stageSelectCanvas.SetActive(true);
        }
        else {
            Debug.LogError("StageSelectManager: StageSelect Canvas が割り当てられていません！",this);
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
            Debug.LogWarning("StageSelectManager: ステージボタンが割り当てられていません！",this);
        }

        if(backButton != null) {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        if(nextButton != null) {
            nextButton.onClick.AddListener(OnNextButtonClicked);
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

        if(nextButton != null) {
            nextButton.onClick.RemoveAllListeners();
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
            bool isClear = GameManager.Instance.IsStageClear(i);
            if(clearIndicators[i] != null) {
                clearIndicators[i].SetActive(isClear);
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    private void OnStageButtonClicked(int index) {
        if(GameManager.Instance != null) {
            if(index >= 0 && index < GameManager.Instance.stageSceneNames.Length) {
                GameManager.Instance.currentStageIndex = index;
                GameManager.Instance.LoadSceneWithFade(GameManager.Instance.stageSceneNames[index]);
            }
            else {
                Debug.LogError($"StageSelectManager: 無効なステージインデックスが渡されました: {index}",this);
            }
        }
        else {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしで遷移します。",this);
            // GameManagerがない場合、ステージ名がわからないため遷移できない
            // 処理を中止
        }
    }

    private void OnBackButtonClicked() {
        if(GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.TitleSceneName);
        }
        else {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしでタイトルに遷移します。",this);
            SceneManager.LoadScene(titleSceneName);
        }
    }

    private void OnNextButtonClicked() {
        if(GameManager.Instance != null) {
            GameManager.Instance.LoadSceneWithFade(nextStageSelectSceneName);
        }
        else {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしで次のステージ選択画面に遷移します。",this);
            SceneManager.LoadScene(nextStageSelectSceneName);
        }
    }
}