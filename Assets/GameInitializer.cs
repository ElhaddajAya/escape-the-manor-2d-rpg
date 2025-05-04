using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    private void Start()
    {
        // Réinitialiser l'état du jeu
        if (PersistentManager.Instance != null)
        {
            PersistentManager.Instance.ResetGameState();
        }

        // Réinitialiser l'état du joueur
        PlayerObj player = FindObjectOfType<PlayerObj>();
        if (player != null)
        {
            player.ResetPlayer();
            
            // Force update health bar
            if (player.healthBar != null)
            {
                player.healthBar.SetMaxHealth(player.health);
                player.healthBar.SetHealth(player.health);
            }
        }

        // Assurez-vous que le bâton est désactivé
        GameObject baton = GameObject.FindGameObjectWithTag("Baton");
        if (baton != null)
        {
            baton.SetActive(false);
        }
        
        // Vider l'inventaire
        Inventory inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            // Vous devrez peut-être ajouter une méthode Reset() à votre Inventory
            for (int i = inventory.GetCurrentSelectedIndex(); i >= 0; i--)
            {
                inventory.RemoveItemAtIndex(i);
            }
        }
    }
}