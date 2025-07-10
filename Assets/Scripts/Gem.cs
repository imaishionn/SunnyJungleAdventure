using UnityEngine;
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

/// <summary>
/// ��΁iGem�j�̓���𐧌䂷��X�N���v�g�B
/// �v���C���[�ɐG���ƃX�R�A�����Z���A���g��j�����܂��B
/// </summary>
public class Gem : MonoBehaviour
{
    [SerializeField, Tooltip("�l���ł���|�C���g")]
    int Point = 10; // ���f�t�H���g�l��0�ȊO�ɐݒ� (��: 10) - Inspector�ŕύX�\��

    private ItemSoundPlayer soundPlayer;

    void Awake()
    {
        // �V�[�����ɂ��� ItemSoundPlayer �������ŒT���Ď擾
        // FindObjectOfType��Awake��Start�ň�x�����Ăяo���̂������I
        soundPlayer = FindObjectOfType<ItemSoundPlayer>();
        if (soundPlayer == null)
        {
            Debug.LogWarning("Gem: ItemSoundPlayer ���V�[���Ɍ�����܂���B");
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log($"Gem: �v���C���[ ({collision.gameObject.name}) ����� ({gameObject.name}) ���l�����܂����I");

            // �X�R�A���Z
            // GameManager���V���O���g���C���X�^���X�Ƃ��Đݒ肳��Ă��邱�Ƃ�O��Ƃ��܂��B
            if (GameManager.instance != null)
            {
                GameManager.instance.AddScore(Point);
                Debug.Log($"Gem: �X�R�A {Point} �����Z���܂����B���݂̍��v�X�R�A: {GameManager.instance.GetCurrentPlayScore()}");
            }
            else
            {
                Debug.LogError("Gem: GameManager.instance ��������܂���I�X�R�A�����Z�ł��܂���ł����B");
            }

            // ����炷
            if (soundPlayer != null)
            {
                soundPlayer.PlayGemSound();
                Debug.Log("Gem: �l�������Đ����܂����B");
            }
            else
            {
                Debug.LogWarning("Gem: ItemSoundPlayer ��������Ȃ����߁A�l�������Đ��ł��܂���B");
            }

            // �A�C�e���폜
            Destroy(gameObject);
            Debug.Log($"Gem: ��΃I�u�W�F�N�g ({gameObject.name}) ��j�����܂����B");
        }
    }
}
