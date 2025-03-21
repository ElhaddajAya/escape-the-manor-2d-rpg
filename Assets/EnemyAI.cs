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
    private float lastDirectionChangeTime = 0f;
    public float directionChangeCooldown = 0.5f; // Délai minimum entre les changements de direction

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

        // Si l'ennemi est bloqué, ajuster la direction
        if (IsStuck())
        {
            direction = AvoidObstacles(direction);
        }

        rb.velocity = direction * patrolSpeed;

        // Vérifier si l'ennemi est proche du point de patrouille
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
        if (Time.time - lastDirectionChangeTime < directionChangeCooldown)
        {
            return direction; // Ne pas changer de direction pendant le cooldown
        }

        // Rayon pour détecter les obstacles
        float rayDistance = obstacleAvoidanceDistance;
        Vector2 rayOrigin = transform.position;

        // Directions à tester : avant, gauche, droite, diagonales
        Vector2[] testDirections = {
            direction, // Avant
            new Vector2(-direction.y, direction.x), // Gauche
            new Vector2(direction.y, -direction.x), // Droite
            (direction + new Vector2(-direction.y, direction.x)).normalized, // Diagonale gauche
            (direction + new Vector2(direction.y, -direction.x)).normalized  // Diagonale droite
        };

        foreach (Vector2 testDir in testDirections)
        {
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, testDir, rayDistance, obstacleLayer);
            if (hit.collider == null)
            {
                // Si aucune collision, utiliser cette direction
                return testDir;
            }
        }

        // Si toutes les directions sont bloquées, reculer
        return -direction;
    }

    private bool IsStuck()
    {
        // Vérifier si l'ennemi est bloqué en détectant des collisions répétées
        RaycastHit2D hit = Physics2D.Raycast(transform.position, rb.velocity.normalized, obstacleAvoidanceDistance, obstacleLayer);
        return hit.collider != null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.layer == obstacleLayer)
        {
            // Calculer une nouvelle direction pour éviter l'obstacle
            Vector2 avoidDirection = (transform.position - collision.transform.position).normalized;

            // Appliquer la nouvelle direction
            rb.velocity = avoidDirection * patrolSpeed;
        }
    }
}