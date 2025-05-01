using UnityEngine;

// Énumération pour les différents types d'objets
public enum ItemType
{
    Key,    // Clé pour ouvrir les portes
    Potion  // Potion de santé
}

// Classe pour stocker les informations d'un item
[System.Serializable]
public class ItemData
{
    public ItemType type;          // Type d'objet (Clé ou Potion)
    public string name;            // Nom de l'objet
    public Sprite sprite;          // Sprite de l'objet
    public AudioClip collectSound; // Son lors de la collecte
    public AudioClip useSound;     // Son lors de l'utilisation
    public int healthRestoreAmount; // Quantité de santé restaurée (pour les potions)
    
    // Constructeur pour faciliter la création d'objets
    public ItemData(ItemType type, string name, Sprite sprite, AudioClip collectSound, AudioClip useSound = null, int healthRestoreAmount = 0)
    {
        this.type = type;
        this.name = name;
        this.sprite = sprite;
        this.collectSound = collectSound;
        this.useSound = useSound;
        this.healthRestoreAmount = healthRestoreAmount;
    }
}