using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public GameObject[] slots; // Array to hold the slots (each slot should be the ItemImage GameObject)
    private List<Sprite> items; // List to hold the collected items
    public int maxItems = 4; // Maximum number of items in the inventory
    public GameObject[] slotMarkers; // Drag & Drop dans l’Inspector
    private int currentSelectedIndex = -1;

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

                    Debug.Log("Slot " + (i + 1) + " sélectionné.");
                }
                else
                {
                    // Si slot vide, on désactive tout
                    foreach (GameObject marker in slotMarkers)
                    {
                        marker.SetActive(false);
                    }
                    Debug.Log("Slot " + (i + 1) + " est vide.");
                }
            }
        }
    }

    public void AddItem(Sprite item)
    {
        if (items.Count < maxItems)
        {
            items.Add(item);
            UpdateInventoryUI();
            Debug.Log("Item added to inventory: " + item.name);
        }
        else
        {
            Debug.Log("Inventory is full!");
        }
    }

    public bool HasItem(string itemName)
    {
        // Check if the item is already in the inventory
        foreach (Sprite item in items)
        {
            if (item != null && item.name == itemName)
            {
                return true;
            }
        }
        return false;
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
            }
            else if (slots[i] != null)
            {
                slots[i].GetComponent<Image>().sprite = null;
                slots[i].GetComponent<Image>().color = new Color(1, 1, 1, 0); // Set alpha to 0
            }
        }
    }
}
