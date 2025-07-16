using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// �v���C���[��Ǐ]����Q�[���J�����̃X�N���v�g�B
/// X����Y���̒Ǐ]�͈͂�Inspector����ݒ�ł���悤�ɂ��܂��B
/// </summary>
public class GameCamera : MonoBehaviour
{
    GameObject m_player; // �Ǐ]����v���C���[�I�u�W�F�N�g

    [SerializeField]
    [Tooltip("�J�����̃I�t�Z�b�g�i�v���C���[����̑��Έʒu�j�BZ�͒ʏ핉�̒l�ŃJ�����̉��s����ݒ肵�܂��B")]
    Vector3 CameraAddPos = new Vector3(0f, 0f, -10f); // �v���C���[����̃J�����̑��Έʒu

    [Header("�J�����Ǐ]�͈͂̐���")]
    [SerializeField]
    [Tooltip("�J������X���W�̒Ǐ]�𐧌����邩�ǂ���")]
    bool UseClampX = false; // X���W�̐������g�p���邩
    [SerializeField]
    [Tooltip("�J������Y���W�̒Ǐ]�𐧌����邩�ǂ���")]
    bool UseClampY = false; // Y���W�̐������g�p���邩

    [SerializeField]
    [Tooltip("�J�������ړ��ł���ő�ʒu (X, Y)")]
    Vector2 CameraMaxPos = Vector2.zero; // �J�����̍ő�ʒu (X, Y)
    [SerializeField]
    [Tooltip("�J�������ړ��ł���ŏ��ʒu (X, Y)")]
    Vector2 CameraMinPos = Vector2.zero; // �J�����̍ŏ��ʒu (X, Y)

    // ���ȑO�� StopFollowY �� m_isCameraStopped �͍폜���܂����B
    // ������ɂ��A�J���������S�ɒ�~����̂ł͂Ȃ��A�w��͈͓��ŒǏ]����悤�ɂȂ�܂��B

    // �V���O���g���C���X�^���X (PlayerMove�Ȃǂ���ȒP�ɎQ�Ƃł���悤�ɂ��邽��)
    public static GameCamera Instance { get; private set; }

    void Awake()
    {
        // �V���O���g���C���X�^���X�̐ݒ�
        if (Instance == null)
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject); // �J��������ɑ��݂���K�v���Ȃ���Εs�v�B�V�[�����Ƃɔz�u����ꍇ�̓R�����g�A�E�g�̂܂܁B
        }
        else
        {
            Destroy(gameObject); // ���ɃC���X�^���X�����݂���ꍇ�́A�V��������j��
        }
    }

    void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        // ClearScene�ł̓v���C���[�ǔ����Ȃ�
        if (currentSceneName != "ClearScene")
        {
            m_player = GameObject.FindGameObjectWithTag("Player"); // "Player"�^�O�̃I�u�W�F�N�g������

            if (m_player != null)
            {
                // �Q�[���J�n���ɃJ�������v���C���[�̏����ʒu�ɍ��킹��
                CameraUpdate();
            }
            else
            {
                UnityEngine.Debug.LogError("Player�^�O�̃Q�[���I�u�W�F�N�g��������܂���I�J�����Ǐ]���ł��܂���B");
            }
        }
    }

    void LateUpdate()
    {
        // LateUpdate�ŃJ�������X�V���邱�ƂŁA�v���C���[�̈ړ���ɃJ�������Ǐ]���A���炩�ȓ����ɂȂ�܂��B
        if (m_player == null) return;

        // �J�����̒Ǐ]���W�b�N�����s
        CameraUpdate();
    }

    /// <summary>
    /// �J�����̈ʒu���X�V���A�ݒ肳�ꂽ�͈͓��ŃN�����v���܂��B
    /// </summary>
    void CameraUpdate()
    {
        if (m_player == null) return;

        // �v���C���[�̈ʒu�ɃI�t�Z�b�g���������ڕW�ʒu���v�Z
        Vector3 targetPos = m_player.transform.position + CameraAddPos;

        // X���W�̐���
        if (UseClampX)
        {
            targetPos.x = Mathf.Clamp(targetPos.x, CameraMinPos.x, CameraMaxPos.x);
        }

        // Y���W�̐���
        if (UseClampY)
        {
            targetPos.y = Mathf.Clamp(targetPos.y, CameraMinPos.y, CameraMaxPos.y);
        }

        // �J�����̈ʒu��ڕW�ʒu�ɐݒ�
        transform.position = targetPos;
    }

    // ���ȑO�� StopCameraFollow() �� StartCameraFollow() �́A���̃N�����v�����ł͕s�v�Ȃ��ߍ폜���܂����B
}
