using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class bat : Enemy
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("飛行速度")]
    [SerializeField] float FlySpeed = 5f;

    [SerializeField] Transform m_player;
    private bool m_isFlying = false;

    // 死亡アニメーション再生時間
    [SerializeField] private float m_deathAnimationDuration = 1.0f;

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

    public override void TakeDamage()
    {
        if (IsDead) return;

        // 死亡アニメーションを再生
        if (m_animator != null && HasAnimatorParameter("des", AnimatorControllerParameterType.Trigger))
        {
            m_animator.SetTrigger("des");
        }

        // 死亡アニメーションが終わるまで待ってから非アクティブにする
        StartCoroutine(DeactivateAfterDelay(m_deathAnimationDuration));
    }

    // 死亡アニメーション後にオブジェクトを非アクティブにするコルーチン
    private IEnumerator DeactivateAfterDelay(float delay)
    {
        IsDead = true; // 死亡状態に設定し、FixedUpdateの処理を止める
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false); // プールに戻す
        IsDead = false; // 復活に備えてリセット
    }
}