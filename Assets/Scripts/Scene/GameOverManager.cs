using UnityEngine;
using UnityEngine.SceneManagement; // �V�[���Ǘ��̂��߂ɕK�v
using UnityEngine.EventSystems;    // UI�C�x���g�V�X�e���̂��߂ɕK�v
using Debug = UnityEngine.Debug;   // Debug�̞B���ȎQ�Ƃ��������邽��

public class GameOverManager : MonoBehaviour
{
    // �Q�[���p�b�h����ōŏ��ɑI����Ԃɂ�����UI�v�f�i�{�^���Ȃǁj
    [SerializeField] GameObject firstSelected;

    // �V�[���J�ڒ��t���O
    private bool m_isTransitioning = false;

    // �X�N���v�g���L���ɂȂ����ŏ��̃t���[���ŌĂяo�����
    void Start()
    {
        Debug.Log("GameOverManager: Start���Ăяo����܂����B");
        // �Q�[���p�b�h�ł�UI����̂��߁A�w�肳�ꂽUI�v�f�Ƀt�H�[�J�X��ݒ肷��
        if (firstSelected != null)
        {
            EventSystem.current.SetSelectedGameObject(null); // ��U���݂̑I��������
            EventSystem.current.SetSelectedGameObject(firstSelected); // �w�肳�ꂽ�I�u�W�F�N�g��I����Ԃɂ���
            Debug.Log($"GameOverManager: UI�̏����I���� '{firstSelected.name}' �ɐݒ肵�܂����B");
        }
        else
        {
            Debug.LogWarning("GameOverManager: firstSelected�����蓖�Ă��Ă��܂���B");
        }
        m_isTransitioning = false; // �V�[�����[�h�������ɑJ�ڃt���O�����Z�b�g
    }

    // ���t���[���Ăяo�����
    void Update()
    {
        // �V�[���J�ڒ��łȂ��ꍇ�̂ݓ��͂��󂯕t����
        if (m_isTransitioning) return;

        // Submit�{�^���iA�{�^��/Enter�L�[/Space�L�[�Ȃǁj�������ꂽ��^�C�g���ɖ߂鏈�������s
        if (Input.GetButtonDown("Submit"))
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

        Time.timeScale = 1f; // �O�̂��߁A�Q�[���̎��Ԃ�ʏ�̑��x�ɖ߂��i�|�[�Y�����j

        // GameManager��ʂ��ă^�C�g���V�[���փt�F�[�h�A�E�g���Ȃ���J�ڂ���
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade("TitleScene");
        }
        else
        {
            // GameManager���Ȃ��ꍇ�̃t�H�[���o�b�N�Ƃ��āA���ڃV�[�������[�h
            Debug.LogError("GameOverManager: GameManager.instance��������܂���I����TitleScene�����[�h���܂��B");
            SceneManager.LoadScene("TitleScene");
        }
    }
}
