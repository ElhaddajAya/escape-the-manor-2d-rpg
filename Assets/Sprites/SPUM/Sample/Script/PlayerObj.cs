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
    public float _charMS; // Character Movement Speed
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
    public int health = 100;
    private bool isAttacking = false;
    private bool isDead = false; // New flag to track if the player is dead

    public float attackRange = 1.5f; // Add this new variable
    public LayerMask enemyLayer; // Add this and set it in Inspector to "Enemy" layer

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        Debug.Log("Scène chargée : " + scene.name);

        // Trouver le spawn point cible
        Transform targetSpawnPoint = SpawnPointManager.GetTargetSpawnPoint();

        if (targetSpawnPoint != null)
        {
            Debug.Log("Spawn point trouvé : " + targetSpawnPoint.name);
            // Déplacer le joueur au spawn point
            transform.position = targetSpawnPoint.position;
        }
        else
        {
            Debug.LogWarning("Le joueur n'a pas été déplacé car aucun spawn point n'a été trouvé.");
        }
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
    }

    public void SetStateAnimationIndex(PlayerState state, int index = 0)
    {
        IndexPair[state] = index;
    }

    public void PlayStateAnimation(PlayerState state)
    {
        _prefabs.PlayAnimation(state, IndexPair[state]);
    }

    // Replaced PerformAttack
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        isAction = true;
        
        // Play attack animation immediately
        animator.Play("ATTACK", -1, 0f);
        animator.SetTrigger("2_Attack");
        
        // Detect and damage enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(
            transform.position, 
            attackRange, 
            enemyLayer
        );
        
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyAIBase>(out var enemyAI))
            {
                enemyAI.TakeDamage(1); // Deal 1 damage per hit
            }
        }

        // Allow movement after 60% of animation
        yield return new WaitForSeconds(0.3f);
        isAction = false;
        
        // Full attack cooldown
        yield return new WaitForSeconds(0.2f);
        isAttacking = false;
    }
    void Update()
    {
        // If the player is dead, stop all movement, actions, and footstep sounds
        if (isDead)
        {
            isFootstepPlaying = false; // Stop footstep sound
            audioSource.Stop(); // Stop any playing sound
            return; // Skip the rest of the update logic when dead
        }

        // Handle player input for movement
        if (!isDead)
        {
            Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            if (inputDirection != Vector2.zero)
            {
                SetMovePos(transform.position + (Vector3)inputDirection);

                // Play footstep sound if player starts moving
                if (!isFootstepPlaying)
                {
                    audioSource.clip = footstepsSound;
                    audioSource.Play();
                    isFootstepPlaying = true;
                }
            }
            else
            {
                // Stop footstep sound when the player stops moving
                if (isFootstepPlaying)
                {
                    audioSource.Stop();
                    isFootstepPlaying = false;
                }
            }

            // Attack input (Space key)
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TriggerAttackAnimation();
            }

            transform.position = new Vector3(transform.position.x, transform.position.y, transform.localPosition.y * 0.01f);
            switch (_currentState)
            {
                case PlayerState.IDLE:
                    break;

                case PlayerState.MOVE:
                    DoMove();
                    break;
            }

            PlayStateAnimation(_currentState);
        }
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
        isAction = false;
        _goalPos = pos;
        _currentState = PlayerState.MOVE;
    }

    private void TriggerAttackAnimation()
    {
        if (isAction) return;
        
        animator.SetTrigger("2_Attack");
        isAction = true;
        StartCoroutine(ResetAfterAttack());
    }

    IEnumerator ResetAfterAttack()
    {
        yield return new WaitForSeconds(0.5f); // Match this to your attack animation length
        isAction = false;
    }

    public void TakeDamage() {
        if (isAction) return; // Prevent taking damage multiple times rapidly

        // Play the damaged animation
        animator.SetTrigger("3_Damaged");

        // Stop movement
        rb.velocity = Vector2.zero;
        isAction = true; // Temporarily prevent movement

        // Reduce health
        health -= 10;
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
        animator.SetTrigger("4_Death");
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
        health = 100;
        transform.position = GameObject.Find("DefaultSpawnPoint").transform.position;

        rb.velocity = Vector2.zero;  // Ensure any residual velocity is cleared.
        rb.simulated = true;  // Resume physics simulation

        isDead = false;  // Set player as alive
        isAction = false;  // Allow movement again

        // Make sure the player is not holding any previous directional input
        _goalPos = transform.position;  // Reset the goal position to the spawn point
        _currentState = PlayerState.IDLE;  // Make sure the player starts in the idle state

        Debug.Log("Player has respawned with full health!");
    }

}