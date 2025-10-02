using System.Collections;
using UnityEngine;

/// <summary>
/// 敵キャラクター「カブトムシ」のAIと動作を制御するスクリプト。
/// 一定の範囲内で上下に移動し、プレイヤーを検知するとボムを投下します。
/// </summary>
public class Bettle : Enemy {
    private const string PLAYER_TAG = "Player";

    [Header("プレイヤーのTransform"), SerializeField]
    private Transform _playerTransform;

    [Header("プレイヤーの検知設定"), SerializeField]
    private float _detectRange = 5f; 

    [Header("ボムを投下する位置"), SerializeField]
    private Transform _launchPoint;

    [Header("ボムのプレハブ"), SerializeField]
    private GameObject _bombPrefab;

    [Header("ボムの投下速度"), SerializeField]
    private float _bombLaunchSpeed = 10f;

    [Header("爆発攻撃のクールタイム"), SerializeField]
    private float _attackInterval = 3f; 

    [Header("上下移動の速度"), SerializeField]
    private float _verticalMoveSpeed = 1f;

    [Header("上下移動の範囲"), SerializeField]
    private float _verticalMoveRange = 2f;

    /// <summary>
    /// 攻撃するまでのタイマー
    /// </summary>
    private float _attackIntervalTimer;

    /// <summary>
    /// 初期位置
    /// </summary>
    private Vector3 _startPosition;

    protected override void Awake() {
        base.Awake();
        FindPlayer();
        _startPosition = transform.position;
    }

    protected override void OnEnable() {
        base.OnEnable();
        _attackIntervalTimer = _attackInterval; // オブジェクト再利用時にタイマーをリセット
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

    private void FindPlayer() {
        if (_playerTransform == null) {
            var playerObject = GameObject.FindGameObjectWithTag(PLAYER_TAG);
            if (playerObject != null) {
                _playerTransform = playerObject.transform;
            }
            else {
                Debug.LogWarning("Bettle: 'Player'タグのGameObjectが見つかりません。プレイヤー追跡は無効になります。", this);
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
            _attackIntervalTimer -= Time.deltaTime;

            if (_attackIntervalTimer <= 0) {
                Attack();
                _attackIntervalTimer = _attackInterval;
            }
        }
    }

    private void Attack() {
        if (_bombPrefab == null || _launchPoint == null) {
            Debug.LogError("Bettle: ボムのプレハブまたは投下位置が設定されていません。", this);
            return;
        }

        GameObject bomb = Instantiate(_bombPrefab, _launchPoint.position, Quaternion.identity);

        Vector2 direction = (_playerTransform.position - _launchPoint.position).normalized;

        if (bomb.TryGetComponent<Bomb>(out Bomb bombScript)) {
            bombScript.Launch(direction, _bombLaunchSpeed);
        }
        else {
            Debug.LogError("Bettle: 生成したオブジェクトにBombコンポーネントがありません。", this);
        }
    }
}
