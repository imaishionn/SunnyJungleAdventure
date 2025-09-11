using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// プレイヤーキャラクターの移動とジャンプを管理するスクリプトです。
/// 敵との相互作用、ゲームオーバー処理も含まれます。
/// </summary>
public class PlayerMove : MonoBehaviour {
    [field: Header("移動設定"), SerializeField]
    public float MoveSpeed { get; } = 7f;

    [Header("ジャンプの高さ"), SerializeField]
    private float _jumpForce = 100f; 

    [Header("ジャンプの回数"), SerializeField]
    private int _maxJumps = 2; 

    [Header("敵との相互作用"), SerializeField]
    private float _stompBounceForce = 7f; 
    [Header("ダメージを受けた後の無敵時間"),SerializeField]
    private float _invincibleDuration = 0.2f; 

    [Header("コンポーネントとオブジェクト"), SerializeField]
    private GroundCheck _groundCheckComponent; 

    [Header("ゲームオーバー条件"), SerializeField]
    private float _gameOverFallHeight = -10f; 

    // [SerializeField]は不要です。Start()で動的に取得します
    private VirtualJoystick _joystick;

    // UIデバッグ用の参照
    [Header("デバッグ"), SerializeField]
    private TextMeshProUGUI _debugText;

    private Rigidbody2D _rb;
    private Animator _animator;
    private ItemSoundPlayer _itemSoundPlayer;

    private bool _isGrounded;
    private bool _isFacingRight = true;
    private int _jumpsRemaining;
    private bool _isInvincible = false;

    public bool IsDead { get; private set; } = false;

    private void Awake() {
        if (!TryGetComponent<Rigidbody2D>(out _rb)) {
            Debug.LogError("PlayerMove: Rigidbody2Dがアタッチされていません。", this);
        }
        if (!TryGetComponent<Animator>(out _animator)) {
            Debug.LogError("PlayerMove: Animatorがアタッチされていません。", this);
        }

        if (_groundCheckComponent == null) {
            _groundCheckComponent = GetComponentInChildren<GroundCheck>();
            if (_groundCheckComponent == null) {
                Debug.LogError("PlayerMove: GroundCheckコンポーネントが見つかりません。", this);
            }
        }

        _itemSoundPlayer = FindObjectOfType<ItemSoundPlayer>();
        _jumpsRemaining = _maxJumps;
    }

    private void Start() {
        // FindObjectOfTypeはAwake()ではなくStart()で行う
        _joystick = FindObjectOfType<VirtualJoystick>();
        if (_joystick == null) {
            Debug.LogWarning("PlayerMove: VirtualJoystickが見つかりませんでした。PCキーボードでの操作になります。");
        }
    }

    public void SetMobileControls(VirtualJoystick joystick) => _joystick = joystick;

    private void Update() {
        if (IsDead) {
            return;
        }

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

        if (_debugText != null) {
            string debugInfo = $"Move Input: {moveInput:F2}\n";
            if (_joystick != null) {
                debugInfo += $"Joystick Direction: {_joystick.InputDirection.x:F2}\n";
            }
            else {
                debugInfo += "Joystick Reference is NULL\n";
            }
            debugInfo += $"Is Grounded: {_isGrounded}";
            _debugText.text = debugInfo;
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
        if (IsDead || _jumpsRemaining <= 0) {
            return;
        }

        if (_rb != null) {
            _rb.velocity = new Vector2(_rb.velocity.x, _jumpForce);
        }
        _jumpsRemaining--;

        if (_itemSoundPlayer != null) {
            _itemSoundPlayer.PlayJumpSound();
        }

        if (_animator != null) {
            if (_isGrounded && HasAnimatorParameter("JumpTrigger", AnimatorControllerParameterType.Trigger)) {
                _animator.SetTrigger("JumpTrigger");
            }
            else if (!_isGrounded && HasAnimatorParameter("DoubleJumpTrigger", AnimatorControllerParameterType.Trigger)) {
                _animator.SetTrigger("DoubleJumpTrigger");
            }
        }
    }

    public void OnMobileJumpButtonPressed() => Jump();

    private void OnCollisionEnter2D(Collision2D collision) {
        if (IsDead || _isInvincible) {
            return;
        }

        if (collision.gameObject.CompareTag("Enemy")) {
            if (transform.position.y > collision.transform.position.y) {
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
        StartCoroutine(BecomeInvincible(_invincibleDuration));
    }

    public void Die() {
        if (IsDead) {
            return;
        }
        IsDead = true;

        if (_itemSoundPlayer != null) {
            _itemSoundPlayer.PlayGameOverSound();
        }

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
        yield return new WaitForSeconds(duration);
        _isInvincible = false;
    }
}
