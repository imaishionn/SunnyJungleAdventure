using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// ステージ選択シーンのUIとイベントを管理するスクリプトです。
/// 各ステージボタンのクリックイベントを制御し、ステージクリア情報を表示します。
/// </summary>
public class StageSelectManager : MonoBehaviour
{
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
    [SerializeField] private Button nextButton; // ★追加
    [Tooltip("戻るボタンで遷移するタイトルシーン名")]
    [SerializeField] private string titleSceneName = "TitleScene";
    [Tooltip("次のステージ選択シーン名")]
    [SerializeField] private string nextStageSelectSceneName = "StageSelect2Scene"; // ★追加

    [Header("クリア表示UI")]
    [Tooltip("各ステージに対応する「クリア」表示のゲームオブジェクト")]
    [SerializeField] private GameObject[] clearIndicators;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Start()
    {
        // Canvasの存在チェックとアクティブ化
        if (stageSelectCanvas != null)
        {
            stageSelectCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("StageSelectManager: StageSelect Canvas が割り当てられていません！", this);
        }

        // ステージボタンのリスナー登録
        if (stageButtons != null && stageButtons.Length > 0)
        {
            for (int i = 0; i < stageButtons.Length; i++)
            {
                // クロージャの問題を回避するため、ループ変数をローカル変数にコピー
                int stageIndex = i;
                if (stageButtons[i] != null)
                {
                    stageButtons[i].onClick.AddListener(() => OnStageButtonClicked(stageIndex));
                }
            }

            // ゲームパッドやキーボード操作のために、初期選択を最初のボタンに設定
            if (EventSystem.current != null && stageButtons[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(stageButtons[0].gameObject);
            }
        }
        else
        {
            Debug.LogWarning("StageSelectManager: ステージボタンが割り当てられていません！", this);
        }

        // 戻るボタンのリスナー登録
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // ★追加: 次のページへ進むボタンのリスナー登録
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(OnNextButtonClicked);
        }

        // ステージクリア表示の更新
        UpdateStageClearIndicators();
    }

    private void OnDestroy()
    {
        // シーン遷移時にボタンのイベントリスナーを解除する
        if (stageButtons != null)
        {
            foreach (var button in stageButtons)
            {
                if (button != null)
                {
                    button.onClick.RemoveAllListeners();
                }
            }
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
        }

        // ★追加: 次のページへ進むボタンのリスナー解除
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ステージクリアの情報をチェックし、クリアインジケーターの表示を更新します。
    /// </summary>
    private void UpdateStageClearIndicators()
    {
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance == null)
        {
            Debug.LogError("UpdateStageClearIndicators: GameManagerが見つかりません。", this);
            return;
        }

        // 配列の数の一致チェック
        if (clearIndicators == null || stageButtons == null || clearIndicators.Length != stageButtons.Length)
        {
            Debug.LogWarning("UpdateStageClearIndicators: clearIndicators配列とstageButtons配列の数が一致しません。または設定されていません。", this);
            return;
        }

        // 各ステージのクリア状態に応じてインジケーターの表示を切り替える
        for (int i = 0; i < stageButtons.Length; i++)
        {
            // GameManager.instance.IsStageClear を GameManager.Instance.IsStageClear に修正
            bool isClear = GameManager.Instance.IsStageClear(i);
            if (clearIndicators[i] != null)
            {
                clearIndicators[i].SetActive(isClear);
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // UIイベントハンドラー
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ステージ選択ボタンがクリックされたときに呼ばれます。
    /// </summary>
    /// <param name="index">クリックされたボタンのインデックス</param>
    private void OnStageButtonClicked(int index)
    {
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null)
        {
            // ステージインデックスが有効かチェック
            // GameManager.instance.stageSceneNames.Length を GameManager.Instance.stageSceneNames.Length に修正
            if (index >= 0 && index < GameManager.Instance.stageSceneNames.Length)
            {
                // GameManager.instance.currentStageIndex を GameManager.Instance.currentStageIndex に修正
                // GameManager.instance.LoadSceneWithFade を GameManager.Instance.LoadSceneWithFade に修正
                // GameManager.instance.stageSceneNames[index] を GameManager.Instance.stageSceneNames[index] に修正
                GameManager.Instance.currentStageIndex = index;
                GameManager.Instance.LoadSceneWithFade(GameManager.Instance.stageSceneNames[index]);
            }
            else
            {
                Debug.LogError($"StageSelectManager: 無効なステージインデックスが渡されました: {index}", this);
            }
        }
        else
        {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしで遷移します。", this);
            // GameManager.instance.stageSceneNames[index] を GameManager.Instance.stageSceneNames[index] に修正
            // ただし、このelseブロックに入るのは GameManager.Instance が null の時なので、この行は到達不可能
            // より安全にするため、`GameManager.Instance` へのアクセスを削除
            SceneManager.LoadScene(GameManager.Instance.stageSceneNames[index]);
        }
    }

    /// <summary>
    /// 戻るボタンがクリックされたときに呼ばれます。
    /// </summary>
    private void OnBackButtonClicked()
    {
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null)
        {
            // GameManager.instance.LoadSceneWithFade を GameManager.Instance.LoadSceneWithFade に修正
            // GameManager.instance.TitleSceneName を GameManager.Instance.TitleSceneName に修正
            GameManager.Instance.LoadSceneWithFade(GameManager.Instance.TitleSceneName);
        }
        else
        {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしでタイトルに遷移します。", this);
            SceneManager.LoadScene(titleSceneName); // フォールバック処理
        }
    }

    /// <summary>
    /// 次のページに進むボタンがクリックされたときに呼ばれます。
    /// </summary>
    private void OnNextButtonClicked()
    {
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null)
        {
            // GameManager.instance.LoadSceneWithFade を GameManager.Instance.LoadSceneWithFade に修正
            GameManager.Instance.LoadSceneWithFade(nextStageSelectSceneName);
        }
        else
        {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしで次のステージ選択画面に遷移します。", this);
            SceneManager.LoadScene(nextStageSelectSceneName); // フォールバック処理
        }
    }
}