using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Nom de la scène à charger
    [SerializeField] private AudioClip doorSound; // Effet sonore d'ouverture de porte
    [SerializeField] private AudioSource audioSource; // Source audio pour jouer le son

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Vérifie si c'est le joueur
        {
            StartCoroutine(LoadScene());
        }
    }

    private IEnumerator LoadScene()
    {
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

        // Charger la scène
        SceneManager.LoadScene(sceneToLoad);
    }
}