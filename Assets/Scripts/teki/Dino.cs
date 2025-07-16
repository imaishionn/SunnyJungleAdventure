using UnityEngine;
using Debug = UnityEngine.Debug;

public class Dino : Enemy
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("走る速度")]
    [SerializeField] float RunSpeed = 3f;

    [Header("パトロール移動範囲（左右）")]
    [SerializeField] float PatrolDistance = 3f;

    [SerializeField] Transform m_player;

    [Header("壁検知設定")]
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
            Debug.LogWarning("Dino: Wall Layerが設定されていません！衝突検知が正しく行われない可能性があります。");
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate(); // EnemyクラスのFixedUpdateも呼び出す

        // ゲームが停止状態（ゲームオーバー、クリアなど）または敵が死亡している場合は処理を停止
        if ((GameManager.instance != null &&
             (GameManager.instance.GetCurrentGameState() == GameManager.GameState.enGameState_GameOver || // ★修正: GetState() -> GetCurrentGameState()★
              GameManager.instance.GetCurrentGameState() == GameManager.GameState.enGameState_Clear)) || m_isDead) // ★修正: GetState() -> GetCurrentGameState()★
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

        // 壁検知
        Vector2 checkOrigin = (Vector2)m_collider.bounds.center + new Vector2(m_collider.bounds.extents.x * currentFacingDirectionX, wallCheckOffset.y);
        RaycastHit2D hit = Physics2D.Raycast(checkOrigin, Vector2.right * currentFacingDirectionX, wallCheckDistance, wallLayer);

        if (hit.collider != null)
        {
            m_patrolDirection *= -1; // 方向転換
            desiredMoveDirectionX = m_patrolDirection;
        }
        else
        {
            float distanceToPlayer = m_player != null ? Vector2.Distance(transform.position, m_player.position) : Mathf.Infinity;

            if (distanceToPlayer < DetectRange)
            {
                // プレイヤーを検知したらプレイヤーに向かって移動
                desiredMoveDirectionX = Mathf.Sign(m_player.position.x - transform.position.x);
            }
            else
            {
                // パトロール範囲の端に達したら方向転換
                float distanceFromStart = transform.position.x - m_startPos.x;

                if (Mathf.Abs(distanceFromStart) >= PatrolDistance)
                {
                    m_patrolDirection *= -1;
                    m_startPos = transform.position; // 新しいパトロール開始位置を現在の位置にする（必要であれば）
                }
                desiredMoveDirectionX = m_patrolDirection;
            }
        }

        m_targetVelocityX = desiredMoveDirectionX * RunSpeed;

        // 移動方向に応じてスプライトを反転
        if ((m_targetVelocityX > 0 && !IsFacingRight) || (m_targetVelocityX < 0 && IsFacingRight))
        {
            FlipSprite();
        }

        m_rb.velocity = new Vector2(m_targetVelocityX, m_rb.velocity.y);

        if (m_animator != null)
        {
            m_animator.SetBool("run", Mathf.Abs(m_rb.velocity.x) > 0.05f); // 速度が一定以上あれば走るアニメーション
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

        // 壁検知用のGizmo
        Gizmos.color = Color.red;
        float currentFacingDirectionX = IsFacingRight ? 1f : -1f;
        Vector2 checkOrigin = (Vector2)m_collider.bounds.center + new Vector2(m_collider.bounds.extents.x * currentFacingDirectionX, wallCheckOffset.y);
        Gizmos.DrawLine(checkOrigin, checkOrigin + Vector2.right * currentFacingDirectionX * wallCheckDistance);

        // コライダーのバウンディングボックス
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(m_collider.bounds.center, m_collider.bounds.size);

        // パトロール範囲のGizmo
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(m_startPos + Vector2.left * PatrolDistance, m_startPos + Vector2.left * PatrolDistance + Vector2.up * 1f);
        Gizmos.DrawLine(m_startPos + Vector2.right * PatrolDistance, m_startPos + Vector2.right * PatrolDistance + Vector2.up * 1f);
    }
}