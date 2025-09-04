using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Rigidbody2D m_rb;
    protected Animator m_animator;
    protected Collider2D m_collider;

    public bool IsDead { get; protected set; } = false;
    public Collider2D EnemyCollider => m_collider;

    private bool hasAnimator = false;

    // --- 修正箇所 ---
    [Header("スコア設定")]
    [Tooltip("この敵を倒したときに加算されるスコア")]
    [SerializeField] private int scoreValue = 100;
    // --- 修正箇所 ---

    protected virtual void Awake()
    {
        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
    }

    protected virtual void Start()
    {
        m_animator = GetComponent<Animator>();
        if (m_animator != null)
        {
            hasAnimator = true;
        }
    }

    protected virtual void OnEnable()
    {
        IsDead = false;
        if (m_collider != null) m_collider.enabled = true;
        if (m_rb != null) m_rb.isKinematic = false;
    }

    public virtual void TakeDamage()
    {
        Die();
    }

    public virtual void Die()
    {
        if (IsDead) return;
        IsDead = true;

        // --- 修正箇所 ---
        // スコア加算
        // GameManager.instance を GameManager.Instance に修正
        if (GameManager.Instance != null)
        {
            // GameManager.instance.AddGem(scoreValue) を GameManager.Instance.AddGem(scoreValue) に修正
            GameManager.Instance.AddGem(scoreValue);
        }
        // --- 修正箇所 ---

        // ★追加: 敵撃破音を再生する
        ItemSoundPlayer soundPlayer = FindObjectOfType<ItemSoundPlayer>();
        if (soundPlayer != null)
        {
            soundPlayer.PlayEnemyDefeatSound();
        }

        if (m_rb != null)
        {
            m_rb.velocity = Vector2.zero;
            m_rb.angularVelocity = 0f;
            m_rb.isKinematic = true;
        }

        if (m_collider != null)
        {
            m_collider.enabled = false;
        }

        if (hasAnimator && HasAnimatorParameter("des", AnimatorControllerParameterType.Trigger))
        {
            m_animator.SetTrigger("des");
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void OnDefeatAnimationEnd()
    {
        Destroy(gameObject);
    }

    protected bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType)
    {
        if (!hasAnimator || m_animator.runtimeAnimatorController == null) return false;
        foreach (AnimatorControllerParameter param in m_animator.parameters)
        {
            if (param.name == paramName && param.type == paramType)
            {
                return true;
            }
        }
        return false;
    }
}