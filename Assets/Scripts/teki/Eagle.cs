using UnityEngine;
using System.Collections;
using Debug = UnityEngine.Debug;

public class Eagle : Enemy
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("飛行速度")]
    [SerializeField] float FlySpeed = 5f;

    [Header("点滅時間と間隔")]
    [SerializeField] float FlashDuration = 1f;
    [SerializeField] float FlashInterval = 0.1f;

    [SerializeField] Transform m_player;
    private bool m_isFlying = false;
    private int m_hp = 3;
    private SpriteRenderer m_spriteRenderer;

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
                Debug.LogWarning("Eagle: 'Player'タグを持つGameObjectが見つかりません。", this);
            }
        }

        m_spriteRenderer = GetComponent<SpriteRenderer>();

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

        // プレイヤーの左右の位置を比較して向きを決定
        if (m_player.position.x > transform.position.x)
        {
            // プレイヤーが右にいる場合
            FlipSprite(true);
        }
        else if (m_player.position.x < transform.position.x)
        {
            // プレイヤーが左にいる場合
            FlipSprite(false);
        }
    }

    // 引数isFacingRightで向きを制御
    void FlipSprite(bool isFacingRight)
    {
        Vector3 scale = transform.localScale;
        if (isFacingRight)
        {
            scale.x = Mathf.Abs(scale.x); // 右向き
        }
        else
        {
            scale.x = -Mathf.Abs(scale.x); // 左向き
        }
        transform.localScale = scale;
    }

    public override void TakeDamage()
    {
        if (IsDead) return;

        m_hp--;
        Debug.Log("Eagle took damage. HP: " + m_hp);

        if (m_hp <= 0)
        {
            base.Die();
        }
        else
        {
            if (m_animator != null && HasAnimatorParameter("hurt", AnimatorControllerParameterType.Trigger))
            {
                m_animator.SetTrigger("hurt");
            }
            StartCoroutine(FlashRoutine());
        }
    }

    private IEnumerator FlashRoutine()
    {
        float timer = 0f;
        bool isFlashing = true;
        while (timer < FlashDuration)
        {
            isFlashing = !isFlashing;
            if (m_spriteRenderer != null)
            {
                m_spriteRenderer.enabled = isFlashing;
            }
            yield return new WaitForSeconds(FlashInterval);
            timer += FlashInterval;
        }

        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.enabled = true;
        }
    }
}