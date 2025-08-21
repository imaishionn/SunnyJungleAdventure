using System.Collections;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// 敵キャラクター「コウモリ」のAIと動作を制御するスクリプトです。
/// プレイヤーを検知すると追跡し、ダメージを受けると死亡アニメーションを再生します。
/// Enemyクラスを継承しています。
/// </summary>
public class bat : Enemy
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プレイヤー検知設定")]
    [Tooltip("プレイヤーを検知する半径")]
    [SerializeField] private float DetectRange = 5f;

    [Header("移動設定")]
    [Tooltip("飛行速度")]
    [SerializeField] private float FlySpeed = 5f;

    [Header("死亡設定")]
    [Tooltip("死亡アニメーションの再生時間")]
    [SerializeField] private float m_deathAnimationDuration = 1.0f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Transform m_player;
    private bool m_isFlying = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake()
    {
        // 親クラスのAwake()を呼び出し、基盤となる初期化を行う
        base.Awake();

        // 'Player'タグを持つオブジェクトを探し、見つかればTransformを取得
        if (m_player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                m_player = playerObj.transform;
            else
            {
                // 'Player'タグが見つからない場合は警告ログを出力
                Debug.LogWarning("Bat: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
            }
        }

        // Rigidbody2Dの設定
        if (m_rb != null)
        {
            m_rb.gravityScale = 0f; // 重力を無効化
            m_rb.drag = 1f;         // 飛行中に滑らかに減速させるための抵抗
        }
    }

    protected override void OnEnable()
    {
        // 親クラスのOnEnable()を呼び出す
        base.OnEnable();

        // オブジェクトがプールから再利用される際の初期化
        IsDead = false;
        m_isFlying = false;

        // 速度をリセット
        if (m_rb != null) m_rb.velocity = Vector2.zero;

        // アニメーションをリセット
        if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
        {
            m_animator.SetBool("fly", false);
        }
    }

    protected void FixedUpdate()
    {
        // 死亡状態の場合は処理を停止
        if (IsDead) return;

        // プレイヤーやRigidbody2Dが設定されていない場合は、動きを停止して処理を抜ける
        if (m_player == null || m_rb == null)
        {
            if (m_rb != null) m_rb.velocity = Vector2.zero;
            if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
            {
                m_animator.SetBool("fly", false);
            }
            return;
        }

        // プレイヤーとの距離を計算
        float distance = Vector2.Distance(transform.position, m_player.position);

        // プレイヤーが検知範囲内にいるかチェック
        if (distance < DetectRange)
        {
            // プレイヤーに向かって飛行
            FlyToPlayer();

            // 飛行状態のアニメーションを有効にする（一度だけ実行）
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
            // プレイヤーが検知範囲外に出た場合
            if (m_isFlying)
            {
                // 飛行アニメーションを無効にする（一度だけ実行）
                if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
                {
                    m_animator.SetBool("fly", false);
                }
                m_isFlying = false;
            }

            // 速度を停止
            m_rb.velocity = Vector2.zero;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッドとプライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プレイヤーに向かって移動します。
    /// </summary>
    private void FlyToPlayer()
    {
        // プレイヤーへの方向を正規化して取得
        Vector2 direction = (m_player.position - transform.position).normalized;
        // Rigidbody2Dに速度を設定して移動
        m_rb.velocity = direction * FlySpeed;

        // キャラクターの向きをプレイヤーに合わせて反転
        if (direction.x > 0 && transform.localScale.x < 0)
        {
            FlipSprite();
        }
        else if (direction.x < 0 && transform.localScale.x > 0)
        {
            FlipSprite();
        }
    }

    /// <summary>
    /// キャラクターのx軸スケールを反転させ、向きを変えます。
    /// </summary>
    private void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    /// <summary>
    /// ダメージを受けた際の処理です。
    /// 親クラスのTakeDamage()をオーバーライドしています。
    /// </summary>
    public override void TakeDamage()
    {
        if (IsDead) return;

        // 親クラスのDie()を呼び出してコライダーを無効化
        base.Die();

        // 死亡アニメーションを再生
        if (m_animator != null && HasAnimatorParameter("des", AnimatorControllerParameterType.Trigger))
        {
            m_animator.SetTrigger("des");
        }

        // 死亡アニメーションが終わるまで待ってから非アクティブにする（オブジェクトプールに戻す）
        StartCoroutine(DeactivateAfterDelay(m_deathAnimationDuration));
    }

    /// <summary>
    /// 指定された時間待機後、ゲームオブジェクトを非アクティブにするコルーチン。
    /// オブジェクトプールの再利用を前提としています。
    /// </summary>
    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}