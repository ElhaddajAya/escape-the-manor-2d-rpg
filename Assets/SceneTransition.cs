using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Nom de la scène à charger
    [SerializeField] private AudioClip doorSound; // Effet sonore d'ouverture de porte
    [SerializeField] private AudioClip errorSound; // Effet sonore d'erreur (clé incorrecte)
    [SerializeField] private AudioSource audioSource; // Source audio pour jouer le son
    [SerializeField] private Animator animator; // Référence à l'Animator
    [SerializeField] private string targetSpawnPointName; // Nom du spawn point cible dans la scène principale
    
    [Header("Key Settings")]
    [SerializeField] private bool requiresKey = false; // La porte nécessite-t-elle une clé?
    [SerializeField] private string requiredKeyName = ""; // Nom de la clé requise
    [SerializeField] private GameObject keyPromptText; // Message "Vous avez besoin d'une clé"
    [SerializeField] private GameObject wrongKeyText; // Message "Mauvaise clé"
    [SerializeField] private float messageDisplayTime = 3f; // Durée d'affichage des messages
    
    // ID unique pour identifier cette porte dans le gestionnaire persistant
    [SerializeField] private string doorID = "";

    private bool playerNearby = false;
    private GameObject playerRef;
    private bool isTransitioning = false;

    private void Start()
    {
        // Générer un ID unique pour la porte si vide
        if (string.IsNullOrEmpty(doorID))
        {
            doorID = gameObject.name + "_" + transform.position.ToString();
        }
        
        // Vérifier si cette porte a déjà été déverrouillée
        if (PersistentManager.Instance != null && PersistentManager.Instance.IsDoorUnlocked(doorID))
        {
            requiresKey = false;
        }
        
        // Assurez-vous que les messages sont désactivés au départ
        if (keyPromptText != null)
            keyPromptText.SetActive(false);
            
        if (wrongKeyText != null)
            wrongKeyText.SetActive(false);
    }

    private void Update()
    {
        // Si le joueur est à proximité et appuie sur F pour utiliser une clé
        if (playerNearby && Input.GetKeyDown(KeyCode.F) && requiresKey && !isTransitioning)
        {
            CheckKey();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerRef = other.gameObject;

            // Si aucune clé n'est requise, déclencher directement la transition
            if (!requiresKey && !isTransitioning)
            {
                // Stocker le spawn point cible dans un manager
                SpawnPointManager.SetTargetSpawnPoint(targetSpawnPointName);

                // Démarrer la coroutine de transition
                StartCoroutine(LoadScene());
            }
            // Sinon, afficher le message qu'une clé est nécessaire
            else if (requiresKey && keyPromptText != null)
            {
                keyPromptText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            
            // Cacher les messages quand le joueur s'éloigne
            if (keyPromptText != null)
                keyPromptText.SetActive(false);
                
            if (wrongKeyText != null)
                wrongKeyText.SetActive(false);
        }
    }

    private void CheckKey()
    {
        Inventory inventory = GameObject.FindGameObjectWithTag("InventoryManager")?.GetComponent<Inventory>();
        
        if (inventory != null)
        {
            // Vérifier si un slot est sélectionné et contient la bonne clé
            string selectedItemName = inventory.GetSelectedItemName();
            
            // Si rien n'est sélectionné, on ne fait rien
            if (string.IsNullOrEmpty(selectedItemName))
                return;
                
            if (selectedItemName == requiredKeyName)
            {
                // Bonne clé! On peut ouvrir la porte
                int selectedIndex = inventory.GetCurrentSelectedIndex();
                inventory.RemoveItemAtIndex(selectedIndex);
                
                // On désactive la condition de clé pour cette porte
                requiresKey = false;
                
                // On enregistre que cette porte est déverrouillée de façon permanente
                if (PersistentManager.Instance != null)
                {
                    PersistentManager.Instance.UnlockDoor(doorID);
                    // Marquer la clé comme utilisée
                    PersistentManager.Instance.MarkKeyAsUsed(requiredKeyName);
                }
                
                // On cache le message de clé nécessaire
                if (keyPromptText != null)
                    keyPromptText.SetActive(false);
                
                // On démarre la transition
                SpawnPointManager.SetTargetSpawnPoint(targetSpawnPointName);
                StartCoroutine(LoadScene());
            }
            else
            {
                // Mauvaise clé! On affiche un message d'erreur
                if (wrongKeyText != null)
                {
                    // Activer d'abord le message d'erreur
                    wrongKeyText.SetActive(true);
                    // Puis démarrer la coroutine qui le désactivera après un délai
                    StartCoroutine(ShowMessageTemporarily(wrongKeyText));
                    Debug.Log("Affichage du message d'erreur: mauvaise clé");
                }
                
                // Jouer un son d'erreur
                if (audioSource != null && errorSound != null)
                {
                    audioSource.PlayOneShot(errorSound);
                }
            }
        }
    }

    private IEnumerator ShowMessageTemporarily(GameObject message)
    {
        // Vérifier que le message existe
        if (message == null)
        {
            Debug.LogError("Message GameObject est null dans ShowMessageTemporarily");
            yield break;
        }
        
        // S'assurer que le message est bien activé
        message.SetActive(true);
        Debug.Log("Message activé: " + message.name);
        
        // Attendre la durée spécifiée
        yield return new WaitForSeconds(messageDisplayTime);
        
        // Désactiver le message seulement si le joueur est toujours à proximité
        // (pour éviter de désactiver un message qui aurait déjà été désactivé par OnTriggerExit)
        if (playerNearby && message != null)
        {
            message.SetActive(false);
            Debug.Log("Message désactivé après délai: " + message.name);
        }
        else
        {
            Debug.Log("Le joueur n'est plus à proximité ou le message est null, pas de désactivation");
        }
    }

    private IEnumerator LoadScene()
    {
        isTransitioning = true;
        
        // Déclencher l'animation de fade in (Start)
        animator.SetTrigger("Start");

        // Jouer l'effet sonore de la porte
        if (audioSource != null && doorSound != null)
        {
            audioSource.PlayOneShot(doorSound);
        }

        // Attendre la fin de l'animation de fade in (1 seconde)
        yield return new WaitForSeconds(1.5f);

        // Charger la scène
        SceneManager.LoadScene(sceneToLoad);

        // Attendre que la scène soit complètement chargée
        yield return new WaitForEndOfFrame();

        // Déclencher l'animation de fade out (End)
        animator.SetTrigger("End");

        // Attendre la fin de l'animation de fade out (1 seconde)
        yield return new WaitForSeconds(1);
        
        isTransitioning = false;
    }
}