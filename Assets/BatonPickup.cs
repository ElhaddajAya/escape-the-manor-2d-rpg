using UnityEngine;

public class BatonPickup : MonoBehaviour
{
    public string playerTag = "Player";
    public GameObject uiPrompt; // Le texte UI "Magic Wand - Press E to collect"

    private bool isPlayerNear = false;
    private GameObject player;

    void Start()
    {
        if (uiPrompt != null)
            uiPrompt.SetActive(false); // Cache le texte au début
    }

    void Update()
    {
        if (isPlayerNear && Input.GetKeyDown(KeyCode.E))
        {
            if (player != null)
            {
                // Trouver automatiquement l'objet P_Weapon dans le joueur
                Transform weapon = player.transform.Find("P_Weapon");
                if (weapon != null)
                {
                    weapon.gameObject.SetActive(true);
                    Debug.Log("Bâton magique équipé !");
                }
                else
                {
                    Debug.LogWarning("L'objet 'P_Weapon' n'a pas été trouvé dans le joueur !");
                }

                if (uiPrompt != null)
                    uiPrompt.SetActive(false);

                Destroy(gameObject); // Supprime l’objet au sol
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            player = other.gameObject;
            isPlayerNear = true;
            if (uiPrompt != null)
                uiPrompt.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            isPlayerNear = false;
            if (uiPrompt != null)
                uiPrompt.SetActive(false);
        }
    }
}
