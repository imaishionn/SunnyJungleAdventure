using UnityEngine;
using UnityEngine.SceneManagement; // �V�[���Ǘ��̂��߂ɕK�v

public class InGameGameOverController : MonoBehaviour
{
    // �Q�[���I�[�o�[���ɕ\������UI�p�l���iGameObject�j
    [SerializeField] GameObject gameOverUI;

    // �X�N���v�g���L���ɂȂ����ŏ��̃t���[���ŌĂяo�����
    void Start()
    {
        // �Q�[���J�n���ɃQ�[���I�[�o�[UI���\���ɂ���
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(false);
        }
    }

    // �Q�[���I�[�o�[UI��\������������\�b�h
    public void ShowGameOver()
    {
        if (gameOverUI != null)
        {
            gameOverUI.SetActive(true); // �Q�[���I�[�o�[UI��\��
            Time.timeScale = 0f;        // �Q�[���̎��Ԃ��~�i�|�[�Y��Ԃɂ���j
        }
        // else �̌x�����O�͍폜�iInspector�Őݒ�𑣂����߁A�J���I�Ղɂ͕s�v�j
    }

    // ���g���C�{�^���������ꂽ�ۂɌĂяo�����������\�b�h
    public void Retry()
    {
        Time.timeScale = 1f; // �Q�[���̎��Ԃ��ĊJ

        // ���݂̃V�[�����t�F�[�h�A�E�g���Ȃ���ă��[�h����
        // GameManager��ʂ��ăV�[���J�ڂ��Ǘ�����
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(SceneManager.GetActiveScene().name);
        }
        else
        {
            // GameManager���Ȃ��ꍇ�̃t�H�[���o�b�N�i���ڃV�[�����[�h�j
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    // �^�C�g���֖߂�{�^���������ꂽ�ۂɌĂяo�����������\�b�h
    public void GoToTitle()
    {
        Time.timeScale = 1f; // �Q�[���̎��Ԃ��ĊJ

        // �^�C�g���V�[���փt�F�[�h�A�E�g���Ȃ���J�ڂ���
        // GameManager��ʂ��ăV�[���J�ڂ��Ǘ�����
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade("TitleScene"); // �ʏ��"TitleScene"�֖߂�
        }
        else
        {
            // GameManager���Ȃ��ꍇ�̃t�H�[���o�b�N�i���ڃV�[�����[�h�j
            SceneManager.LoadScene("TitleScene"); // ������"TitleScene"����ʓI
        }
    }
}