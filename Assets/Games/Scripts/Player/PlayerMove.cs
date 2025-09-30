using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// プレイヤーキャラクターの移動とジャンプを管理するスクリプトです。
/// 敵との相互作用、ゲームオーバー処理も含まれます。
/// </summary>
public class PlayerMove : MonoBehaviour {
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
    // private SpriteRenderer _spriteRenderer; // ★削除: 点滅処理が不要なため

    [Header("ゲームオーバー条件"), SerializeField]
    private float _gameOverFallHeight = -10f;

    private VirtualJoystick _joystick;

    private Rigidbody2D _rb;
    private Animator _animator;

    private bool _isGrounded;
    private bool _isFacingRight = true;
    private int _jumpsRemaining;
    private bool _isInvincible = false;

    public bool IsDead { get; private set; } = false;

    public static event Action OnPlayerDie;
    public static event Action OnEnemyStomp;

    private void Awake() {
        if (!TryGetComponent<Rigidbody2D>(out _rb)) {
            Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。このスクリプトは動作しません。", this);
            enabled = false;
            return;
        }

        TryGetComponent<Animator>(out _animator);

        if (_groundCheckComponent == null) {
            _groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (_groundCheckComponent == null) {
                Debug.LogWarning("PlayerMove: GroundCheckコンポーネントが見つかりません。接地判定は行われません。", this);
            }
        }

        _jumpsRemaining = _maxJumps;
    }

    private void Start() {
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

        if (_animator != null && HasAnimatorParameter("GameOver", AnimatorControllerParameterType.Trigger)) {
            _animator.ResetTrigger("GameOver");
        }

        // ★重要: ジャンプ不能の原因となる可能性のあるフラグをすべてリセットしたことをログで確認
        Debug.Log("PlayerMove State Reset Complete. IsDead=false, JumpsRemaining=" + _maxJumps);
    }


    private void Update() {
        if (IsDead) {
            return;
        }

        // ゲームステートによる入力ブロックは削除しました。

        bool previousIsGrounded = _isGrounded;
        if (_groundCheckComponent != null) {
            _isGrounded = _groundCheckComponent.GetIsGround();
        }

        if (!previousIsGrounded && _isGrounded) {
            _jumpsRemaining = _maxJumps;
        }

        float moveInput = (_joystick != null) ? _joystick.InputDirection.x : Input.GetAxis("Horizontal");
        HandleMovementInput(moveInput);

        if (Input.GetButtonDown("Jump")) {
            Jump();
        }

        if (_rb != null) {
            UpdateAnimatorParameters(_rb.velocity.x);
        }

        if (transform.position.y < _gameOverFallHeight) {
            Die();
        }
    }

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
            // ジャンプがブロックされたことをログで確認
            Debug.LogWarning($"Jump Blocked! IsDead: {IsDead}, Jumps Remaining: {_jumpsRemaining}");
            return;
        }

        if (_rb != null) {
            _rb.velocity = new Vector2(_rb.velocity.x, 0);
            _rb.AddForce(Vector2.up * _jumpForce, ForceMode2D.Impulse);
        }
        _jumpsRemaining--;
        Debug.Log($"Jump Success. Jumps Remaining: {_jumpsRemaining}");

        if (ItemSoundPlayer.Instance != null) {
            ItemSoundPlayer.Instance.PlayJumpSound();
        }
    }

    public void OnMobileJumpButtonPressed() => Jump();

    private void OnCollisionEnter2D(Collision2D collision) {
        if (IsDead || _isInvincible) {
            return;
        }

        if (collision.gameObject.CompareTag("Enemy")) {
            if (_rb == null) {
                return;
            }

            if (_rb.velocity.y < 0) {
                StompEnemy(collision.gameObject);
            }
            else {
                Die();
            }
        }
    }

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
            _rb.isKinematic = true;
        }
        if (TryGetComponent<Collider2D>(out Collider2D playerCollider)) {
            playerCollider.enabled = false;
        }

        if (_animator != null && HasAnimatorParameter("GameOver", AnimatorControllerParameterType.Trigger)) {
            _animator.SetTrigger("GameOver");
        }
        else {
            StartCoroutine(FallbackDeathSequence());
        }
    }

    public void OnGameOverAnimationEnd() => FinalizeDeathAndSceneTransition();

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

        if (HasAnimatorParameter("run", AnimatorControllerParameterType.Bool)) {
            _animator.SetBool("run", Mathf.Abs(moveInput) > 0.01f);
        }

        if (HasAnimatorParameter("isGrounded", AnimatorControllerParameterType.Bool)) {
            _animator.SetBool("isGrounded", _isGrounded);
        }

        if (HasAnimatorParameter("velocityY", AnimatorControllerParameterType.Float) && _rb != null) {
            _animator.SetFloat("velocityY", _rb.velocity.y);
        }
    }

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

        // 点滅ロジックは完全に削除されています。

        yield return new WaitForSecondsRealtime(duration);

        _isInvincible = false;
    }
}
