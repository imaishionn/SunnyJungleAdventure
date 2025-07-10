using TMPro; // TextMeshPro���g�p���邽�߂ɂ��̍s���K�v�ł�
using UnityEngine;
using UnityEngine.UI; // ���ꂪ UnityEngine.UI.Image ��񋟂��܂�

// ������ 'using System.Net.Mime;' ������΍폜���Ă��������B

// ���̃X�N���v�g���A�^�b�`�����GameObject�ɂ́A�K��TMP_Text�R���|�[�l���g���K�v�ł��邱�Ƃ�Unity�ɓ`���܂��B
[RequireComponent(typeof(TMP_Text))]
public class ScoreDisplay : MonoBehaviour
{
    // �X�R�A�\���p��Text�R���|�[�l���g (TextMeshProUGUI���g�p)
    // Inspector�Ŋ��蓖�Ă��Ă��Ȃ��Ă��AAwake�Ŏ����I�Ɏ擾�����݂܂��B
    [SerializeField] private TMP_Text scoreText;

    // �X�R�A�̔w�iImage�ւ̎Q�Ƃ�UnityEngine.UI.Image�Ɩ����I�Ɏw��
    // Inspector�Ŋ��蓖�Ă邩�AAwake��Find("Image")�ȂǂŎ擾���邱�Ƃ�z��
    [SerializeField] private UnityEngine.UI.Image scoreBackgroundImage;

    void Awake()
    {
        // Inspector��scoreText�����蓖�Ă��Ă��Ȃ��ꍇ�A
        // �܂��͉��炩�̗��R�ŎQ�Ƃ�����ꂽ�ꍇ�ɁAGetComponent�Ŏ擾�����݂܂��B
        if (scoreText == null)
        {
            scoreText = GetComponent<TMP_Text>();
        }

        // ����ł�scoreText��null�̏ꍇ�i��FTMP_Text�R���|�[�l���g���{���ɂȂ��ꍇ�j�A�G���[�����O�ɏo���܂��B
        if (scoreText == null)
        {
            UnityEngine.Debug.LogError("ScoreDisplay: TextMeshProUGUI �R���|�[�l���g��������܂���BScoreText�I�u�W�F�N�g�ɃA�^�b�`����Ă��邩�m�F���Ă��������B", this);
        }

        // scoreBackgroundImage�����蓖�Ă��Ă��Ȃ��ꍇ�A�e��Image�R���|�[�l���g��T��
        // ScoreText�̐e��Image�ł���Ƃ����\����O��Ƃ��Ă��܂��B
        if (scoreBackgroundImage == null && transform.parent != null)
        {
            scoreBackgroundImage = transform.parent.GetComponent<UnityEngine.UI.Image>(); // �����I�Ɏw��
            if (scoreBackgroundImage == null)
            {
                // �e��Image���Ȃ��ꍇ�A����Canvas����"Image"�Ƃ������O�̃I�u�W�F�N�g��T��
                GameObject imgGO = GameObject.Find("Image");
                if (imgGO != null)
                {
                    scoreBackgroundImage = imgGO.GetComponent<UnityEngine.UI.Image>(); // �����I�Ɏw��
                }
            }
        }
        if (scoreBackgroundImage == null)
        {
            UnityEngine.Debug.LogWarning("ScoreDisplay: �X�R�A�̔w�iImage�R���|�[�l���g��������܂���B");
        }
    }

    // ���݂̃X�R�A���X�V����Public���\�b�h�BGameManager����Ă΂��
    public void ScoreUpdate(int score)
    {
        if (scoreText != null)
        {
            scoreText.text = score.ToString(); // �X�R�A�𕶎���ɕϊ����ĕ\��
            UnityEngine.Debug.Log($"ScoreDisplay ScoreUpdate: UI�� {score} �ɍX�V�B");
        }
        else
        {
            UnityEngine.Debug.LogWarning("Score Text UI is not assigned or found on ScoreDisplay!");
        }
    }

    // �X�R�A�\���i�e�L�X�g�Ɣw�i�j���\���ɂ���
    public void HideScore()
    {
        if (scoreText != null && scoreText.gameObject.activeSelf)
        {
            scoreText.gameObject.SetActive(false);
            UnityEngine.Debug.Log("ScoreDisplay: �X�R�A�e�L�X�g���\���ɂ��܂����B");
        }
        // �w�i�摜����\���ɂ���
        if (scoreBackgroundImage != null && scoreBackgroundImage.gameObject.activeSelf)
        {
            scoreBackgroundImage.gameObject.SetActive(false);
            UnityEngine.Debug.Log("ScoreDisplay: �X�R�A�w�i�摜���\���ɂ��܂����B");
        }
    }

    // �X�R�A�\���i�e�L�X�g�Ɣw�i�j��\������
    public void ShowScore()
    {
        if (scoreText != null && !scoreText.gameObject.activeSelf)
        {
            scoreText.gameObject.SetActive(true);
            UnityEngine.Debug.Log("ScoreDisplay: �X�R�A�e�L�X�g��\�����܂����B");
        }
        // �w�i�摜���\������
        if (scoreBackgroundImage != null && !scoreBackgroundImage.gameObject.activeSelf)
        {
            scoreBackgroundImage.gameObject.SetActive(true);
            UnityEngine.Debug.Log("ScoreDisplay: �X�R�A�w�i�摜��\�����܂����B");
        }
    }
}
