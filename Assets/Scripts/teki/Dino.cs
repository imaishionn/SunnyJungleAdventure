using UnityEngine;
using Debug = UnityEngine.Debug;

public class Dino : Enemy
{
    [Header("�v���C���[���m����")]
    [SerializeField] float DetectRange = 5f;

    [Header("���鑬�x")]
    [SerializeField] float RunSpeed = 3f;

    [Header("�p�g���[���ړ��͈́i���E�j")]
    [SerializeField] float PatrolDistance = 3f;

    [SerializeField] Transform m_player;

    [Header("�ǌ��m�ݒ�")]
    [SerializeField] LayerMask wallLayer;
    [SerializeField] float wallCheckDistance = 0.3f;
    [SerializeField] Vector2 wallCheckOffset = new Vector2(0, 0.1f);

    private Vector2 m_startPos;
    private int m_patrolDirection = 1;

    private float m_targetVelocityX = 0f;

    private bool IsFacingRight => transform.localScale.x > 0;

    protected override void Awake()
    {
        base.Awake();

        if (m_rb != null)
        {
            m_rb.freezeRotation = true;
        }

        m_startPos = transform.position;

        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
        }

        if (wallLayer.value == 0)
        {
            Debug.LogWarning("Dino: Wall Layer���ݒ肳��Ă��܂���I�Փˌ��m���������s���Ȃ��\��������܂��B");
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
            if (m_animator != null)
            {
                m_animator.SetBool("run", false);
            }
            if (m_rb != null)
            {
                m_rb.velocity = Vector2.zero;
            }
            return;
        }

        if (m_rb == null || !m_rb.simulated)
        {
            if (m_rb != null) m_rb.velocity = Vector2.zero;
            if (m_animator != null) m_animator.SetBool("run", false);
            return;
        }

        float desiredMoveDirectionX = 0f;
        float currentFacingDirectionX = IsFacingRight ? 1f : -1f;

        // �ǌ��m
        Vector2 checkOrigin = (Vector2)m_collider.bounds.center + new Vector2(m_collider.bounds.extents.x * currentFacingDirectionX, wallCheckOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.right * currentFacingDirectionX, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            m_patrolDirection *= -1; // �����]��
            desiredMoveDirectionX = m_patrolDirection;
        }
        else
        {
            float distanceToPlayer = m_player != null ? Vector2.Distance(transform.position, m_player.position) : Mathf.Infinity;

            if (distanceToPlayer < DetectRange)
            {
                // �v���C���[�����m������v���C���[�Ɍ������Ĉړ�
                desiredMoveDirectionX = Mathf.Sign(m_player.position.x - transform.position.x);
            }
            else
            {
                // �p�g���[���͈͂̒[�ɒB����������]��
                float distanceFromStart = transform.position.x - m_startPos.x;

                if (Mathf.Abs(distanceFromStart) >= PatrolDistance)
                {
                    m_patrolDirection *= -1;
                    m_startPos = transform.position; // �V�����p�g���[���J�n�ʒu�����݂̈ʒu�ɂ���i�K�v�ł���΁j
                }
                desiredMoveDirectionX = m_patrolDirection;
            }
        }

        m_targetVelocityX = desiredMoveDirectionX * RunSpeed;

        // �ړ������ɉ����ăX�v���C�g�𔽓]
        if ((m_targetVelocityX > 0 && !IsFacingRight) || (m_targetVelocityX < 0 && IsFacingRight))
        {
            FlipSprite();
        }

        m_rb.velocity = new Vector2(m_targetVelocityX, m_rb.velocity.y);

        if (m_animator != null)
        {
            m_animator.SetBool("run", Mathf.Abs(m_rb.velocity.x) > 0.05f); // ���x�����ȏ゠��Α���A�j���[�V����
        }
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void OnDrawGizmos()
    {
        if (m_collider == null) return;

        // �ǌ��m�p��Gizmo
        Gizmos.color = Color.red;
        float currentFacingDirectionX = IsFacingRight ? 1f : -1f;
        Vector2 checkOrigin = (Vector2)m_collider.bounds.center + new Vector2(m_collider.bounds.extents.x * currentFacingDirectionX, wallCheckOffset.y);
        Gizmos.DrawLine(checkOrigin, checkOrigin + Vector2.right * currentFacingDirectionX * wallCheckDistance);

        // �R���C�_�[�̃o�E���f�B���O�{�b�N�X
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(m_collider.bounds.center, m_collider.bounds.size);

        // �p�g���[���͈͂�Gizmo
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_startPos + Vector2.left * PatrolDistance, m_startPos + Vector2.left * PatrolDistance + Vector2.up * 1f);
        Gizmos.DrawLine(m_startPos + Vector2.right * PatrolDistance, m_startPos + Vector2.right * PatrolDistance + Vector2.up * 1f);
    }
}