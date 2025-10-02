using System.Collections;
using UnityEngine;

/// <summary>
/// 敵が投下するボムの挙動を制御するスクリプト。
/// プレイヤーや地面に接触すると爆発し、コライダーを拡大してダメージを与えます。
/// </summary>
public class Bomb : MonoBehaviour {

    private static class AnimatorParams {
        public const string ExplodeTrigger = "Explode";
    }

    /// <summary>
    /// プレイヤーを識別するためのタグ
    /// </summary>
    private const string PLAYER_TAG = "Player";

    /// <summary>
    /// 地面を識別するためのタグ
    /// </summary>
    private const string GROUND_TAG = "Ground";

    [Header("爆発アニメーションの再生時間とコライダー拡大にかかる時間"), SerializeField]
    private float _explosionDuration = 0.5f; 

    [Header("爆発の当たり判定"), SerializeField]
    private float _startRadius = 0.05f; 

    [Header("爆発の最終の当たり判定"), SerializeField]
    private float _endRadius = 0.5f;

    /// <summary>
    /// Rigidbody2Dコンポーネントへの参照
    /// </summary>
    private Rigidbody2D _rb;

    /// <summary>
    /// Animatorコンポーネントへの参照
    /// </summary>
    private Animator _anim;

    /// <summary>
    /// CircleCollider2Dコンポーネントへの参照
    /// </summary>
    private CircleCollider2D _bombCollider;

    /// <summary>
    /// 爆発が既に発生したかどうかのフラグ
    /// </summary>
    private bool _hasExploded = false;

    private void Awake() {
        _rb = GetComponent<Rigidbody2D>();
        _anim = GetComponent<Animator>();
        _bombCollider = GetComponent<CircleCollider2D>();

        if (_rb == null || _anim == null || _bombCollider == null) {
            Debug.LogWarning("Bomb: 必要なコンポーネント（Rigidbody2D, Animator, CircleCollider2D）が一つ以上見つかりません。", this);
        }
    }

    private void OnEnable() {
        _hasExploded = false;
        if (_bombCollider != null) {
            _bombCollider.radius = _startRadius;
            _bombCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// ボムを投射します。
    /// </summary>
    /// <param name="direction">投射方向</param>
    /// <param name="speed">投射速度</param>
    public void Launch(Vector2 direction, float speed) {
        if (_rb != null) {
            _rb.velocity = direction.normalized * speed;
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (_hasExploded) {
            return;
        }

        if (other.CompareTag(GROUND_TAG) || other.CompareTag(PLAYER_TAG)) {
            Explode();

            if (other.CompareTag(PLAYER_TAG) && other.TryGetComponent<PlayerMove>(out PlayerMove player)) {
                player.Die();
            }
        }
    }

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
        while (timer < _explosionDuration) {
            timer += Time.deltaTime;
            float t = timer / _explosionDuration;

            if (_bombCollider != null) {
                _bombCollider.radius = Mathf.Lerp(_startRadius, _endRadius, t);
            }
            yield return null;
        }

        // オブジェクトプールを考慮し、非アクティブ化
        gameObject.SetActive(false);
    }
}
