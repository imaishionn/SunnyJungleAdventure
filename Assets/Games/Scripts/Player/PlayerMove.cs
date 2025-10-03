using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーの移動、ジャンプ、敵との相互作用を管理するスクリプト。
/// </summary>
public class PlayerMove : MonoBehaviour {
    /// <summary>
    /// プロパティ
    /// </summary>
    [field: Header("移動設定"), SerializeField]
    public float MoveSpeed { get; } = 7f;

    [Header("ジャンプの高さ"), SerializeField]
    private float _jumpForce = 10f;

    [Header("ジャンプの回数"), SerializeField]
    private int _maxJumps = 2;

    [Header("敵との相互作用"), SerializeField]
    private float _stompBounceForce = 7f;
    [Header("ダメージを受けた後の無敵時間"), SerializeField]
    private float _invincibleDuration = 0.2f;

    [Header("コンポーネントとオブジェクト"), SerializeField]
    private GroundCheck _groundCheckComponent;

    [Header("ゲームオーバー条件"), SerializeField]
    private float _gameOverFallHeight = -10f;

    /// <summary>
    /// モバイルコントロール用のジョイスティック
    /// </summary>
    private VirtualJoystick _joystick;

    /// <summary>
    /// プライベートフィールド
    /// </summary>
    private Rigidbody2D _rb;
    /// <summary>
    /// アニメーターコンポーネントへの参照
    /// </summary>
    private Animator _animator;

    // ★修正: アニメーターパラメータのハッシュIDを定義
    private int _animParamRun;
    private int _animParamIsGrounded;
    private int _animParamVelocityY;
    // ★修正: トリガー名を定数として定義
    private const string ANIM_TRIGGER_JUMP = "JumpTrigger";
    private const string ANIM_TRIGGER_GAMEOVER = "GameOver";


    /// <summary>
    /// 接地状態
    /// </summary>
    private bool _isGrounded;

    /// <summary>
    /// プレイヤーの向き
    /// </summary>
    private bool _isFacingRight = true;

    /// <summary>
    /// 残りのジャンプ回数
    /// </summary>
    private int _jumpsRemaining;

    /// <summary>
    /// 無敵状態かどうか
    /// </summary>
    private bool _isInvincible = false;

    /// <summary>
    /// イベント
    /// </summary>
    public bool IsDead { get; private set; } = false;

    public static event Action OnPlayerDie;
    public static event Action OnEnemyStomp;


    // -------------------- 初期化とリセット --------------------

    private void Awake() {
        if (!TryGetComponent<Rigidbody2D>(out _rb)) {
            Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。このスクリプトは動作しません。", this);
            enabled = false;
            return;
        }

        if (TryGetComponent<Animator>(out _animator)) {
            // ★修正: アニメーターパラメータをハッシュ化してキャッシュ
            _animParamRun = Animator.StringToHash("run");
            _animParamIsGrounded = Animator.StringToHash("isGrounded");
            _animParamVelocityY = Animator.StringToHash("velocityY");
            // JumpTriggerはトリガーのためハッシュ化は必須ではないが、一応定義
            // _animParamJumpTrigger = Animator.StringToHash(ANIM_TRIGGER_JUMP);
        }

        if (_groundCheckComponent == null) {
            _groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (_groundCheckComponent == null) {
                Debug.LogWarning("PlayerMove: GroundCheckコンポーネントが見つかりません。接地判定は行われません。", this);
            }
        }

        _jumpsRemaining = _maxJumps;
    }

    private void Start() {
        // GameManagerからの参照が設定されるべきなので、ここでは警告のみ
        if (_joystick == null && Application.isMobilePlatform) {
            Debug.LogWarning("PlayerMove: VirtualJoystickはGameManager経由で設定される必要があります。");
        }
    }

    public void SetMobileControls(VirtualJoystick joystick) => _joystick = joystick;

    /// <summary>
    /// GameManagerからの呼び出し用: シーンロード時にプレイヤーの状態をリセットします。
    /// </summary>
    public void ResetPlayerState() {
        IsDead = false;
        _isInvincible = false;
        _jumpsRemaining = _maxJumps;

        if (_rb != null) {
            _rb.velocity = Vector2.zero;
            _rb.isKinematic = false;
        }
        if (TryGetComponent<Collider2D>(out Collider2D playerCollider)) {
            playerCollider.enabled = true;
        }

        // 死亡トリガーをリセット (アニメーションが残るのを防ぐ)
        if (_animator != null) {
            if (HasAnimatorParameter(ANIM_TRIGGER_GAMEOVER, AnimatorControllerParameterType.Trigger)) {
                _animator.ResetTrigger(ANIM_TRIGGER_GAMEOVER);
            }
        }

        Debug.Log("PlayerMove State Reset Complete. IsDead=false, JumpsRemaining=" + _maxJumps);
    }

    // -------------------- 更新処理 --------------------

    private void Update() {
        if (IsDead) {
            return;
        }

        // 接地判定の更新とジャンプ回数のリセット
        bool previousIsGrounded = _isGrounded;
        if (_groundCheckComponent != null) {
            _isGrounded = _groundCheckComponent.GetIsGround();
        }

        // 着地した瞬間、ジャンプ回数をリセット
        if (!previousIsGrounded && _isGrounded) {
            _jumpsRemaining = _maxJumps;
        }

        float moveInput = (_joystick != null) ? _joystick.InputDirection.x : Input.GetAxis("Horizontal");
        HandleMovementInput(moveInput);

        // PC/WebGLでのジャンプ入力
        if (Input.GetButtonDown("Jump")) {
            Jump();
        }

        if (_rb != null) {
            UpdateAnimatorParameters(_rb.velocity.x);
        }

        // 落下によるゲームオーバー判定
        if (transform.position.y < _gameOverFallHeight) {
            Die();
        }
    }

