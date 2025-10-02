using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵キャラクターの基本的な挙動を管理する基底クラスです。
/// </summary>
public class Enemy : MonoBehaviour {

    [Header("スコア設定"), SerializeField]
    private int _scoreValue = 100;



    /// <summary>
    /// Rigidbody2Dコンポーネントへの参照
    /// </summary>
    protected Rigidbody2D m_rb;

    /// <summary>
    /// アニメーターコンポーネントへの参照
    /// </summary>
    protected Animator m_animator;

    /// <summary>
    /// Collider2Dコンポーネントへの参照 
    /// </summary>

    protected Collider2D m_collider;

    /// <summary>
    /// 敵が死亡しているかどうかの状態
    /// </summary>
    public bool IsDead { get; protected set; } = false;

    /// <summary>
    /// 敵のCollider2Dコンポーネントへの公開プロパティ
    /// </summary>
    public Collider2D EnemyCollider => m_collider;

    /// <summary>
    /// アニメーターが存在するかどうかのフラグ
    /// </summary>
    private bool _hasAnimator = false;

    /// <summary>
    /// アニメーターの「des」トリガーパラメータのハッシュ値
    /// </summary>
    private static readonly int _animatorDesHash = Animator.StringToHash("des");



    protected virtual void Awake() {
        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>();
    }

    protected virtual void Start() {
        m_animator = GetComponent<Animator>();
        if (m_animator != null) {
            _hasAnimator = true;
        }
    }

    protected virtual void OnEnable() {
        IsDead = false;
        if (m_collider != null) {
            m_collider.enabled = true;
        }

        if (m_rb != null) {
            m_rb.isKinematic = false;
        }
    }

    public virtual void TakeDamage() => Die();

    public virtual void Die() {
        if (IsDead) {
            return;
        }

        IsDead = true;

        if (GameManager.Instance != null) {
            GameManager.Instance.AddScore(_scoreValue);
        }

        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
            m_rb.angularVelocity = 0f;
            m_rb.isKinematic = true;
        }

        if (m_collider != null) {
            m_collider.enabled = false;
        }

        if (_hasAnimator && HasAnimatorParameter(_animatorDesHash, AnimatorControllerParameterType.Trigger)) {
            m_animator.SetTrigger(_animatorDesHash);
        }
        else {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// アニメーションイベントから呼び出され、オブジェクトを非アクティブ化します。
    /// </summary>
    public void OnDefeatAnimationEnd() {
        if (gameObject != null) {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// アニメーターに指定されたパラメータが存在するか確認します。
    /// </summary>
    /// <param name="paramHash">ハッシュ化されたパラメータ名</param>
    /// <param name="paramType">パラメータの型</param>
    /// <returns>存在する場合はtrue、しない場合はfalse</returns>
    protected bool HasAnimatorParameter(int paramHash, AnimatorControllerParameterType paramType) {
        if (!_hasAnimator || m_animator.runtimeAnimatorController == null) {
            return false;
        }

        foreach (AnimatorControllerParameter param in m_animator.parameters) {
            if (param.nameHash == paramHash && param.type == paramType) {
                return true;
            }
        }
        return false;
    }
}
