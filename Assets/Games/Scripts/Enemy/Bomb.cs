using System.Collections;
using UnityEngine;

/// <summary>
/// 敵が投下するボムの挙動を制御するスクリプトです。
/// プレイヤーや地面に接触すると爆発し、コライダーを拡大してダメージを与えます。
/// </summary>
public class Bomb : MonoBehaviour {
    // 爆発アニメーターのパラメーター名を定数で定義
    private static class AnimatorParams {
        public const string ExplodeTrigger = "Explode";
    }

    // タグ名を定数で定義
    private const string PLAYER_TAG = "Player";
    private const string GROUND_TAG = "Ground";

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("爆発設定")]
    [Tooltip("爆発アニメーションの再生時間。コライダーの拡大時間と合わせる。")]
    public float explosionDuration = 0.5f;

    [Header("コライダーアニメーション設定")]
    [Tooltip("爆発開始時のコライダーの半径")]
    [SerializeField]
    private float _startRadius = 0.05f;
    [Tooltip("爆発終了時のコライダーの最大半径")]
    [SerializeField]
    private float _endRadius = 0.5f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Rigidbody2D _rb;
    private Animator _anim;
    private CircleCollider2D _bombCollider;
    private bool _hasExploded = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake() {
        // 必要なコンポーネントの参照を取得
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _bombCollider = GetComponent<CircleCollider2D>();

        // コンポーネントが取得できなかった場合の警告
        if (_rb == null) {
            Debug.LogWarning("Bomb: Rigidbody2Dがアタッチされていません。", this);
        }
        if (_anim == null) {
            Debug.LogWarning("Bomb: Animatorがアタッチされていません。", this);
        }
        if (_bombCollider == null) {
            Debug.LogWarning("Bomb: CircleCollider2Dがアタッチされていません。", this);
        }
    }

    private void OnEnable() {
        // オブジェクトプールからの再利用時に状態をリセット
        _hasExploded = false;
        if (_bombCollider != null) {
            _bombCollider.radius = _startRadius;
            _bombCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// ボムを指定された方向に指定された速度で発射します。
    /// </summary>
    /// <param name="direction">発射方向</param>
    /// <param name="speed">発射速度</param>
    public void Launch(Vector2 direction, float speed) {
        if (_rb != null) {
            _rb.velocity = direction.normalized * speed;
        }
    }

    /// <summary>
    /// 他のコライダーと接触したときに呼び出されます。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        if (_hasExploded) {
            return;
        }

        if (other.CompareTag(GROUND_TAG) || other.CompareTag(PLAYER_TAG)) {
            Explode();

            if (other.CompareTag(PLAYER_TAG)) {
                if (other.TryGetComponent<PlayerMove>(out PlayerMove playerMove) && !playerMove.IsDead) {
                    playerMove.Die();
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Explode() {
        _hasExploded = true;

        if (_rb != null) {
            _rb.velocity = Vector2.zero;
        }

        if (_anim != null) {
            _anim.SetTrigger(AnimatorParams.ExplodeTrigger);
        }

        StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine() {
        float timer = 0f;
        while (timer < explosionDuration) {
            timer += Time.deltaTime;
            float t = timer / explosionDuration;

            if (_bombCollider != null) {
                _bombCollider.radius = Mathf.Lerp(_startRadius, _endRadius, t);
            }
            yield return null;
        }

        gameObject.SetActive(false);
    }

    /// <summary>
    /// アニメーションイベントから呼び出され、オブジェクトを非アクティブ化します。
    /// </summary>
    public void DeactivateBomb() => gameObject.SetActive(false);
}
