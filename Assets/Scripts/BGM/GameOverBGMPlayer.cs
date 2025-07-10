using UnityEngine;
using UnityEngine.UI;

public class GameOverBGMPlayer : MonoBehaviour
{
    [SerializeField] private AudioClip gameOverBGM;
    [SerializeField, Range(0f, 1f)] private float defaultVolume = 0.5f;
    [SerializeField] private Slider volumeSlider; // UI�̃X���C�_�[�i�C�Ӂj

    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (gameOverBGM != null)
        {
            audioSource.clip = gameOverBGM;
            audioSource.loop = true;
            audioSource.volume = defaultVolume;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("�Q�[���I�[�o�[BGM���ݒ肳��Ă��܂���I");
        }

        // �X���C�_�[���ݒ肳��Ă���Ώ�����
        if (volumeSlider != null)
        {
            volumeSlider.value = defaultVolume;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }
}
