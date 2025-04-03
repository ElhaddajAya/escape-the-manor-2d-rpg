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

    void Update()
    {
        if (isAction) return;

        // Handle player input for movement
        Vector2 inputDirection = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        if (inputDirection != Vector2.zero)
        {
            SetMovePos(transform.position + (Vector3)inputDirection);

            // Jouer le son des pas si le joueur commence à bouger
            if (!isFootstepPlaying)
            {
                audioSource.clip = footstepsSound;
                audioSource.Play();
                isFootstepPlaying = true;
            }
        }
        else
        {
            // Arrêter le son des pas lorsque le joueur arrête de se déplacer
            if (isFootstepPlaying)
            {
                audioSource.Stop();
                isFootstepPlaying = false;
            }
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


        if (_dirMVec.x > 0) _prefabs.transform.localScale = new Vector3(-1.3f, 1.3f, 1.3f);
        else if (_dirMVec.x < 0) _prefabs.transform.localScale = new Vector3(1.3f, 1.3f, 1.3f);
    }

    public void SetMovePos(Vector2 pos)
    {
        isAction = false;
        _goalPos = pos;
        _currentState = PlayerState.MOVE;
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
        rb.simulated = false;
        isAction = true;

        StartCoroutine(RespawnCoroutine()); // Call coroutine instead of using WaitForSeconds directly
    }

    IEnumerator RespawnCoroutine() {
        yield return new WaitForSeconds(5f); // Wait before respawning
        Respawn();
    }

    public void Respawn() {
        health = 100;
        transform.position = GameObject.Find("DefaultSpawnPoint").transform.position;

        rb.simulated = true;
        isAction = false;
        Debug.Log("Player has respawned with full health!");
    }

}
