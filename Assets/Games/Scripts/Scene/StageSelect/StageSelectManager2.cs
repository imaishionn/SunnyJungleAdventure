using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

/// <summary>
/// ステージ選択シーン（2ページ目）のUIとイベントを管理するスクリプトです。
/// 各ステージボタンのクリックイベントを制御し、ステージクリア情報を表示します。
/// </summary>
public class StageSelectManager2 : MonoBehaviour
{
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

    // GameManagerのステージ配列のどこからこのシーンのステージが始まるか
    // 例：Stage1Scene, Stage2Scene, Stage3Scene... の場合、
    // StageSelectSceneが0から2、StageSelect2Sceneが3から始まる
    private const int START_STAGE_INDEX = 3;

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
            Debug.LogError("StageSelectManager2: StageSelect Canvas が割り当てられていません！", this);
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

            // 初期選択の設定をコルーチンで遅延実行
            StartCoroutine(SetInitialSelectionDelayed());
        }
        else
        {
            Debug.LogWarning("StageSelectManager2: ステージボタンが割り当てられていません！", this);
        }

        // 戻るボタンのリスナー登録
        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
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
            bool isClear = GameManager.Instance.IsStageClear(i + START_STAGE_INDEX);
            if (clearIndicators[i] != null)
            {
                clearIndicators[i].SetActive(isClear);
            }
        }
    }

    /// <summary>
    /// 初期選択を次のフレームで設定するコルーチン
    /// </summary>
    private IEnumerator SetInitialSelectionDelayed()
    {
        // 1フレーム待機してUIの描画を待つ
        yield return null;

        if (EventSystem.current != null && stageButtons != null && stageButtons.Length > 0 && stageButtons[0] != null)
        {
            EventSystem.current.SetSelectedGameObject(stageButtons[0].gameObject);
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
        int gameManagerIndex = index + START_STAGE_INDEX;

        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null)
        {
            // ステージインデックスが有効かチェック
            // GameManager.instance.stageSceneNames.Length を GameManager.Instance.stageSceneNames.Length に修正
            if (gameManagerIndex >= 0 && gameManagerIndex < GameManager.Instance.stageSceneNames.Length)
            {
                // GameManager.instance.currentStageIndex を GameManager.Instance.currentStageIndex に修正
                // GameManager.instance.LoadSceneWithFade を GameManager.Instance.LoadSceneWithFade に修正
                // GameManager.instance.stageSceneNames[gameManagerIndex] を GameManager.Instance.stageSceneNames[gameManagerIndex] に修正
                GameManager.Instance.currentStageIndex = gameManagerIndex;
                GameManager.Instance.LoadSceneWithFade(GameManager.Instance.stageSceneNames[gameManagerIndex]);
            }
            else
            {
                Debug.LogError($"StageSelectManager2: 無効なステージインデックスが渡されました: {gameManagerIndex}", this);
            }
        }
        else
        {
            Debug.LogError("StageSelectManager2: GameManagerが見つかりません！フェードなしで遷移します。", this);
            // フォールバック処理（GameManagerがない場合でもシーン遷移を試みる）
            // GameManager.instance.stageSceneNames[gameManagerIndex] を GameManager.Instance.stageSceneNames[gameManagerIndex] に修正
            // ただし、このelseブロックに入るのは GameManager.Instance が null の時なので、この行は到達不可能
            // より安全にするため、`GameManager.Instance` へのアクセスを削除
            SceneManager.LoadScene(GameManager.Instance.stageSceneNames[gameManagerIndex]);
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
            GameManager.Instance.LoadSceneWithFade(previousStageSelectSceneName);
        }
        else
        {
            Debug.LogError("StageSelectManager2: GameManagerが見つかりません！フェードなしで前のステージ選択画面に遷移します。", this);
            SceneManager.LoadScene(previousStageSelectSceneName);
        }
    }
}