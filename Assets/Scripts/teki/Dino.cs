using UnityEngine;
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

/// <summary>
/// Dino�i�����j�̓G�̓���𐧌䂵�܂��B
/// �v���C���[�����m����ƒǔ����A����ȊO�̓p�g���[�����܂��B
/// ���S���̓x�[�X��Enemy�N���X��Die���\�b�h���Ăяo���܂��B
/// </summary>
public class Dino : Enemy // Enemy�N���X���p��
{
    [Header("�v���C���[���m����")]
    [SerializeField] float DetectRange = 5f;

    [Header("���鑬�x")]
    [SerializeField] float RunSpeed = 3f;

    [Header("�p�g���[���ړ��͈́i���E�j")]
    [SerializeField] float PatrolDistance = 3f;

    [SerializeField] Transform m_player; // �����̃t�B�[���h�Ƀv���C���[��Transform�����蓖�Ă܂���

    private bool m_isRunning = false;
    private Vector2 m_startPos;
    private int m_patrolDirection = 1; // 1:�E, -1:��

    protected override void Awake() // Start����Awake�ɕύX (�x�[�X�N���X��Awake���Ăяo��)
    {
        base.Awake(); // �e�N���X(Enemy)��Awake���Ăяo��

        if (m_rb != null)
        {
            m_rb.freezeRotation = true; // ��]���Œ�
        }

        m_startPos = transform.position; // �����ʒu��ۑ�

        // m_player��Inspector�Ŋ��蓖�Ă��Ă��Ȃ��ꍇ�̂݁A�^�O�Ō��������݂�
        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
            else
                Debug.LogWarning("Dino: �v���C���[�I�u�W�F�N�g��������܂���B�^�O 'Player' ���m�F���Ă��������B");
        }
    }

    void Update()
    {
        if (m_isDead || m_rb == null || !m_rb.simulated) return; // ���S���ARigidbody���Ȃ��A�܂��͕������Z�������ȏꍇ�͏������I��

        float distance = m_player != null ? Vector2.Distance(transform.position, m_player.position) : Mathf.Infinity;

        if (distance < DetectRange)
        {
            RunToPlayer();
        }
        else
        {
            Patrol();
        }
    }

    void RunToPlayer()
    {
        Vector2 direction = (m_player.position - transform.position).normalized;
        if (m_rb != null)
        {
            Vector2 velocity = new Vector2(direction.x * RunSpeed, m_rb.velocity.y);
            m_rb.velocity = velocity;
        }

        Flip(direction.x);
        SetRunningAnimation(true);
    }

    void Patrol()
    {
        float distanceFromStart = transform.position.x - m_startPos.x;

        if (Mathf.Abs(distanceFromStart) >= PatrolDistance)
        {
            m_patrolDirection *= -1; // �����]��
        }

        if (m_rb != null)
        {
            Vector2 velocity = new Vector2(m_patrolDirection * RunSpeed, m_rb.velocity.y);
            m_rb.velocity = velocity;
        }

        Flip(m_patrolDirection);
        SetRunningAnimation(true);
    }

    void Flip(float dirX)
    {
        if (dirX != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(dirX) * -1f;
            transform.localScale = scale;
        }
    }

    void SetRunningAnimation(bool isRunning)
    {
        if (m_isRunning != isRunning)
        {
            m_isRunning = isRunning;
            if (m_animator != null)
            {
                m_animator.SetBool("run", isRunning);
            }
        }
    }
}