    // -------------------- 移動・ジャンプ --------------------

    private void HandleMovementInput(float moveInput) {
        if (_rb != null) {
            _rb.velocity = new Vector2(moveInput * MoveSpeed, _rb.velocity.y);
        }

        if (moveInput > 0 && !_isFacingRight || moveInput < 0 && _isFacingRight) {
            Flip();
        }
    }

    public void Jump() {
        // ★重要: ジャンプをブロックする条件はIsDeadと_jumpsRemainingのみ
        if (IsDead || _jumpsRemaining <= 0) {
            Debug.LogWarning($"Jump Blocked! IsDead: {IsDead}, Jumps Remaining: {_jumpsRemaining}");
            return;
        }

        if (_rb != null) {
            // 既存のY軸速度をリセットしてからジャンプ力を加える (多段ジャンプ時の挙動を安定させる)
            _rb.velocity = new Vector2(_rb.velocity.x, 0);
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }

        // ★【修正】JumpTriggerを起動する処理を追加
        if (_animator != null && HasAnimatorParameter(ANIM_TRIGGER_JUMP, AnimatorControllerParameterType.Trigger)) {
            _animator.SetTrigger(ANIM_TRIGGER_JUMP);
        }

        _jumpsRemaining--;
        Debug.Log($"Jump Success. Jumps Remaining: {_jumpsRemaining}");

        if (ItemSoundPlayer.Instance != null) {
            ItemSoundPlayer.Instance.PlayJumpSound();
        }
    }

    /// <summary>
    /// モバイルのジャンプボタンからの呼び出し用
    /// </summary>
    public void OnMobileJumpButtonPressed() => Jump();

    // -------------------- 衝突・ダメージ --------------------

    // ★★★★ このメソッドを修正しました ★★★★
    private void OnCollisionEnter2D(Collision2D collision) {
        if (IsDead || _isInvincible) {
            return;
        }

        // 敵本体のコライダーとの接触は全て横/下からの接触とみなし、ダメージ（死亡）とする。
        // 踏みつけ判定は、子オブジェクトのStompCheckスクリプトに完全に委譲されました。
        if (collision.gameObject.CompareTag("Enemy")) {
            Die();
        }
    }
    // ★★★★ 修正終わり ★★★★

    public void StompEnemy(GameObject enemyObject) {
        if (IsDead || _rb == null) {
            return;
        }

        if (!enemyObject.TryGetComponent<Enemy>(out Enemy enemy)) {
            return;
        }

        _rb.velocity = new Vector2(_rb.velocity.x, _stompBounceForce);
        enemy.TakeDamage();

        if (ItemSoundPlayer.Instance != null) {
            ItemSoundPlayer.Instance.PlayEnemyDefeatSound();
        }

        OnEnemyStomp?.Invoke();

        StartCoroutine(BecomeInvincible(_invincibleDuration));
    }

    public void Die() {
        if (IsDead) {
            return;
        }
        IsDead = true;
        Debug.Log("Player Die.");

        if (ItemSoundPlayer.Instance != null) {
            ItemSoundPlayer.Instance.PlayGameOverSound();
        }

        OnPlayerDie?.Invoke();

        if (_rb != null) {
            _rb.velocity = Vector2.zero;
            _rb.isKinematic = true; // 物理演算を停止
        }
        if (TryGetComponent<Collider2D>(out Collider2D playerCollider)) {
            playerCollider.enabled = false;
        }

        // アニメーションを再生
        if (_animator != null && HasAnimatorParameter(ANIM_TRIGGER_GAMEOVER, AnimatorControllerParameterType.Trigger)) {
            _animator.SetTrigger(ANIM_TRIGGER_GAMEOVER);
        }
        else {
            StartCoroutine(FallbackDeathSequence());
        }
    }

    public void OnGameOverAnimationEnd() => FinalizeDeathAndSceneTransition();

    // -------------------- アニメーションとユーティリティ --------------------

    private void Flip() {
        _isFacingRight = !_isFacingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    private void UpdateAnimatorParameters(float moveInput) {
        if (_animator == null) {
            return;
        }

        // ★修正: ハッシュIDを使用して高速にパラメータを設定
        _animator.SetBool(_animParamRun, Mathf.Abs(moveInput) > 0.01f);
        _animator.SetBool(_animParamIsGrounded, _isGrounded);
        if (_rb != null) {
            _animator.SetFloat(_animParamVelocityY, _rb.velocity.y);
        }
    }

    /// <summary>
    /// アニメーターに特定のパラメータが存在するかチェックする (汎用的なチェックを残す)
    /// </summary>
    private bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType paramType) {
        if (_animator == null) {
            return false;
        }
        foreach (AnimatorControllerParameter param in _animator.parameters) {
            if (param.name == paramName && param.type == paramType) {
                return true;
            }
        }
        return false;
    }

    private IEnumerator FallbackDeathSequence() {
        yield return new WaitForSeconds(1.0f);
        FinalizeDeathAndSceneTransition();
    }

    private void FinalizeDeathAndSceneTransition() {
        if (GameManager.Instance != null) {
            GameManager.Instance.GameOver();
        }
        else {
            Debug.LogError("PlayerMove: GameManagerのインスタンスが見つかりません！タイトルシーンへ直接遷移します。");
            SceneManager.LoadScene("TitleScene");
        }
    }

    private IEnumerator BecomeInvincible(float duration) {
        _isInvincible = true;
        yield return new WaitForSecondsRealtime(duration);
        _isInvincible = false;
    }
}
