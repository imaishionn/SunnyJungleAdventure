using UnityEngine;
using Debug = UnityEngine.Debug;

public class Dino : Enemy // Enemy‚ðŒp³
{
    [Header("ˆÚ“®‘¬“x")]
    [SerializeField] float moveSpeed = 3f;

    [Header("ˆÚ“®”ÍˆÍ")]
    [SerializeField] float patrolRange = 5f;

    [Header("ŠR‚ÌŒŸ’mÝ’è")]
    [SerializeField] private float groundCheckDistance = 0.6f;
    [SerializeField] private LayerMask groundLayer;

    private Vector2 initialPosition;
    private int moveDirection = 1;

    protected override void Awake()
    {
        base.Awake();
        initialPosition = transform.position;

        if (m_rb != null)
        {
            m_rb.gravityScale = 1f;
            m_rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }

    protected void FixedUpdate()
    {
        if (IsDead) return;

        Vector2 startPoint = transform.position + new Vector3(moveDirection * 0.5f, -0.5f, 0);
        RaycastHit2D groundHit = Physics2D.Raycast(startPoint, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(startPoint, Vector2.down * groundCheckDistance, Color.red);

        if (groundHit.collider == null ||
            (moveDirection == 1 && transform.position.x > initialPosition.x + patrolRange) ||
            (moveDirection == -1 && transform.position.x < initialPosition.x - patrolRange))
        {
            moveDirection *= -1;
            FlipSprite();
        }

        m_rb.velocity = new Vector2(moveDirection * moveSpeed, m_rb.velocity.y);

        if (m_animator != null && HasAnimatorParameter("run", AnimatorControllerParameterType.Bool))
        {
            m_animator.SetBool("run", true);
        }
    }

    void FlipSprite()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}