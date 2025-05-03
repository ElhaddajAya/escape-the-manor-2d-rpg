using UnityEngine;

public class DoorToOutro : MonoBehaviour 
{
    [Header("Settings")]
    [SerializeField] private string sceneToLoad = "OutroScene";
    [SerializeField] private SceneFader sceneFader; // Reference to your existing SceneFader

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player"))
        {
            // Trigger fade through your existing SceneFader
            sceneFader.FadeToScene(sceneToLoad);
            
            // Optional: Freeze player during transition
            other.GetComponent<PlayerObj>().enabled = false;
        }
    }
}