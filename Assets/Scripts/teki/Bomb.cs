using System.Collections;
using UnityEngine;

// MonoBehaviourを継承し、敵の基本クラスは継承しない
public class Bomb : MonoBehaviour
{
    // explosionDurationはアニメーションの長さに合わせます
    public float explosionDuration = 0.5f;

    [Header("コライダーアニメーション設定")]
    [SerializeField] private float startRadius = 0.05f; // 開始時の半径
    [SerializeField] private float endRadius = 0.5f;    // 終了時の半径

    [Header("プレイヤーのタグ")]
    [SerializeField] private string playerTag = "Player";

    private Rigidbody2D rb;
    private Animator anim;
    private CircleCollider2D bombCollider;
    private bool hasExploded = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bombCollider = GetComponent<CircleCollider2D>();

        // 初期状態のコライダーサイズを設定
        if (bombCollider != null)
        {
            bombCollider.radius = startRadius;
            // プレイヤーに触れたことを検知するため、Is Triggerを有効にする
            bombCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// ボムを指定された方向に指定された速度で発射する。
    /// </summary>
    public void Launch(Vector2 direction, float speed)
    {
        if (rb != null)
        {
            rb.velocity = direction.normalized * speed;
        }
    }

    // プレイヤーや地面に触れたときに爆発する
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;

        // プレイヤー、または地面に触れたら爆発
        if (other.CompareTag(playerTag) || other.CompareTag("Ground"))
        {
            Explode(other);
        }
    }

    /// <summary>
    /// ボムの爆発処理。
    /// </summary>
    private void Explode(Collider2D target)
    {
        hasExploded = true;

        if (rb != null)
        {
            rb.velocity = Vector2.zero;
        }

        if (anim != null)
        {
            // ここで"Explode"トリガーを呼び出す
            anim.SetTrigger("Explode");
        }

        // ターゲットがプレイヤーだった場合の処理
        if (target.CompareTag(playerTag))
        {
            PlayerMove playerMove = target.GetComponent<PlayerMove>();
            if (playerMove != null && !playerMove.IsDead)
            {
                playerMove.Die();
            }
        }

        // 爆発コルーチンを開始
        StartCoroutine(ExplosionRoutine());
    }

    private IEnumerator ExplosionRoutine()
    {
        float timer = 0f;
        while (timer < explosionDuration)
        {
            timer += Time.deltaTime;
            float t = timer / explosionDuration;

            if (bombCollider != null)
            {
                // 爆発に合わせてコライダーの半径を広げる
                bombCollider.radius = Mathf.Lerp(startRadius, endRadius, t);
            }
            yield return null;
        }

        // 爆発終了時にオブジェクトを破棄
        Destroy(gameObject);
    }
}