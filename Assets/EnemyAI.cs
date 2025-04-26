using System.Collections;
using UnityEngine;
using Pathfinding;

public class EnemyAIBase : MonoBehaviour
{
    public Transform[] patrolPoints;
    public float speed;
    public float chaseRange;
    public float attackRange = 1f;
    public float attackCooldown;
    protected int currentPatrolIndex = 0;
    protected Transform target;
    protected Seeker seeker;
    protected Path path;
    protected int currentWaypoint = 0;
    protected Rigidbody2D rb;
    protected Animator animator;
    protected Transform player;
    protected bool isChasing = false;
    protected bool isAttacking = false;
    protected float lastAttackTime = 0.5f;
    private int patrolDirection = 1;
    private float lastDirection = 0f;
    public int maxHealth; // New variable
    protected int currentHealth; // New variable
    public float deathAnimationTime = 3f; // New variable
    public int damageForce = 10; // default damage value for player
    public HealthBar healthBar;
    public AudioClip detectionSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    private AudioSource sfxSource;
    private bool hasPlayedDetectionSound = false;

    protected virtual void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        target = patrolPoints[currentPatrolIndex];
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        InvokeRepeating(nameof(UpdatePath), 0f, 1f);

        currentHealth = maxHealth; // Initialize health

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(maxHealth);
            healthBar.Hide(); // la cache au départ
        }

        // 🔥 Créer un audio source dédié pour les SFX
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;
        sfxSource.volume = 0.2f; // Volume à 50% pour tous les sons joués par ce AudioSource
        sfxSource.spatialBlend = 0f; // 2D
    }

    // NEW METHOD: Handle taking damage
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (healthBar != null)
        {
            healthBar.SetHealth(currentHealth);
            if (healthBar != null && !healthBar.gameObject.activeSelf)
            {
                healthBar.Show(); // Afficher la barre de vie
            }
        }

        if (currentHealth <= 0)
        {
            animator.SetTrigger("4_Death"); // joue l’anim
            sfxSource.PlayOneShot(deathSound); // Play death sound
            rb.velocity = Vector2.zero;
            rb.simulated = false;

            StartCoroutine(DelayedDestroy());
        }
        else
        {
            animator.SetTrigger("3_Damaged"); // joue l’anim de dégât

            // Changer le pitch du sfx pour un effet plus natural
            sfxSource.clip = damageSound;
            sfxSource.pitch = Random.Range(0.9f, 1.2f); // Random pitch 🎯
            sfxSource.Play(); // ✅ Utilise Play() pas PlayOneShot()
            
            StartCoroutine(DamageFeedback()); // Joue le feedback de dégât  
        }
    }

    IEnumerator DamageFeedback()
    {
        // Knockback effect (push enemy slightly away from the enemy)
        GameObject player = GameObject.FindWithTag("Player"); // Assuming the player has the tag "Player"
        if (player != null)
        {
            Vector2 knockbackDirection = (transform.position - player.transform.position).normalized;
            rb.AddForce(knockbackDirection * 4f, ForceMode2D.Impulse); // Adjust knockback force
        }

        // Stop all movement and path updating
        rb.velocity = Vector2.zero;
        isChasing = false;
        isAttacking = true;
        seeker.enabled = false;

        // Wait for the knockback effect to complete
        yield return new WaitForSeconds(1.5f);

        // Resume movement and path updating
        isAttacking = false;
        seeker.enabled = true;
        rb.velocity = Vector2.zero;
    }

    IEnumerator DelayedDestroy()
    {
        yield return new WaitForSeconds(3f); // durée de l'animation de mort
        Destroy(gameObject); // supprime l’ennemi après
    }

    // NEW METHOD: Handle death
    protected virtual void Die()
    {
        // Disable all enemy functionality
        isAttacking = true;
        isChasing = false;
        rb.velocity = Vector2.zero; // Stop all movement
        rb.simulated = false; // Disable physics simulation
        this.enabled = false; // Disable the script
        GetComponent<Collider2D>().enabled = false; // Disable collisions

        // Stop pathfinding
        if (seeker != null)
        {
            seeker.enabled = false; // Disable the Seeker component
        }
        path = null; // Clear the current path

        // Play death animation
        animator.SetTrigger("4_Death");

        // Destroy after animation completes
        Destroy(gameObject, deathAnimationTime);
    }

    protected virtual void Update()
    {
        if (isAttacking) return;

        if (player == null || player.GetComponent<PlayerObj>().health <= 0)
        {
            isChasing = false;
            target = patrolPoints[currentPatrolIndex];
            hasPlayedDetectionSound = false; // 🔥 Remettre à zéro si player mort
            return;
        }

        float playerDistance = Vector2.Distance(transform.position, player.position);

        if (playerDistance < attackRange && Time.time > lastAttackTime + attackCooldown)
        {
            Attack();
        }
        else if (playerDistance < chaseRange)
        {
            if (!isChasing) // 🔥 Si on commence la chasse
            {
                if (detectionSound != null && sfxSource != null && !hasPlayedDetectionSound)
                {
                    sfxSource.PlayOneShot(detectionSound);
                    hasPlayedDetectionSound = true;
                }
            }
            isChasing = true;
            target = player;
        }
        else if (isChasing && playerDistance > chaseRange + 2f)
        {
            isChasing = false;
            target = patrolPoints[currentPatrolIndex];
            hasPlayedDetectionSound = false; // 🔥 Peut rejouer si perdu et retrouvé
        }
    }

    protected virtual void FixedUpdate()
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

        if (Mathf.Abs(rb.velocity.x) > 0.1f)
        {
            float newDirection = Mathf.Sign(rb.velocity.x);
            if (Mathf.Abs(newDirection - lastDirection) > 0.5f)
            {
                transform.localScale = new Vector3(-1.45f * newDirection, 1.45f, 1.45f);
                lastDirection = Mathf.Lerp(lastDirection, newDirection, Time.deltaTime * 5);
            }
        }

        float distance = Vector2.Distance(rb.position, path.vectorPath[currentWaypoint]);
        if (distance < 0.5f)
        {
            currentWaypoint++;
        }

        if (!isChasing && Vector2.Distance(rb.position, target.position) < 0.5f)
        {
            currentPatrolIndex += patrolDirection;
            if (currentPatrolIndex >= patrolPoints.Length || currentPatrolIndex < 0)
            {
                patrolDirection *= -1;
                currentPatrolIndex += patrolDirection;
            }
            target = patrolPoints[currentPatrolIndex];
        }
    }

    protected virtual void Attack()
    {
        if (isAttacking || player.GetComponent<PlayerObj>().isAction) return;

        float playerDistance = Vector2.Distance(transform.position, player.position);
        if (playerDistance > attackRange) return; // add this check

        isAttacking = true;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("2_Attack");
        animator.SetBool("1_Move", false);
        lastAttackTime = Time.time;

        StartCoroutine(ResumeAfterAttack());
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerObj playerObj = collision.GetComponent<PlayerObj>();
            if (playerObj != null)
            {
                float playerDistance = Vector2.Distance(transform.position, playerObj.transform.position);
                if (playerDistance <= attackRange)
                {
                    playerObj.TakeDamage(damageForce);
                }
            }
        }
    }

    public IEnumerator ResumeAfterAttack()
    {
        yield return new WaitForSeconds(0.3f);
        isAttacking = false;
        seeker.enabled = true;
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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            rb.velocity = Vector2.zero;
        }
    }
}