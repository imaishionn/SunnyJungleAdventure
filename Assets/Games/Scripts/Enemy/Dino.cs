using UnityEngine;

/// <summary>
/// 敵キャラクター「恐竜」のAIと動作を制御するスクリプトです。
/// 地面をパトロールし、指定された移動範囲の端に到達すると向きを変えます。
/// Enemyクラスを継承しています。
/// </summary>
public class Dino : Enemy {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("移動設定")]
    [Tooltip("パトロール時の移動速度")]
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("初期位置からのパトロール範囲")]
    [SerializeField] private float patrolRange = 5f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Vector2 initialPosition;
    private int moveDirection = 1; // 1:右, -1:左

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        // 親クラスのAwake()を呼び出し、基盤となる初期化を行う
        base.Awake();

        // 初期位置を保存
        initialPosition = transform.position;

        // Rigidbody2Dの設定
        if(m_rb != null) {
            m_rb.gravityScale = 1f;
            m_rb.constraints = RigidbodyConstraints2D.FreezeRotation; // 物理演算で回転しないようにする
        }
        else {
            Debug.LogError("Dino: Rigidbody2Dがアタッチされていません。移動できません。",this);
        }
    }

    protected void FixedUpdate() {
        // 死亡状態の場合は処理を停止
        if(IsDead) return;

        // ------------------
        // 移動方向の決定
        // ------------------
        // パトロール範囲の端に到達した場合、向きを変える
        if((moveDirection == 1 && transform.position.x > initialPosition.x + patrolRange) ||
            (moveDirection == -1 && transform.position.x < initialPosition.x - patrolRange)) {
            moveDirection *= -1; // 進行方向を反転
            FlipSprite();        // スプライトの向きを反転
        }

        // ------------------
        // 移動処理
        // ------------------
        // 速度を直接操作して移動
        m_rb.velocity = new Vector2(moveDirection * moveSpeed,m_rb.velocity.y);

        // ------------------
        // アニメーション設定
        // ------------------
        // 常に走るアニメーションを再生
        if(m_animator != null && HasAnimatorParameter("run",AnimatorControllerParameterType.Bool)) {
            m_animator.SetBool("run",true);
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// キャラクターのx軸スケールを反転させ、向きを変えます。
    /// </summary>
    private void FlipSprite() {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
