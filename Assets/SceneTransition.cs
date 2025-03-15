using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string sceneToLoad; // Nom de la scène à charger
    [SerializeField] private Image blackScreen; // Image noire pour le fondu
    [SerializeField] private AudioClip doorSound; // Effet sonore d'ouverture de porte
    [SerializeField] private AudioSource audioSource; // Source audio pour jouer le son

    private void Start()
    {
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true); // Assurer que l'écran est visible
            blackScreen.color = new Color(0, 0, 0, 0); // Initialement transparent
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Vérifie si c'est le joueur
        {
            StartCoroutine(LoadSceneWithFade());
        }
    }

    private IEnumerator LoadSceneWithFade()
    {
        // Jouer l'effet sonore de la porte
        if (audioSource != null && doorSound != null)
        {
            audioSource.PlayOneShot(doorSound);
        }

        // Faire le fondu
        float fadeDuration = 2.5f; // Durée du fondu (1 seconde)
        float time = 0f;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Clamp01(time / fadeDuration);
            blackScreen.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Charger la scène après le fondu
        SceneManager.LoadScene(sceneToLoad);
    }
}
