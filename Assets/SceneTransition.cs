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

    private bool playerNearby = false;
    private GameObject playerRef;
    private bool isTransitioning = false;

    private void Start()
    {
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
                    StartCoroutine(ShowMessageTemporarily(wrongKeyText));
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
        message.SetActive(true);
        yield return new WaitForSeconds(messageDisplayTime);
        message.SetActive(false);
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