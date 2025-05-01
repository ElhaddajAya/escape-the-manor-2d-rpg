using System.Collections.Generic;
using UnityEngine;

public class PersistentManager : MonoBehaviour
{
    public static PersistentManager Instance { get; private set; }

    public List<string> collectedItems = new List<string>(); // 🔥 Contient les noms d'items collectés
    private List<string> unlockedDoors = new List<string>(); // 🔑 Contient les IDs des portes déverrouillées

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void AddItem(string itemName)
    {
        if (!collectedItems.Contains(itemName))
        {
            collectedItems.Add(itemName);
            Debug.Log("Item ajouté au gestionnaire global : " + itemName);
        }
    }

    public bool HasItem(string itemName)
    {
        return collectedItems.Contains(itemName);
    }

    public void RemoveItem(string itemName)
    {
        if (collectedItems.Contains(itemName))
        {
            collectedItems.Remove(itemName);
            Debug.Log("Item supprimé : " + itemName);
        }
    }
    
    // Méthodes pour gérer les portes déverrouillées
    public void UnlockDoor(string doorID)
    {
        if (!unlockedDoors.Contains(doorID))
        {
            unlockedDoors.Add(doorID);
            Debug.Log("Porte déverrouillée de façon permanente : " + doorID);
        }
    }
    
    public bool IsDoorUnlocked(string doorID)
    {
        return unlockedDoors.Contains(doorID);
    }
}