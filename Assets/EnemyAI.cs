using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public Transform[] patrolPoints;
    private int currentPatrolIndex = 0;
    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private bool isChasing = false;
    private int patrolDirection = 1; // 1 for forward, -1 for backward

    private AIPath aiPath; // A* Pathfinding movement
    private AIDestinationSetter destinationSetter; // A* Target setting

    public LayerMask obstacleLayer;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        aiPath = GetComponent<AIPath>();
        destinationSetter = GetComponent<AIDestinationSetter>();

        // Set first patrol point as target
        if (patrolPoints.Length > 0)
            destinationSetter.target = patrolPoints[currentPatrolIndex];
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            isChasing = true;
            destinationSetter.target = player; // Follow player
            aiPath.maxSpeed = chaseSpeed;
        }
        else
        {
            isChasing = false;
            Patrol();
        }

        UpdateAnimations();
        UpdateFacingDirection();
    }

    void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        if (!isChasing) 
        {
            destinationSetter.target = patrolPoints[currentPatrolIndex]; // Move to patrol point
            aiPath.maxSpeed = patrolSpeed;
        }

        // Check if enemy reached patrol point
        if (Vector2.Distance(transform.position, patrolPoints[currentPatrolIndex].position) < 0.2f)
        {
            currentPatrolIndex += patrolDirection;
            if (currentPatrolIndex >= patrolPoints.Length || currentPatrolIndex < 0)
            {
                patrolDirection *= -1;
                currentPatrolIndex += patrolDirection;
            }
            destinationSetter.target = patrolPoints[currentPatrolIndex];
        }
    }

    void UpdateAnimations()
    {
        animator.SetBool("1_Move", aiPath.velocity.magnitude > 0.1f);
    }

    void UpdateFacingDirection()
    {
        if (aiPath.velocity.x > 0)
        {
            transform.localScale = new Vector3(-2, 2, 2); // Face right
        }
        else if (aiPath.velocity.x < 0)
        {
            transform.localScale = new Vector3(2, 2, 2); // Face left
        }
    }

    void AttackPlayer()
    {
        aiPath.maxSpeed = 0; // Stop enemy movement
        animator.SetTrigger("2_Attack");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector2.zero; // Stop pushing the player
        }
    }
}
