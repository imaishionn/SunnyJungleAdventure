using System;
using System.Linq;
using UnityEngine;

/// <summary>
/// 敵キャラクター「犬」のAIと動作を制御するスクリプト。
/// プレイヤーを検知すると地面を追跡し、崖の手前で停止または反転します。
/// </summary>
public class Dog : Enemy {
    private const string PLAYER_TAG = "Player";
    private const string RUN_TRIGGER = "run";

    [Header("プレイヤーを検知する範囲"), SerializeField]
    private float _detectRange = 5f; 

    [Header("プレイヤー追跡時の移動速度"), SerializeField]
    private float _runSpeed = 3f; 

    [Header("プレイヤーがこの範囲内に入ったら停止"), SerializeField]
    private float _flipDeadZone = 0.2f; 

    [Header("地面をチェックする位置のTransform"), SerializeField]
    private Transform _groundCheck;

    [Header("地面として認識するレイヤー"), SerializeField]
    private LayerMask _groundLayer; 

    [Header("地面チェックの円の半径"), SerializeField]
    private float _groundCheckRadius = 0.2f; 

    [Header("進行方向の崖をチェックする距離"), SerializeField]
    private float _groundAheadCheckDistance = 0.5f; 

    private Transform _player;
    private bool _isPlayerDetected = false;

    protected override void Awake() {
        base.Awake();
        FindPlayer();
        SetupRigidbody();
    }

    private void FixedUpdate() {
        if (IsDead || _player == null || m_rb == null) {
            m_rb.velocity = Vector2.zero;
            return;
        }
        HandleMovement();
    }

    public override void TakeDamage() {
        if (IsDead) {
            return;
        }
        Die();
    }

    private void FindPlayer() {
        var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null) {
            _player = playerObj.transform;
        }
        else {
            Debug.LogWarning("Dog: 'Player'タグのゲームオブジェクトが見つかりません。プレイヤー追跡は無効になります。", this);
        }
    }

    private void SetupRigidbody() {
        if (m_rb != null) {
            m_rb.gravityScale = 1f;
        }
    }

    private void HandleMovement() {
        float horizontalDistance = _player.position.x - transform.position.x;
        float distanceToPlayer = Vector2.Distance(transform.position, _player.position);

        if (!_isPlayerDetected && distanceToPlayer < _detectRange) {
            _isPlayerDetected = true;
            if (m_animator != null && HasAnimatorParameter(RUN_TRIGGER, AnimatorControllerParameterType.Trigger)) {
                m_animator.SetTrigger(RUN_TRIGGER);
            }
        }

        if (!_isPlayerDetected) {
            return;
        }

        if (!IsGrounded()) {
            m_rb.velocity = new Vector2(0, m_rb.velocity.y);
            return;
        }

        if (Mathf.Abs(horizontalDistance) < _flipDeadZone) {
            m_rb.velocity = new Vector2(0, m_rb.velocity.y);
        }
        else {
            float directionX = Mathf.Sign(horizontalDistance);
            var moveDirection = new Vector2(directionX, 0);

            m_rb.velocity = IsGroundAhead(moveDirection) ? new Vector2(moveDirection.x * _runSpeed, m_rb.velocity.y) : new Vector2(0, m_rb.velocity.y);

            FlipSprite(directionX);
        }
    }

    private bool IsGrounded() {
        if (_groundCheck == null || _groundLayer == 0) {
            Debug.LogWarning("IsGrounded: GroundCheck TransformまたはGroundLayerが設定されていません。", this);
            return false;
        }
        return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer) != null;
    }

    private bool IsGroundAhead(Vector2 direction) {
        if (_groundCheck == null || _groundLayer == 0) {
            return false;
        }
        Vector2 checkPosition = (Vector2)_groundCheck.position + direction * _groundAheadCheckDistance;
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, _groundAheadCheckDistance, _groundLayer);
        Debug.DrawRay(checkPosition, Vector2.down * _groundAheadCheckDistance, Color.red);
        return hit.collider != null;
    }

    private void FlipSprite(float directionX) {
        Vector3 scale = transform.localScale;
        if ((directionX > 0 && scale.x < 0) || (directionX < 0 && scale.x > 0)) {
            scale.x *= -1;
            transform.localScale = scale;
        }
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
