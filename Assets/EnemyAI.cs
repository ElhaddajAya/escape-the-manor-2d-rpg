using UnityEngine;

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
    public float obstacleAvoidanceDistance = 1f; // Distance pour détecter les obstacles
    public LayerMask obstacleLayer; // Layer pour les obstacles
    private int patrolDirection = 1; // 1 pour aller, -1 pour retour


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < detectionRange)
        {
            isChasing = true;
        }
        else
        {
            isChasing = false;
        }

        if (isChasing)
        {
            ChasePlayer();
            if (distanceToPlayer < attackRange)
            {
                AttackPlayer();
            }
        }
        else
        {
            Patrol();
        }

        UpdateAnimations();
        UpdateFacingDirection();
    }

    void Patrol()
    {
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        // Éviter les obstacles
        direction = AvoidObstacles(direction);

        rb.velocity = direction * patrolSpeed;

        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // Mettre à jour l'index du point de patrouille en fonction de la direction
            currentPatrolIndex += patrolDirection;

            // Inverser la direction si on atteint le début ou la fin du tableau
            if (currentPatrolIndex >= patrolPoints.Length || currentPatrolIndex < 0)
            {
                patrolDirection *= -1; // Inverser la direction
                currentPatrolIndex += patrolDirection; // Revenir au point précédent
            }
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;

        // Éviter les obstacles
        direction = AvoidObstacles(direction);

        rb.velocity = direction * chaseSpeed;
    }

void AttackPlayer()
{
    // Arrêter le mouvement de l'ennemi
    rb.velocity = Vector2.zero;
    
    // Mettre ici la logique d'attaque
    animator.SetTrigger("2_Attack");
}

    void UpdateAnimations()
    {
        if (isChasing || rb.velocity.magnitude > 0.1f)
        {
            animator.SetBool("1_Move", true);
        }
        else
        {
            animator.SetBool("1_Move", false);
        }
    }

    void UpdateFacingDirection()
    {
        if (rb.velocity.x > 0)
        {
            transform.localScale = new Vector3(-2, 2, 2); // Faire face à droite
        }
        else if (rb.velocity.x < 0)
        {
            transform.localScale = new Vector3(2, 2, 2); // Faire face à gauche
        }
    }

    Vector2 AvoidObstacles(Vector2 direction)
    {
        // Eviter les objets ayant le tage "Obstacle"
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, obstacleAvoidanceDistance, obstacleLayer);

        if (hit.collider != null)
        {
            // Inverser la direction si un obstacle est détecté
            direction *= -1;
        }

        return direction;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            // Empêcher l'ennemi de pousser le joueur
            rb.velocity = Vector2.zero;
        }
    }
}