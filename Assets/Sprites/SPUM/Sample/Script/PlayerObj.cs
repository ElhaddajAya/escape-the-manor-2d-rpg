using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

// This file contains a script that manages the player object in a 2D game.
// The player object can move and play animations based on user input.
public class PlayerObj : MonoBehaviour
{
    public SPUM_Prefabs _prefabs;
    public float _charMS = 8; // Character Movement Speed
    private PlayerState _currentState;

    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;
    public Vector3 _goalPos;
    public bool isAction = false;
    public Dictionary<PlayerState, int> IndexPair = new();

    public AudioClip footstepsSound; // Son des pas
    private AudioSource audioSource; // Composant AudioSource
    private bool isFootstepPlaying = false; // Pour vérifier si le son est déjà joué
    private Animator animator;
    public int health = 150;
    private bool isAttacking = false;
    private bool isDead = false; // New flag to track if the player is dead

    public float attackRange = 3f; // Add this new variable
    public LayerMask enemyLayer; // Add this and set it in Inspector to "Enemy" layer
    public GameObject firePrefab; // Le feu à lancer (prefab avec animation)
    public Transform fireSpawnPoint; // Point de départ du feu (ex: la main du joueur)
    public GameObject batonObject; // L'objet "Bâton" dans la hiérarchie
    public HealthBar healthBar;
    [SerializeField] private AudioClip attackMeleeSound;
    [SerializeField] private AudioClip attackMagicSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    private AudioSource audioSourceSFX;

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
{
    Debug.Log("Scène chargée : " + scene.name);

    // Find target spawn point
    Transform targetSpawnPoint = SpawnPointManager.GetTargetSpawnPoint();

    if (targetSpawnPoint != null)
    {
        Debug.Log("Spawn point trouvé : " + targetSpawnPoint.name);
        // Move player to spawn point
        transform.position = targetSpawnPoint.position;
        
        // Unfreeze after positioning
        StartCoroutine(UnfreezeAfterSpawn());
    }
    else
    {
        Debug.LogWarning("Aucun spawn point trouvé.");
        StartCoroutine(UnfreezeAfterSpawn());
    }
}

private IEnumerator UnfreezeAfterSpawn()
{
    yield return new WaitForSeconds(0.5f);
    GameState.IsFrozen = false;
    Debug.Log("Game unfrozen after scene transition");
}
    
    void Awake()
    {
        // Vérifie s'il existe déjà un Player dans la scène
        if (FindObjectsOfType<PlayerObj>().Length > 1)
        {
            Destroy(gameObject); // Évite les doublons
            return;
        }

        DontDestroyOnLoad(gameObject); // Empêche la destruction du Player entre les scènes

        // Abonnez-vous à l'événement SceneManager.sceneLoaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        // Désabonnez-vous pour éviter les fuites de mémoire
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Start()
    {
        Debug.Log("Time.timeScale: " + Time.timeScale);
        
        // Récupérer le composant AudioSource
        audioSource = GetComponent<AudioSource>();

        // Vérifiez si le clip audio des pas est assigné
        if (footstepsSound == null)
        {
            Debug.LogError("Le clip audio des pas n'est pas assigné !");
        }

        if (healthBar != null)
        {
            healthBar.SetMaxHealth(health);
            healthBar.Hide(); // la cache au départ
        }

        // Assurez-vous que l'AudioSource est configuré pour la lecture en boucle
        if (audioSource != null)
        {
            audioSource.loop = true; // Le son sera joué en boucle
        }

        // Get the Rigidbody2D component attached to the player
        rb = GetComponent<Rigidbody2D>();

        // Prevent the player from rotating when colliding with objects
        rb.freezeRotation = true;

        if (_prefabs == null)
        {
            _prefabs = GetComponent<SPUM_Prefabs>();
            if (!_prefabs.allListsHaveItemsExist())
            {
                _prefabs.PopulateAnimationLists();
            }
        }
        _prefabs.OverrideControllerInit();
        foreach (PlayerState state in Enum.GetValues(typeof(PlayerState)))
        {
            IndexPair[state] = 0;
        }

        animator = GetComponentInChildren<Animator>();

        // Créer un AudioSource indépendant pour les SFX (attaque, dégâts, mort)
        audioSourceSFX = gameObject.AddComponent<AudioSource>();
        audioSourceSFX.playOnAwake = false;
        audioSourceSFX.loop = false;
        audioSourceSFX.spatialBlend = 0f;
        audioSourceSFX.volume = 1f; // Tu peux ajuster
    }

    public void SetStateAnimationIndex(PlayerState state, int index = 0)
    {
        IndexPair[state] = index;
    }

    public void PlayStateAnimation(PlayerState state)
    {
        _prefabs.PlayAnimation(state, IndexPair[state]);
    }

    IEnumerator PerformAttack()
    {
        // IMPORTANT: Ne pas attaquer si le jeu est gelé
        if (GameState.IsFrozen)
            yield break;
            
        isAttacking = true;
        isAction = true;

        // Jouer l'animation d'attaque
        animator.SetTrigger("2_Attack");

        // 🔊 Son attaque
        if (batonObject != null && batonObject.activeInHierarchy)
        {
            audioSourceSFX.PlayOneShot(attackMagicSound); // Son magique
        }
        else
        {
            audioSourceSFX.PlayOneShot(attackMeleeSound); // Son mêlée
        }

        yield return new WaitForSeconds(0.5f); // Petit délai avant d'agir

        // 🎯 Attaque au corps (mains)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyAIBase>(out var enemyAI))
            {
                // 🔥 Attaque magique si le bâton est actif
                if (batonObject != null && batonObject.activeInHierarchy) {
                    enemyAI.TakeDamage(10);
                } else {
                    enemyAI.TakeDamage(5); // Attaque de base
                }
            }
        }

        // 🔥 Attaque magique si le bâton est actif
        if (batonObject != null && batonObject.activeInHierarchy)
        {
            // Calcule la direction vers la souris ou un clic
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mouseWorldPos - fireSpawnPoint.position).normalized;

            // Créer une étincelle à la fin du bâton
            CreateSparkEffect(fireSpawnPoint.position); // Nouvelle fonction pour gérer l'étincelle
        }

