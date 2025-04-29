using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    public AudioClip collectSound;
    public Sprite itemSprite;
    public string itemName;

    public GameObject promptText; // <- à glisser depuis l'inspecteur

    private bool playerNearby = false;
    private GameObject playerRef;

    void Start()
    {
        if (PersistentManager.Instance != null && PersistentManager.Instance.HasItem(itemName))
        {
            Destroy(gameObject); // Déjà ramassé
        }

        if (promptText != null)
        {
            promptText.SetActive(false); // Assure qu'il est caché au départ
        }
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (PersistentManager.Instance != null)
            {
                Inventory inventory = GameObject.FindGameObjectWithTag("InventoryManager")?.GetComponent<Inventory>();
                if (inventory != null && !inventory.IsFull())
                {
                    inventory.AddItem(itemSprite);
                    PersistentManager.Instance.AddItem(itemName);

                    AudioSource.PlayClipAtPoint(collectSound, transform.position);

                    if (promptText != null) promptText.SetActive(false);

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
            if (promptText != null) promptText.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (promptText != null) promptText.SetActive(false);
        }
    }
}
