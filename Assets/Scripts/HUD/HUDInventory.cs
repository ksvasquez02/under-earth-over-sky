using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HUDManager))]
public class HUDInventory : MonoBehaviour
{
    private HUDManager HUD;

    public GameObject inventoryPanel;
    private readonly Dictionary<ItemType, GameObject> inventorySubs = new();
    public GameObject itemButtonPrefab;

    public bool Initialize()
    {
        HUD = GetComponent<HUDManager>();

        if (inventoryPanel != null)
        {
            Transform subContainer = inventoryPanel.transform.GetChild(1);
            inventorySubs.Add(ItemType.Artifact, subContainer.GetChild(0).gameObject);
            inventorySubs.Add(ItemType.Language, subContainer.GetChild(1).gameObject);
            inventorySubs.Add(ItemType.Memory, subContainer.GetChild(2).gameObject);
            inventoryPanel.SetActive(false);
            HUD.MenuPanels.Insert((int)MenuState.Inventory, inventoryPanel);
            return true;
        } else
        {
            return false;
        }

    }

    public void PopulateInventory(Inventory inventory)
    {
        foreach ((ItemType type, GameObject sub) in inventorySubs)
        {
            int count = 0;
            Transform container = sub.transform.GetChild(1);
            ItemData[] items = inventory.Contents[type].Values.ToArray();

            // First use existing children
            foreach (Transform child in container)
            {
                if (count < items.Length)
                {
                    ItemData item = items[count];
                    GameObject button = child.gameObject;
                    SetInventoryItemButton(button, item);
                }
                // Deactivate if more buttons than items
                else
                {
                    child.gameObject.SetActive(false);
                }
                count++;
            }

            // Then instantiate more
            while (count < items.Length)
            {
                ItemData item = items[count];
                GameObject button = Instantiate(itemButtonPrefab);
                button.GetComponent<ButtonItem>().hud = HUD;
                button.transform.SetParent(container, false);
                SetInventoryItemButton(button, item);
                count++;
            }
        }
    }

    private void SetInventoryItemButton(GameObject button, ItemData item)
    {
        TextMeshProUGUI itemText = button.GetComponentInChildren<TextMeshProUGUI>();
        Image itemIcon = button.transform.GetChild(0).GetComponent<Image>();
        itemText.text = item.name;
        itemIcon.sprite = item.image;
        button.GetComponent<ButtonItem>().item = item;
        button.SetActive(true);
    }
}
