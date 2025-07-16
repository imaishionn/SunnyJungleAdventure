using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageSelectManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageNameText; // ���݂̃X�e�[�W����\������UI�e�L�X�g
    [SerializeField] private Button nextButton; // ���̃X�e�[�W�֐i�ރ{�^��
    [SerializeField] private Button prevButton; // �O�̃X�e�[�W�֖߂�{�^��
    [SerializeField] private Button startButton; // �X�e�[�W���J�n����{�^��

    void Start()
    {
        // �{�^���̃��X�i�[��ݒ�
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
            Debug.LogError("StageSelectManager: GameManager.instance��������܂���B");
            return;
        }

        // GameManager�̃X�e�[�W�z��ƃC���f�b�N�X���Q��
        if (GameManager.instance.stageSceneNames != null && GameManager.instance.stageSceneNames.Length > 0)
        {
            string currentStageName = GameManager.instance.stageSceneNames[GameManager.instance.currentStageIndex];
            if (stageNameText != null)
            {
                stageNameText.text = currentStageName;
            }

            // �{�^���̗L��/������ݒ�
            if (prevButton != null)
            {
                prevButton.interactable = GameManager.instance.currentStageIndex > 0;
            }
            if (nextButton != null)
            {
                // �z���Length�v���p�e�B���g�p
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
            Debug.LogWarning("StageSelectManager: ���[�h����X�e�[�W������܂���B");
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