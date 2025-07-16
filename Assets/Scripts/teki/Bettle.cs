using UnityEngine;
using Debug = UnityEngine.Debug;

public class Bettle : Enemy
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("飛行速度")]
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
            m_rb.gravityScale = 0f; // 重力無効
            m_rb.drag = 1f; // 空気抵抗
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
            if (m_rb != null)
            {
                m_rb.velocity = Vector2.zero;
            }
            if (m_animator != null)
            {
                // 必要であれば停止アニメーションを設定
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
            m_rb.velocity = Vector2.zero; // 範囲外では停止
        }
    }

    void FlyToPlayer()
    {
        Vector2 direction = (m_player.position - transform.position).normalized;
        m_rb.velocity = direction * FlySpeed;

        // プレイヤーの方向に応じてスプライトを反転
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