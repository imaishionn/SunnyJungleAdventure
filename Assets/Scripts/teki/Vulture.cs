using UnityEngine;
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

/// <summary>
/// Vulture�i�n�Q�^�J�j�̓G�̓���𐧌䂵�܂��B
/// �v���C���[�����m����ƒǔ����A���S���̓x�[�X��Enemy�N���X��Die���\�b�h���Ăяo���܂��B
/// </summary>
public class Vulture : Enemy // Enemy�N���X���p��
{
    [Header("�v���C���[���m����")]
    [SerializeField] float DetectRange = 5f;

    [Header("��s���x")]
    [SerializeField] float FlySpeed = 5f;

    [SerializeField] Transform m_player;

    private bool m_isFlying = false;

    protected override void Awake() // Start����Awake�ɕύX (�x�[�X�N���X��Awake���Ăяo��)
    {
        base.Awake(); // �e�N���X(Enemy)��Awake���Ăяo��

        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
            else
                Debug.LogWarning("Vulture: �v���C���[�I�u�W�F�N�g��������܂���B�^�O 'Player' ���m�F���Ă��������B");
        }

        if (m_rb != null)
        {
            m_rb.gravityScale = 0f; // �d�͂𖳌������Ĕ�s
        }
    }

    void Update()
    {
        if (m_isDead || m_player == null) return; // ���S���܂��̓v���C���[��������Ȃ��ꍇ�͏������I��

        float distance = Vector2.Distance(transform.position, m_player.position);

        if (distance < DetectRange)
        {
            FlyToPlayer();

            if (!m_isFlying)
            {
                if (m_animator != null)
                {
                    m_animator.SetBool("fly", true);
                }
                m_isFlying = true;
            }
        }
        else
        {
            if (m_isFlying)
            {
                if (m_animator != null)
                {
                    m_animator.SetBool("fly", false);
                }
                m_isFlying = false;
            }

            if (m_rb != null)
            {
                m_rb.velocity = Vector2.zero; // �v���C���[���͈͊O�ɏo�����~
            }
        }
    }

    void FlyToPlayer()
    {
        Vector2 direction = (m_player.position - transform.position).normalized;
        if (m_rb != null)
        {
            m_rb.velocity = direction * FlySpeed;
        }

        if (direction.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(direction.x);
            transform.localScale = scale;
        }
    }

    // Vulture�ŗL�̎��S�������K�v�ȏꍇ�́A������ override public void Die() �������ł��܂�
    // ����̓x�[�X��Enemy.Die()���g�p���܂�
}
