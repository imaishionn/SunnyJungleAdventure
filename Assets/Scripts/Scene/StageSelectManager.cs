using UnityEngine;
using UnityEngine.EventSystems; // ボタンの初期選択用
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Debugの曖昧な参照を解消するため明示的に指定
using Debug = UnityEngine.Debug;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private Button[] stageButtons; // 各ステージボタンをインスペクターで設定してください
    [SerializeField] private Button backButton; // 戻るボタンなどがあれば
    [SerializeField] private GameObject stageSelectCanvas; // ステージ選択シーンのメインCanvas
    [SerializeField] private string titleSceneName = "TitleScene"; // 戻るボタンのフォールバック用

    // --- ★ここから追加★ ---
    [Header("UI要素 (Inspectorで設定)")]
    // 各ステージに対応する「クリア」文字のオブジェクト
    [SerializeField] private GameObject[] clearIndicators;
    // --- ★追加ここまで★ ---

    private void Start()
    {
        if (stageSelectCanvas != null)
        {
            stageSelectCanvas.SetActive(true);
        }
        else
        {
            Debug.LogError("StageSelectManager: StageSelect Canvas が割り当てられていません！", this);
        }

        if (stageButtons != null && stageButtons.Length > 0)
        {
            for (int i = 0; i < stageButtons.Length; i++)
            {
                int stageIndex = i; // クロージャの問題回避
                if (stageButtons[i] != null)
                {
                    stageButtons[i].onClick.AddListener(() => OnStageButtonClicked(stageIndex));
                }
            }

            // 初期選択を設定（最初のステージボタンなど）
            if (EventSystem.current != null && stageButtons[0] != null)
            {
                EventSystem.current.SetSelectedGameObject(stageButtons[0].gameObject);
            }
        }
        else
        {
            Debug.LogWarning("StageSelectManager: ステージボタンが割り当てられていません！", this);
        }

        if (backButton != null)
        {
            backButton.onClick.AddListener(OnBackButtonClicked);
        }

        // --- ★ここから追加★ ---
        // ステージクリア表示の更新
        UpdateStageClearIndicators();
        // --- ★追加ここまで★ ---
    }

    private void OnStageButtonClicked(int index)
    {
        if (GameManager.instance != null)
        {
            if (index >= 0 && index < GameManager.instance.stageSceneNames.Length)
            {
                GameManager.instance.currentStageIndex = index;
                GameManager.instance.LoadSceneWithFade(GameManager.instance.stageSceneNames[index]);
            }
            else
            {
                Debug.LogError($"StageSelectManager: 無効なステージインデックス: {index}", this);
            }
        }
        else
        {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしで遷移します。", this);
            if (index >= 0 && index < stageButtons.Length)
            {
                // GameManagerが存在しないため、ステージ名を直接指定してロードする必要がある
                // 例: SceneManager.LoadScene("Demo_tileset");
            }
        }
    }

    private void OnBackButtonClicked()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.instance.TitleSceneName);
        }
        else
        {
            Debug.LogError("StageSelectManager: GameManagerが見つかりません！フェードなしでタイトルに遷移します。", this);
            SceneManager.LoadScene(titleSceneName);
        }
    }

    // --- ★ここから追加★ ---
    /// <summary>
    /// ステージクリアの情報をチェックし、クリアインジケーターの表示を更新します。
    /// </summary>
    private void UpdateStageClearIndicators()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("UpdateStageClearIndicators: GameManagerが見つかりません。", this);
            return;
        }

        if (clearIndicators == null || stageButtons == null || clearIndicators.Length != stageButtons.Length)
        {
            Debug.LogWarning("UpdateStageClearIndicators: clearIndicators配列とstageButtons配列の数が一致しません。または設定されていません。", this);
            return;
        }

        for (int i = 0; i < stageButtons.Length; i++)
        {
            bool isClear = GameManager.instance.IsStageClear(i);
            if (clearIndicators[i] != null)
            {
                clearIndicators[i].SetActive(isClear);
            }
        }
    }
    // --- ★追加ここまで★ ---
}