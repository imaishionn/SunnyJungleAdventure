using UnityEngine;
using UnityEngine.SceneManagement; // シーン遷移のために追加
using Debug = UnityEngine.Debug; // Debugの曖昧な参照を解消するため

public class PlayerMove : MonoBehaviour
{
    [Header("移動設定")]
    public float moveSpeed = 5f; // 移動速度
    [SerializeField] private float jumpForce = 10f; // ジャンプ力
    [SerializeField, Header("最大ジャンプ回数")]
    private int maxJumps = 2; // 最大ジャンプ回数 (Inspectorで設定可能)
    private int currentJumps; // 現在のジャンプ回数

    // 敵を踏みつけた際の跳ね返り力
    [SerializeField, Header("踏みつけ跳ね返り力")]
    private float stompBounceForce = 7f; // 敵を踏んだ際にプレイヤーが跳ね上がる力

    // ★追加: 踏みつけ判定のYオフセット★
    [SerializeField, Header("踏みつけ判定Yオフセット")]
    [Tooltip("プレイヤーの足元が敵の頭からどれだけ下まで食い込んでいても踏みつけと判定するか。負の値で設定。")]
    private float stompYOffset = -0.5f; // デフォルト値を-0.5fに設定（より踏みやすく）

    [Header("地面判定")]
    [SerializeField] private GroundCheck groundCheckScript; // GroundCheckスクリプトをInspectorで割り当てる

    [Header("ヘルス設定")]
    [SerializeField] private int currentHealth = 1; // プレイヤーの初期HP
    [SerializeField] private string enemyTag = "Enemy"; // 敵として判定するタグ
    [SerializeField] private string gameOverSceneName = "GameOver"; // ゲームオーバーシーンの名前

    [SerializeField, Header("落下限界Y座標")]
    private float fallThresholdY = -10f; // このY座標を下回るとゲームオーバー

    private Rigidbody2D rb;
    private Animator animator; // Animatorコンポーネントへの参照
    private Collider2D playerCollider; // プレイヤーのCollider2Dへの参照

    private bool isFacingRight = true; // プレイヤーが右を向いているか
    private bool isDead = false; // プレイヤーが死亡しているか

    // ItemSoundPlayerへの参照
    private ItemSoundPlayer itemSoundPlayer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Animatorコンポーネントを取得
        playerCollider = GetComponent<Collider2D>(); // プレイヤーのCollider2Dを取得

        if (rb == null)
        {
            Debug.LogError("Rigidbody2Dが見つかりません。プレイヤーにRigidbody2Dを追加してください。");
        }
        if (animator == null)
        {
            Debug.LogError("Animatorが見つかりません。プレイヤーにAnimatorを追加してください。");
        }
        if (groundCheckScript == null)
        {
            Debug.LogError("GroundCheck Scriptが割り当てられていません。PlayerMoveスクリプトのInspectorで設定してください。");
        }
        if (playerCollider == null)
        {
            Debug.LogError("Collider2Dが見つかりません。プレイヤーにCollider2Dを追加してください。");
        }

        // ItemSoundPlayerのインスタンスを取得
        itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        if (itemSoundPlayer == null)
        {
            Debug.LogWarning("PlayerMove: ItemSoundPlayer がシーンに見つかりません。ジャンプ音を再生できません。");
        }

