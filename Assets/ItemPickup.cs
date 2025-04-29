using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    public AudioClip collectSound; // Sound to play when the item is collected
    public Sprite itemSprite; // Sprite of the item to add to the inventory
    public string itemName; // Name of the item (for debugging or UI purposes)

    private bool playerNearby = false;
    private GameObject playerRef;

    void Start()
    {
        // Check if the player already has this item
        GameObject inventoryManager = GameObject.FindGameObjectWithTag("InventoryManager");
        if (inventoryManager != null)
        {
            Inventory inventory = inventoryManager.GetComponent<Inventory>();
            if (inventory != null && inventory.HasItem(itemName))
            {
                // Player already has the item → destroy the object in the scene
                Destroy(gameObject);
            }
        }
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            GameObject inventoryManager = GameObject.FindGameObjectWithTag("InventoryManager");
            if (inventoryManager != null)
            {
                Inventory inventory = inventoryManager.GetComponent<Inventory>();
                if (inventory != null)
                {
                    // Add item to inventory
                    inventory.AddItem(itemSprite);

                    // Play collect sound
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);

                    // Destroy the item object in the scene
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            playerRef = other.gameObject;
            Debug.Log("Player detected nearby.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            Debug.Log("Player left the trigger area.");
        }
    }
}