        isAction = false;
        isAttacking = false;
    }

    void CreateSparkEffect(Vector3 spawnPosition)
    {
        // Crée une petite étincelle ou un effet visuel au bout du bâton
        GameObject spark = Instantiate(firePrefab, spawnPosition, Quaternion.identity); 
        // Tu peux personnaliser la durée de l'étincelle si nécessaire
        Destroy(spark, 0.2f); // Supprimer l'étincelle après un court délai
    }

    void Update()
    {
        // 🔒 IMPORTANT: Freeze check - si le jeu est gelé, arrêter TOUT mouvement et interaction
        if (GameState.IsFrozen)
        {
            // S'assurer que tout son s'arrête
            if (isFootstepPlaying)
            {
                audioSource.Stop();
                isFootstepPlaying = false;
            }
            
            // S'assurer que le joueur reste immobile
            rb.velocity = Vector2.zero;
            
            // Garder la position actuelle
            _goalPos = transform.position;
            
            // Rester en état IDLE
            _currentState = PlayerState.IDLE;
            
            return; // Ne pas traiter le reste de la mise à jour
        }

        // 💀 Death check: stop everything if dead
        if (isDead)
        {
            isFootstepPlaying = false;
            audioSource.Stop();
            return;
        }

        // 🎮 Movement input
        Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (inputDirection != Vector2.zero)
        {
            SetMovePos(transform.position + (Vector3)inputDirection);

            if (!isFootstepPlaying)
            {
                audioSource.clip = footstepsSound;
                audioSource.Play();
                isFootstepPlaying = true;
            }
        }
        else
        {
            if (isFootstepPlaying)
            {
                audioSource.Stop();
                isFootstepPlaying = false;
            }
        }

        // 🗡 Attack input
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space)) && !isAttacking)
        {
            StartCoroutine(PerformAttack());
        }

        // 🧭 Depth sorting
        transform.position = new Vector3(transform.position.x, transform.position.y, transform.localPosition.y * 0.01f);

        // 🧍‍♂️ State behavior
        switch (_currentState)
        {
            case PlayerState.IDLE:
                break;

            case PlayerState.MOVE:
                DoMove();
                break;
        }

        // 🕺 Play animation
        PlayStateAnimation(_currentState);
    }

    void DoMove()
    {
        Vector3 _dirVec = _goalPos - transform.position;
        Vector3 _disVec = (Vector2)_goalPos - (Vector2)transform.position;
        if (_disVec.sqrMagnitude < 0.1f)
        {
            _currentState = PlayerState.IDLE;
            return;
        }
        Vector3 _dirMVec = _dirVec.normalized;
        transform.position += _dirMVec * _charMS * Time.deltaTime;

        if (_dirMVec.x > 0) _prefabs.transform.localScale = new Vector3(-1.45f, 1.45f, 1.45f);
        else if (_dirMVec.x < 0) _prefabs.transform.localScale = new Vector3(1.45f, 1.45f, 1.45f);
    }

    public void SetMovePos(Vector2 pos)
    {
        // IMPORTANT: Ne pas accepter de mouvement si le jeu est gelé
        if (GameState.IsFrozen)
            return;
            
        isAction = false;
        _goalPos = pos;
        _currentState = PlayerState.MOVE;
    }

    private void TriggerAttackAnimation()
    {
        // IMPORTANT: Ne pas attaquer si le jeu est gelé
        if (GameState.IsFrozen || isAction)
            return;
        
        animator.SetTrigger("2_Attack");
        isAction = true;
        StartCoroutine(ResetAfterAttack());
    }

    IEnumerator ResetAfterAttack()
    {
        yield return new WaitForSeconds(0.5f); // Match this to your attack animation length
        isAction = false;
    }

    public void TakeDamage(int damage) {
        // IMPORTANT: Ne pas prendre de dégâts si le jeu est gelé
        if (GameState.IsFrozen || isAction)
            return;

        // Play the damaged animation
        animator.SetTrigger("3_Damaged");

        audioSourceSFX.PlayOneShot(damageSound); // Play damage sound

        // Stop movement
        rb.velocity = Vector2.zero;
        isAction = true; // Temporarily prevent movement

        // Reduce health
        health -= damage;

        // Mettre à jour la barre de vie
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
            if (healthBar != null && !healthBar.gameObject.activeSelf)
            {
                healthBar.Show(); // Afficher la barre de vie
            }
        }

        if (health <= 0) {
            Die();
            return;
        }

        // Knockback effect (push player slightly away from the enemy)
        GameObject enemy = GameObject.FindWithTag("Enemy"); // Assuming the enemy has the tag "Enemy"
        if (enemy != null)
        {
            Vector2 knockbackDirection = (transform.position - enemy.transform.position).normalized;
            rb.AddForce(knockbackDirection * 3f, ForceMode2D.Impulse); // Adjust knockback force
        }

        // Prevent sliding by resetting velocity after a short delay
        StartCoroutine(ResetVelocityAfterKnockback());
    }

    IEnumerator ResetVelocityAfterKnockback() {
        yield return new WaitForSeconds(0.15f); // Short delay before stopping sliding
        rb.velocity = Vector2.zero; // Stop any unwanted sliding
        isAction = false; // Allow movement again
    }
    
    IEnumerator RecoverFromHit() {
        yield return new WaitForSeconds(0.3f); // Time to recover
        isAction = false; // Allow movement again
    }

    public void Die() {
        // IMPORTANT: Ne pas mourir si le jeu est gelé
        if (GameState.IsFrozen)
            return;
            
        animator.SetTrigger("4_Death");
        audioSourceSFX.PlayOneShot(deathSound); // Play death sound
        Debug.Log("Player has died!");

        rb.velocity = Vector2.zero;
        rb.simulated = false;  // Stop physics simulation
        isAction = true;
        isDead = true;  // Set player as dead

        StartCoroutine(RespawnCoroutine()); // Start respawn process
    }

    IEnumerator RespawnCoroutine() {
        yield return new WaitForSeconds(5f); // Wait before respawning
        Respawn();
    }

    public void Respawn() {
        // IMPORTANT: Ne pas respawn si le jeu est gelé
        if (GameState.IsFrozen)
            return;
            
        health = 150;
        transform.position = GameObject.Find("DefaultSpawnPoint").transform.position;

        rb.velocity = Vector2.zero;  // Ensure any residual velocity is cleared.
        rb.simulated = true;  // Resume physics simulation

        isDead = false;  // Set player as alive
        isAction = false;  // Allow movement again

        // Refill health
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }

        // Make sure the player is not holding any previous directional input
        _goalPos = transform.position;  // Reset the goal position to the spawn point
        _currentState = PlayerState.IDLE;  // Make sure the player starts in the idle state

        Debug.Log("Player has respawned with full health!");
    }
}