using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public Transform waypointA; // First patrol point
    public Transform waypointB; // Second patrol point
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 2f;
    
    private float lastAttackTime;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movementDirection;
    private Transform currentTarget; // Current patrol target

    private enum EnemyState { IDLE, PATROL, MOVE, ATTACK }
    private EnemyState currentState = EnemyState.PATROL; // Start with patrolling

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // No gravity for 2D top-down
        rb.freezeRotation = true; // Prevents rotation
        currentTarget = waypointA; // Start patrolling towards waypointA
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else if (Time.time >= lastAttackTime + attackCooldown)
            {
                Attack();
            }
        }
        else
        {
            Patrol(); // Move between waypoints when no player detected
        }

        UpdateZIndex();
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        movementDirection = direction;

        rb.velocity = direction * moveSpeed;

        FlipSprite(direction);

        animator.SetBool("1_Move", true);
        animator.SetBool("2_Attack", false);

        currentState = EnemyState.MOVE;
    }

    void Attack()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("1_Move", false);
        animator.SetBool("2_Attack", true);
        animator.SetTrigger("attack");

        lastAttackTime = Time.time;
        Debug.Log("Enemy attacks the player!");

        currentState = EnemyState.ATTACK;
    }

    void Patrol()
    {
        if (currentState != EnemyState.PATROL)
        {
            currentState = EnemyState.PATROL;
            animator.SetBool("1_Move", true);
        }

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        rb.velocity = direction * (moveSpeed * 0.5f); // Slower speed for patrolling

        FlipSprite(direction);

        // Check if the enemy reached the current target
        if (Vector2.Distance(transform.position, currentTarget.position) < 0.2f)
        {
            currentTarget = (currentTarget == waypointA) ? waypointB : waypointA; // Switch target
        }
    }

    void Idle()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("1_Move", false);
        animator.SetBool("2_Attack", false);
        currentState = EnemyState.IDLE;
    }

    void FlipSprite(Vector2 direction)
    {
        if (direction.x > 0) transform.localScale = new Vector3(-2, 2, 2);
        else if (direction.x < 0) transform.localScale = new Vector3(2, 2, 2);
    }

    void UpdateZIndex()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.01f);
    }
}
