using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement; // �V�[���Ǘ��̂��߂ɕK�v
using Debug = UnityEngine.Debug;   // Debug�̞B���ȎQ�Ƃ��������邽��

public class BGMManager : MonoBehaviour
{
    // �V���O���g���C���X�^���X
    public static BGMManager instance;

    // �e�V�[����BGM�N���b�v��Inspector���犄�蓖�Ă邽�߂̃t�B�[���h
    [SerializeField, Header("�^�C�g��BGM")]
    private AudioClip titleBGM;
    [SerializeField, Header("�Q�[���v���CBGM")]
    private AudioClip gameBGM;
    [SerializeField, Header("�Q�[���I�[�o�[BGM")]
    private AudioClip gameOverBGM;
    [SerializeField, Header("�Q�[���N���ABGM")]
    private AudioClip clearBGM;

    // BGM�̃}�X�^�[���� (0����1�͈̔͂�Inspector���璲���\)
    [SerializeField, Range(0f, 1f), Header("BGM�}�X�^�[����")]
    private float bgmMasterVolume = 0.7f; // �f�t�H���g�l��0.7�ɐݒ�

    // BGM���Đ����邽�߂�AudioSource�R���|�[�l���g
    private AudioSource audioSource;
    // ���ݎ��s���̃t�F�[�h�R���[�`���ւ̎Q��
    private Coroutine fadeCoroutine;

    // �X�N���v�g�����[�h���ꂽ�ۂɈ�x�����Ăяo�����
    void Awake()
    {
        // �V���O���g���p�^�[���̎���
        if (instance == null)
        {
            instance = this;
            // ����GameObject���V�[���J�ڂ��Ă��j�����Ȃ��悤�ɐݒ�
            DontDestroyOnLoad(gameObject);

            // AudioSource�R���|�[�l���g���擾�܂��͒ǉ�
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false; // �����Đ����Ȃ�
                audioSource.loop = true; // BGM�̓f�t�H���g�Ń��[�v����悤�ɐݒ�
            }
            Debug.Log("BGMManager: �C���X�^���X���ݒ肳��ADontDestroyOnLoad�ɐݒ肳��܂����B");

            // �V�[�������[�h���ꂽ�Ƃ��ɌĂяo�����C�x���g��o�^
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // ���ɃC���X�^���X������ꍇ�́A���̏d�������I�u�W�F�N�g��j��
            Destroy(gameObject);
            Debug.Log("BGMManager: �d�������C���X�^���X��j�����܂����B");
        }
    }

    // GameObject���j�������Ƃ��ɌĂяo�����
    void OnDestroy()
    {
        // �C�x���g�̓o�^���� (�d�����s��h������)
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // �V�[�������[�h���ꂽ�Ƃ��ɌĂяo�����R�[���o�b�N���\�b�h
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"BGMManager: �V�[�� '{scene.name}' �����[�h����܂����B�K�؂�BGM���Đ����܂��B");
        // ���[�h���ꂽ�V�[���̖��O�ɂ���čĐ�����BGM��؂�ւ���
        switch (scene.name)
        {
            case "TitleScene":
                PlayBGM(titleBGM);
                break;
            case "Demo_tileset": // �Q�[���v���C�V�[��
                PlayBGM(gameBGM);
                break;
            case "GameOver": // �Q�[���I�[�o�[�V�[��
            case "GameOverScene": // �O�̂���GameOverScene���Ή�
                PlayBGM(gameOverBGM);
                break;
            case "ClearScene": // �Q�[���N���A�V�[��
                PlayBGM(clearBGM);
                break;
            default:
                StopBGM(); // ���̑��̃V�[���ł�BGM���~����
                break;
        }
    }

    // �w�肳�ꂽBGM�N���b�v���Đ�����������\�b�h
    public void PlayBGM(AudioClip clip, bool loop = true, float fadeInDuration = 0.5f)
    {
        if (audioSource == null)
        {
            Debug.LogError("BGMManager: AudioSource��������܂���BBGM���Đ��ł��܂���B");
            return;
        }

        // ���ݍĐ�����BGM�������ł���΁A�������Ȃ�
        if (audioSource.clip == clip && audioSource.isPlaying)
        {
            Debug.Log($"BGMManager: BGM '{clip?.name}' �͊��ɍĐ����ł��B");
            return;
        }

        // �����̃t�F�[�h�R���[�`��������Β�~����
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        audioSource.Stop(); // ���݂�BGM���~
        audioSource.clip = clip; // �V����BGM�N���b�v��ݒ�
        audioSource.loop = loop; // ���[�v�ݒ�
        audioSource.volume = 0f; // �t�F�[�h�C���̂��߂ɏ����{�����[����0�ɐݒ�
        audioSource.Play(); // �Đ��J�n
        Debug.Log($"BGMManager: BGM '{clip?.name}' �̍Đ����J�n���܂����B");

        // �t�F�[�h�C������
        if (fadeInDuration > 0)
        {
            // �t�F�[�h�C���̖ڕW�{�����[����bgmMasterVolume�ɐݒ�
            fadeCoroutine = StartCoroutine(FadeAudio(audioSource, 0f, bgmMasterVolume, fadeInDuration));
        }
        else
        {
            audioSource.volume = bgmMasterVolume; // �t�F�[�h�C�����Ȃ��ꍇ�͑����Ƀ}�X�^�[�{�����[��
        }
    }

    // ���݂�BGM���~����������\�b�h
    public void StopBGM(float fadeOutDuration = 0.5f)
    {
        if (audioSource == null || !audioSource.isPlaying)
        {
            return; // AudioSource���Ȃ����A�Đ����łȂ���Ή������Ȃ�
        }

        // �����̃t�F�[�h�R���[�`��������Β�~����
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        // �t�F�[�h�A�E�g����
        if (fadeOutDuration > 0)
        {
            // �t�F�[�h�A�E�g�̊J�n�{�����[�������݂̃{�����[���ɐݒ�
            fadeCoroutine = StartCoroutine(FadeAudio(audioSource, audioSource.volume, 0f, fadeOutDuration, true));
        }
        else
        {
            audioSource.Stop(); // �t�F�[�h�A�E�g���Ȃ��ꍇ�͑����ɒ�~
            audioSource.volume = bgmMasterVolume; // ����Đ��̂��߂Ƀ{�����[�������Z�b�g
        }
        Debug.Log("BGMManager: BGM���~���܂����B");
    }

    // �I�[�f�B�I�̃t�F�[�h�C��/�A�E�g���s���R���[�`��
    private IEnumerator FadeAudio(AudioSource source, float startVolume, float endVolume, float duration, bool stopOnEnd = false)
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, endVolume, timer / duration);
            yield return null;
        }
        source.volume = endVolume; // �ŏI�{�����[����ݒ�
        if (stopOnEnd && endVolume == 0)
        {
            source.Stop(); // �t�F�[�h�A�E�g������ɒ�~
        }
        fadeCoroutine = null; // �R���[�`�����I�������̂ŎQ�Ƃ��N���A
    }
}
