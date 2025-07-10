using UnityEngine;
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

[RequireComponent(typeof(AudioSource))] // AudioSource�R���|�[�l���g���K�{�ł��邱�Ƃ�����
public class ButtonSoundEffect : MonoBehaviour
{
    [Tooltip("�{�^���N���b�N���ɍĐ�������ʉ�")]
    public AudioClip clickSound;

    // ���ʉ��̉��� (0����1�͈̔͂�Inspector���璲���\)
    [SerializeField, Range(0f, 1f), Header("���ʉ�����")]
    private float sfxVolume = 0.7f; // �f�t�H���g�l��0.7�ɐݒ�

    private AudioSource audioSource;

    void Awake()
    {
        // ����GameObject�ɃA�^�b�`����Ă���AudioSource���擾
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false; // �����Đ����Ȃ�
        audioSource.loop = false; // ���[�v���Ȃ��i���ʉ��̂��߁j
    }

    /// <summary>
    /// UI Button �� OnClick �C�x���g����Ăяo�����\�b�h�B
    /// �ݒ肳�ꂽ�N���b�N������x�����Đ����܂��B
    /// </summary>
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            // PlayOneShot�̑�2�����ŉ��ʂ��w��
            audioSource.PlayOneShot(clickSound, sfxVolume);
            Debug.Log($"ButtonSoundEffect: '{clickSound.name}' ������ {sfxVolume} �ōĐ����܂����B");
        }
        else
        {
            Debug.LogWarning($"ButtonSoundEffect: '{gameObject.name}' �� clickSound ���ݒ肳��Ă��܂���B");
        }
    }
}
