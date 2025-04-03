using System.Collections;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 4f;
    public float nextWaypointDistance = 0.5f;
    
    private int currentPatrolIndex = 0;
    private Transform target;
    private Seeker seeker;
    private Path path;
    private int currentWaypoint = 0;
    private Rigidbody2D rb;
    private Animator animator;
    public Transform player;
    public float chaseRange = 5f;
    private bool isChasing = false;
    public float attackRange = 1f;
    public float attackCooldown = 1f;
    private float lastAttackTime = 0f;
    private bool isAttacking = false;
    private int patrolDirection = 1; // 1 for forward, -1 for backward
    private float lastDirection = 0f; // Tracks the last movement direction for sprite flipping

    void Start()
    {
         seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        // Find player dynamically to avoid missing reference issues
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        target = patrolPoints[currentPatrolIndex];

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Update path every 1 second
        InvokeRepeating(nameof(UpdatePath), 0f, 1f);
    }

    void Update()
    {
        if (isAttacking) return; // Prevent movement while attacking

        if (player == null || player.GetComponent<PlayerObj>().health <= 0) 
        {
            // Player is dead, stop chasing and return to patrol
            isChasing = false;
            target = patrolPoints[currentPatrolIndex];
            return;
        }

        float playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance < attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
        }
        else if (playerDistance < chaseRange)
        {
            isChasing = true;
            target = player;
        }
        else if (isChasing && playerDistance > chaseRange + 2f)
        {
            isChasing = false;
            target = patrolPoints[currentPatrolIndex];
        }
    }

    void UpdatePath()
    {
        if (seeker.IsDone() && target != null)
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if (!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        if (path == null || isAttacking) return;

        if (currentWaypoint >= path.vectorPath.Count)
        {
            rb.velocity = Vector2.zero;
            animator.SetBool("1_Move", false);
            return;
        }

        Vector2 direction = ((Vector2)path.vectorPath[currentWaypoint] - rb.position).normalized;
        rb.velocity = direction * speed;
        animator.SetBool("1_Move", rb.velocity.magnitude > 0.1f);

        // Flip sprite based on movement direction
        if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            float newDirection = Mathf.Sign(rb.velocity.x);
            if (Mathf.Abs(newDirection - lastDirection) > 0.5f) // Prevents instant flips
            {
                transform.localScale = new Vector3(-1.45f * newDirection, 1.45f, 1.45f);
                lastDirection = Mathf.Lerp(lastDirection, newDirection, Time.deltaTime * 5);
            }
        }
        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Patrol logic
        if (!isChasing && Vector2.Distance(rb.position, target.position) < 0.5f)
        {
            // Update patrol index based on direction
            currentPatrolIndex += patrolDirection;

            // Reverse direction if at the end or start of patrol points
            if (currentPatrolIndex >= patrolPoints.Length || currentPatrolIndex < 0)
            {
                patrolDirection *= -1; // Reverse direction
                currentPatrolIndex += patrolDirection; // Correct the index
            }

            target = patrolPoints[currentPatrolIndex];
        }
    }

    void Attack() {
        if (isAttacking || player.GetComponent<PlayerObj>().isAction) return; // Prevent infinite attack loop

        isAttacking = true;
        rb.velocity = Vector2.zero; // Stop movement while attacking
        animator.SetTrigger("2_Attack");
        animator.SetBool("1_Move", false);
        lastAttackTime = Time.time;

        // Trigger damage animation on Player
        PlayerObj playerObj = player.GetComponent<PlayerObj>();
        if (playerObj != null) {
            playerObj.TakeDamage();
        }

        StartCoroutine(ResumeAfterAttack());
    }

    IEnumerator ResumeAfterAttack()
    {
        yield return new WaitForSeconds(0.3f); // Adjust based on attack animation duration
        isAttacking = false;
        seeker.enabled = true; // Re-enable pathfinding
    }

    // Prevent enemy from pushing Player and preventing Player from sliding off
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector2.zero;
        }
    }
}
