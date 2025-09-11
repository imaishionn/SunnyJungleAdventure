using System.Collections;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵キャラクター「コウモリ」のAIと動作を制御するスクリプト。
/// プレイヤーを検知すると追跡し、ダメージを受けると死亡アニメーションを再生します。
/// </summary>
public class Bat : Enemy {
    private static class AnimatorParams {
        public const string Fly = "fly";
        public const string Death = "des";
    }

    private const string PLAYER_TAG = "Player";

    [Header("プレイヤーを検知する範囲"), SerializeField]
    private float _detectRange = 5f;

    [Header("プレイヤー追跡時の移動速度"), SerializeField]
    private float _flySpeed = 5f;

    [Header("死亡アニメーションの再生時間"), SerializeField]
    private float _deathAnimationDuration = 1.0f; 

    private Transform _player;
    private bool _isFlying = false;

    protected override void Awake() {
        base.Awake();
        FindPlayer();
        SetupRigidbody();
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

        float distance = Vector2.Distance(transform.position, _player.position);

        if (distance < _detectRange) {
            FlyToPlayer();
            SetFlyingState(true);
        }
        else {
            StopMovement();
            SetFlyingState(false);
        }
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

    private void FindPlayer() {
        if (_player == null) {
            var playerObject = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObject != null) {
                _player = playerObject.transform;
            }
            else {
                Debug.LogWarning("Bat: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能は無効になります。", this);
            }
        }
    }

    private void SetupRigidbody() {
        if (m_rb != null) {
            m_rb.gravityScale = 0f;
            m_rb.drag = 1f;
        }
    }

    private void ResetState() {
        IsDead = false;
        _isFlying = false;
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }
        SetFlyingState(false);
    }

    private void StopMovement() {
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
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

    private void FlyToPlayer() {
        Vector2 direction = (_player.position - transform.position).normalized;
        m_rb.velocity = direction * _flySpeed;

        if ((direction.x > 0 && transform.localScale.x < 0) || (direction.x < 0 && transform.localScale.x > 0)) {
            FlipSprite();
        }
    }

    private void FlipSprite() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    private IEnumerator DeactivateAfterDelay(float delay) {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType) {
        if (m_animator == null) {
            return false;
        }

        foreach (AnimatorControllerParameter param in m_animator.parameters) {
            if (param.nameHash == Animator.StringToHash(paramName) && param.type == paramType) {
                return true;
            }
        }
        return false;
    }
}
