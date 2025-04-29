using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    public AudioClip collectSound;
    public Sprite itemSprite;
    public string itemName;

    private bool playerNearby = false;
    private GameObject playerRef;

    void Start()
    {
        // Vérifie si l'objet a déjà été ramassé
        if (PersistentManager.Instance != null && PersistentManager.Instance.HasItem(itemName))
        {
            Destroy(gameObject); // Déjà ramassé, on ne l'affiche pas
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
                    // Ajouter dans UI
                    inventory.AddItem(itemSprite);

                    // Ajouter dans la base de données globale
                    PersistentManager.Instance.AddItem(itemName);

                    // Jouer son
                    AudioSource.PlayClipAtPoint(collectSound, transform.position);

                    // Supprimer objet
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
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
