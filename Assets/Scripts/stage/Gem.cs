using System.Collections; // �R���[�`�����g�����߂ɕK�v
using UnityEngine;
// using System.Diagnostics; // ���̍s������ꍇ�͍폜���Ă�������

public class Gem : MonoBehaviour
{
    [Header("�X�R�A�ݒ�")]
    [SerializeField] private int scoreValue = 1; // ���̃W�F�����^����X�R�A (GemManager��AddGem�ɓn���l)

    [Header("���ʉ��ݒ�")]
    [SerializeField] private AudioClip collectSound; // ���W���̌��ʉ�
    [SerializeField, Range(0.01f, 1.0f), Tooltip("�������Ă���Gem��������܂ł̃f�B���C�b��")]
    private float deactivateDelay = 0.2f; // �������Ă���Gem��������܂ł̃f�B���C�b��

    private AudioSource audioSource; // ���ʉ��Đ��p

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // �����Đ����Ȃ�
            audioSource.spatialBlend = 0; // 2D�T�E���h�Ƃ��čĐ��i�C�ӁA�K�v�ɉ����āj
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // GameManager�����݂���΃X�R�A�����Z���A�W�F�����W��ʒm
            if (GameManager.instance != null)
            {
                // ���C���O: GameManager.instance.AddScore(scoreValue);
                // ���C���O: GameManager.instance.CollectGem();
                GameManager.instance.AddGem(scoreValue); // ���C����
            }

            // ���ʉ����Đ�
            if (collectSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(collectSound);
                // Debug.Log($"Gem: '{collectSound.name}' �̉����Đ����܂����BAudioSource�̉���: {audioSource.volume}");

                // �R���C�_�[���ꎞ�I�ɖ��������āA������E����̂�h��
                if (GetComponent<Collider2D>() != null)
                {
                    GetComponent<Collider2D>().enabled = false;
                }

                // ���ʉ��̍Đ����I������܂ő҂��Ă���I�u�W�F�N�g���A�N�e�B�u�ɂ���R���[�`�����J�n
                StartCoroutine(DeactivateAfterSound(deactivateDelay));
            }
            else
            {
                // �����ݒ肳��Ă��Ȃ����AAudioSource���Ȃ��ꍇ�͂����ɔ�A�N�e�B�u��
                if (collectSound == null) UnityEngine.Debug.LogWarning("Gem: collectSound���ݒ肳��Ă��܂���B");
                if (audioSource == null) UnityEngine.Debug.LogWarning("Gem: AudioSource��������܂���B");
                gameObject.SetActive(false); // �W�F�����A�N�e�B�u�ɂ���
            }
        }
    }

    // ���ʉ��̍Đ����I������܂ő҂��Ă���I�u�W�F�N�g���A�N�e�B�u�ɂ���R���[�`��
    private IEnumerator DeactivateAfterSound(float delay)
    {
        yield return new WaitForSeconds(delay); // �w�肳�ꂽ���ԑҋ@
        gameObject.SetActive(false); // �W�F�����A�N�e�B�u�ɂ���
        Destroy(gameObject); // GameObject���̂�j������
    }
}