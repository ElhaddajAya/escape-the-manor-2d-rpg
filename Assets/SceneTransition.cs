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
        // Déclencher l'animation de début
        animator.SetTrigger("Start");

        // Jouer l'effet sonore de la porte
        if (audioSource != null && doorSound != null)
        {
            audioSource.PlayOneShot(doorSound);
        }

        // Attendre la fin du son avant de charger la scène
        if (doorSound != null)
        {
            yield return new WaitForSeconds(doorSound.length);
        }

        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // Attendre 1 seconde (durée de l'animation)

        // Charger la scène
        SceneManager.LoadScene(sceneToLoad);
        
        // Déclencher l'animation de fin
        animator.SetTrigger("End");

        // Attendre la fin de l'animation de fin
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length); // Attendre 1 seconde (durée de l'animation)
    }

}