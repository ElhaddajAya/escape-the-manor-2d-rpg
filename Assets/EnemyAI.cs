using System.Collections;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed = 2f;
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

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();

        target = patrolPoints[currentPatrolIndex];

        // Update path every 0.5s for faster response
        InvokeRepeating(nameof(UpdatePath), 0f, 0.5f);
    }

    void Update()
    {
        if (isAttacking) return; // Prevent movement while attacking

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

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        // Patrol logic
        if (!isChasing && Vector2.Distance(rb.position, target.position) < 0.5f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
            target = patrolPoints[currentPatrolIndex];
        }
    }

    void Attack()
    {
        isAttacking = true;
        rb.velocity = Vector2.zero; // Stop movement while attacking
        animator.SetTrigger("2_Attack");
        animator.SetBool("1_Move", false);
        lastAttackTime = Time.time;

        // Resume movement after attack
        StartCoroutine(ResumeAfterAttack());
    }

    IEnumerator ResumeAfterAttack()
    {
        yield return new WaitForSeconds(0.5f); // Adjust based on attack animation duration
        isAttacking = false;
    }
}
