using UnityEngine;
using Debug = UnityEngine.Debug;

public class Vulture : Enemy // Enemyを継承
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("飛行速度")]
    [SerializeField] float FlySpeed = 5f;

    [SerializeField] Transform m_player;
    private bool m_isFlying = false;

    protected override void Awake()
    {
        base.Awake();

        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
            else
            {
                Debug.LogWarning("Vulture: 'Player'タグを持つGameObjectが見つかりません。", this);
            }
        }

        if (m_rb != null)
        {
            m_rb.gravityScale = 0f;
            m_rb.drag = 1f;
        }
    }

    protected void FixedUpdate()
    {
        if (IsDead) return;

        if (m_player == null || m_rb == null)
        {
            if (m_rb != null) m_rb.velocity = Vector2.zero;
            if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
            {
                m_animator.SetBool("fly", false);
            }
            return;
        }

        float distance = Vector2.Distance(transform.position, m_player.position);

        if (distance < DetectRange)
        {
            FlyToPlayer();

            if (!m_isFlying)
            {
                if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
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
                if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
                {
                    m_animator.SetBool("fly", false);
                }
                m_isFlying = false;
            }

            m_rb.velocity = Vector2.zero;
        }
    }

    void FlyToPlayer()
    {
        Vector2 direction = (m_player.position - transform.position).normalized;
        m_rb.velocity = direction * FlySpeed;

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