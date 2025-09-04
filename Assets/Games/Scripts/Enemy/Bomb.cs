using System.Collections;
using UnityEngine;

/// <summary>
/// 敵が投下するボムの挙動を制御するスクリプトです。
/// プレイヤーや地面に接触すると爆発し、コライダーを拡大してダメージを与えます。
/// </summary>
public class Bomb : MonoBehaviour {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("爆発設定")]
    [Tooltip("爆発アニメーションの再生時間。コライダーの拡大時間と合わせる。")]
    public float explosionDuration = 0.5f;

    [Header("コライダーアニメーション設定")]
    [Tooltip("爆発開始時のコライダーの半径")]
    [SerializeField] private float startRadius = 0.05f;
    [Tooltip("爆発終了時のコライダーの最大半径")]
    [SerializeField] private float endRadius = 0.5f;

    [Header("ターゲット設定")]
    [Tooltip("プレイヤーのタグ")]
    [SerializeField] private string playerTag = "Player";

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private Rigidbody2D rb;
    private Animator anim;
    private CircleCollider2D bombCollider;
    private bool hasExploded = false;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    private void Awake() {
        // 必要なコンポーネントの参照を取得
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        bombCollider = GetComponent<CircleCollider2D>();

        // コンポーネントが取得できなかった場合の警告
        if(rb == null) Debug.LogWarning("Bomb: Rigidbody2Dがアタッチされていません。",this);
        if(anim == null) Debug.LogWarning("Bomb: Animatorがアタッチされていません。",this);
        if(bombCollider == null) Debug.LogWarning("Bomb: CircleCollider2Dがアタッチされていません。",this);

        // コライダーの初期設定
        if(bombCollider != null) {
            bombCollider.radius = startRadius;
            bombCollider.isTrigger = true;
        }
    }

    /// <summary>
    /// ボムを指定された方向に指定された速度で発射します。
    /// </summary>
    /// <param name="direction">発射方向</param>
    /// <param name="speed">発射速度</param>
    public void Launch(Vector2 direction,float speed) {
        if(rb != null) {
            rb.velocity = direction.normalized * speed;
        }
    }

    /// <summary>
    /// 他のコライダーと接触したときに呼び出されます。
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other) {
        // 既に爆発済みなら処理をスキップ
        if(hasExploded) return;

        // 地面またはプレイヤーに触れたら爆発処理を開始
        if(other.CompareTag("Ground") || other.CompareTag(playerTag)) {
            Explode();

            // プレイヤーに当たった場合の処理（爆発開始時のみ）
            if(other.CompareTag(playerTag)) {
                PlayerMove playerMove = other.GetComponent<PlayerMove>();
                if(playerMove != null && !playerMove.IsDead) {
                    playerMove.Die();
                }
            }
        }
    }

    /// <summary>
    /// 他のコライダーと接触している間、毎フレーム呼び出されます。
    /// </summary>
    private void OnTriggerStay2D(Collider2D other) {
        // 爆発処理中（コライダーが拡大中）にプレイヤーが範囲内にいるかチェック
        if(hasExploded && other.CompareTag(playerTag)) {
            // プレイヤーのDie()メソッドを呼び出す
            PlayerMove playerMove = other.GetComponent<PlayerMove>();
            if(playerMove != null && !playerMove.IsDead) {
                playerMove.Die();
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ボムの爆発処理を開始します。
    /// </summary>
    private void Explode() {
        hasExploded = true; // 爆発フラグを立てて、二重に爆発しないようにする

        // ボムの移動を停止
        if(rb != null) {
            rb.velocity = Vector2.zero;
        }

        // 爆発アニメーションを再生
        if(anim != null) {
            anim.SetTrigger("Explode");
        }

        // 爆発コルーチンを開始
        StartCoroutine(ExplosionRoutine());
    }

    /// <summary>
    /// 爆発のアニメーションとコライダーの拡大を制御するコルーチン。
    /// </summary>
    private IEnumerator ExplosionRoutine() {
        float timer = 0f;
        while(timer < explosionDuration) {
            timer += Time.deltaTime;
            float t = timer / explosionDuration;

            if(bombCollider != null) {
                // 爆発に合わせてコライダーの半径をLerpで滑らかに広げる
                bombCollider.radius = Mathf.Lerp(startRadius,endRadius,t);
            }
            yield return null;
        }
    }

    /// <summary>
    /// オブジェクトを破棄する。アニメーションイベントから呼び出される。
    /// </summary>
    public void DestroyBomb() {
        Destroy(gameObject);
    }
}
