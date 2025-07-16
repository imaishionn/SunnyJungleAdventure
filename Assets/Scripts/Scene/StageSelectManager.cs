using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageNameText; // 現在のステージ名を表示するUIテキスト
    [SerializeField] private Button nextButton; // 次のステージへ進むボタン
    [SerializeField] private Button prevButton; // 前のステージへ戻るボタン
    [SerializeField] private Button startButton; // ステージを開始するボタン

    void Start()
    {
        // ボタンのリスナーを設定
        if (nextButton != null)
        {
            nextButton.onClick.AddListener(NextStage);
        }
        if (prevButton != null)
        {
            prevButton.onClick.AddListener(PrevStage);
        }
        if (startButton != null)
        {
            startButton.onClick.AddListener(StartStage);
        }

        UpdateStageDisplay();
    }

    void UpdateStageDisplay()
    {
        if (GameManager.instance == null)
        {
            Debug.LogError("StageSelectManager: GameManager.instanceが見つかりません。");
            return;
        }

        // GameManagerのステージ配列とインデックスを参照
        if (GameManager.instance.stageSceneNames != null && GameManager.instance.stageSceneNames.Length > 0)
        {
            string currentStageName = GameManager.instance.stageSceneNames[GameManager.instance.currentStageIndex];
            if (stageNameText != null)
            {
                stageNameText.text = currentStageName;
            }

            // ボタンの有効/無効を設定
            if (prevButton != null)
            {
                prevButton.interactable = GameManager.instance.currentStageIndex > 0;
            }
            if (nextButton != null)
            {
                // 配列のLengthプロパティを使用
                nextButton.interactable = GameManager.instance.currentStageIndex < GameManager.instance.stageSceneNames.Length - 1;
            }
        }
        else
        {
            if (stageNameText != null)
            {
                stageNameText.text = "No Stages Available";
            }
            if (nextButton != null) nextButton.interactable = false;
            if (prevButton != null) prevButton.interactable = false;
            if (startButton != null) startButton.interactable = false;
        }
    }

    public void NextStage()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.stageSceneNames != null && GameManager.instance.currentStageIndex < GameManager.instance.stageSceneNames.Length - 1)
        {
            GameManager.instance.currentStageIndex++;
            UpdateStageDisplay();
        }
    }

    public void PrevStage()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.currentStageIndex > 0)
        {
            GameManager.instance.currentStageIndex--;
            UpdateStageDisplay();
        }
    }

    public void StartStage()
    {
        if (GameManager.instance == null) return;

        if (GameManager.instance.stageSceneNames != null && GameManager.instance.stageSceneNames.Length > 0)
        {
            string sceneToLoad = GameManager.instance.stageSceneNames[GameManager.instance.currentStageIndex];
            GameManager.instance.LoadSceneWithFade(sceneToLoad);
        }
        else
        {
            Debug.LogWarning("StageSelectManager: ロードするステージがありません。");
        }
    }

    public void BackToTitle()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.TitleSceneName);
        }
    }
}