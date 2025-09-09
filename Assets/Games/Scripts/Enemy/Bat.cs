using UnityEngine;

/// <summary>
/// 敵キャラクター「コウモリ」のAIと動作を制御するスクリプトです。
/// プレイヤーを検知すると追跡し、ダメージを受けると死亡アニメーションを再生します。
/// Enemyクラスを継承しています。
/// </summary>
public class Bat : Enemy {
    // アニメーターパラメーターの定数定義
    private static class AnimatorParams {
        public const string Fly = "fly";
        public const string Death = "des";
    }

    // プレイヤーのタグを定数で定義
    private const string PLAYER_TAG = "Player";

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プレイヤー検知設定")]
    [Tooltip("プレイヤーを検知する半径")]
    [SerializeField]
    private float _detectRange = 5f;


    [Header("移動設定")]
    [Tooltip("飛行速度")]
    [SerializeField]
    private float _flySpeed = 5f;

    [Header("死亡設定")]
    [Tooltip("死亡アニメーションの再生時間")]
    [SerializeField]
    private float _deathAnimationDuration = 1.0f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Transform _player;
    private bool _isFlying = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        base.Awake();
        FindPlayer();
        SetupRigidbody();
    }

    protected override void OnEnable() {
        base.OnEnable();
        InitializeStateForReuse();
    }

    private void FixedUpdate() {
        if (IsDead || _player == null || m_rb == null) {
            HandleInactiveState();
            return;
        }

        float distance = Vector2.Distance(transform.position, _player.position);

        if (distance < _detectRange) {
            FlyToPlayer();
            SetFlyingState(true);
        }
        else {
            SetFlyingState(false);
            m_rb.velocity = Vector2.zero;
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッドとプライベートメソッド
    // ----------------------------------------------------------------------------------------------------

    private void FindPlayer() {
        if (_player == null) {
            var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null) {
                _player = playerObj.transform;
            }
            else {
                Debug.LogWarning("Bat: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
            }
        }
    }

    private void SetupRigidbody() {
        if (m_rb != null) {
            m_rb.gravityScale = 0f;
            m_rb.drag = 1f;
        }
    }

    private void InitializeStateForReuse() {
        IsDead = false;
        _isFlying = false;
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }
        SetFlyingState(false); // アニメーションをリセット
    }

    private void HandleInactiveState() {
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }
        SetFlyingState(false);
    }

    private void SetFlyingState(bool isFlying) {
        if (_isFlying == isFlying) {
            return;
        }
        _isFlying = isFlying;
        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool(AnimatorParams.Fly, _isFlying);
        }
    }

    private void FlyToPlayer() {
        Vector2 direction = (_player.position - transform.position).normalized;
        m_rb.velocity = direction * _flySpeed;

        if (direction.x > 0 && transform.localScale.x < 0 || direction.x < 0 && transform.localScale.x > 0) {
            FlipSprite();
        }
    }

    private void FlipSprite() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public override void TakeDamage() {
        if (IsDead) {
            return;
        }
        base.Die();

        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Death, AnimatorControllerParameterType.Trigger)) {
            m_animator.SetTrigger(AnimatorParams.Death);
        }

        StartCoroutine(DeactivateAfterDelay(_deathAnimationDuration));
    }

    private System.Collections.IEnumerator DeactivateAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }
}
