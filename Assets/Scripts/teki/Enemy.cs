using UnityEngine;
using Debug = UnityEngine.Debug; // Debug�̞B���ȎQ�Ƃ��������邽��

/// <summary>
/// �G�L�����N�^�[�̃x�[�X�N���X�B
/// ���ʂ̃v���p�e�B�Ǝ��S������񋟂��܂��B
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("���ʉ��ݒ�")]
    public AudioClip knockDownClip; // �G���|���ꂽ���̌��ʉ�

    protected AudioSource audioSource; // ���ʉ����Đ����邽�߂�AudioSource
    protected Animator m_animator;    // �G�̃A�j���[�^�[
    protected Rigidbody2D m_rb;      // �G��Rigidbody2D
    protected Collider2D m_collider; // �G��Collider2D

    // ���C���_: Collider2D���O������Q�Ƃł���悤�ɂ���v���p�e�B (public)��
    public Collider2D EnemyCollider => m_collider;

    protected bool m_isDead = false;  // �G�����S���Ă��邩�ǂ����̃t���O

    // ���S��Ԃ��擾����v���p�e�B (�O������Q�Ƃł���悤��)
    public bool IsDead => m_isDead;


    // �X�N���v�g���L���ɂȂ����ŏ��̃t���[���ŌĂяo�����
    protected virtual void Awake() // Start����Awake�ɕύX (�R���|�[�l���g�擾��Awake�ōs���̂���ʓI)
    {
        // �K�v�ȃR���|�[�l���g���擾
        audioSource = GetComponent<AudioSource>();
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>(); // Collider2D���擾

        if (audioSource == null)
        {
            Debug.LogWarning($"Enemy: AudioSource�R���|�[�l���g��'{gameObject.name}'�Ɍ�����܂���B�ǉ����܂��B");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (m_animator == null)
        {
            Debug.LogWarning($"Enemy: Animator�R���|�[�l���g��'{gameObject.name}'�Ɍ�����܂���B���S�A�j���[�V�������Đ��ł��܂���B");
        }
        if (m_rb == null)
        {
            Debug.LogWarning($"Enemy: Rigidbody2D�R���|�[�l���g��'{gameObject.name}'�Ɍ�����܂���B�����I�Ȕ��������Ғʂ�łȂ��\��������܂��B");
        }
        if (m_collider == null)
        {
            Debug.LogError($"Enemy: Collider2D�R���|�[�l���g��'{gameObject.name}'�Ɍ�����܂���B�Փ˔��肪�ł��܂���B");
        }
    }

    // �G�����S�����ۂ̏���
    public virtual void Die()
    {
        // ���Ɏ��S���Ă���ꍇ�͏������I��
        if (m_isDead) return;

        m_isDead = true; // ���S�t���O�𗧂Ă�
        Debug.Log($"Enemy: '{gameObject.name}' ���|����܂����I");

        // ���ʉ��̍Đ�
        if (knockDownClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(knockDownClip);
        }

        // �A�j���[�V�����̐؂�ւ�
        if (m_animator != null)
        {
            SetAnimatorBoolSafe("des", true); // "des" �p�����[�^��true�ɐݒ�i���S�A�j���[�V�����Ȃǁj
            SetAnimatorBoolSafe("fly", false); // "fly" �p�����[�^��false�ɐݒ�
            SetAnimatorBoolSafe("run", false); // "run" �p�����[�^��false�ɐݒ�
        }

        // ���������̒�~
        if (m_rb != null)
        {
            m_rb.velocity = Vector2.zero; // ���x�����Z�b�g
            m_rb.simulated = false;      // �������Z�𖳌���
        }

        // �R���C�_�[�𖳌������āA����ȏ�v���C���[�ƏՓ˂��Ȃ��悤�ɂ���
        if (m_collider != null)
        {
            m_collider.enabled = false;
        }

        // 0.5�b��Ɏ��g��j�����郁�\�b�h���Ăяo��
        Invoke(nameof(DestroySelf), 0.5f);
    }

    // �Q�[���I�u�W�F�N�g���g��j������
    protected virtual void DestroySelf()
    {
        Destroy(gameObject);
        Debug.Log($"Enemy: '{gameObject.name}' �I�u�W�F�N�g��j�����܂����B");
    }

    // Animator��Bool�p�����[�^�����S�ɐݒ肷��w���p�[���\�b�h
    // (�w�肵���p�����[�^�����݂��A�^��Bool�̏ꍇ�̂ݐݒ肷��)
    protected void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (m_animator == null) return; // �A�j���[�^�[���Ȃ���Ή������Ȃ�

        // �A�j���[�^�[�̑S�p�����[�^�𑖍�
        foreach (var p in m_animator.parameters)
        {
            // ���O�ƌ^����v����p�����[�^�����������ꍇ
            if (p.name == paramName && p.type == AnimatorControllerParameterType.Bool)
            {
                m_animator.SetBool(paramName, value); // �p�����[�^��ݒ�
                return; // �������I��
            }
        }
    }
}
