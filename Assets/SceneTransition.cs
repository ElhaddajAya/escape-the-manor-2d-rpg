using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Nom de la scène à charger
    [SerializeField] private AudioClip doorSound; // Effet sonore d'ouverture de porte
    [SerializeField] private AudioSource audioSource; // Source audio pour jouer le son
    [SerializeField] private Animator animator; // Référence à l'Animator
    [SerializeField] private string targetSpawnPointName; // Nom du spawn point cible dans la scène principale

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Stocker le spawn point cible dans un manager
            SpawnPointManager.SetTargetSpawnPoint(targetSpawnPointName);

            // Démarrer la coroutine de transition
            StartCoroutine(LoadScene());
        }
    }

    private IEnumerator LoadScene()
    {
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
    }

}