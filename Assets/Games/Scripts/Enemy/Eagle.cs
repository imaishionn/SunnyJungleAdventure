using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵キャラクター「ワシ」のAIと動作を制御するスクリプト。
/// プレイヤーを検知すると追跡し、ダメージを受けると点滅します。
/// </summary>
public class Eagle : Enemy {
    private const string PLAYER_TAG = "Player";
    private static class AnimatorParams {
        public const string Fly = "fly";
        public const string Hurt = "hurt";
    }

    [Header("プレイヤーを検知する範囲"), SerializeField]
    private float _detectionRange = 5f;

    [Header("プレイヤー追跡時の移動速度"), SerializeField]
    private float _flySpeed = 5f;

    [Header("ダメージ"), SerializeField]
    private float _flashDuration = 1f; // 点滅する合計時間
    [Header("点滅の間隔"), SerializeField]
    private float _flashInterval = 0.1f; // 点滅の間隔

    private Transform _player;
    private bool _isFlying;
    private int _hp = 3;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _flashCoroutine;

    protected override void Awake() {
        base.Awake();
        FindPlayer();
        SetupComponents();
    }

    protected override void OnEnable() {
        base.OnEnable();
        ResetState();
    }

    private void FixedUpdate() {
        if (IsDead || _player == null || m_rb == null) {
            StopMovement();
            return;
        }
        HandlePlayerDetection();
    }

    public override void TakeDamage() {
        if (IsDead) {
            return;
        }

        _hp--;
        Debug.Log($"Eagle took damage. HP: {_hp}");

        if (_hp <= 0) {
            base.Die();
            return;
        }

        // ダメージを受けたときの処理
        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Hurt, AnimatorControllerParameterType.Trigger)) {
            m_animator.SetTrigger(AnimatorParams.Hurt);
        }

        if (_flashCoroutine != null) {
            StopCoroutine(_flashCoroutine);
        }
        _flashCoroutine = StartCoroutine(FlashRoutine());
    }

    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType) => m_animator != null && m_animator.parameters.Any(param => param.nameHash == Animator.StringToHash(paramName) && param.type == paramType);

    private void FindPlayer() {
        if (_player == null) {
            var playerObject = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObject != null) {
                _player = playerObject.transform;
            }
            else {
                Debug.LogWarning("Eagle: 'Player'タグのGameObjectが見つかりません。プレイヤー追跡機能は無効になります。", this);
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

        if (_flashCoroutine != null) {
            StopCoroutine(_flashCoroutine);
        }

        if (_spriteRenderer != null) {
            _spriteRenderer.enabled = true;
        }

        SetFlyingState(false);
    }

    private void StopMovement() {
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }
        SetFlyingState(false);
    }

    private void HandlePlayerDetection() {
        float distance = Vector2.Distance(transform.position, _player.position);
        if (distance < _detectionRange) {
            FlyToPlayer();
            SetFlyingState(true);
        }
        else {
            StopMovement();
            SetFlyingState(false);
        }
    }

    private void FlyToPlayer() {
        Vector3 direction = (_player.position - transform.position).normalized;
        m_rb.velocity = direction * _flySpeed;

        if ((direction.x > 0 && transform.localScale.x < 0) || (direction.x < 0 && transform.localScale.x > 0)) {
            FlipSprite();
        }
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

    private void FlipSprite() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
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
