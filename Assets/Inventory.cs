using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using TMPro;

public class Inventory : MonoBehaviour
{
    public GameObject[] slots; // Array to hold the slots (each slot should be the ItemImage GameObject)
    private List<Sprite> items; // List to hold the collected items
    private List<string> itemNames; // Pour stocker les noms des items 
    public int maxItems = 4; // Maximum number of items in the inventory
    public GameObject[] slotMarkers; // Drag & Drop dans l'Inspector
    private int currentSelectedIndex = -1;
    public TextMeshProUGUI titleText;
    public float fadeDuration = 1f;
    public float displayTime = 3f; // Time fully visible before fade-out

    void Awake()
    {
        // Ensure this object persists across scenes
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (items == null)
        {
            items = new List<Sprite>(maxItems);
            itemNames = new List<string>(maxItems);
        }

        // Ensure slots are correctly assigned
        if (slots == null || slots.Length == 0)
        {
            Debug.LogError("Slots array is not assigned or empty. Please assign the slots in the Inspector.");
            return;
        }

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                Image imageComponent = slots[i].GetComponent<Image>();
                imageComponent.sprite = null; // Ensure slots are empty at start
                imageComponent.color = new Color(1, 1, 1, 0); // Set alpha to 0

                // Set anchors to center-middle
                RectTransform rectTransform = slots[i].GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
                rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = Vector2.zero;
            }
            else
            {
                Debug.LogError("Slot " + i + " is not assigned in the Inspector.");
            }
        }

        if (slotMarkers != null)
        {
            foreach (GameObject marker in slotMarkers)
            {
                marker.SetActive(false);
            }
        }
    }

    public IEnumerator ShowTitleWithFade()
    {
        titleText.gameObject.SetActive(true);

        // Fade In
        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(t / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        // Wait while fully visible
        yield return new WaitForSeconds(displayTime);

        // Fade Out
        t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - (t / fadeDuration));
            SetAlpha(alpha);
            yield return null;
        }

        titleText.gameObject.SetActive(false);
    }

    void SetAlpha(float a)
    {
        Color color = titleText.color;
        color.a = a;
        titleText.color = color;
    }

    void Update()
    {
        HandleSlotSelection();
    }

    void HandleSlotSelection()
    {
        for (int i = 0; i < maxItems; i++)
        {
            if (Input.GetKeyDown((KeyCode)((int)KeyCode.Alpha1 + i)))
            {
                if (i < items.Count)
                {
                    currentSelectedIndex = i;

                    // Désactiver toutes les flèches
                    for (int j = 0; j < slotMarkers.Length; j++)
                    {
                        if (slotMarkers[j] != null)
                            slotMarkers[j].SetActive(j == i); // Activer uniquement celle du slot sélectionné
                    }

                    Debug.Log("Slot " + (i + 1) + " sélectionné. Item: " + itemNames[i]);
                }
                else
                {
                    // Si slot vide, on désactive tout
                    currentSelectedIndex = -1;
                    foreach (GameObject marker in slotMarkers)
                    {
                        marker.SetActive(false);
                    }
                    Debug.Log("Slot " + (i + 1) + " est vide.");
                }
            }
        }
    }

    // Ajouter un item avec son nom et son sprite
    public void AddItem(Sprite item, string itemName)
    {
        if (items.Count < maxItems)
        {
            items.Add(item);
            itemNames.Add(itemName);
            UpdateInventoryUI();
            Debug.Log("Item added to inventory: " + itemName);
        }
        else
        {
            StartCoroutine(ShowTitleWithFade());
            Debug.Log("Inventory is full!");
        }
    }

    // Méthode pour supprimer un item à un index spécifique
    public void RemoveItemAtIndex(int index)
    {
        if (index >= 0 && index < items.Count)
        {
            string removedName = itemNames[index];
            
            // Supprimer l'item du PersistentManager aussi
            if (PersistentManager.Instance != null)
            {
                PersistentManager.Instance.RemoveItem(removedName);
            }
            
            // Supprimer l'item de l'inventaire
            items.RemoveAt(index);
            itemNames.RemoveAt(index);
            
            // Réinitialiser l'index sélectionné
            currentSelectedIndex = -1;
            
            // Désactiver tous les marqueurs
            if (slotMarkers != null)
            {
                foreach (GameObject marker in slotMarkers)
                {
                    marker.SetActive(false);
                }
            }
            
            // Mettre à jour l'UI
            UpdateInventoryUI();
            
            Debug.Log("Item removed from inventory: " + removedName);
        }
    }
    
    // Obtenir l'index actuellement sélectionné
    public int GetCurrentSelectedIndex()
    {
        return currentSelectedIndex;
    }
    
    // Obtenir le nom de l'item sélectionné
    public string GetSelectedItemName()
    {
        if (currentSelectedIndex >= 0 && currentSelectedIndex < itemNames.Count)
        {
            return itemNames[currentSelectedIndex];
        }
        return "";
    }

    public bool HasItem(string itemName)
    {
        // Check if the item is already in the inventory
        return itemNames.Contains(itemName);
    }

    public bool IsFull()
    {
        return items.Count >= maxItems;
    }

    private void UpdateInventoryUI()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null && i < items.Count)
            {
                slots[i].GetComponent<Image>().sprite = items[i];
                slots[i].GetComponent<Image>().color = new Color(1, 1, 1, 1); // Set alpha to 255
                // set native size
                slots[i].GetComponent<Image>().SetNativeSize(); // Set the size of the image to its native size
            }
            else if (slots[i] != null)
            {
                slots[i].GetComponent<Image>().sprite = null;
                slots[i].GetComponent<Image>().color = new Color(1, 1, 1, 0); // Set alpha to 0
            }
        }
    }
}