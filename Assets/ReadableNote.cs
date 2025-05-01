using UnityEngine;
using UnityEngine.UI;

public class ReadableNote : MonoBehaviour
{
    public string noteText; // What will show in the GUI
    public GameObject guiPanel; // The GUI panel (Message + X button)
    public Text messageText; // The UI text component in the GUI
    public GameObject promptText; // The world-space "Press E to read" prompt

    private bool playerNearby = false;

    void Start()
    {
        if (guiPanel != null)
            guiPanel.SetActive(false);

        if (promptText != null)
            promptText.SetActive(false);
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (guiPanel != null && messageText != null)
            {
                guiPanel.SetActive(true);
                messageText.text = noteText;
            }

            if (promptText != null)
                promptText.SetActive(false); // Hide prompt when reading
        }
    }

    public void CloseNote()
    {
        if (guiPanel != null)
            guiPanel.SetActive(false);

        if (playerNearby && promptText != null)
            promptText.SetActive(true); // Reshow prompt after closing if still nearby
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;

            if (promptText != null)
                promptText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;

            if (promptText != null)
                promptText.SetActive(false);

            if (guiPanel != null)
                guiPanel.SetActive(false);
        }
    }
}
