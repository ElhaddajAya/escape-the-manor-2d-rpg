using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour
{
    public AudioClip collectSound;  // Son lors de la collecte
    public AudioClip useSound;      // Son lors de l'utilisation (pour les potions)
    public Sprite itemSprite;
    public string itemName;
    
    [Header("Item Type")]
    public ItemType itemType = ItemType.Key;  // Type d'objet (Clé par défaut)
    public int healthRestoreAmount = 15;      // Quantité de santé restaurée (pour les potions)

    public GameObject promptText; // <- à glisser depuis l'inspecteur

    private bool playerNearby = false;
    private GameObject playerRef;

    void Start()
    {
        if (PersistentManager.Instance != null)
        {
            // Vérifier si l'item a déjà été collecté ou si c'est une clé déjà utilisée
            if (PersistentManager.Instance.HasItem(itemName) || 
                (itemType == ItemType.Key && PersistentManager.Instance.WasKeyUsed(itemName)))
            {
                Destroy(gameObject); // Déjà ramassé ou utilisé
            }
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
                if (inventory != null)
                {
                    if (inventory.IsFull())
                    {
                        // 🔥 Affiche le message et bloque la collecte
                        StartCoroutine(inventory.ShowTitleWithFade());
                        return;
                    }

                    // ✅ Sinon, collecte normale
                    inventory.AddItem(itemSprite, itemName, itemType, useSound, healthRestoreAmount);
                    PersistentManager.Instance.AddItem(itemName, itemType);

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
            Debug.Log("Player entered trigger zone of: " + gameObject.name); // ← TEST
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