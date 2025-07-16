using UnityEngine;
using UnityEngine.SceneManagement; // SceneManager���g�p���邽��
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

// using System.Diagnostics; // ���̍s�͍폜���܂��B

public class Goal : MonoBehaviour
{
    [SerializeField, Header("���̃V�[����")]
    private string nextSceneName = "GameClear"; // �f�t�H���g��GameClear

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"Goal OnTriggerEnter2D: {collision.gameObject.name} �ƏՓ˂��܂����B");

        // �v���C���[�Ƃ̏Փ˂��m�F�i�v���C���[��"Player"�^�O���t���Ă��邱�Ƃ�O��Ƃ��܂��j
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Goal: �v���C���[���S�[���ɓ��B���܂����I");

            // �v���C���[�̓������~�߂�i��FPlayerMove�X�N���v�g�𖳌����j
            var playerMove = collision.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                playerMove.enabled = false;
                Debug.Log("Goal: �v���C���[�̓������~���܂����B");
            }
            else
            {
                Debug.LogWarning("Goal: �v���C���[��PlayerMove�X�N���v�g��������܂���ł����B");
            }

            // GameManager�̃C���X�^���X�����݂��邩�m�F���A�V�[���J�ڂ�v��
            if (GameManager.instance != null)
            {
                Debug.Log($"Goal: {nextSceneName} �V�[���֑J�ڂ��܂��B");
                GameManager.instance.LoadSceneWithFade(nextSceneName);
            }
            else
            {
                Debug.LogError("Goal: GameManager.instance��������܂���I�V�[���J�ڂł��܂���B���ڃV�[�������[�h���܂��B");
                // GameManager��������Ȃ��ꍇ�̃t�H�[���o�b�N�i�f�o�b�O�p�j
                SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
            }
        }
    }
}
