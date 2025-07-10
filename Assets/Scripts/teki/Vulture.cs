using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

/// <summary>
/// Vulture（ハゲタカ）の敵の動作を制御します。
/// プレイヤーを検知すると追尾し、死亡時はベースのEnemyクラスのDieメソッドを呼び出します。
/// </summary>
public class Vulture : Enemy // Enemyクラスを継承
{
    [Header("プレイヤー検知距離")]
    [SerializeField] float DetectRange = 5f;

    [Header("飛行速度")]
    [SerializeField] float FlySpeed = 5f;

    [SerializeField] Transform m_player;

    private bool m_isFlying = false;

    protected override void Awake() // StartからAwakeに変更 (ベースクラスのAwakeを呼び出す)
    {
        base.Awake(); // 親クラス(Enemy)のAwakeを呼び出す

        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
            else
                Debug.LogWarning("Vulture: プレイヤーオブジェクトが見つかりません。タグ 'Player' を確認してください。");
        }

        if (m_rb != null)
        {
            m_rb.gravityScale = 0f; // 重力を無効化して飛行
        }
    }

    void Update()
    {
        if (m_isDead || m_player == null) return; // 死亡中またはプレイヤーが見つからない場合は処理を終了

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
                m_rb.velocity = Vector2.zero; // プレイヤーが範囲外に出たら停止
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

    // Vulture固有の死亡処理が必要な場合は、ここで override public void Die() を実装できます
    // 今回はベースのEnemy.Die()を使用します
}
