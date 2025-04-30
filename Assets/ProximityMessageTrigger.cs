using UnityEngine;
using UnityEngine.UI; // or TMPro if using TextMeshPro

public class ProximityMessageTrigger : MonoBehaviour
{
    [TextArea]
    public string messageToShow;

    public GameObject uiPanel;       // The panel GameObject (from GUI Canvas)
    public Text messageText;         // The UI Text component to display the message
    // If using TextMeshPro instead: public TMP_Text messageText;

    void Start()
    {
        if (uiPanel != null)
            uiPanel.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && uiPanel != null && messageText != null)
        {
            uiPanel.SetActive(true);
            messageText.text = messageToShow;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && uiPanel != null)
        {
            uiPanel.SetActive(false);
        }
    }
}
