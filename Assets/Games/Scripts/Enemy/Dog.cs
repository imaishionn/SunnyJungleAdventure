using UnityEngine;

/// <summary>
/// 敵キャラクター「犬」のAIと動作を制御するスクリプトです。
/// プレイヤーを検知すると地面を追跡し、崖の手前で停止または反転します。
/// Enemyクラスを継承しています。
/// </summary>
public class Dog : Enemy {
    // タグ名とアニメーターパラメーターの定数定義
    private const string PLAYER_TAG = "Player";
    private const string RUN_TRIGGER = "run";

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プレイヤー検知設定")]
    [Tooltip("プレイヤーを検知する半径")]
    [SerializeField]
    private float _detectRange = 5f;

    [Header("移動設定")]
    [Tooltip("プレイヤー追跡時の移動速度")]
    [SerializeField]
    private float _runSpeed = 3f;
    [Tooltip("プレイヤーが真上にいると判断する水平距離の許容範囲")]
    [SerializeField]
    private float _flipDeadZone = 0.2f;

    [Header("接地・崖の検知設定")]
    [Tooltip("地面を判定するための子オブジェクトのTransform")]
    [SerializeField]
    private Transform _groundCheck;
    [Tooltip("地面として認識するレイヤー")]
    [SerializeField]
    private LayerMask _groundLayer;
    [Tooltip("地面判定のOverlapCircleの半径")]
    [SerializeField]
    private float _groundCheckRadius = 0.2f;
    [Tooltip("進行方向の先に崖があるかをチェックする距離")]
    [SerializeField]
    private float _groundAheadCheckDistance = 0.5f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Transform _player;
    private bool _isPlayerDetected = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        base.Awake();
        FindPlayer();
        SetupRigidbody();
    }

    private void FixedUpdate() {
        if (IsDead || _player == null) {
            StopMovement();
            return;
        }
        DetectAndTrackPlayer();
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    private void FindPlayer() {
        var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (playerObj != null) {
            _player = playerObj.transform;
        }
        else {
            Debug.LogWarning($"Dog: '{PLAYER_TAG}'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
        }
    }

    private void SetupRigidbody() {
        if (m_rb != null) {
            m_rb.gravityScale = 1f;
        }
    }

    private void StopMovement() {
        if (m_rb != null) {
            m_rb.velocity = Vector2.zero;
        }
    }

    private void DetectAndTrackPlayer() {
        if (!_isPlayerDetected) {
            float distance = Vector2.Distance(transform.position, _player.position);
            if (distance < _detectRange) {
                _isPlayerDetected = true;
                if (m_animator != null && HasAnimatorParameter(RUN_TRIGGER, AnimatorControllerParameterType.Trigger)) {
                    m_animator.SetTrigger(RUN_TRIGGER);
                }
            }
        }
        if (_isPlayerDetected) {
            HandleMovement();
        }
    }

    private void HandleMovement() {
        if (!IsGrounded()) {
            if (m_rb != null) {
                m_rb.velocity = new Vector2(0, m_rb.velocity.y);
            }
            return;
        }
        float horizontalDistance = _player.position.x - transform.position.x;

        if (Mathf.Abs(horizontalDistance) < _flipDeadZone) {
            if (m_rb != null) {
                m_rb.velocity = new Vector2(0, m_rb.velocity.y);
            }
        }
        else {
            float directionX = Mathf.Sign(horizontalDistance);
            var moveDirection = new Vector2(directionX, 0);
            if (IsGroundAhead(moveDirection)) {
                if (m_rb != null) {
                    m_rb.velocity = new Vector2(moveDirection.x * _runSpeed, m_rb.velocity.y);
                }
            }
            else {
                if (m_rb != null) {
                    m_rb.velocity = new Vector2(0, m_rb.velocity.y);
                }
            }
            FlipSprite(directionX);
        }
    }

    private bool IsGrounded() {
        if (_groundCheck == null || _groundLayer == 0) {
            Debug.LogWarning("IsGrounded: GroundCheck Transform または GroundLayerが設定されていません。", this);
            return false;
        }
        Collider2D collider = Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
        return collider != null;
    }

    private bool IsGroundAhead(Vector2 direction) {
        if (_groundCheck == null || _groundLayer == 0) {
            return false;
        }
        Vector2 checkPosition = (Vector2)_groundCheck.position + direction * 0.1f;
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

    public override void TakeDamage() {
        if (IsDead) {
            return;
        }
        Die();
    }
}
