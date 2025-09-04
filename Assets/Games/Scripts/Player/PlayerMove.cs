using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーキャラクターの移動とジャンプを管理するスクリプトです。
/// 敵との相互作用、ゲームオーバー処理も含まれます。
/// </summary>
public class PlayerMove : MonoBehaviour {
    // ====================================================================================================
    // #region: インスペクターから設定する変数
    // ====================================================================================================
    [Header("移動設定")]
    [Tooltip("プレイヤーの横移動速度")]
    [SerializeField] private float moveSpeed = 5f;
    [Tooltip("ジャンプ時の上向きの力")]
    [SerializeField] private float jumpForce = 10f;
    [Tooltip("ジャンプできる回数（例: 2は2段ジャンプ）")]
    [SerializeField] private int maxJumps = 2;

    [Header("敵との相互作用")]
    [Tooltip("敵を踏みつけたときに跳ねる力")]
    [SerializeField] private float m_stompBounceForce = 7f;
    [Tooltip("ダメージを受けた後の無敵時間")]
    [SerializeField] private float invincibleDuration = 0.2f;

    [Header("コンポーネントとオブジェクト")]
    [Tooltip("地面判定を行う子オブジェクトのコンポーネント")]
    [SerializeField] private GroundCheck m_groundCheckComponent;

    // Mobile Control Canvasから参照を受け取るためのプライベート変数
    private VirtualJoystick m_joystick;

    [Header("ゲームオーバー条件")]
    [Tooltip("プレイヤーがこのY座標より下に落ちるとゲームオーバー")]
    [SerializeField] private float m_gameOverFallHeight = -10f;

    // ====================================================================================================
    // #region: スクリプト内部で管理する変数
    // ====================================================================================================
    private Rigidbody2D rb; // プレイヤーの物理挙動を制御する
    private Animator animator; // プレイヤーのアニメーションを制御する
    private ItemSoundPlayer itemSoundPlayer; // サウンドを再生する

    private bool isGrounded; // 地面にいるかどうかの状態
    private bool isFacingRight = true; // プレイヤーが右を向いているか
    private int jumpsRemaining; // 残りのジャンプ回数
    private bool isInvincible = false; // 無敵状態かどうか

    public bool IsDead { get; private set; } = false; // プレイヤーが死亡しているか
    public float MoveSpeed => moveSpeed;

    // ====================================================================================================
    // #region: MonoBehaviour ライフサイクル
    // ====================================================================================================

    /// <summary>
    /// オブジェクトがアクティブになった時に一度だけ実行される初期化処理
    /// </summary>
    private void Awake() {
        // 必須コンポーネントを取得
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        // コンポーネントが正しく取得できたか確認し、エラーをログに出力
        if (rb == null) UnityEngine.Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。", this);
        if (animator == null) UnityEngine.Debug.LogError("PlayerMove: Animatorがアタッチされていません。", this);

        // GroundCheckコンポーネントがインスペクターで設定されていない場合、子オブジェクトから探す
        if (m_groundCheckComponent == null) {
            m_groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (m_groundCheckComponent == null) {
                UnityEngine.Debug.LogError("PlayerMove: GroundCheckコンポーネントが見つかりません。", this);
            }
        }

        // シーン内のItemSoundPlayerオブジェクトを探す
        itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        // 残りのジャンプ回数を最大値に設定
        jumpsRemaining = maxJumps;
    }

    /// <summary>
    /// GameManagerからモバイル操作用のジョイスティックとジャンプボタンの参照を受け取る
    /// </summary>
    public void SetMobileControls(VirtualJoystick joystick, JumpButtonController jump) {
        m_joystick = joystick;
    }

