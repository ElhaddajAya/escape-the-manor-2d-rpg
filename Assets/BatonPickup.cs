using UnityEngine;
using UnityEngine.UI;

public class BatonPickup : MonoBehaviour
{
    private bool playerNearby = false;
    private GameObject playerRef;
    public GameObject equipPromptUI;

    void Start()
    {
        // Vérifie si le joueur possède déjà le bâton
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            PlayerObj player = playerObj.GetComponent<PlayerObj>();
            if (player != null && player.batonObject != null && player.batonObject.activeInHierarchy)
            {
                // Le joueur a déjà le bâton → on supprime l'objet de la scène (le sprite qui pulse)
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            PlayerObj player = playerRef.GetComponent<PlayerObj>();
            if (player != null && player.batonObject != null)
            {
                // Active le bâton dans la hiérarchie du joueur
                player.batonObject.SetActive(true);

                // Détruit l'objet de la scène
                Destroy(gameObject);
            }
        }
        
    }

    void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerNearby = true;
        playerRef = other.gameObject;

        if (equipPromptUI != null)
        {
            equipPromptUI.SetActive(true);
        }
    }
}

    void OnTriggerExit2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        playerNearby = false;

        if (equipPromptUI != null)
        {
            equipPromptUI.SetActive(false);
        }
    }
}
}
