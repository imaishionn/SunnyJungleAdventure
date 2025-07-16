using UnityEngine;
using UnityEngine.EventSystems; // EventTrigger ���g�p���邽�߂ɕK�v
using UnityEngine.UI; // Button �N���X���g�p���邽�߂ɕK�v

public class ButtonSoundEffect : MonoBehaviour
{
    [SerializeField]
    private AudioClip clickSound; // �N���b�N��
    [SerializeField]
    private AudioClip hoverSound; // �z�o�[�� (�J�[�\����������������I����)

    private AudioSource audioSource;
    private bool isHovered = false; // �z�o�[��Ԃ�ǐՂ���t���O

    void Awake()
    {
        // ����GameObject��AudioSource�R���|�[�l���g���A�^�b�`����Ă��邩�m�F
        // �Ȃ���ΐV�����ǉ�����
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false; // Awake���Ɏ����Đ����Ȃ�
            audioSource.spatialBlend = 0; // 2D�T�E���h�Ƃ��čĐ� (UI�T�E���h�ɓK���Ă���)
        }

        // ����GameObject�A�܂��͂��̎q�I�u�W�F�N�g�Ɋ܂܂��S�Ă�Button�R���|�[�l���g���擾
        // (true ���w�肷�邱�ƂŁA��A�N�e�B�u�Ȏq�I�u�W�F�N�g���̃{�^�����擾�Ώۂɂ���)
        Button[] buttons = GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            // --- 1. �N���b�N���̐ݒ� ---
            // Button��onClick�C�x���g�Ƀ��X�i�[��ǉ�
            // �{�^�����N���b�N���ꂽ�Ƃ���PlayClickSound()���Ă΂��悤�ɂȂ�
            button.onClick.AddListener(() => PlayClickSound());

            // --- 2. �z�o�[�� (PointerEnter, Select) �̐ݒ� ---
            // EventTrigger�R���|�[�l���g���擾�܂��͒ǉ�����
            // EventTrigger�́AButton�R���|�[�l���g�����ł͈����Ȃ��l�X��UI�C�x���g��ߑ����邽�߂Ɏg�p
            EventTrigger trigger = button.gameObject.GetComponent<EventTrigger>();
            if (trigger == null)
            {
                // Debug.LogWarning($"ButtonSoundEffect: Adding EventTrigger to {button.gameObject.name}");
                trigger = button.gameObject.AddComponent<EventTrigger>();
            }

            // a. PointerEnter (�}�E�X�J�[�\�����{�^���ɏ��グ����)
            EventTrigger.Entry entryEnter = new EventTrigger.Entry();
            entryEnter.eventID = EventTriggerType.PointerEnter; // �C�x���g�^�C�v��PointerEnter�ɐݒ�
            entryEnter.callback.AddListener((data) => {
                // �z�o�[���ł͂Ȃ��ꍇ�̂݃z�o�[�����Đ����AisHovered�t���O��true�ɂ���
                // ����ɂ��A�}�E�X���{�^����𓮂���邽�тɉ�����̂�h��
                if (!isHovered)
                {
                    PlayHoverSound();
                    isHovered = true;
                }
            });
            trigger.triggers.Add(entryEnter); // EventTrigger�ɃC�x���g�G���g���[��ǉ�

            // b. PointerExit (�}�E�X�J�[�\�����{�^�����痣�ꂽ��)
            EventTrigger.Entry entryExit = new EventTrigger.Entry();
            entryExit.eventID = EventTriggerType.PointerExit; // �C�x���g�^�C�v��PointerExit�ɐݒ�
            entryExit.callback.AddListener((data) => {
                isHovered = false; // �z�o�[��Ԃ����Z�b�g
            });
            trigger.triggers.Add(entryExit); // EventTrigger�ɃC�x���g�G���g���[��ǉ�

            // c. Select (�L�[�{�[�h��Q�[���p�b�h�Ń{�^�����I�����ꂽ��)
            EventTrigger.Entry entrySelect = new EventTrigger.Entry();
            entrySelect.eventID = EventTriggerType.Select; // �C�x���g�^�C�v��Select�ɐݒ�
            entrySelect.callback.AddListener((data) => {
                // Select�����z�o�[�����Đ�����
                // isHovered�t���O�̃`�F�b�N�͕s�v�A�I�����ꂽ���ɖ炷
                PlayHoverSound();
            });
            trigger.triggers.Add(entrySelect); // EventTrigger�ɃC�x���g�G���g���[��ǉ�

            // d. Deselect (�L�[�{�[�h��Q�[���p�b�h�Ń{�^���̑I�����O�ꂽ��)
            EventTrigger.Entry entryDeselect = new EventTrigger.Entry();
            entryDeselect.eventID = EventTriggerType.Deselect; // �C�x���g�^�C�v��Deselect�ɐݒ�
            entryDeselect.callback.AddListener((data) => {
                // Deselect���͓��ɉ���炷�K�v���Ȃ���΁A�����ł͉������Ȃ�
            });
            trigger.triggers.Add(entryDeselect); // EventTrigger�ɃC�x���g�G���g���[��ǉ�
        }
        Debug.Log("ButtonSoundEffect: �{�^����SE���X�i�[��ݒ肵�܂����B");
    }

    // �N���b�N�����Đ����郁�\�b�h
    private void PlayClickSound()
    {
        // clickSound��audioSource��null�łȂ����Ƃ��m�F���Ă���Đ�
        if (clickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(clickSound); // OneShot�ŏd�˂čĐ��\
            Debug.Log("ButtonSoundEffect: �N���b�N�����Đ����܂����B");
        }
        else if (clickSound == null)
        {
            Debug.LogWarning("ButtonSoundEffect: �N���b�N����AudioClip���ݒ肳��Ă��܂���B");
        }
    }

    // �z�o�[�����Đ����郁�\�b�h
    private void PlayHoverSound()
    {
        // hoverSound��null�łȂ���΍Đ��B
        // �������� 'else if (hoverSound == null)' ��Debug.LogWarning���폜
        if (hoverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hoverSound); // OneShot�ŏd�˂čĐ��\
            Debug.Log("ButtonSoundEffect: �z�o�[�����Đ����܂����B");
        }
        // �z�o�[����AudioClip���ݒ肳��Ă��Ȃ��Ă��A�x���͕\�����Ȃ�
    }
}