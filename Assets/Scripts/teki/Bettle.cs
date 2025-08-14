using UnityEngine;
using Debug = UnityEngine.Debug;

public class Bettle : Enemy // MonoBehaviourをEnemyに置き換える
{
    [Header("プレイヤー検知設定")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private float detectRange = 5f;

    [Header("ボム設定")]
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private Transform launchPoint;
    [SerializeField] private float bombLaunchSpeed = 10f;
    [SerializeField] private float attackInterval = 3f;

    [Header("上下移動設定")]
    [SerializeField] private float verticalMoveSpeed = 1f;
    [SerializeField] private float verticalMoveRange = 2f;

    private float attackTimer;
    private Vector3 startPosition;

    protected override void Awake()
    {
        base.Awake();

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        startPosition = transform.position;
    }

    void Update()
    {
        if (IsDead) return;

        float newY = startPosition.y + Mathf.Sin(Time.time * verticalMoveSpeed) * verticalMoveRange;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);

        if (playerTransform != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer < detectRange)
            {
                attackTimer -= Time.deltaTime;
                if (attackTimer <= 0)
                {
                    Attack();
                    attackTimer = attackInterval;
                }
            }
        }
    }

    void Attack()
    {
        if (bombPrefab == null || launchPoint == null)
        {
            return;
        }

        GameObject bomb = Instantiate(bombPrefab, launchPoint.position, Quaternion.identity);
        Vector2 direction = (playerTransform.position - launchPoint.position).normalized;

        Bomb bombScript = bomb.GetComponent<Bomb>();
        if (bombScript != null)
        {
            bombScript.Launch(direction, bombLaunchSpeed);
        }
    }
}