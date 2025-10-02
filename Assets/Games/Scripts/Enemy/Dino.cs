using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵キャラクター「恐竜」のAIと動作を制御するスクリプト。
/// 地面をパトロールし、指定された移動範囲の端または壁に到達すると向きを変えます。
/// </summary>
public class Dino : Enemy {
    /// <summary>
    /// アニメーターパラメーターの定数
    /// </summary>
    private static class AnimatorParams {
        /// <summary>
        /// 走るアニメーションのパラメータ名
        /// </summary>
        public const string Run = "run";
    }

    [Header("パトロール時の設定"), SerializeField]
    private float _moveSpeed = 3f;

    [Header("パトロールの範囲"), SerializeField]
    private float _patrolRange = 5f;

    [Header("壁の認識"), SerializeField]
    private LayerMask _wallLayer;

    [Header("壁をチェックする距離"), SerializeField]
    private float _wallCheckDistance = 0.5f;

    /// <summary>
    /// 初期位置
    /// </summary>
    private Vector2 _initialPosition;

    /// <summary>
    /// 現在の移動方向（1: 右, -1: 左）
    /// </summary>
    private int _moveDirection = 1; 

    protected override void Awake() {
        base.Awake();
        _initialPosition = transform.position;
        SetupRigidbody();
    }

    protected override void OnEnable() {
        base.OnEnable();
        _moveDirection = 1; // オブジェクトが再利用される際に、初期方向をリセット
    }

    private void FixedUpdate() {
        if (IsDead) {
            if (m_rb != null) {
                m_rb.velocity = Vector2.zero; // 死亡時に動きを止める
                SetRunAnimation(false); // 死亡時はアニメーションを停止
            }
            return;
        }

        HandlePatrolMovement();
    }

    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType) => m_animator != null && m_animator.parameters.Any(param => param.nameHash == Animator.StringToHash(paramName) && param.type == paramType);

    private void SetupRigidbody() {
        if (m_rb != null) {
            m_rb.gravityScale = 1f;
            m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else {
            Debug.LogError("Dino: Rigidbody2Dがアタッチされていません。移動が機能しません。", this);
        }
    }

    private void SetRunAnimation(bool isRunning) {
        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Run, AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool(AnimatorParams.Run, isRunning);
        }
    }

    private void HandlePatrolMovement() {
        bool needsToFlip = CheckForWall() || CheckPatrolRange();

        if (needsToFlip) {
            _moveDirection *= -1;
            FlipSprite();
        }

        if (m_rb != null) {
            float currentMoveSpeed = _moveDirection * _moveSpeed;
            m_rb.velocity = new Vector2(currentMoveSpeed, m_rb.velocity.y);

            // ★修正: 速度が0でない場合にアニメーションを再生
            if (Mathf.Abs(currentMoveSpeed) > 0.01f) {
                SetRunAnimation(true);
            }
            else {
                SetRunAnimation(false);
            }
        }
    }

    private bool CheckForWall() {
        Vector2 raycastOrigin = transform.position;
        Vector2 raycastDirection = Vector2.right * _moveDirection;

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, _wallCheckDistance, _wallLayer);
        Debug.DrawRay(raycastOrigin, raycastDirection * _wallCheckDistance, hit.collider != null ? Color.red : Color.green);
        return hit.collider != null;
    }

    private bool CheckPatrolRange() {
        bool atRightEdge = _moveDirection == 1 && transform.position.x > _initialPosition.x + _patrolRange;
        bool atLeftEdge = _moveDirection == -1 && transform.position.x < _initialPosition.x - _patrolRange;
        return atRightEdge || atLeftEdge;
    }

    private void FlipSprite() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
