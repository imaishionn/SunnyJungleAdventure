using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// �G�L�����N�^�[�̃x�[�X�N���X�B
/// ���ʂ̃v���p�e�B�Ǝ��S������񋟂��܂��B
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("���ʉ��ݒ�")]
    public AudioClip knockDownClip;

    protected AudioSource audioSource;
    protected Animator m_animator;
    protected Rigidbody2D m_rb;
    protected Collider2D m_collider;

    public Collider2D EnemyCollider => m_collider;

    protected bool m_isDead = false;

    public bool IsDead => m_isDead;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        if (m_rb != null)
        {
            m_rb.simulated = true;
            m_rb.isKinematic = false;
        }

        if (m_collider != null)
        {
            m_collider.enabled = true;
            m_collider.isTrigger = false;
        }
    }

    protected virtual void FixedUpdate()
    {
        // GameManager�̏�Ԃ��Q�[���I�[�o�[�܂��̓N���A�̏ꍇ�͒�~
        if (GameManager.instance != null &&
           (GameManager.instance.GetCurrentGameState() == GameManager.GameState.enGameState_GameOver ||
            GameManager.instance.GetCurrentGameState() == GameManager.GameState.enGameState_Clear))
        {
            if (m_rb != null)
            {
                m_rb.velocity = Vector2.zero;
            }
            if (m_animator != null)
            {
                // ��~��Ԃ̃A�j���[�V������K�p����Ȃ�
                SetAnimatorBoolSafe("run", false); // ��: ����A�j���[�V�������~�߂�
                SetAnimatorBoolSafe("fly", false); // ��: ��ԃA�j���[�V�������~�߂�
            }
        }
    }

    public virtual void Die()
    {
        if (m_isDead) return;

        m_isDead = true;

        if (knockDownClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(knockDownClip);
        }

        if (m_animator != null)
        {
            SetAnimatorBoolSafe("des", true); // ���S�A�j���[�V�����i��j
            SetAnimatorBoolSafe("fly", false);
            SetAnimatorBoolSafe("run", false);
        }

        if (m_rb != null)
        {
            m_rb.velocity = Vector2.zero;
            m_rb.simulated = false; // �������Z���~
        }

        if (m_collider != null)
        {
            m_collider.enabled = false; // �R���C�_�[�𖳌���
        }

        Invoke(nameof(DestroySelf), 0.5f); // 0.5�b��ɃI�u�W�F�N�g��j��
    }

    protected virtual void DestroySelf()
    {
        Destroy(gameObject);
    }

    protected void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (m_animator == null) return;

        foreach (var p in m_animator.parameters)
        {
            if (p.name == paramName && p.type == AnimatorControllerParameterType.Bool)
            {
                m_animator.SetBool(paramName, value);
                return;
            }
        }
    }
}