    /// <summary>
    /// 毎フレーム実行される更新処理
    /// </summary>
    private void Update() {
        // プレイヤーが死亡している場合は、これ以降の処理を停止
        if (IsDead) return;

        // 1. 地面判定の更新
        bool previousIsGrounded = isGrounded;
        isGrounded = m_groundCheckComponent != null && m_groundCheckComponent.GetIsGround();

        // 地面に着地した場合、ジャンプ回数をリセット
        if (!previousIsGrounded && isGrounded) {
            jumpsRemaining = maxJumps;
        }

        // 2. 入力処理
        float moveInput = 0f;

        // モバイルプラットフォームで実行されている場合はジョイスティックの入力を優先
        if (UnityEngine.Application.isMobilePlatform && m_joystick != null) {
            moveInput = m_joystick.InputDirection.x;
        }
        else {
            // PCの場合はキーボードとコントローラーの入力を受け付ける
            moveInput = Input.GetAxis("Horizontal");
        }

        // 入力に基づいて移動と向きの反転を処理
        HandleMovementInput(moveInput);

        // PCでジャンプボタンが押されたらジャンプを実行
        if (Input.GetButtonDown("Jump")) {
            Jump();
        }

        // 3. アニメーションパラメーターの更新
        UpdateAnimatorParameters(rb.velocity.x);

        // 4. ゲームオーバー判定
        // プレイヤーが指定されたY座標より下に落ちたら死亡
        if (transform.position.y < m_gameOverFallHeight) {
            Die();
        }
    }

    // ====================================================================================================
    // #region: プレイヤーアクション
    // ====================================================================================================

    /// <summary>
    /// 移動入力を処理し、速度と向きを更新します。
    /// </summary>
    /// <param name="moveInput">横方向の入力（-1から1）</param>
    private void HandleMovementInput(float moveInput) {
        if (rb != null) {
            // Rigidbody2Dの速度を直接設定して横移動
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);
        }

