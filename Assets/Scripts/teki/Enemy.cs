using UnityEngine;
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

/// <summary>
/// 敵キャラクターのベースクラス。
/// 共通のプロパティと死亡処理を提供します。
/// </summary>
public class Enemy : MonoBehaviour
{
    [Header("効果音設定")]
    public AudioClip knockDownClip; // 敵が倒された時の効果音

    protected AudioSource audioSource; // 効果音を再生するためのAudioSource
    protected Animator m_animator;    // 敵のアニメーター
    protected Rigidbody2D m_rb;      // 敵のRigidbody2D
    protected Collider2D m_collider; // 敵のCollider2D

    // ★修正点: Collider2Dを外部から参照できるようにするプロパティ (public)★
    public Collider2D EnemyCollider => m_collider;

    protected bool m_isDead = false;  // 敵が死亡しているかどうかのフラグ

    // 死亡状態を取得するプロパティ (外部から参照できるように)
    public bool IsDead => m_isDead;


    // スクリプトが有効になった最初のフレームで呼び出される
    protected virtual void Awake() // StartからAwakeに変更 (コンポーネント取得はAwakeで行うのが一般的)
    {
        // 必要なコンポーネントを取得
        audioSource = GetComponent<AudioSource>();
        m_animator = GetComponent<Animator>();
        m_rb = GetComponent<Rigidbody2D>();
        m_collider = GetComponent<Collider2D>(); // Collider2Dも取得

        if (audioSource == null)
        {
            Debug.LogWarning($"Enemy: AudioSourceコンポーネントが'{gameObject.name}'に見つかりません。追加します。");
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        if (m_animator == null)
        {
            Debug.LogWarning($"Enemy: Animatorコンポーネントが'{gameObject.name}'に見つかりません。死亡アニメーションを再生できません。");
        }
        if (m_rb == null)
        {
            Debug.LogWarning($"Enemy: Rigidbody2Dコンポーネントが'{gameObject.name}'に見つかりません。物理的な反応が期待通りでない可能性があります。");
        }
        if (m_collider == null)
        {
            Debug.LogError($"Enemy: Collider2Dコンポーネントが'{gameObject.name}'に見つかりません。衝突判定ができません。");
        }
    }

    // 敵が死亡した際の処理
    public virtual void Die()
    {
        // 既に死亡している場合は処理を終了
        if (m_isDead) return;

        m_isDead = true; // 死亡フラグを立てる
        Debug.Log($"Enemy: '{gameObject.name}' が倒されました！");

        // 効果音の再生
        if (knockDownClip != null && audioSource != null)
        {
            audioSource.PlayOneShot(knockDownClip);
        }

        // アニメーションの切り替え
        if (m_animator != null)
        {
            SetAnimatorBoolSafe("des", true); // "des" パラメータをtrueに設定（死亡アニメーションなど）
            SetAnimatorBoolSafe("fly", false); // "fly" パラメータをfalseに設定
            SetAnimatorBoolSafe("run", false); // "run" パラメータをfalseに設定
        }

        // 物理挙動の停止
        if (m_rb != null)
        {
            m_rb.velocity = Vector2.zero; // 速度をリセット
            m_rb.simulated = false;      // 物理演算を無効化
        }

        // コライダーを無効化して、これ以上プレイヤーと衝突しないようにする
        if (m_collider != null)
        {
            m_collider.enabled = false;
        }

        // 0.5秒後に自身を破棄するメソッドを呼び出す
        Invoke(nameof(DestroySelf), 0.5f);
    }

    // ゲームオブジェクト自身を破棄する
    protected virtual void DestroySelf()
    {
        Destroy(gameObject);
        Debug.Log($"Enemy: '{gameObject.name}' オブジェクトを破棄しました。");
    }

    // AnimatorのBoolパラメータを安全に設定するヘルパーメソッド
    // (指定したパラメータが存在し、型がBoolの場合のみ設定する)
    protected void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (m_animator == null) return; // アニメーターがなければ何もしない

        // アニメーターの全パラメータを走査
        foreach (var p in m_animator.parameters)
        {
            // 名前と型が一致するパラメータが見つかった場合
            if (p.name == paramName && p.type == AnimatorControllerParameterType.Bool)
            {
                m_animator.SetBool(paramName, value); // パラメータを設定
                return; // 処理を終了
            }
        }
    }
}
