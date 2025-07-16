using System.Collections; // �R���[�`���̂��߂ɕK�v
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem ���g�����߂ɒǉ�
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button �N���X���g�����߂ɒǉ�
using Debug = UnityEngine.Debug; // �������d�v: Debug�̞B���ȎQ�Ƃ��������邽�߁AUnityEngine.Debug�𖾎��I�Ɏw��

/// <summary>
/// �^�C�g���V�[����UI�{�^���̃N���b�N�C�x���g���������A
/// GameManager��ʂ��ăQ�[���v���C�V�[���֑J�ڂ��邽�߂̃X�N���v�g�B
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    [SerializeField, Header("�����I���{�^��")]
    private Button startButton; // START�{�^���ւ̎Q�Ƃ�Inspector�Őݒ�

    // ���Ȃ��̃X�e�[�W�I���V�[�����ɐݒ肵�Ă�������
    [SerializeField, Header("�X�e�[�W�I���V�[����")]
    private string stageSelectSceneName = "StageSelect"; // ��: "StageSelectScene" �� "LevelSelect" �Ȃ�

    void Start()
    {
        // �V�[�����[�h���ɏ����{�^����I����Ԃɂ���
        if (EventSystem.current != null && startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
        }
        else if (EventSystem.current == null)
        {
            Debug.LogError("TitleSceneManager: EventSystem.current ��������܂���BGameManager��EventSystem���쐬���Ă��邩�m�F���Ă��������B");
        }
        else if (startButton == null)
        {
            Debug.LogWarning("TitleSceneManager: startButton ��Inspector�Ŋ��蓖�Ă��Ă��܂���B");
        }
    }

    /// <summary>
    /// START�{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo����܂��B
    /// �X�e�[�W�I���V�[���֑J�ڂ��܂��B
    /// </summary>
    public void OnStartButtonClicked()
    {
        // ���ݑI������Ă���UI�v�f���N���A
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // �R���[�`�����J�n����GameManager�̏�����҂�
        StartCoroutine(LoadStageSelectSceneWhenReady());
    }

    /// <summary>
    /// GameManager�̃C���X�^���X�������ł���܂őҋ@���A���̌�X�e�[�W�I���V�[���J�ڂ��s���R���[�`���B
    /// </summary>
    private IEnumerator LoadStageSelectSceneWhenReady()
    {
        // GameManager.instance��null�łȂ��Ȃ�܂őҋ@
        while (GameManager.instance == null)
        {
            yield return null; // 1�t���[���҂�
        }

        // �X�e�[�W�I���V�[���֑J��
        GameManager.instance.LoadSceneWithFade(stageSelectSceneName);
    }
}