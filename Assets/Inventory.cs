using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public GameObject[] slots; // Array to hold the slots (each slot should be the ItemImage GameObject)
    private List<Sprite> items; // List to hold the collected items
    public int maxItems = 4; // Maximum number of items in the inventory

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
