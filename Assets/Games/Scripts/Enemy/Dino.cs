using UnityEngine;

/// <summary>
/// 敵キャラクター「恐竜」のAIと動作を制御するスクリプトです。
/// 地面をパトロールし、指定された移動範囲の端に到達すると向きを変えます。
/// Enemyクラスを継承しています。
/// </summary>
public class Dino : Enemy {
    // アニメーターパラメーターの定数定義
    private static class AnimatorParams {
        public const string Run = "run";
    }

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("移動設定")]
    [Tooltip("パトロール時の移動速度")]
    [SerializeField]
    private float _moveSpeed = 3f;
    [Tooltip("初期位置からのパトロール範囲")]
    [SerializeField]
    private float _patrolRange = 5f;

    [Header("壁検出設定")]
    [Tooltip("壁のレイヤー")]
    [SerializeField] private LayerMask _wallLayer;
    [Tooltip("壁を検出するためのレイキャストの長さ")]
    [SerializeField] private float _wallCheckDistance = 0.5f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Vector2 _initialPosition;
    private int _moveDirection = 1; // 1:右, -1:左

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        base.Awake();
        _initialPosition = transform.position;
        SetupRigidbody();
    }

    private void FixedUpdate() {
        if (IsDead) {
            return;
        }

        HandlePatrolMovement();
        AnimateMovement();
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    private void SetupRigidbody() {
        if (m_rb != null) {
            m_rb.gravityScale = 1f;
            m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
        else {
            Debug.LogError("Dino: Rigidbody2Dがアタッチされていません。移動できません。", this);
        }
    }

    private void HandlePatrolMovement() {
        // 物理的な壁または設定された移動範囲の端に到達したかチェック
        bool needsToFlip = CheckForWall() || CheckPatrolRange();

        if (needsToFlip) {
            _moveDirection *= -1; // 進行方向を反転
            FlipSprite(); // スプライトの向きを反転
        }

        // 速度を直接操作して移動
        if (m_rb != null) {
            m_rb.velocity = new Vector2(_moveDirection * _moveSpeed, m_rb.velocity.y);
        }
    }

    private void AnimateMovement() {
        // 常に走るアニメーションを再生
        if (m_animator != null && HasAnimatorParameter(AnimatorParams.Run, AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool(AnimatorParams.Run, true);
        }
    }

    /// <summary>
    /// 前方に壁があるかをレイキャストでチェックする
    /// </summary>
    private bool CheckForWall() {
        Vector2 raycastOrigin = transform.position;
        Vector2 raycastDirection = Vector2.right * _moveDirection;

        RaycastHit2D hit = Physics2D.Raycast(raycastOrigin, raycastDirection, _wallCheckDistance, _wallLayer);

        // デバッグ用の可視化
        Debug.DrawRay(raycastOrigin, raycastDirection * _wallCheckDistance, hit ? Color.red : Color.green);

        return hit.collider != null;
    }

    /// <summary>
    /// パトロール範囲の端に到達したかをチェックする
    /// </summary>
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