        // 移動入力量に基づいてプレイヤーの向きを反転
        if (moveInput > 0 && !isFacingRight) {
            Flip(); // 右を向いていない時に右へ移動したら反転
        }
        else if (moveInput < 0 && isFacingRight) {
            Flip(); // 右を向いている時に左へ移動したら反転
        }
    }

    /// <summary>
    /// ジャンプを実行します。
    /// </summary>
    public void Jump() {
        // 死亡時やジャンプ回数が残っていない場合は処理を停止
        if (IsDead) return;
        if (jumpsRemaining <= 0) return;

        // Rigidbody2DのY軸の速度をジャンプ力に設定
        if (rb != null) {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
        jumpsRemaining--; // ジャンプ回数を1減らす

        // サウンドとアニメーションを再生
        if (itemSoundPlayer != null) itemSoundPlayer.PlayJumpSound();
        if (animator != null) {
            // 地面に着地しているか、空中にいるかで異なるジャンプアニメーションを再生
            if (isGrounded && HasAnimatorParameter("JumpTrigger", AnimatorControllerParameterType.Trigger)) {
                animator.SetTrigger("JumpTrigger"); // 通常ジャンプアニメーション
            }
            else if (!isGrounded && HasAnimatorParameter("DoubleJumpTrigger", AnimatorControllerParameterType.Trigger)) {
                animator.SetTrigger("DoubleJumpTrigger"); // 2段ジャンプアニメーション
            }
        }
    }

    /// <summary>
    /// モバイルジャンプボタンがクリックされたときに呼び出されるメソッド
    /// </summary>
    public void OnMobileJumpButtonPressed() {
        Jump(); // PCの入力と同様にジャンプを実行
    }

    /// <summary>
    /// 衝突発生時に実行される処理
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision) {
        // 死亡時や無敵状態では衝突処理をスキップ
        if (IsDead || isInvincible) return;

        // 衝突したオブジェクトが"Enemy"タグを持つ場合
        if (collision.gameObject.CompareTag("Enemy")) {
            // 敵との接触位置を判定
            // プレイヤーのy座標が敵のy座標より高い場合、踏みつけたと判定
            if (transform.position.y > collision.transform.position.y) {
                StompEnemy(collision.gameObject); // 敵を踏みつける処理
            }
            else {
                // プレイヤーが敵の横または下から衝突した場合、死亡処理
                Die();
            }
        }
    }

    /// <summary>
    /// 敵を踏みつけた時の処理
    /// </summary>
    public void StompEnemy(GameObject enemyObject) {
        if (IsDead) return;

        // 敵のコンポーネントを取得
        Enemy enemy = enemyObject.GetComponent<Enemy>();
        if (enemy == null) return;

        // 敵を踏みつけて上に跳ねる
        if (rb != null) {
            rb.velocity = new Vector2(rb.velocity.x, m_stompBounceForce);
        }
        // 敵にダメージを与える
        enemy.TakeDamage();
        // 一定時間無敵状態にするコルーチンを開始
        StartCoroutine(BecomeInvincible(invincibleDuration));
    }

    /// <summary>
    /// プレイヤーの死亡処理
    /// </summary>
    public void Die() {
        if (IsDead) return;
        IsDead = true; // 死亡フラグを立てる

        if (itemSoundPlayer != null) itemSoundPlayer.PlayGameOverSound();

        // 速度をゼロにし、物理演算を停止
        if (rb != null) {
            rb.velocity = Vector2.zero;
            rb.isKinematic = true; // 物理演算を無効にする
        }
        // プレイヤーのColliderを無効化して他のオブジェクトとの衝突を停止
        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null) playerCollider.enabled = false;

        // 死亡アニメーションを再生
        if (animator != null && HasAnimatorParameter("GameOver", AnimatorControllerParameterType.Trigger)) {
            animator.SetTrigger("GameOver");
        }
        // アニメーションがない場合は、即座に次の処理へ移行するコルーチンを開始
        else {
            StartCoroutine(FallbackDeathSequence());
        }
    }

    /// <summary>
    /// ゲームオーバーアニメーションの終了時に呼び出されるメソッド
    /// （アニメーションイベントとして設定）
    /// </summary>
    public void OnGameOverAnimationEnd() {
        gameObject.SetActive(false); // プレイヤーを非表示にする
        FinalizeDeathAndSceneTransition(); // シーン遷移処理へ
    }

    // ====================================================================================================
    // #region: ヘルパーメソッド（内部処理）
    // ====================================================================================================

    /// <summary>
    /// プレイヤーの向きを反転させます。
    /// </summary>
    private void Flip() {
        isFacingRight = !isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1; // ローカルスケールのX軸を反転
        transform.localScale = scaler;
    }

    /// <summary>
    /// アニメーションのパラメーターを更新します。
    /// </summary>
    private void UpdateAnimatorParameters(float moveInput) {
        if (animator == null) return;

        // runパラメーターを更新（移動中か）
        if (HasAnimatorParameter("run", AnimatorControllerParameterType.Bool)) {
            animator.SetBool("run", Mathf.Abs(moveInput) > 0.01f);
        }

        // isGroundedパラメーターを更新（地面にいるか）
        if (HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool)) {
            animator.SetBool("isGrounded", isGrounded);
        }

        // velocityYパラメーターを更新（垂直方向の速度）
        if (HasAnimatorParameter("velocityY", AnimatorControllerParameterType.Float) && rb != null) {
            animator.SetFloat("velocityY", rb.velocity.y);
        }
    }

    /// <summary>
    /// 指定されたアニメーターパラメーターが存在するか確認します。
    /// </summary>
    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType) {
        if (animator == null) return false;
        foreach (AnimatorControllerParameter param in animator.parameters) {
            if (param.name == paramName && param.type == paramType) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 死亡アニメーションがない場合に実行される代替処理
    /// </summary>
    private IEnumerator FallbackDeathSequence() {
        yield return new WaitForSeconds(1.0f); // 1秒待機
        FinalizeDeathAndSceneTransition(); // シーン遷移処理を実行
    }

    /// <summary>
    /// 死亡処理の最終段階として、プレイヤーを非表示にし、シーン遷移を促します。
    /// </summary>
    private void FinalizeDeathAndSceneTransition() {
        gameObject.SetActive(false); // プレイヤーオブジェクトを非アクティブにする

        if (GameManager.Instance != null) {
            // GameManagerが存在すればゲームオーバー状態に設定してシーン遷移を委ねる
            GameManager.Instance.SetGameOverStateImmediately();
        }
        else {
            UnityEngine.Debug.LogError("PlayerMove: GameManagerのインスタンスが見つかりません！タイトルシーンへ直接遷移します。");
            SceneManager.LoadScene("TitleScene"); // GameManagerがない場合は直接タイトルへ
        }
    }

    /// <summary>
    /// 一定時間無敵状態にするコルーチン
    /// </summary>
    private IEnumerator BecomeInvincible(float duration) {
        isInvincible = true; // 無敵状態を有効化
        yield return new WaitForSeconds(duration); // 指定された時間だけ待機
        isInvincible = false; // 無敵状態を無効化
    }
}
