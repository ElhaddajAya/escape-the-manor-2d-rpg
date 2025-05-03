using UnityEngine;
using System.Collections;

public class SecretPassageExit : MonoBehaviour
{
    [Header("Paramètres")]
    [SerializeField] private string sceneToLoad = "Long_Corridor"; // Nom de la scène du couloir obscur
    [SerializeField] private string targetSpawnPointName = "LongCorridorDoor"; // Point d'apparition dans la nouvelle scène
    [SerializeField] private AudioClip passageSound; // Son lors de l'entrée dans le passage
    [SerializeField] private float transitionDelay = 0.5f; // Délai avant de lancer la transition
    
    [Header("Références")]
    [SerializeField] private Animator transitionAnimator; // Référence à l'animateur de transition
    
    private bool playerInside = false;
    private bool isTransitioning = false;
    
    private void Start()
    {
        // S'assurer que le collider est actif
        if (GetComponent<Collider2D>() != null)
        {
            GetComponent<Collider2D>().enabled = true;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isTransitioning)
        {
            playerInside = true;
            
            // Lancer la transition vers la scène finale
            StartCoroutine(TransitionToFinalScene());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
    
    private IEnumerator TransitionToFinalScene()
    {
        isTransitioning = true;
        
        // Jouer le son de passage
        if (passageSound != null)
        {
            AudioSource.PlayClipAtPoint(passageSound, Camera.main.transform.position);
        }
        
        // Petit délai pour que le joueur entre complètement dans le passage
        yield return new WaitForSeconds(transitionDelay);
        
        // Définir le point d'apparition dans la scène cible
        SpawnPointManager.SetTargetSpawnPoint(targetSpawnPointName);
        
        // Geler le jeu pendant la transition
        GameState.IsFrozen = true;
        
        // Lancer l'animation de transition si disponible
        if (transitionAnimator != null)
        {
            transitionAnimator.SetTrigger("Start");
            
            // Attendre que l'animation de fondu soit terminée
            yield return new WaitForSeconds(1.3f);
        }
        
        // Charger la scène du couloir obscur
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneToLoad);
        
        // Le jeu reste gelé après le chargement (sera dégelé par PlayerObj)
    }
}