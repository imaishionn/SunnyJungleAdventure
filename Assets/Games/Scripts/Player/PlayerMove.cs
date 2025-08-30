using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
using UnityEngine.EventSystems;
using System.Collections.Generic;

/// <summary>
/// プレイヤーキャラクターの移動とジャンプを管理するスクリプトです。
/// 敵との相互作用、ゲームオーバー処理も含まれます。
/// </summary>
public class PlayerMove : MonoBehaviour
{
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

    // Mobile Control Canvasから参照を受け取るためのプライベート変数
    private VirtualJoystick m_joystick;
    private JumpButtonController m_jumpButton;

    [Header("ゲームオーバー条件")]
    [Tooltip("プレイヤーがこのY座標より下に落ちるとゲームオーバー")]
    [SerializeField] private float m_gameOverFallHeight = -10f;

    private Rigidbody2D rb;
    private Animator animator;
    private ItemSoundPlayer itemSoundPlayer;

    private bool isGrounded;
    private bool isFacingRight = true;
    private int jumpsRemaining;
    private bool isInvincible = false;

    public bool IsDead { get; private set; } = false;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null) Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。", this);
        if (animator == null) Debug.LogError("PlayerMove: Animatorがアタッチされていません。", this);

        if (m_groundCheckComponent == null)
        {
            m_groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (m_groundCheckComponent == null)
            {
                Debug.LogError("PlayerMove: GroundCheckコンポーネントが見つかりません。", this);
            }
        }

        itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        jumpsRemaining = maxJumps;
    }

    /// <summary>
    /// GameManagerからモバイル操作用のボタンを参照として受け取ります。
    /// </summary>
    public void SetMobileControls(VirtualJoystick joystick, JumpButtonController jump)
    {
        m_joystick = joystick;
        m_jumpButton = jump;

        if (m_joystick == null || m_jumpButton == null)
        {
            Debug.LogError("PlayerMove: ジョイスティックまたはジャンプボタンの割り当てに失敗しました。GameManagerからの参照を確認してください。", this);
        }
    }

    private void Update()
    {
        if (IsDead) return;

        bool previousIsGrounded = isGrounded;
        isGrounded = m_groundCheckComponent != null && m_groundCheckComponent.GetIsGround();

        if (!previousIsGrounded && isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        // 入力処理を統合
        float moveInput = 0f;
        if (!UnityEngine.Application.isMobilePlatform)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }
        else
        {
            if (m_joystick != null)
            {
                moveInput = m_joystick.InputDirection.x;
            }
        }

        HandleMovementInput(moveInput);

        if (!UnityEngine.Application.isMobilePlatform && Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        UpdateAnimatorParameters(rb.velocity.x);

        if (transform.position.y < m_gameOverFallHeight)
        {
            Die();
        }
    }

    private void HandleMovementInput(float moveInput)
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    public void Jump()
    {
        if (IsDead) return;
        if (jumpsRemaining <= 0) return;

        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        jumpsRemaining--;

        if (itemSoundPlayer != null)
        {
            itemSoundPlayer.PlayJumpSound();
        }
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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDead || isInvincible) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Die();
        }
    }

    public void StompEnemy(GameObject enemyObject)
    {
        if (IsDead) return;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;

        if (rb != null)
        {
            rb.velocity = new Vector2(rb.velocity.x, m_stompBounceForce);
        }
        enemy.TakeDamage();
        StartCoroutine(BecomeInvincible(invincibleDuration));
    }

    public void Die()
    {
        if (IsDead) return;
        IsDead = true;

        if (itemSoundPlayer != null) itemSoundPlayer.PlayGameOverSound();

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true;
        }
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        if (animator != null && HasAnimatorParameter("GameOver", AnimatorControllerParameterType.Trigger))
        {
            animator.SetTrigger("GameOver");
        }
        else
        {
            StartCoroutine(FallbackDeathSequence());
        }
    }

    public void OnGameOverAnimationEnd()
    {
        gameObject.SetActive(false);
        FinalizeDeathAndSceneTransition();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void UpdateAnimatorParameters(float moveInput)
    {
        if (animator == null) return;

        if (HasAnimatorParameter("run", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("run", Mathf.Abs(moveInput) > 0.01f);
        }

        if (HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("isGrounded", isGrounded);
        }

        if (HasAnimatorParameter("velocityY", AnimatorControllerParameterType.Float) && rb != null)
        {
            animator.SetFloat("velocityY", rb.velocity.y);
        }
    }

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

    private IEnumerator FallbackDeathSequence()
    {
        yield return new WaitForSeconds(1.0f);
        FinalizeDeathAndSceneTransition();
    }

    private void FinalizeDeathAndSceneTransition()
    {
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetGameOverStateImmediately();
        }
        else
        {
            Debug.LogError("PlayerMove: GameManagerのインスタンスが見つかりません！タイトルシーンへ直接遷移します。");
            SceneManager.LoadScene("TitleScene");
        }
    }

    private IEnumerator BecomeInvincible(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}