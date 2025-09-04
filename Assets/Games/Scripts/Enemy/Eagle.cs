using System.Collections;
using UnityEngine;

/// <summary>
/// 敵キャラクター「ワシ」のAIと動作を制御するスクリプトです。
/// プレイヤーを検知すると追跡し、ダメージを受けるとHPが減り、点滅します。
/// Enemyクラスを継承しています。
/// </summary>
public class Eagle : Enemy
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

    [Header("ダメージ時設定")]
    [Tooltip("ダメージを受けた際の点滅時間")]
    [SerializeField] private float FlashDuration = 1f;
    [Tooltip("点滅の間隔 (表示/非表示の切り替え時間)")]
    [SerializeField] private float FlashInterval = 0.1f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    [SerializeField] private Transform m_player;
    private bool m_isFlying = false;
    private int m_hp = 3;
    private SpriteRenderer m_spriteRenderer;

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
                Debug.LogWarning("Eagle: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
            }
        }

        // SpriteRendererコンポーネントの参照を取得
        m_spriteRenderer = GetComponent<SpriteRenderer>();
        if (m_spriteRenderer == null)
        {
            Debug.LogError("Eagle: SpriteRendererがアタッチされていません。", this);
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
        m_hp = 3; // HPをリセット

        // 速度をリセット
        if (m_rb != null) m_rb.velocity = Vector2.zero;

        // アニメーションをリセット
        if (m_animator != null && HasAnimatorParameter("fly", AnimatorControllerParameterType.Bool))
        {
            m_animator.SetBool("fly", false);
        }

        // スプライトの状態をリセット
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.enabled = true;
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
        if (m_player.position.x > transform.position.x)
        {
            FlipSprite(true); // 右向き
        }
        else if (m_player.position.x < transform.position.x)
        {
            FlipSprite(false); // 左向き
        }
    }

    /// <summary>
    /// キャラクターのx軸スケールを反転させ、向きを変えます。
    /// </summary>
    /// <param name="isFacingRight">右向きならtrue、左向きならfalse</param>
    private void FlipSprite(bool isFacingRight)
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

    /// <summary>
    /// ダメージを受けた際の処理。
    /// 親クラスのTakeDamage()をオーバーライドしています。
    /// </summary>
    public override void TakeDamage()
    {
        if (IsDead) return;

        m_hp--;
        Debug.Log("Eagle took damage. HP: " + m_hp);

        if (m_hp <= 0)
        {
            // HPが0以下になったら死亡処理
            base.Die();
        }
        else
        {
            // ダメージアニメーションを再生
            if (m_animator != null && HasAnimatorParameter("hurt", AnimatorControllerParameterType.Trigger))
            {
                m_animator.SetTrigger("hurt");
            }
            // 点滅コルーチンを開始
            StartCoroutine(FlashRoutine());
        }
    }

    /// <summary>
    /// SpriteRendererを一定時間、点滅させるコルーチン。
    /// </summary>
    private IEnumerator FlashRoutine()
    {
        float timer = 0f;
        while (timer < FlashDuration)
        {
            // SpriteRendererの表示/非表示を切り替える
            if (m_spriteRenderer != null)
            {
                m_spriteRenderer.enabled = !m_spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(FlashInterval);
            timer += FlashInterval;
        }

        // 点滅終了後、スプライトを必ず表示状態に戻す
        if (m_spriteRenderer != null)
        {
            m_spriteRenderer.enabled = true;
        }
    }
}