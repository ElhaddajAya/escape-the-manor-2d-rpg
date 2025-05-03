using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using TMPro;

public class HiddenTreasureChest : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private GameObject promptText; // Texte "Vous avez besoin d'une clé"
    [SerializeField] private string requiredKeyName = "Chest_Key"; // Nom de la clé requise
    [SerializeField] private GameObject wrongKeyText; // Message "Mauvaise clé"
    [SerializeField] private GameObject wrongItemTypeText; // Message "Ce n'est pas une clé" 
    [SerializeField] private AudioClip unlockSound; // Son de déverrouillage
    [SerializeField] private AudioClip errorSound; // Son d'erreur
    
    [Header("Tilemaps")]
    [SerializeField] private Tilemap chestCamouflageTilemap; // Tilemap du coffre camouflé
    [SerializeField] private Tilemap openChestTilemap; // Tilemap du coffre ouvert
    [SerializeField] private Tilemap wallCamouflageTilemap; // Tilemap du mur qui cache l'escalier
    [SerializeField] private Tilemap stairsTilemap; // Tilemap de l'escalier secret
    
    [Header("Paramètres")]
    [SerializeField] private float fadeTime = 1.3f; // Durée du fondu
    [SerializeField] private float messageDisplayTime = 1.5f; // Durée d'affichage des messages
    [SerializeField] private string chestID = "final_treasure_chest"; // ID unique pour le coffre
    
    // Variables privées
    private bool playerNearby = false;
    private bool isChestUnlocked = false;
    private bool isPassageRevealed = false;

    private void Start()
    {
        // Vérifier si ce coffre a déjà été déverrouillé dans une session précédente
        CheckPreviouslyUnlocked();
        
        // Configurer les tilemaps au départ
        SetupInitialTilemaps();
        
        // Cacher les messages au départ
        HideAllMessages();
    }
    
    private void CheckPreviouslyUnlocked()
    {
        if (PersistentManager.Instance != null && PersistentManager.Instance.IsDoorUnlocked(chestID))
        {
            // Le coffre est déjà déverrouillé, révéler directement le passage
            isChestUnlocked = true;
            isPassageRevealed = true;
            
            // Configurer les tilemaps en état déverrouillé
            if (chestCamouflageTilemap != null) chestCamouflageTilemap.gameObject.SetActive(false);
            if (openChestTilemap != null) openChestTilemap.gameObject.SetActive(true);
            if (wallCamouflageTilemap != null) wallCamouflageTilemap.gameObject.SetActive(false);
            if (stairsTilemap != null) stairsTilemap.gameObject.SetActive(true);
        }
    }
    
    private void SetupInitialTilemaps()
    {
        // Si le coffre n'est pas déjà déverrouillé
        if (!isChestUnlocked)
        {
            // Afficher le coffre camouflé
            if (chestCamouflageTilemap != null) chestCamouflageTilemap.gameObject.SetActive(true);
            if (openChestTilemap != null) openChestTilemap.gameObject.SetActive(false);
            
            // Cacher l'escalier, afficher le mur
            if (wallCamouflageTilemap != null) wallCamouflageTilemap.gameObject.SetActive(true);
            if (stairsTilemap != null) stairsTilemap.gameObject.SetActive(false);
        }
    }
    
    private void HideAllMessages()
    {
        if (promptText != null) promptText.SetActive(false);
        if (wrongKeyText != null) wrongKeyText.SetActive(false);
        if (wrongItemTypeText != null) wrongItemTypeText.SetActive(false);
    }

    private void Update()
    {
        // Si le joueur est à proximité et appuie sur F pour utiliser une clé
        if (playerNearby && Input.GetKeyDown(KeyCode.F) && !isChestUnlocked)
        {
            CheckKey();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            
            // Si le coffre n'est pas déverrouillé, afficher le message
            if (!isChestUnlocked && promptText != null)
            {
                promptText.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            
            // Cacher tous les messages
            HideAllMessages();
        }
    }

    private void CheckKey()
    {
        Inventory inventory = GameObject.FindGameObjectWithTag("InventoryManager")?.GetComponent<Inventory>();
        
        if (inventory != null)
        {
            // Vérifier si un slot est sélectionné
            string selectedItemName = inventory.GetSelectedItemName();
            
            // Si rien n'est sélectionné, on ne fait rien
            if (string.IsNullOrEmpty(selectedItemName))
                return;
            
            // Vérifier si l'objet sélectionné est bien une clé
            if (inventory.GetSelectedItemType() != ItemType.Key)
            {
                // Ce n'est pas une clé mais une potion!
                if (wrongItemTypeText != null)
                {
                    wrongItemTypeText.SetActive(true);
                    StartCoroutine(ShowMessageTemporarily(wrongItemTypeText));
                }
                
                // Jouer un son d'erreur
                PlaySound(errorSound);
                
                return;
            }
                
            // C'est une clé, vérifions si c'est la bonne
            if (selectedItemName == requiredKeyName)
            {
                // Bonne clé! On peut ouvrir le coffre
                int selectedIndex = inventory.GetCurrentSelectedIndex();
                inventory.RemoveItemAtIndex(selectedIndex);
                
                // On marque le coffre comme déverrouillé
                isChestUnlocked = true;
                
                // Enregistrer dans le gestionnaire persistant
                if (PersistentManager.Instance != null)
                {
                    PersistentManager.Instance.UnlockDoor(chestID);
                    PersistentManager.Instance.MarkKeyAsUsed(requiredKeyName);
                }
                
                // On cache le message de clé nécessaire
                if (promptText != null)
                    promptText.SetActive(false);
                
                // Jouer le son de déverrouillage
                PlaySound(unlockSound);
                
                // Révéler le coffre et le passage secret
                StartCoroutine(RevealTreasureAndPassage());
            }
            else
            {
                // Mauvaise clé! On affiche un message d'erreur
                if (wrongKeyText != null)
                {
                    wrongKeyText.SetActive(true);
                    StartCoroutine(ShowMessageTemporarily(wrongKeyText));
                }
                
                // Jouer un son d'erreur
                PlaySound(errorSound);
            }
        }
    }
    
    private IEnumerator RevealTreasureAndPassage()
    {
        // 1. Faire disparaître le coffre camouflé
        if (chestCamouflageTilemap != null)
        {
            yield return StartCoroutine(FadeTilemapOut(chestCamouflageTilemap));
            chestCamouflageTilemap.gameObject.SetActive(false);
        }
        
        // 2. Faire apparaître le coffre ouvert
        if (openChestTilemap != null)
        {
            openChestTilemap.gameObject.SetActive(true);
            yield return StartCoroutine(FadeTilemapIn(openChestTilemap));
        }
        
        // Petite pause pour l'effet dramatique
        yield return new WaitForSeconds(0.5f);
        
        // 3. Jouer un son pour le mur qui s'ouvre (utiliser le même son de déverrouillage)
        PlaySound(unlockSound);
        
        // 4. Faire disparaître le mur qui cache l'escalier
        if (wallCamouflageTilemap != null)
        {
            yield return StartCoroutine(FadeTilemapOut(wallCamouflageTilemap));
            wallCamouflageTilemap.gameObject.SetActive(false);
        }
        
        // 5. Faire apparaître l'escalier
        if (stairsTilemap != null)
        {
            stairsTilemap.gameObject.SetActive(true);
            yield return StartCoroutine(FadeTilemapIn(stairsTilemap));
        }
        
        // Le passage est maintenant révélé
        isPassageRevealed = true;
    }
    
    private IEnumerator FadeTilemapOut(Tilemap tilemap)
    {
        Color originalColor = tilemap.color;
        float elapsedTime = 0;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeTime));
            
            Color newColor = originalColor;
            newColor.a = alpha;
            tilemap.color = newColor;
            
            yield return null;
        }
    }
    
    private IEnumerator FadeTilemapIn(Tilemap tilemap)
    {
        Color originalColor = tilemap.color;
        Color startColor = originalColor;
        startColor.a = 0;
        tilemap.color = startColor;
        
        float elapsedTime = 0;
        
        while (elapsedTime < fadeTime)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeTime);
            
            Color newColor = originalColor;
            newColor.a = alpha;
            tilemap.color = newColor;
            
            yield return null;
        }
    }

    private IEnumerator ShowMessageTemporarily(GameObject message)
    {
        // S'assurer que le message est activé
        message.SetActive(true);
        
        // Attendre la durée spécifiée
        yield return new WaitForSeconds(messageDisplayTime);
        
        // Désactiver le message si le joueur est toujours à proximité
        if (playerNearby && message != null)
        {
            message.SetActive(false);
        }
    }
    
    private void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position);
        }
    }
}