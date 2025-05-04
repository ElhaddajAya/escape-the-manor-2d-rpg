using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    private bool _isTransitioning = false;
    public AudioClip footstepsSound;
    private AudioSource audioSource;
    private bool isFootstepPlaying = false;
    private Animator animator;
    public int health = 150;
    private bool isAttacking = false;
    private bool isDead = false;

    public float attackRange = 3f;
    public LayerMask enemyLayer;
    public GameObject firePrefab;
    public Transform fireSpawnPoint;
    public GameObject batonObject;
    public HealthBar healthBar;
    [SerializeField] private AudioClip attackMeleeSound;
    [SerializeField] private AudioClip attackMagicSound;
    [SerializeField] private AudioClip damageSound;
    [SerializeField] private AudioClip deathSound;
    private AudioSource audioSourceSFX;
    [SerializeField] private float _transitionBlendTime = 0.3f;
    
    // Ajout de nouveaux paramètres pour gérer l'attaque
    [SerializeField] private float attackAnimationDuration = 0.43f; // Durée de l'animation d'attaque
    [SerializeField] private float attackCooldown = 0.1f; // Temps de récupération entre les attaques
    private float lastAttackTime = 0f; // Moment de la dernière attaque
    [SerializeField] private float minPitchVariation = 0.9f; // Variation minimale du pitch
    [SerializeField] private float maxPitchVariation = 1f; // Variation maximale du pitch
    
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
{
    Debug.Log("Scène chargée : " + scene.name);
    
    // Vérification de sécurité pour les références nulles
    if (rb == null)
    {
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D est null dans OnSceneLoaded!");
            return;
        }
    }
    
    // Reset all movement immediately
    rb.velocity = Vector2.zero;
    _goalPos = transform.position;
    _currentState = PlayerState.IDLE;
    
    // Vérifier si le joueur vient de mourir et a besoin de respawn dans Main_Scene
    if (GameState.PlayerNeedsRespawn && scene.name == "Main_Scene")
    {
        Debug.Log("Respawn dans Main_Scene après la mort dans: " + GameState.LastSceneBeforeDeath);
        
        // Forcer le joueur au DefaultSpawnPoint
        GameObject defaultSpawn = GameObject.Find("DefaultSpawnPoint");
        if (defaultSpawn != null)
        {
            transform.position = defaultSpawn.transform.position;
            _goalPos = transform.position;
            Debug.Log("Joueur replacé au DefaultSpawnPoint");
        }
        
        // Reset player state
        rb.simulated = true;
        health = 150;
        isDead = false;
        isAction = false;
        
        // Update health display
        if (healthBar != null)
        {
            healthBar.SetHealth(health);
        }
        
        GameState.PlayerNeedsRespawn = false;
    }
    else
    {
        // Fonctionnement normal pour les transitions de scène non-respawn
        Transform targetSpawnPoint = SpawnPointManager.GetTargetSpawnPoint();
        if (targetSpawnPoint != null)
        {
            Debug.Log("Spawn point trouvé : " + targetSpawnPoint.name);
            transform.position = targetSpawnPoint.position;
            _goalPos = transform.position;
        }
    }

    // IMPORTANT: Ne pas jouer l'animation tout de suite
    StartCoroutine(SafePlayAnimation());
    
    // Unfreeze after a slight delay
    StartCoroutine(CompleteTransitionReset());
}

    // Nouvelle coroutine pour jouer l'animation en toute sécurité
    private IEnumerator SafePlayAnimation()
    {
        // Attendre quelques frames pour s'assurer que tout est initialisé
        yield return new WaitForSeconds(0.1f);
        
        // Vérifier que _prefabs est prêt
        if (_prefabs != null)
        {
            // S'assurer que les listes d'animation sont initialisées
            if (!_prefabs.allListsHaveItemsExist())
            {
                _prefabs.PopulateAnimationLists();
                _prefabs.OverrideControllerInit();
            }
            
            // Maintenant c'est sûr de jouer l'animation
            PlayStateAnimation(PlayerState.IDLE);
        }
    }
    
    private IEnumerator CompleteTransitionReset()
    {
        // Wait for one full physics frame
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        
        // Reset all movement variables again
        rb.velocity = Vector2.zero;
        _goalPos = transform.position;
        _currentState = PlayerState.IDLE;
        isAction = false;
        
        // Unfreeze the game
        GameState.IsFrozen = false;
        _isTransitioning = false;
        // Optional: Gradually blend back to normal
        float elapsed = 0f;
        while (elapsed < _transitionBlendTime)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        _isTransitioning = false;
    }
    
    public void SetTransitioning(bool transitioning)
    {
        _isTransitioning = transitioning;
        if (transitioning)
        {
            rb.velocity = Vector2.zero;
            _goalPos = transform.position;
            _currentState = PlayerState.IDLE;
            isAction = true;
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
        audioSourceSFX.volume = 1f;
        audioSourceSFX.pitch = 1f; // Pitch par défaut
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
        
        // Vérifier si l'attaque est en cours de récupération
        if (Time.time - lastAttackTime < attackCooldown)
            yield break;
            
        // Enregistrer le temps d'attaque
        lastAttackTime = Time.time;
            
        isAttacking = true;
        // Ne pas bloquer le mouvement pendant l'attaque
        // isAction = true; <-- Supprimé pour permettre le mouvement pendant l'attaque

        // Jouer l'animation d'attaque
        animator.SetTrigger("2_Attack");

        // Appliquer une variation aléatoire au pitch du son d'attaque
        float randomPitch = UnityEngine.Random.Range(minPitchVariation, maxPitchVariation);
        audioSourceSFX.pitch = randomPitch;

        // Son attaque
        if (batonObject != null && batonObject.activeInHierarchy)
        {
            audioSourceSFX.PlayOneShot(attackMagicSound); // Son magique
        }
        else
        {
            audioSourceSFX.PlayOneShot(attackMeleeSound); // Son mêlée
        }

        // Délai avant de faire des dégâts (pour synchroniser avec l'animation)
        yield return new WaitForSeconds(0.2f); // Réduit pour être plus réactif

        // Attaque au corps (mains)
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<EnemyAIBase>(out var enemyAI))
            {
                // Attaque magique si le bâton est actif
                if (batonObject != null && batonObject.activeInHierarchy) {
                    enemyAI.TakeDamage(10);
                } else {
                    enemyAI.TakeDamage(5); // Attaque de base
                }
            }
        }

        // Attaque magique si le bâton est actif
        if (batonObject != null && batonObject.activeInHierarchy)
        {
            // Calcule la direction vers la souris ou un clic
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 direction = (mouseWorldPos - fireSpawnPoint.position).normalized;

            // Créer une étincelle à la fin du bâton
            CreateSparkEffect(fireSpawnPoint.position);
        }

        // Attendre que l'animation se termine (mais ne pas bloquer le mouvement)
        yield return new WaitForSeconds(attackAnimationDuration - 0.2f); // Reste de la durée
        
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
        // Early exit if frozen or transitioning
        if (GameState.IsFrozen || _isTransitioning)
        {
            // Stop movement
            rb.velocity = Vector2.zero;
            _goalPos = transform.position;
            _currentState = PlayerState.IDLE;

            // Stop footstep sound if playing
            if (isFootstepPlaying)
            {
                audioSource.Stop();
                isFootstepPlaying = false;
            }

            // Force idle animation
            PlayStateAnimation(PlayerState.IDLE);

            return;
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

        // 🗡 Attack input - permet des attaques répétées sans être bloqué par isAction
        if ((Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Space)))
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
        // Permet de bouger même pendant l'attaque, sauf si on est en transition ou mort
        if (GameState.IsFrozen || _isTransitioning || isDead)
            return;
            
        // Reset velocity before setting new position
        rb.velocity = Vector2.zero;
        _goalPos = pos;
        _currentState = PlayerState.MOVE;
    }

    public void TakeDamage(int damage) {
        // IMPORTANT: Ne pas prendre de dégâts si le jeu est gelé
        if (GameState.IsFrozen || isDead)
            return;

        // Play the damaged animation
        animator.SetTrigger("3_Damaged");

        // Variation aléatoire du pitch pour le son de dégât
        float randomPitch = UnityEngine.Random.Range(minPitchVariation, maxPitchVariation);
        audioSourceSFX.pitch = randomPitch;
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
        yield return new WaitForSeconds(0.10f); // Short delay before stopping sliding
        rb.velocity = Vector2.zero; // Stop any unwanted sliding
        isAction = false; // Allow movement again
    }

    public void Die() {
    // IMPORTANT: Ne pas mourir si le jeu est gelé
    if (GameState.IsFrozen)
        return;
            
    animator.SetTrigger("4_Death");
    // Son de mort avec pitch légèrement plus grave
    audioSourceSFX.pitch = UnityEngine.Random.Range(0.8f, 0.95f);
    audioSourceSFX.PlayOneShot(deathSound); // Play death sound
    Debug.Log("Player has died!");

    rb.velocity = Vector2.zero;
    rb.simulated = false;  // Stop physics simulation
    isAction = true;
    isDead = true;  // Set player as dead
    
    // Enregistrer la scène actuelle où le joueur est mort
    GameState.LastSceneBeforeDeath = SceneManager.GetActiveScene().name;
    GameState.PlayerNeedsRespawn = true;
    
    StartCoroutine(RespawnCoroutine()); // Start respawn process
}

