using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

/// <summary>
/// プレイヤーキャラクターの移動、ジャンプ、敵との相互作用、およびゲームオーバーを管理するスクリプトです。
/// </summary>
public class PlayerMove : MonoBehaviour
{
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("移動設定")]
    [Tooltip("プレイヤーの横移動速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("ジャンプ時の上向きの力")]
    [SerializeField] private float jumpForce = 10f;
    [Tooltip("ジャンプできる回数（例: 2は2段ジャンプ）")]
    [SerializeField] private int maxJumps = 2;

    [Header("敵との相互作用")]
    [Tooltip("敵を踏みつけたときに跳ねる力")]
    [SerializeField] private float m_stompBounceForce = 7f;
    [Tooltip("ダメージを受けた後の無敵時間")]
    [SerializeField] private float invincibleDuration = 0.2f;

    [Header("コンポーネントとオブジェクト")]
    [Tooltip("地面判定を行う子オブジェクトのコンポーネント")]
    [SerializeField] private GroundCheck m_groundCheckComponent;

    [Header("ゲームオーバー条件")]
    [Tooltip("プレイヤーがこのY座標より下に落ちるとゲームオーバー")]
    [SerializeField] private float m_gameOverFallHeight = -10f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Rigidbody2D rb;
    private Animator animator;
    private ItemSoundPlayer itemSoundPlayer;

    private bool isGrounded;
    private bool isFacingRight = true;
    private int jumpsRemaining;
    private bool isInvincible = false;

    // ----------------------------------------------------------------------------------------------------
    // パブリックプロパティ
    // ----------------------------------------------------------------------------------------------------
    public bool IsDead { get; private set; } = false;
    public float MoveSpeed => moveSpeed;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake()
    {
        // コンポーネントの参照を取得
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // コンポーネントの存在チェック
        if (rb == null) Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。", this);
        if (animator == null) Debug.LogError("PlayerMove: Animatorがアタッチされていません。", this);

        // GroundCheckコンポーネントの参照を取得
        if (m_groundCheckComponent == null)
        {
            m_groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (m_groundCheckComponent == null)
            {
                Debug.LogError("PlayerMove: GroundCheckコンポーネントが見つかりません。", this);
            }
        }

        // シーン内のItemSoundPlayerのインスタンスを検索
        itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();

        // 初期化
        jumpsRemaining = maxJumps;
    }

    private void Update()
    {
        // 死亡状態の場合は、これ以上の処理を行わない
        if (IsDead) return;

        // 地面判定の更新
        bool previousIsGrounded = isGrounded;
        isGrounded = m_groundCheckComponent != null && m_groundCheckComponent.GetIsGround();

        // 地面に接地した瞬間、ジャンプ回数をリセット
        if (!previousIsGrounded && isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // 水平方向の移動処理
        float moveInput = Input.GetAxis("Horizontal");
        if (rb != null)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        // キャラクターの向きを更新
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        // ジャンプ処理
        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            if (rb != null)
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            }
            jumpsRemaining--;

            // サウンド再生
            if (itemSoundPlayer != null)
            {
                itemSoundPlayer.PlayJumpSound();
            }

            // アニメーション制御
            if (animator != null)
            {
                if (isGrounded && HasAnimatorParameter("JumpTrigger", AnimatorControllerParameterType.Trigger))
                {
                    animator.SetTrigger("JumpTrigger");
                }
                else if (!isGrounded && HasAnimatorParameter("DoubleJumpTrigger", AnimatorControllerParameterType.Trigger))
                {
                    animator.SetTrigger("DoubleJumpTrigger");
                }
            }
        }

        // アニメーターパラメーターの更新
        UpdateAnimatorParameters(moveInput);

        // ゲームオーバー条件のチェック
        if (transform.position.y < m_gameOverFallHeight)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 死亡状態または無敵状態の場合は処理を停止
        if (IsDead || isInvincible) return;

        // 敵との衝突判定
        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 踏みつけ以外の衝突は即座にダメージを受ける（ここでは死亡）
            Die();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 敵を踏みつけたときに呼び出されます。
    /// </summary>
    public void StompEnemy(GameObject enemyObject)
    {
        if (IsDead) return;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;

        // 踏みつけ成功
        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, m_stompBounceForce);
        }
        enemy.TakeDamage();
        StartCoroutine(BecomeInvincible(invincibleDuration));
    }

    /// <summary>
    /// プレイヤーを死亡状態にします。
    /// </summary>
    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (itemSoundPlayer != null) itemSoundPlayer.PlayGameOverSound();

        // 物理挙動と当たり判定を無効化
        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        // ゲームオーバーアニメーションを再生
        if (animator != null && HasAnimatorParameter("GameOver", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("GameOver");
        }
        else
        {
            // アニメーションがない場合のフォールバック処理
            StartCoroutine(FallbackDeathSequence());
        }
    }

    /// <summary>
    /// ゲームオーバーアニメーションの最後に呼び出されるイベントメソッドです。
    /// </summary>
    public void OnGameOverAnimationEnd()
    {
        // アニメーションイベントから呼ばれた場合にシーン遷移を最終化
        gameObject.SetActive(false);
        FinalizeDeathAndSceneTransition();
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// プレイヤーの向きを反転させます。
    /// </summary>
    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    /// <summary>
    /// アニメーターのパラメーターを更新します。
    /// </summary>
    private void UpdateAnimatorParameters(float moveInput)
    {
        if (animator == null) return;

        // 走るアニメーション
        if (HasAnimatorParameter("run", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("run", Mathf.Abs(moveInput) > 0.01f);
        }

        // 地面に着地しているか
        if (HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("isGrounded", isGrounded);
        }

        // Y軸の速度（ジャンプ/落下）
        if (HasAnimatorParameter("velocityY", AnimatorControllerParameterType.Float) && rb != null)
        {
            animator.SetFloat("velocityY", rb.velocity.y);
        }
    }

    /// <summary>
    /// 指定された名前とタイプのAnimatorパラメーターが存在するか確認します。
    /// </summary>
    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType)
    {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName && param.type == paramType)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// ゲームオーバーアニメーションがない場合のフォールバック処理です。
    /// </summary>
    private IEnumerator FallbackDeathSequence()
    {
        yield return new WaitForSeconds(1.0f); // 1秒待機
        FinalizeDeathAndSceneTransition();
    }

    /// <summary>
    /// 死亡処理とシーン遷移を最終化します。
    /// </summary>
    private void FinalizeDeathAndSceneTransition()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetGameOverStateImmediately();
        }
        else
        {
            Debug.LogError("GameManagerのインスタンスが見つかりません。直接シーンをロードします。");
            SceneManager.LoadScene("GameOverScene");
        }
    }

    /// <summary>
    /// プレイヤーを一時的に無敵状態にするコルーチンです。
    /// </summary>
    private IEnumerator BecomeInvincible(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}