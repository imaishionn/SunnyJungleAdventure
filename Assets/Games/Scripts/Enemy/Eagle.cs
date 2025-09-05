using System.Collections;
using UnityEngine;

/// <summary>
/// 敵キャラクター「ワシ」のAIと動作を制御するスクリプトです。
/// プレイヤーを検知すると追跡し、ダメージを受けるとHPが減り、点滅します。
/// Enemyクラスを継承しています。
/// </summary>
public class Eagle : Enemy {
    // タグ名とアニメーターパラメーターの定数定義
    private const string PLAYER_TAG = "Player";
    private static class AnimatorParams {
        public const string Fly = "fly";
        public const string Hurt = "hurt";
    }

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

    [Header("ダメージ時設定")]
    [Tooltip("ダメージを受けた際の点滅時間")]
    [SerializeField]
    private float _flashDuration = 1f;
    [Tooltip("点滅の間隔 (表示/非表示の切り替え時間)")]
    [SerializeField]
    private float _flashInterval = 0.1f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    [SerializeField]
    private Transform _player;
    private bool _isFlying = false;
    private int _hp = 3;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _flashCoroutine;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        base.Awake();
        FindPlayer();
        SetupComponents();
    }

    protected override void OnEnable() {
        base.OnEnable();
        ResetState();
    }

    protected void FixedUpdate() {
        if (IsDead || _player == null || m_rb == null) {
            StopAndReset();
            return;
        }

        HandlePlayerDetection();
    }

    // ----------------------------------------------------------------------------------------------------
    // パブリックメソッドとプライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    public override void TakeDamage() {
        if (IsDead) {
            return;
        }

        _hp--;
        Debug.Log($"Eagle took damage. HP: {_hp}");

        if (_hp <= 0) {
            base.Die();
        }
        else {
            if (m_animator != null && HasAnimatorParameter(AnimatorParams.Hurt, AnimatorControllerParameterType.Trigger)) {
                m_animator.SetTrigger(AnimatorParams.Hurt);
            }
            if (_flashCoroutine != null) {
                StopCoroutine(_flashCoroutine);
            }
            _flashCoroutine = StartCoroutine(FlashRoutine());
        }
    }

    private void FindPlayer() {
        if (_player == null) {
            var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null) {
                _player = playerObj.transform;
            }
            else {
                Debug.LogWarning($"Eagle: '{PLAYER_TAG}'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
            }
        }
    }

    private void SetupComponents() {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        if (_spriteRenderer == null) {
            Debug.LogError("Eagle: SpriteRendererがアタッチされていません。", this);
        }

        if (m_rb != null) {
            m_rb.gravityScale = 0f;
            m_rb.drag = 1f;
        }
    }

    private void ResetState() {
        IsDead = false;
        _isFlying = false;
        _hp = 3;

        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }

        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool(AnimatorParams.Fly, false);
        }

        if (_spriteRenderer != null) {
            _spriteRenderer.enabled = true;
        }
    }

    private void StopAndReset() {
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }
        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool(AnimatorParams.Fly, false);
        }
        _isFlying = false;
    }

    private void HandlePlayerDetection() {
        float distance = Vector2.Distance(transform.position, _player.position);

        if (distance < _detectRange) {
            FlyToPlayer();
            if (!_isFlying) {
                if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
                    m_animator.SetBool(AnimatorParams.Fly, true);
                }
                _isFlying = true;
            }
        }
        else {
            if (_isFlying) {
                if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
                    m_animator.SetBool(AnimatorParams.Fly, false);
                }
                _isFlying = false;
            }
            if (m_rb != null) {
                m_rb.velocity = Vector2.zero;
            }
        }
    }

    private void FlyToPlayer() {
        Vector3 direction = (_player.position - transform.position).normalized;
        m_rb.velocity = direction * _flySpeed;

        if (direction.x > 0) {
            FlipSprite(true);
        }
        else if (direction.x < 0) {
            FlipSprite(false);
        }
    }

    private void FlipSprite(bool isFacingRight) {
        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private IEnumerator FlashRoutine() {
        float flashEndTime = Time.time + _flashDuration;
        while (Time.time < flashEndTime) {
            if (_spriteRenderer != null) {
                _spriteRenderer.enabled = !_spriteRenderer.enabled;
            }
            yield return new WaitForSeconds(_flashInterval);
        }

        if (_spriteRenderer != null) {
            _spriteRenderer.enabled = true;
        }
    }
}
