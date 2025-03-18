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
    public float _charMS = 4f;
    private PlayerState _currentState;

    // Reference to the Rigidbody2D component for physics-based movement
    private Rigidbody2D rb;
    public Vector3 _goalPos;
    public bool isAction = false;
    public Dictionary<PlayerState, int> IndexPair = new();

    public AudioClip footstepsSound; // Son des pas
    private AudioSource audioSource; // Composant AudioSource
    private bool isFootstepPlaying = false; // Pour vérifier si le son est déjà joué

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


        if (_dirMVec.x > 0) _prefabs.transform.localScale = new Vector3(-2, 2, 2);
        else if (_dirMVec.x < 0) _prefabs.transform.localScale = new Vector3(2, 2, 2);
    }

    public void SetMovePos(Vector2 pos)
    {
        isAction = false;
        _goalPos = pos;
        _currentState = PlayerState.MOVE;
    }
}