        // 初期ジャンプ回数をリセット
        currentJumps = 0;
    }

    void Update()
    {
        if (isDead) return; // 死亡中は操作を受け付けない

        // 落下限界Y座標のチェック
        if (transform.position.y < fallThresholdY)
        {
            Debug.Log($"PlayerMove: プレイヤーが落下限界Y座標 ({fallThresholdY}) を下回りました。ゲームオーバー。");
            Die(); // ゲームオーバー処理を呼び出す
            return; // 死亡したのでこれ以上Update処理を行わない
        }

        // 地面にいるかどうかの判定
        bool grounded = groundCheckScript != null ? groundCheckScript.GetIsGround() : false;
        // Debug.Log($"IsGrounded (from GroundCheck): {grounded}, currentJumps: {currentJumps}"); // ログが多すぎる場合はコメントアウト

        // 地面にいる場合、ジャンプ回数をリセット
        if (grounded)
        {
            currentJumps = 0;
        }

        // 水平方向の入力取得
        float moveInput = Input.GetAxis("Horizontal");

        // プレイヤーの移動
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // プレイヤーの向きの反転
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }

        // アニメーションの更新 (run)
        if (animator != null)
        {
            animator.SetBool("run", Mathf.Abs(moveInput) > 0.1f); // 0.1fはわずかな入力でも走ると判定するための閾値

            // アニメーションの更新 (jump1 & jump2)
            if (!grounded) // 地面にいない場合
            {
                if (rb.velocity.y > 0.1f) // 上昇中
                {
                    animator.SetBool("jump1", true); // jump1をtrue
                    animator.SetBool("jump2", false); // jump2をfalse
                }
                else if (rb.velocity.y < -0.1f) // 下降中
                {
                    animator.SetBool("jump1", false); // jump1をfalse
                    animator.SetBool("jump2", true); // jump2をtrue
                }
                else // ほぼ停止 (ジャンプの頂点など)
                {
                    animator.SetBool("jump1", false);
                    animator.SetBool("jump2", false);
                }
            }
            else // 地面にいる場合
            {
                animator.SetBool("jump1", false); // jump1をfalse
                animator.SetBool("jump2", false); // jump2をfalse
            }
        }

        // ジャンプ入力
        // 現在のジャンプ回数が最大ジャンプ回数未満の場合のみジャンプを許可
        if (Input.GetButtonDown("Jump") && currentJumps < maxJumps)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            currentJumps++; // ジャンプ回数を増やす
            Debug.Log($"PlayerMove: ジャンプしました。現在のジャンプ回数: {currentJumps}");

            // ジャンプ音を再生
            if (itemSoundPlayer != null)
            {
                itemSoundPlayer.PlayJumpSound();
            }
        }
    }

    // プレイヤーの向きを反転させる
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    // --- 衝突・トリガー判定とダメージ処理 ---

    // 衝突判定（Collider2DのIs Triggerがオフの場合）
    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleInteraction(collision);
    }

    // トリガー判定（Collider2DのIs Triggerがオンの場合）
    private void OnTriggerEnter2D(Collider2D collision)
    {
        HandleInteraction(collision);
    }

    // Collision2Dオブジェクトを受け取るように変更し、より正確な踏みつけ判定を行う
    private void HandleInteraction(Collision2D collision) // OnCollisionEnter2Dから呼ばれる場合
    {
        GameObject otherObject = collision.gameObject;
        if (otherObject.CompareTag(enemyTag))
        {
            Enemy enemy = otherObject.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead) // 敵がまだ生きている場合のみ処理
            {
                // プレイヤーのコライダーの底辺のY座標
                float playerBottomY = playerCollider.bounds.min.y;
                // 敵のコライダーの頂点のY座標
                float enemyTopY = enemy.EnemyCollider.bounds.max.y;

                // 衝突点のY座標 (プレイヤーが敵に触れたY座標)
                float contactPointY = collision.contacts[0].point.y;

                Debug.Log($"PlayerMove Collision with {otherObject.name}: " +
                          $"PlayerBottomY={playerBottomY:F2}, EnemyTopY={enemyTopY:F2}, " +
                          $"ContactPointY={contactPointY:F2}, PlayerVelocityY={rb.velocity.y:F2}");

                // プレイヤーが敵のコライダーの頂点よりも上にいて、かつ下向きに移動している（踏みつけ）
                // プレイヤーの速度が下向き（負の値）であることを確認
                // stompYOffsetを使用し、接触点が敵の頭の少し上にあることも確認
                if (rb.velocity.y < 0 && playerBottomY > enemyTopY + stompYOffset && contactPointY > enemyTopY - 0.1f) // ★修正点: stompYOffsetを使用★
                {
                    Debug.Log($"PlayerMove: 敵 ({otherObject.name}) を踏みつけました！");
                    enemy.Die(); // 敵を倒すメソッドを呼び出す
                    rb.velocity = new Vector2(rb.velocity.x, stompBounceForce); // プレイヤーを上方向に跳ねさせる
                    currentJumps = 0; // 踏みつけ後もジャンプ回数をリセット
                }
                else // 敵の側面などに衝突した場合はダメージを受ける
                {
                    Debug.Log($"PlayerMove (Health): 敵 ({otherObject.name}) と側面衝突しました！");
                    TakeDamage(1); // 敵に触れたら1ダメージ
                }
            }
        }
    }

    private void HandleInteraction(Collider2D collision) // OnTriggerEnter2Dから呼ばれる場合
    {
        GameObject otherObject = collision.gameObject;
        if (otherObject.CompareTag(enemyTag))
        {
            Enemy enemy = otherObject.GetComponent<Enemy>();
            if (enemy != null && !enemy.IsDead) // 敵がまだ生きている場合のみ処理
            {
                // トリガーの場合はOnCollisionEnter2Dのような物理的な衝突情報がないため、
                // 踏みつけ判定はより限定的になるか、別の方法を検討する必要があります。
                // ここでは、単純にプレイヤーが敵のY座標より上にいて、下向きに移動している場合を簡易的に踏みつけとします。
                // より厳密なトリガーでの踏みつけ判定には、プレイヤーの下部に専用のトリガーを設けるなどの工夫が必要です。
                Debug.Log($"PlayerMove Trigger with {otherObject.name}: PlayerBottomY={playerCollider.bounds.min.y:F2}, EnemyCenterY={collision.bounds.center.y:F2}, PlayerVelocityY={rb.velocity.y:F2}");
                if (rb.velocity.y < 0 && playerCollider.bounds.min.y > collision.bounds.center.y + 0.1f) // 敵の中心Yより少し上
                {
                    Debug.Log($"PlayerMove: 敵 ({otherObject.name}) をトリガーで踏みつけました！");
                    enemy.Die(); // 敵を倒すメソッドを呼び出す
                    rb.velocity = new Vector2(rb.velocity.x, stompBounceForce); // プレイヤーを上方向に跳ねさせる
                    currentJumps = 0; // 踏みつけ後もジャンプ回数をリセット
                }
                else
                {
                    Debug.Log($"PlayerMove (Health): 敵 ({otherObject.name}) とトリガーで衝突しました！");
                    TakeDamage(1); // 敵に触れたら1ダメージ
                }
            }
        }
    }


    // ダメージを受ける処理
    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // 死亡中はダメージを受けない

        currentHealth -= damageAmount;
        Debug.Log($"PlayerMove (Health): ダメージを受けました。残りHP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // プレイヤーが死亡する処理
    private void Die()
    {
        if (isDead) return; // 既に死亡している場合は何もしない
        isDead = true; // 死亡フラグを立てる

        Debug.Log("PlayerMove (Health): プレイヤーが死亡しました！");

        // プレイヤーの動きを完全に止める
        this.enabled = false; // PlayerMoveスクリプト自体を無効化
        if (rb != null)
        {
            rb.velocity = Vector2.zero; // 速度を0にする
            rb.isKinematic = true; // 物理演算を停止（任意）
        }

        // 死亡アニメーションがあれば再生
        if (animator != null)
        {
            // Animatorに "GameOver" というTriggerパラメータをセット
            animator.SetTrigger("GameOver");
            Debug.Log("PlayerMove (Health): 死亡アニメーションを再生します。");
        }
        else
        {
            Debug.LogWarning("PlayerMove (Health): Animatorが見つからないため、死亡アニメーションを再生できません。");
        }

        // シーン遷移はアニメーションイベント (OnGameOverAnimationEnd) で行うため、ここでは呼び出しません
    }

    // アニメーションイベントから呼ばれるメソッド
    // Hurt_Animationの最後にこのイベントを追加してください。
    public void OnGameOverAnimationEnd()
    {
        Debug.Log("OnGameOverAnimationEnd: 死亡アニメーションが終了しました。シーン遷移を開始します。");
        // GameManagerを通してゲームオーバーシーンへ遷移
        if (GameManager.instance != null)
        {
            GameManager.instance.LoadSceneWithFade(gameOverSceneName);
        }
        else
        {
            Debug.LogError("PlayerMove (Health): GameManager.instanceが見つかりません！ゲームオーバーシーンへ遷移できません。直接シーンをロードします。");
            SceneManager.LoadScene(gameOverSceneName, LoadSceneMode.Single);
        }
    }

    // HPを回復する処理（必要であれば）
    public void Heal(int healAmount)
    {
        if (isDead) return;
        currentHealth += healAmount;
        Debug.Log($"PlayerMove (Health): HPが回復しました。残りHP: {currentHealth}");
    }

    // ★追加: デバッグ用Gizmos★
    void OnDrawGizmos()
    {
        if (playerCollider == null) return;

        // プレイヤーのコライダーの底辺を赤線で表示
        Gizmos.color = Color.red;
        Vector2 playerBottomLeft = new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.min.y);
        Vector2 playerBottomRight = new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.min.y);
        Gizmos.DrawLine(playerBottomLeft, playerBottomRight);

        // 踏みつけ判定のYオフセットラインを黄色で表示
        Gizmos.color = Color.yellow;
        // 敵のコライダーの頂点のY座標 + stompYOffset の位置にラインを表示
        // このラインは、プレイヤーの足元がこのラインより上にあれば踏みつけ判定の条件を満たすことを示します
        // ただし、このGizmoは敵のColliderが取得できないと正確な位置に表示されません
        // 敵がシーンに存在し、PlayerMoveが敵のColliderを取得できる場合にのみ有効
        // より正確には、敵の頭上判定ラインを敵のスクリプト側で描画する方が良い
        // ここでは簡易的にプレイヤーのY座標から相対的に描画
        Vector2 stompLineStart = new Vector2(playerCollider.bounds.min.x, playerCollider.bounds.min.y - (stompYOffset * 2)); // 適当なオフセットで表示
        Vector2 stompLineEnd = new Vector2(playerCollider.bounds.max.x, playerCollider.bounds.min.y - (stompYOffset * 2));
        Gizmos.DrawLine(stompLineStart, stompLineEnd);
    }
}
