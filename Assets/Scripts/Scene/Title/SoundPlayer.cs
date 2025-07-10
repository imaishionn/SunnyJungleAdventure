using UnityEngine;
using Debug = UnityEngine.Debug;                   // Debug �� UnityEngine �����g�p

[RequireComponent(typeof(AudioSource))]
public class SoundPlayer : MonoBehaviour
{
    [Tooltip("��Ȃ瓯�� GameObject �� AudioSource �������擾")]
    public AudioSource audioSource;

    [Tooltip("�X�^�[�g�{�^�����������Ƃ��ɖ���ʉ�")]
    public AudioClip geMusutatoClip;

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        audioSource.playOnAwake = false;
    }

    /// <summary>UI Button �� OnClick ����Ăԃ��\�b�h</summary>
    public void PlayGeMusutato()
    {
        if (geMusutatoClip != null)
            audioSource.PlayOneShot(geMusutatoClip);
        else
            Debug.LogWarning("[SoundPlayer] geMusutatoClip ���ݒ肳��Ă��܂���");
    }
}
