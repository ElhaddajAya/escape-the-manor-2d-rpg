using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Nom de la scène à charger
    [SerializeField] private AudioClip doorSound; // Effet sonore d'ouverture de porte
    [SerializeField] private AudioSource audioSource; // Source audio pour jouer le son
    [SerializeField] private Animator animator; // Référence à l'Animator

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Vérifie si c'est le joueur
        {
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
            yield return new WaitForSeconds(doorSound.length * 0.5f);
        }

        // Déclencher l'animation de fin
        animator.SetTrigger("End");

        // Attendre la fin de l'animation de fin
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        // Charger la scène
        SceneManager.LoadScene(sceneToLoad);
    }
}