using UnityEngine;
using UnityEngine.UI;

public class ReadableNote : MonoBehaviour
{
    public string noteText;              // The story snippet
    public GameObject guiPanel;         // GUI Canvas > Panel
    public Text messageText;            // Panel > MessageText
    public GameObject promptText;       // Paper (1) > Canvas > PromptText

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
                guiPanel.SetActive(true);// Replace placeholder with actual line breaks
                messageText.text = noteText.Replace("||", "\n");
                //messageText.text = noteText;
            }

            if (promptText != null)
                promptText.SetActive(false);
        }
    }

    public void CloseNote()
    {
        if (guiPanel != null)
            guiPanel.SetActive(false);

        if (playerNearby && promptText != null)
            promptText.SetActive(true);
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
