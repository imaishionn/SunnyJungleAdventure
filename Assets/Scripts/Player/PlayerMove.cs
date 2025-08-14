using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private int maxJumps = 2;

    [Header("Enemy Interaction")]
    [SerializeField] private float m_stompBounceForce = 7f;
    [SerializeField] private float invincibleDuration = 0.2f;
    private bool isInvincible = false;

    [Header("Ground Detection Settings")]
    [SerializeField] private GroundCheck m_groundCheckComponent;

    [Header("Components")]
    private Rigidbody2D rb;
    private Animator animator;
    private ItemSoundPlayer itemSoundPlayer;

    [Header("Game Over Conditions")]
    [SerializeField] private float m_gameOverFallHeight = -10f;

    private bool isGrounded;
    private bool isFacingRight = true;
    private int jumpsRemaining;

    public bool IsDead { get; private set; } = false;
    public float MoveSpeed => moveSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (rb == null) UnityEngine.Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。", this);
        if (animator == null) UnityEngine.Debug.LogError("PlayerMove: Animatorがアタッチされていません。", this);

        if (m_groundCheckComponent == null)
        {
            m_groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (m_groundCheckComponent == null)
            {
                UnityEngine.Debug.LogError("PlayerMove: GroundCheckコンポーネントが見つかりません。", this);
            }
        }

        itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        if (itemSoundPlayer == null)
        {
            // UnityEngine.Debug.LogWarning("PlayerMove: シーンにItemSoundPlayerが見つかりません。");
        }

        jumpsRemaining = maxJumps;
    }

    void Update()
    {
        if (IsDead) return;

        bool previousIsGrounded = isGrounded;
        isGrounded = m_groundCheckComponent != null && m_groundCheckComponent.GetIsGround();

        if (animator != null && HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("isGrounded", isGrounded);
        }

        if (!previousIsGrounded && isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        float moveInput = Input.GetAxis("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        if (animator != null && HasAnimatorParameter("run", AnimatorControllerParameterType.Bool))
        {
            animator.SetBool("run", Mathf.Abs(moveInput) > 0.01f);
        }

        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        if (animator != null && HasAnimatorParameter("velocityY", AnimatorControllerParameterType.Float))
        {
            animator.SetFloat("velocityY", rb.velocity.y);
        }

        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpsRemaining--;

            if (itemSoundPlayer != null)
            {
                itemSoundPlayer.PlayJumpSound();
            }

            if (isGrounded && animator != null && HasAnimatorParameter("JumpTrigger", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("JumpTrigger");
            }
            else if (!isGrounded && animator != null && HasAnimatorParameter("DoubleJumpTrigger", AnimatorControllerParameterType.Trigger))
            {
                animator.SetTrigger("DoubleJumpTrigger");
            }
        }

        if (transform.position.y < m_gameOverFallHeight && !IsDead)
        {
            Die();
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsDead || isInvincible) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            // 踏みつけ以外の衝突は即座にダメージを受ける
            Die();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
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

    // StompCheck.csから呼び出されるメソッド
    public void StompEnemy(GameObject enemyObject)
    {
        if (IsDead) return;

        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;

        // 踏みつけ成功
        rb.velocity = new Vector2(rb.velocity.x, m_stompBounceForce);
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

    private IEnumerator FallbackDeathSequence()
    {
        yield return new WaitForSeconds(1.0f);
        FinalizeDeathAndSceneTransition();
    }

    public void OnGameOverAnimationEnd()
    {
        gameObject.SetActive(false);
        FinalizeDeathAndSceneTransition();
    }

    private void FinalizeDeathAndSceneTransition()
    {
        if (GameManager.instance != null)
        {
            GameManager.instance.SetGameOverStateImmediately();
        }
        else
        {
            UnityEngine.Debug.LogError("GameManagerのインスタンスが見つかりません。");
            SceneManager.LoadScene("GameOverScene");
        }
    }

    private IEnumerator BecomeInvincible(float duration)
    {
        isInvincible = true;
        yield return new WaitForSeconds(duration);
        isInvincible = false;
    }
}