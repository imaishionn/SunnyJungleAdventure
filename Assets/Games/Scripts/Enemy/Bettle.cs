using UnityEngine;

/// <summary>
/// 敵キャラクター「カブトムシ」のAIと動作を制御するスクリプトです。
/// 一定の範囲内で上下に移動し、プレイヤーを検知するとボムを投下します。
/// Enemyクラスを継承しています。
/// </summary>
public class Bettle : Enemy {
    // プレイヤーのタグを定数で定義
    private const string PLAYER_TAG = "Player";

    // ----------------------------------------------------------------------------------------------------
    // インスペクターで設定するパラメーター
    // ----------------------------------------------------------------------------------------------------
    [Header("プレイヤー検知設定")]
    [Tooltip("プレイヤーのTransform。見つからない場合は'Player'タグで自動検索します。")]
    [SerializeField]
    private Transform _playerTransform;
    [Tooltip("プレイヤーを検知する半径")]
    [SerializeField]
    private float _detectRange = 5f;

    [Header("ボム設定")]
    [Tooltip("投下するボムのPrefab")]
    [SerializeField]
    private GameObject _bombPrefab;
    [Tooltip("ボムを投下する位置")]
    [SerializeField]
    private Transform _launchPoint;
    [Tooltip("ボムを投下する速度")]
    [SerializeField]
    private float _bombLaunchSpeed = 10f;
    [Tooltip("ボムを投下する間隔 (秒)")]
    [SerializeField]
    private float _attackInterval = 3f;

    [Header("上下移動設定")]
    [Tooltip("上下移動の速度")]
    [SerializeField]
    private float _verticalMoveSpeed = 1f;
    [Tooltip("上下移動の振幅 (中心からの距離)")]
    [SerializeField]
    private float _verticalMoveRange = 2f;

    // ----------------------------------------------------------------------------------------------------
    // プライベート変数
    // ----------------------------------------------------------------------------------------------------
    private float _attackTimer;
    private Vector3 _startPosition;

    // ----------------------------------------------------------------------------------------------------
    // MonoBehaviourのライフサイクルメソッド
    // ----------------------------------------------------------------------------------------------------
    protected override void Awake() {
        base.Awake();
        FindPlayer();
        _startPosition = transform.position;
        _attackTimer = _attackInterval;
    }

    private void Update() {
        if (IsDead) {
            return;
        }

        MoveVertically();

        if (_playerTransform != null) {
            HandleAttack();
        }
    }

    // ----------------------------------------------------------------------------------------------------
    // プライベートメソッド
    // ----------------------------------------------------------------------------------------------------

    private void FindPlayer() {
        if (_playerTransform == null) {
            var playerObj = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObj != null) {
                _playerTransform = playerObj.transform;
            }
            else {
                Debug.LogWarning($"Bettle: '{PLAYER_TAG}'タグを持つGameObjectが見つかりません。プレイヤー追跡機能が無効になります。", this);
            }
        }
    }

    private void MoveVertically() {
        float newY = _startPosition.y + Mathf.Sin(Time.time * _verticalMoveSpeed) * _verticalMoveRange;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }

    private void HandleAttack() {
        float distanceToPlayer = Vector2.Distance(transform.position, _playerTransform.position);

        if (distanceToPlayer < _detectRange) {
            _attackTimer -= Time.deltaTime;

            if (_attackTimer <= 0) {
                Attack();
                _attackTimer = _attackInterval;
            }
        }
    }

    private void Attack() {
        if (_bombPrefab == null) {
            Debug.LogError("Bettle: bombPrefabが割り当てられていません。", this);
            return;
        }
        if (_launchPoint == null) {
            Debug.LogError("Bettle: launchPointが割り当てられていません。", this);
            return;
        }

        GameObject bomb = Instantiate(_bombPrefab, _launchPoint.position, Quaternion.identity);

        Vector2 direction = (_playerTransform.position - _launchPoint.position).normalized;

        if (bomb.TryGetComponent<Bomb>(out Bomb bombScript)) {
            bombScript.Launch(direction, _bombLaunchSpeed);
        }
        else {
            Debug.LogError("Bettle: 生成したボムにBombコンポーネントが見つかりません。", this);
        }
    }
}
