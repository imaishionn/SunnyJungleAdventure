using System.Collections;

using UnityEngine;

public class Dog : Enemy
{
    [Header("プレイヤー検知距離")]
    [SerializeField] private float detectRange = 5f;

    [Header("移動設定")]
    [SerializeField] private float runSpeed = 3f;

    [Header("アニメーション")]
    [SerializeField] private string runAnimationTrigger = "run";

    [Header("接地判定")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [Header("足場端の判定")]
    [SerializeField] private float groundAheadCheckDistance = 0.5f;

    private Transform m_player;
    private bool m_isPlayerDetected = false;

    protected override void Awake()
    {
        base.Awake();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            m_player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("Dog: 'Player'タグを持つGameObjectが見つかりません。", this);
        }

        if (m_rb != null)
        {
            // Dogが地面を歩くように重力を有効化
            m_rb.gravityScale = 1f;
        }
    }

    protected void FixedUpdate()
    {
        if (IsDead || m_player == null)
        {
            m_rb.velocity = Vector2.zero;
            return;
        }

        // プレイヤーを検知するまで待機
        if (!m_isPlayerDetected)
        {
            float distance = Vector2.Distance(transform.position, m_player.position);
            if (distance < detectRange)
            {
                m_isPlayerDetected = true;
                if (m_animator != null && HasAnimatorParameter(runAnimationTrigger, AnimatorControllerParameterType.Trigger))
                {
                    m_animator.SetTrigger(runAnimationTrigger);
                }
            }
        }

        if (m_isPlayerDetected)
        {
            bool isGrounded = IsGrounded();

            if (isGrounded)
            {
                Vector2 direction = (m_player.position.x > transform.position.x) ? Vector2.right : Vector2.left;

                // 進行方向の先に足場があるかチェック
                if (IsGroundAhead(direction))
                {
                    m_rb.velocity = new Vector2(direction.x * runSpeed, m_rb.velocity.y);
                }
                else
                {
                    // 足場の端なので停止
                    m_rb.velocity = new Vector2(0, m_rb.velocity.y);
                    // ここで方向を反転させるロジックを追加することも可能
                }

                FlipSprite(direction.x);
            }
            else
            {
                // 地面にいない場合は横移動を停止
                m_rb.velocity = new Vector2(0, m_rb.velocity.y);
            }
        }
    }

    // 地面を判定するメソッド
    private bool IsGrounded()
    {
        if (groundCheck == null || groundLayer == 0)
        {
            Debug.LogWarning("GroundCheckまたはGroundLayerが設定されていません。", this);
            return false;
        }

        Collider2D collider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return collider != null;
    }

    // 進行方向の先に足場があるかを判定するメソッド
    private bool IsGroundAhead(Vector2 direction)
    {
        if (groundCheck == null || groundLayer == 0)
        {
            return false;
        }

        // groundCheckの位置から進行方向に向かって少しずらした位置を始点とする
        Vector2 checkPosition = (Vector2)groundCheck.position + direction * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, groundAheadCheckDistance, groundLayer);

        Debug.DrawRay(checkPosition, Vector2.down * groundAheadCheckDistance, Color.red);

        return hit.collider != null;
    }

    void FlipSprite(float directionX)
    {
        Vector3 scale = transform.localScale;
        if (directionX > 0 && scale.x < 0)
        {
            scale.x *= -1;
        }
        else if (directionX < 0 && scale.x > 0)
        {
            scale.x *= -1;
        }
        transform.localScale = scale;
    }

    public override void TakeDamage()
    {
        if (IsDead) return;
        Die();
    }
}