using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

/// <summary>
/// Dino（恐竜）の敵の動作を制御します。
/// プレイヤーを検知すると追尾し、それ以外はパトロールします。
/// 死亡時はベースのEnemyクラスのDieメソッドを呼び出します。
/// </summary>
public class Dino : Enemy // Enemyクラスを継承
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("走る速度")]
    [SerializeField] float RunSpeed = 3f;

    [Header("パトロール移動範囲（左右）")]
    [SerializeField] float PatrolDistance = 3f;

    [SerializeField] Transform m_player; // ★このフィールドにプレイヤーのTransformを割り当てます★

    private bool m_isRunning = false;
    private Vector2 m_startPos;
    private int m_patrolDirection = 1; // 1:右, -1:左

    protected override void Awake() // StartからAwakeに変更 (ベースクラスのAwakeを呼び出す)
    {
        base.Awake(); // 親クラス(Enemy)のAwakeを呼び出す

        if (m_rb != null)
        {
            m_rb.freezeRotation = true; // 回転を固定
        }

        m_startPos = transform.position; // 初期位置を保存

        // m_playerがInspectorで割り当てられていない場合のみ、タグで検索を試みる
        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
            else
                Debug.LogWarning("Dino: プレイヤーオブジェクトが見つかりません。タグ 'Player' を確認してください。");
        }
    }

    void Update()
    {
        if (m_isDead || m_rb == null || !m_rb.simulated) return; // 死亡中、Rigidbodyがない、または物理演算が無効な場合は処理を終了

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
            m_patrolDirection *= -1; // 方向転換
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
