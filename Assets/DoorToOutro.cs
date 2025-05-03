using UnityEngine;

public class DoorToOutro : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] private string sceneToLoad = "OutroScene";
    // Door opening sound
    [SerializeField] private AudioClip doorSound;
    [SerializeField] private SceneFader sceneFader; // Reference to your existing SceneFader

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            // Play door sound
            AudioSource.PlayClipAtPoint(doorSound, Camera.main.transform.position);

            // Trigger fade through your existing SceneFader
            sceneFader.FadeToScene(sceneToLoad);

            // Stop player movement, disable animations and sound
            other.GetComponent<PlayerObj>().StopAllCoroutines(); // Stop all player sounds

            // disable player footsteps sound
            AudioSource playerAudio = other.GetComponent<AudioSource>();
            if (playerAudio != null && playerAudio.clip != null && playerAudio.clip.name == "footstepsSound")
            {
                playerAudio.Stop();
            }
        }
    }
}