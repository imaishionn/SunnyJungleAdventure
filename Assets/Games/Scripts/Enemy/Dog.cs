using UnityEngine;

/// <summary>
/// 敵キャラクター「犬」のAIと動作を制御するスクリプトです。
/// プレイヤーを検知すると地面を追跡し、崖の手前で停止または反転します。
/// Enemyクラスを継承しています。
/// </summary>
public class Dog : Enemy {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プレイヤー検知設定")]
    [Tooltip("プレイヤーを検知する半径")]
    [SerializeField] private float detectRange = 5f;

    [Header("移動設定")]
    [Tooltip("プレイヤー追跡時の移動速度")]
    [SerializeField] private float runSpeed = 3f;
    [Tooltip("プレイヤーが真上にいると判断する水平距離の許容範囲")]
    [SerializeField] private float m_flipDeadZone = 0.2f;

    [Header("アニメーション設定")]
    [Tooltip("走るアニメーションを再生するためのトリガー名")]
    [SerializeField] private string runAnimationTrigger = "run";

    [Header("接地・崖の検知設定")]
    [Tooltip("地面を判定するための子オブジェクトのTransform")]
    [SerializeField] private Transform groundCheck;
    [Tooltip("地面として認識するレイヤー")]
    [SerializeField] private LayerMask groundLayer;
    [Tooltip("地面判定のOverlapCircleの半径")]
    [SerializeField] private float groundCheckRadius = 0.2f;
    [Tooltip("進行方向の先に崖があるかをチェックする距離")]
    [SerializeField] private float groundAheadCheckDistance = 0.5f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Transform m_player;
    private bool m_isPlayerDetected = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        // 親クラスのAwake()を呼び出し、基盤となる初期化を行う
        base.Awake();

        // 'Player'タグを持つオブジェクトを探し、見つかればTransformを取得
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) {
            m_player = playerObj.transform;
        }
        else {
            Debug.LogWarning("Dog: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
        }

        // Rigidbody2Dの設定
        if (m_rb != null) {
            m_rb.gravityScale = 1f; // Dogが地面を歩くように重力を有効化
        }
    }

    protected void FixedUpdate() {
        // 死亡状態またはプレイヤーが見つからない場合は処理を停止
        if (IsDead || m_player == null) {
            if (m_rb != null) m_rb.velocity = Vector2.zero;
            return;
        }

        // プレイヤーを検知するまで待機
        if (!m_isPlayerDetected) {
            float distance = Vector2.Distance(transform.position, m_player.position);
            if (distance < detectRange) {
                m_isPlayerDetected = true;
                // プレイヤー検知後、走るアニメーションを開始
                if (m_animator != null && HasAnimatorParameter(runAnimationTrigger, AnimatorControllerParameterType.Trigger)) {
                    m_animator.SetTrigger(runAnimationTrigger);
                }
            }
        }

        // プレイヤーを検知した場合の追跡処理
        if (m_isPlayerDetected) {
            bool isGrounded = IsGrounded();

            if (isGrounded) {
                // プレイヤーとの水平距離を計算
                float horizontalDistance = m_player.position.x - transform.position.x;

                // プレイヤーがデッドゾーン内にいる場合、向きを変えずに停止
                if (Mathf.Abs(horizontalDistance) < m_flipDeadZone) {
                    if (m_rb != null) {
                        m_rb.velocity = new Vector2(0, m_rb.velocity.y);
                    }
                }
                else // プレイヤーがデッドゾーン外にいる場合、追跡と向きの更新を行う
                {
                    // プレイヤーが右にいるか左にいるかで移動方向を決定
                    Vector2 direction = (horizontalDistance > 0) ? Vector2.right : Vector2.left;

                    // 進行方向の先に足場があるかチェック
                    if (IsGroundAhead(direction)) {
                        // 足場があれば移動
                        if (m_rb != null) {
                            m_rb.velocity = new Vector2(direction.x * runSpeed, m_rb.velocity.y);
                        }
                    }
                    else {
                        // 足場の端なので停止
                        if (m_rb != null) {
                            m_rb.velocity = new Vector2(0, m_rb.velocity.y);
                        }
                    }

                    // キャラクターの向きを更新
                    FlipSprite(direction.x);
                }
            }
            else {
                // 地面にいない場合は横移動を停止
                if (m_rb != null) {
                    m_rb.velocity = new Vector2(0, m_rb.velocity.y);
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// 地面に接地しているかを判定します。
    /// </summary>
    private bool IsGrounded() {
        if (groundCheck == null || groundLayer == 0) {
            Debug.LogWarning("IsGrounded: GroundCheck Transform または GroundLayerが設定されていません。", this);
            return false;
        }
        // 指定された位置と半径で、地面レイヤーのコライダーを探す
        Collider2D collider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return collider != null;
    }

    /// <summary>
    /// 進行方向の先に足場があるかを判定します。
    /// </summary>
    /// <param name="direction">移動方向</param>
    private bool IsGroundAhead(Vector2 direction) {
        if (groundCheck == null || groundLayer == 0) {
            return false;
        }
        // groundCheckの位置から進行方向に向かって少しずらした位置を始点とする
        Vector2 checkPosition = (Vector2)groundCheck.position + direction * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(checkPosition, Vector2.down, groundAheadCheckDistance, groundLayer);

        // デバッグ用にRayを可視化
        Debug.DrawRay(checkPosition, Vector2.down * groundAheadCheckDistance, Color.red);

        return hit.collider != null;
    }

    /// <summary>
    /// キャラクターのx軸スケールを反転させ、向きを変えます。
    /// </summary>
    private void FlipSprite(float directionX) {
        Vector3 scale = transform.localScale;
        // 進行方向が右で現在のスケールが左向き（x < 0）なら反転
        if (directionX > 0 && scale.x < 0) {
            scale.x *= -1;
        }
        // 進行方向が左で現在のスケールが右向き（x > 0）なら反転
        else if (directionX < 0 && scale.x > 0) {
            scale.x *= -1;
        }
        transform.localScale = scale;
    }

    /// <summary>
    /// ダメージを受けた際の処理。
    /// 親クラスのTakeDamage()をオーバーライドしています。
    /// </summary>
    public override void TakeDamage() {
        if (IsDead) return;
        Die(); // 親クラスのDie()メソッドを呼び出す
    }
}
