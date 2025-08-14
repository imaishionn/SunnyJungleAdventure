using System.Collections;
using UnityEngine;

// Resolves ambiguous reference to Debug
using Debug = UnityEngine.Debug;

// Unityエディタ上でのみ使用する名前空間を定義
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MovingPlatform : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Movement speed for the back-and-forth platform")]
    [SerializeField] private float moveSpeed = 2.0f;
    [Tooltip("Wait time before moving to the next destination")]
    [SerializeField] private float waitTimeAtPoint = 1.0f;
    [Tooltip("Movement distance along the X-axis (positive for right, negative for left)")]
    [SerializeField] private float moveDistanceX = 0f;
    [Tooltip("Movement distance along the Y-axis (positive for up, negative for down)")]
    [SerializeField] private float moveDistanceY = 0f;

    // The platform's initial position
    private Vector3 initialPosition;
    // The target position for back-and-forth movement
    private Vector3 targetPosition;
    // The end position offset from the initial position
    private Vector3 endOffsetPosition;

    private bool movingToEnd = true;
    private bool isPlayerOnPlatform = false;

    // Variable to manage the movement coroutine
    private Coroutine movementCoroutine;

    void Start()
    {
        initialPosition = transform.position;
        endOffsetPosition = initialPosition + new Vector3(moveDistanceX, moveDistanceY, 0f);

        if (moveDistanceX != 0f || moveDistanceY != 0f)
        {
            targetPosition = endOffsetPosition;
            movementCoroutine = StartCoroutine(MovePlatform());
        }
        else
        {
            Debug.LogWarning("MovingPlatform: Movement distance is 0. The platform will not move.", this);
        }
    }

    private IEnumerator MovePlatform()
    {
        while (true)
        {
            while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
                yield return null;
            }

            yield return new WaitForSeconds(waitTimeAtPoint);

            movingToEnd = !movingToEnd;
            targetPosition = movingToEnd ? endOffsetPosition : initialPosition;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isPlayerOnPlatform) return;

            isPlayerOnPlatform = true;
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (!other.IsTouching(GetComponent<Collider2D>()))
            {
                isPlayerOnPlatform = false;
                if (other.transform.parent == transform)
                {
                    other.transform.SetParent(null);
                }
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        Vector3 gizmoInitialPos = EditorApplication.isPlayingOrWillChangePlaymode ? initialPosition : transform.position;
        Vector3 gizmoEndOffsetPos = gizmoInitialPos + new Vector3(moveDistanceX, moveDistanceY, 0f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gizmoInitialPos, gizmoEndOffsetPos);
        Gizmos.DrawWireSphere(gizmoInitialPos, 0.2f);
        Gizmos.DrawWireSphere(gizmoEndOffsetPos, 0.2f);
    }
#endif
}