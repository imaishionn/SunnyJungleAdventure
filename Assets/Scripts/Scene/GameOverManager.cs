using UnityEngine;
using UnityEngine.SceneManagement; // �V�[���Ǘ��̂��߂ɕK�v
using UnityEngine.EventSystems;    // UI�C�x���g�V�X�e���̂��߂ɕK�v
using UnityEngine.UI;              // Button�R���|�[�l���g�𑀍삷�邽�߂ɕK�v
using Debug = UnityEngine.Debug;   // Debug�̞B���ȎQ�Ƃ��������邽��

public class GameOverManager : MonoBehaviour
{
    // �Q�[���p�b�h����ōŏ��ɑI����Ԃɂ�����UI�v�f�i�{�^���Ȃǁj
    [SerializeField] GameObject firstSelected;

    // �V�[���J�ڒ��t���O
    private bool m_isTransitioning = false;

    // ���݃A�N�e�B�u�ȃQ�[���I�[�o�[�{�^���iOn Click�C�x���g�ɕR�t�����Ă���z��j
    private Button m_gameOverButton; // ���ǉ��F�{�^���ւ̎Q��

    // �X�N���v�g���L���ɂȂ����ŏ��̃t���[���ŌĂяo�����
    void Start()
    {
        Debug.Log("GameOverManager: Start���Ăяo����܂����B");

        // firstSelected ���{�^���ł��邱�Ƃ����҂��A�Q�Ƃ��擾����
        if (firstSelected != null)
        {
            m_gameOverButton = firstSelected.GetComponent<Button>();
            if (m_gameOverButton == null)
            {
                Debug.LogWarning("GameOverManager: firstSelected��Button�R���|�[�l���g��������܂���B");
            }

            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(firstSelected);
            Debug.Log($"GameOverManager: UI�̏����I���� '{firstSelected.name}' �ɐݒ肵�܂����B");
        }
        else
        {
            Debug.LogWarning("GameOverManager: firstSelected�����蓖�Ă��Ă��܂���B");
        }
    }

    // ���t���[���Ăяo�����
    void Update()
    {
        // �V�[���J�ڒ��łȂ��ꍇ�̂ݓ��͂��󂯕t����
        if (m_isTransitioning) return;

        // Submit�{�^���iA�{�^��/Enter�L�[/Space�L�[�Ȃǁj�������ꂽ��^�C�g���ɖ߂鏈�������s
        // �����ł́A�{�^�����C���^���N�g�\�ł��邱�Ƃ������ɒǉ�
        if (Input.GetButtonDown("Submit") && m_gameOverButton != null && m_gameOverButton.interactable)
        {
            Debug.Log("GameOverManager: Submit�{�^����������܂����B");
            OnClickReturnTitle();
        }
    }

    // UI�{�^����OnClick�C�x���g��������ڌĂяo����������\�b�h
    public void OnClickReturnTitle()
    {
        // ���ɑJ�ڒ��ł���Ή������Ȃ�
        if (m_isTransitioning)
        {
            Debug.Log("GameOverManager: ���ɃV�[���J�ڒ��̂��߁A�������X�L�b�v���܂��B");
            return;
        }

        m_isTransitioning = true; // �J�ڒ��t���O�𗧂Ă�
        Debug.Log("GameOverManager: �^�C�g���ɖ߂鏈�����J�n���܂��B");

        // �{�^�����L���ȏꍇ�A�����ɃC���^���N�g�s�\�ɂ���
        if (m_gameOverButton != null)
        {
            m_gameOverButton.interactable = false; // ���ǉ��F�{�^����񊈐���
            Debug.Log("GameOverManager: �o�^�{�^����񊈐������܂����B");
        }

        Time.timeScale = 1f; // �O�̂��߁A�Q�[���̎��Ԃ�ʏ�̑��x�ɖ߂��i�|�[�Y�����j

        // GameManager��ʂ��ă^�C�g���V�[���փt�F�[�h�A�E�g���Ȃ���J�ڂ���
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(GameManager.TitleSceneName); // �萔���g�p
        }
        else
        {
            // GameManager���Ȃ��ꍇ�̃t�H�[���o�b�N�Ƃ��āA���ڃV�[�������[�h
            Debug.LogError("GameOverManager: GameManager.instance��������܂���I����TitleScene�����[�h���܂��B");
            SceneManager.LoadScene(GameManager.TitleSceneName); // �萔���g�p
        }
    }
}