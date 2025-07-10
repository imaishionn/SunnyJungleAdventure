using System.Collections; // �R���[�`���̂��߂ɕK�v
using UnityEngine;
using UnityEngine.EventSystems; // EventSystem ���g�����߂ɒǉ�
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Button �N���X���g�����߂ɒǉ�
using Debug = UnityEngine.Debug; // ���������d�v: Debug�̞B���ȎQ�Ƃ��������邽�߁AUnityEngine.Debug�𖾎��I�Ɏw�聚

/// <summary>
/// �^�C�g���V�[����UI�{�^���̃N���b�N�C�x���g���������A
/// GameManager��ʂ��ăQ�[���v���C�V�[���֑J�ڂ��邽�߂̃X�N���v�g�B
/// </summary>
public class TitleSceneManager : MonoBehaviour
{
    [SerializeField, Header("�����I���{�^��")]
    private Button startButton; // START�{�^���ւ̎Q�Ƃ�Inspector�Őݒ�

    void Start()
    {
        // �V�[�����[�h���ɏ����{�^����I����Ԃɂ���
        // GameManager��DontDestroyOnLoad��EventSystem���쐬���Ă��邽�߁A
        // ������EventSystem.current�����p�\�ɂȂ��Ă���͂�
        if (EventSystem.current != null && startButton != null)
        {
            EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            Debug.Log("TitleSceneManager: �����{�^���u" + startButton.name + "�v��I����Ԃɂ��܂����B");
        }
        else if (EventSystem.current == null)
        {
            Debug.LogError("TitleSceneManager: EventSystem.current ��������܂���BGameManager��EventSystem���쐬���Ă��邩�m�F���Ă��������B");
        }
        else if (startButton == null)
        {
            Debug.LogWarning("TitleSceneManager: startButton ��Inspector�Ŋ��蓖�Ă��Ă��܂���B"); // �����̌x�����o�Ă��܂���
        }
    }

    /// <summary>
    /// START�{�^�����N���b�N���ꂽ�Ƃ��ɌĂяo����܂��B
    /// �Q�[���v���C�V�[���֑J�ڂ��܂��B
    /// </summary>
    public void OnStartButtonClicked()
    {
        UnityEngine.Debug.Log("TitleSceneManager: OnStartButtonClicked���Ăяo����܂����I"); // �{�^���������ꂽ���Ƃ��m�F

        // ���ݑI������Ă���UI�v�f���N���A
        // ����ɂ��A�V�����V�[�������[�h���ꂽ�Ƃ��ɁA���̃V�[���̃f�t�H���g�̑I���\��UI�v�f�������I�ɑI�������悤�ɂ��܂��B
        if (EventSystem.current != null)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // �R���[�`�����J�n����GameManager�̏�����҂�
        StartCoroutine(LoadGameSceneWhenReady());
    }

    /// <summary>
    /// GameManager�̃C���X�^���X�������ł���܂őҋ@���A���̌�V�[���J�ڂ��s���R���[�`���B
    /// </summary>
    private IEnumerator LoadGameSceneWhenReady()
    {
        UnityEngine.Debug.Log("TitleSceneManager: GameManager�̏�����ҋ@��...");

        // GameManager.instance��null�łȂ��Ȃ�܂őҋ@
        while (GameManager.instance == null)
        {
            UnityEngine.Debug.Log("TitleSceneManager: GameManager.instance�͂܂�null�ł��B1�t���[���ҋ@���܂��B");
            yield return null; // 1�t���[���҂�
        }

        UnityEngine.Debug.Log("TitleSceneManager: GameManager.instance��������܂����I�V�[���J�ڂ��J�n���܂��B");
        GameManager.instance.LoadSceneWithFade("Demo_tileset"); // "Demo_tileset" �͂��Ȃ��̃��C���Q�[���V�[���̖��O
    }
}
