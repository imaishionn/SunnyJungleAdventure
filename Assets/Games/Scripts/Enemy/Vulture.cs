using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵キャラクター「ハゲタカ」のAIと動作を制御するスクリプト。
/// プレイヤーを検知すると追跡を開始し、向きを反転させながら飛行します。
/// </summary>
public class Vulture : Enemy {
    /// <summary>
    /// プレイヤーを識別するためのタグ
    /// </summary>
    private const string PLAYER_TAG = "Player";

    private static class AnimatorParams {
        /// <summary>
        /// 飛行アニメーションのパラメータ名
        /// </summary>
        public const string Fly = "fly";
    }

    [Header("プレイヤーを検知する範囲"), SerializeField]
    private float _detectRange = 5f;

    [Header("プレイヤー追跡時の移動速度"), SerializeField]
    private float _flySpeed = 5f;

    /// <summary>
    /// プレイヤーを格納するTransform 
    /// </summary>
    private Transform _player;

    /// <summary>
    /// 飛行中かどうかの状態
    /// </summary>
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

    protected void FixedUpdate() {
        if (IsDead || _player == null || m_rb == null) {
            m_rb.velocity = Vector2.zero;
            if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
                m_animator.SetBool(AnimatorParams.Fly, false);
            }
            _isFlying = false;
            return;
        }

        HandlePlayerDetection();
    }

    /// <summary>
    /// アニメーターに指定されたパラメータが存在するか確認します。
    /// </summary>
    /// <param name="paramName">パラメータ名</param>
    /// <param name="paramType">パラメータの型</param>
    /// <returns>存在する場合はtrue、しない場合はfalse</returns>
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

    private void FindPlayer() {
        var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null) {
            _player = playerObj.transform;
        }
        else {
            Debug.LogWarning("Vulture: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
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
        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Fly, AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool(AnimatorParams.Fly, false);
        }
    }

    private void HandlePlayerDetection() {
        float distance = Vector2.Distance(transform.position, _player.position);

        if (distance < _detectRange) {
            FlyToPlayer();
            SetFlyingState(true);
        }
        else {
            m_rb.velocity = Vector2.zero;
            SetFlyingState(false);
        }
    }

    private void FlyToPlayer() {
        Vector2 direction = (_player.position - transform.position).normalized;
        m_rb.velocity = direction * _flySpeed;

        if (direction.x > 0 && transform.localScale.x < 0 || direction.x < 0 && transform.localScale.x > 0) {
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
}
