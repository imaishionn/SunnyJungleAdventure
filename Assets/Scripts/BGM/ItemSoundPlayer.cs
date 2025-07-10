using UnityEngine;
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

/// <summary>
/// �A�C�e���l������W�����v���Ȃǂ̌��ʉ����Đ����邽�߂̃X�N���v�g�B
/// �V�[���Ɉ�������݂��A�e�I�u�W�F�N�g����Ăяo����܂��B
/// </summary>
public class ItemSoundPlayer : MonoBehaviour
{
    // AudioSource�R���|�[�l���g��Inspector�Ŋ��蓖�Ă�K�v������܂�
    public AudioSource audioSource;

    [Header("��Ίl����")]
    public AudioClip gemClip;

    [Header("�W�����v��")]
    public AudioClip jumpClip;  // �W�����v���p�N���b�v

    /// <summary>
    /// ��Ίl�������Đ����܂��B
    /// �����N���b�v��2��Đ����Ă��܂����A�Ӑ}�I�łȂ����1��Ɍ��炷���Ƃ��������Ă��������B
    /// </summary>
    public void PlayGemSound()
    {
        if (audioSource != null && gemClip != null)
        {
            audioSource.PlayOneShot(gemClip);
            audioSource.PlayOneShot(gemClip, 0.4f); // 2��ڂ̍Đ� (����0.4f)
            Debug.Log("ItemSoundPlayer: ��Ίl�������Đ����܂����B");
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("ItemSoundPlayer: AudioSource�����蓖�Ă��Ă��܂���B");
            if (gemClip == null) Debug.LogWarning("ItemSoundPlayer: ��Ίl�����N���b�v�����蓖�Ă��Ă��܂���B");
        }
    }

    /// <summary>
    /// �W�����v�����Đ����܂��B
    /// </summary>
    public void PlayJumpSound()
    {
        if (audioSource != null && jumpClip != null)
        {
            audioSource.PlayOneShot(jumpClip);
            Debug.Log("ItemSoundPlayer: �W�����v�����Đ����܂����B");
        }
        else
        {
            if (audioSource == null) Debug.LogWarning("ItemSoundPlayer: AudioSource�����蓖�Ă��Ă��܂���B");
            if (jumpClip == null) Debug.LogWarning("ItemSoundPlayer: �W�����v���N���b�v�����蓖�Ă��Ă��܂���B");
        }
    }
}
