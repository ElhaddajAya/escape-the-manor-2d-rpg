using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public Transform player;
    public float moveSpeed = 2f;
    public float detectionRange = 5f;
    public float attackRange = 1f;
    public float attackCooldown = 2f;

    private float lastAttackTime;
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movementDirection;

    private enum EnemyState { IDLE, MOVE, ATTACK }
    private EnemyState currentState = EnemyState.IDLE;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Pas de gravité pour un jeu 2D top-down
        rb.freezeRotation = true; // Empêche la rotation
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
            Idle();
        }

        UpdateZIndex();
    }

    void MoveTowardsPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        movementDirection = direction;

        // Appliquer une vitesse fluide avec Rigidbody2D
        rb.velocity = direction * moveSpeed;

        // Gestion du Flip
        if (direction.x > 0) transform.localScale = new Vector3(-2, 2, 2);
        else if (direction.x < 0) transform.localScale = new Vector3(2, 2, 2);

        // Définir animations
        animator.SetBool("1_Move", true);
        animator.SetBool("2_Attack", false);

        currentState = EnemyState.MOVE;
    }

    void Attack()
    {
        rb.velocity = Vector2.zero; // Stopper le mouvement avant d'attaquer
        animator.SetBool("1_Move", false);
        animator.SetBool("2_Attack", true);
        animator.SetTrigger("attack");

        lastAttackTime = Time.time;
        Debug.Log("Enemy attacks the player!");

        currentState = EnemyState.ATTACK;
    }

    void Idle()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("1_Move", false);
        animator.SetBool("2_Attack", false);
        currentState = EnemyState.IDLE;
    }

    void UpdateZIndex()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.y * 0.01f);
    }
}
