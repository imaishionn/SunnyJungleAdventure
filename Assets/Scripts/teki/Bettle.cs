using UnityEngine;
using Debug = UnityEngine.Debug;

public class Bettle : Enemy
{
    [Header("�v���C���[���m����")]
    [SerializeField] float DetectRange = 5f;

    [Header("��s���x")]
    [SerializeField] float FlySpeed = 5f;

    [SerializeField] Transform m_player;

    protected override void Awake()
    {
        base.Awake();

        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
        }

        if (m_rb != null)
        {
            m_rb.gravityScale = 0f; // �d�͖���
            m_rb.drag = 1f; // ��C��R
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate(); // Enemy�N���X��FixedUpdate���Ăяo��

        // �Q�[������~��ԁi�Q�[���I�[�o�[�A�N���A�Ȃǁj�܂��͓G�����S���Ă���ꍇ�͏������~
        if ((GameManager.instance != null &&
             (GameManager.instance.GetCurrentGameState() == GameManager.GameState.enGameState_GameOver || // ���C��: GetState() -> GetCurrentGameState()��
              GameManager.instance.GetCurrentGameState() == GameManager.GameState.enGameState_Clear)) || m_isDead) // ���C��: GetState() -> GetCurrentGameState()��
        {
            if (m_rb != null)
            {
                m_rb.velocity = Vector2.zero;
            }
            if (m_animator != null)
            {
                // �K�v�ł���Β�~�A�j���[�V������ݒ�
                // m_animator.SetBool("fly", false);
            }
            return;
        }

        if (m_player == null || m_rb == null)
        {
            if (m_rb != null) m_rb.velocity = Vector2.zero;
            return;
        }

        float distance = Vector2.Distance(transform.position, m_player.position);

        if (distance < DetectRange)
        {
            FlyToPlayer();
        }
        else
        {
            m_rb.velocity = Vector2.zero; // �͈͊O�ł͒�~
        }
    }

    void FlyToPlayer()
    {
        Vector2 direction = (m_player.position - transform.position).normalized;
        m_rb.velocity = direction * FlySpeed;

        // �v���C���[�̕����ɉ����ăX�v���C�g�𔽓]
        if (direction.x > 0 && transform.localScale.x < 0)
        {
            FlipSprite();
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            FlipSprite();
        }
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}