IEnumerator RespawnCoroutine() {
    yield return new WaitForSeconds(3f); // Réduit de 5 à 3 secondes
    
    // IMPORTANT: On charge directement la Main_Scene
    health = 150;
    isDead = false;
    
    // Vérifier si nous sommes déjà dans la Main_Scene
    if (SceneManager.GetActiveScene().name != "Main_Scene")
    {
        // On s'assure que le SpawnPointManager utilisera DefaultSpawnPoint
        SpawnPointManager.SetTargetSpawnPoint("DefaultSpawnPoint");
        
        // On charge la Main_Scene
        SceneFader fader = FindObjectOfType<SceneFader>();
        if (fader != null)
        {
            fader.FadeToScene("Main_Scene");
        }
        else
        {
            SceneManager.LoadScene("Main_Scene");
        }
    }
    else
    {
        // Si déjà dans Main_Scene, téléporter directement au DefaultSpawnPoint
        ForceRespawnInMainScene();
    }
}

// Nouvelle méthode pour forcer le respawn dans la Main_Scene
private void ForceRespawnInMainScene()
{
    // Trouver le DefaultSpawnPoint dans la scène actuelle
    GameObject defaultSpawn = GameObject.Find("DefaultSpawnPoint");
    if (defaultSpawn != null)
    {
        transform.position = defaultSpawn.transform.position;
        Debug.Log("Player respawned at DefaultSpawnPoint in Main_Scene");
    }
    else
    {
        Debug.LogError("DefaultSpawnPoint not found in Main_Scene!");
        transform.position = Vector3.zero;
    }
    
    // Reset player state
    health = 150;
    if (rb != null)
    {
        rb.velocity = Vector2.zero;
        rb.simulated = true;
    }
    
    isDead = false;
    isAction = false;
    _goalPos = transform.position;
    _currentState = PlayerState.IDLE;
    
    // Restore health display
    if (healthBar != null)
    {
        healthBar.SetHealth(health);
    }
    
    // Play idle animation
    StartCoroutine(SafePlayAnimation());
    
    GameState.PlayerNeedsRespawn = false;
}

// Replaced Respawn() method with ForceRespawnInMainScene() method

    // Nouvelle coroutine pour jouer l'animation en toute sécurité après respawn
    private IEnumerator SafePlayAnimationAfterRespawn()
    {
        // Attendre quelques frames pour s'assurer que tout est initialisé
        yield return new WaitForSeconds(0.1f);
        
        // Vérifier que _prefabs est prêt
        if (_prefabs != null)
        {
            // S'assurer que les listes d'animation sont initialisées
            if (!_prefabs.allListsHaveItemsExist())
            {
                _prefabs.PopulateAnimationLists();
                _prefabs.OverrideControllerInit();
            }
            
            // Maintenant c'est sûr de jouer l'animation
            PlayStateAnimation(PlayerState.IDLE);
        }
    }
    }