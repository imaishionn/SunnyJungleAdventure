using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Rigidbody2D m_rb;
    protected Animator m_animator;
    protected Collider2D m_collider;

    public bool IsDead { get; protected set; } = false;
    public Collider2D EnemyCollider => m_collider;

    private bool hasAnimator = false;

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

    // このメソッドに'virtual'を追加しました
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