using UnityEngine;

/// <summary>
/// 敵キャラクター「カブトムシ」のAIと動作を制御するスクリプトです。
/// 一定の範囲内で上下に移動し、プレイヤーを検知するとボムを投下します。
/// Enemyクラスを継承しています。
/// </summary>
public class Bettle : Enemy {
    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プレイヤー検知設定")]
    [Tooltip("プレイヤーのTransform。見つからない場合は'Player'タグで自動検索します。")]
    [SerializeField] private Transform playerTransform;
    [Tooltip("プレイヤーを検知する半径")]
    [SerializeField] private float detectRange = 5f;

    [Header("ボム設定")]
    [Tooltip("投下するボムのPrefab")]
    [SerializeField] private GameObject bombPrefab;
    [Tooltip("ボムを投下する位置")]
    [SerializeField] private Transform launchPoint;
    [Tooltip("ボムを投下する速度")]
    [SerializeField] private float bombLaunchSpeed = 10f;
    [Tooltip("ボムを投下する間隔 (秒)")]
    [SerializeField] private float attackInterval = 3f;

    [Header("上下移動設定")]
    [Tooltip("上下移動の速度")]
    [SerializeField] private float verticalMoveSpeed = 1f;
    [Tooltip("上下移動の振幅 (中心からの距離)")]
    [SerializeField] private float verticalMoveRange = 2f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private float attackTimer;
    private Vector3 startPosition;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        // 親クラスのAwake()を呼び出し、基盤となる初期化を行う
        base.Awake();

        // プレイヤーのTransformが割り当てられていない場合、'Player'タグで自動検索
        if(playerTransform == null) {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if(playerObj != null) {
                playerTransform = playerObj.transform;
            }
            else {
                Debug.LogWarning("Bettle: 'Player'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。",this);
            }
        }

        // 初期位置を保存
        startPosition = transform.position;
        // 攻撃タイマーを初期化
        attackTimer = attackInterval;
    }

    private void Update() {
        // 死亡状態の場合は処理を停止
        if(IsDead) return;

        // ------------------
        // 上下移動処理
        // ------------------
        // Sin関数を使って、初期位置を中心に滑らかな上下移動を表現
        float newY = startPosition.y + Mathf.Sin(Time.time * verticalMoveSpeed) * verticalMoveRange;
        transform.position = new Vector3(transform.position.x,newY,transform.position.z);

        // ------------------
        // プレイヤー追跡・攻撃処理
        // ------------------
        if(playerTransform != null) {
            float distanceToPlayer = Vector2.Distance(transform.position,playerTransform.position);

            // プレイヤーが検知範囲内にいるかチェック
            if(distanceToPlayer < detectRange) {
                attackTimer -= Time.deltaTime;

                // 攻撃タイマーがゼロになったら攻撃
                if(attackTimer <= 0) {
                    Attack();
                    attackTimer = attackInterval; // タイマーをリセット
                }
            }
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------
    /// <summary>
    /// ボムを生成し、プレイヤーの方向へ投下します。
    /// </summary>
    private void Attack() {
        // 必要な参照が設定されているか確認
        if(bombPrefab == null) {
            Debug.LogError("Bettle: bombPrefabが割り当てられていません。",this);
            return;
        }
        if(launchPoint == null) {
            Debug.LogError("Bettle: launchPointが割り当てられていません。",this);
            return;
        }

        // ボムを生成
        GameObject bomb = Instantiate(bombPrefab,launchPoint.position,Quaternion.identity);

        // プレイヤーへの方向を正規化して取得
        Vector2 direction = (playerTransform.position - launchPoint.position).normalized;

        // ボムのスクリプトを取得して起動
        Bomb bombScript = bomb.GetComponent<Bomb>();
        if(bombScript != null) {
            bombScript.Launch(direction,bombLaunchSpeed);
        }
        else {
            Debug.LogError("Bettle: 生成したボムにBombコンポーネントが見つかりません。",this);
        }
    }
}
