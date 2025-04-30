using UnityEngine;
using UnityEngine.UI;

public class ProximityMessageTrigger : MonoBehaviour
{
    public GameObject uiPanel;           // Reference to the Panel (background)
    public Text messageText;             // Reference to the Text
    public string messageToShow;         // The message for this object

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            uiPanel.SetActive(true);
            messageText.gameObject.SetActive(true);
            messageText.text = messageToShow;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            uiPanel.SetActive(false);
            messageText.gameObject.SetActive(false);
        }
    }
}
