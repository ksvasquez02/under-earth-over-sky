using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class HUDManager : MonoBehaviour
{
    public GameObject menu;
    private List<GameObject> menuPanels;

    public GameObject lorePanel;
    private TextMeshProUGUI loreSpeaker;
    private TextMeshProUGUI loreText;
    private Image itemImage;

    public GameObject inventoryPanel;
    private Dictionary<ItemType, GameObject> inventorySubsections;
    [SerializeField]
    private GameObject itemButtonPrefab;

    public GameObject tooltip;
    private TextMeshProUGUI tooltipText;
    private TextMeshProUGUI tooltipKey;

    private void Awake()
    {
        menuPanels = new List<GameObject>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (lorePanel != null)
        {
            GameObject lorePanelDialogue = lorePanel.transform.GetChild(0).gameObject;
            loreSpeaker = lorePanelDialogue.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            loreText = lorePanelDialogue.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            itemImage = lorePanel.transform.GetChild(1).gameObject.GetComponent<Image>();
            menuPanels.Insert(0, lorePanel);
        }
        if (inventoryPanel != null)
        {
            inventorySubsections = new Dictionary<ItemType, GameObject>();
            GameObject subContainer = inventoryPanel.transform.GetChild(0).GetChild(1).gameObject;
            inventorySubsections.Add(ItemType.Artifact, subContainer.transform.GetChild(0).gameObject);
            inventorySubsections.Add(ItemType.Language, subContainer.transform.GetChild(1).gameObject);
            inventorySubsections.Add(ItemType.Memory, subContainer.transform.GetChild(2).gameObject);
            menuPanels.Insert(1, inventoryPanel);
        }
        if (tooltip != null)
        {
            TextMeshProUGUI[] ttTexts = tooltip.GetComponentsInChildren<TextMeshProUGUI>();
            tooltipText = ttTexts[0];
            tooltipKey = ttTexts[1];
        }
    }

    // Global Menu
    public void ShowMenu(int state)
    {
        foreach (GameObject go in menuPanels)
        {
            go.SetActive(false);
        }
        menuPanels[state].SetActive(true);
        menu.SetActive(true);
    }
    public void HideMenu()
    {
        menu.SetActive(false);
    }
    public void HideMenu(int state)
    {
        if (menuPanels[state].activeInHierarchy) menu.SetActive(false);
    }

    // Lore Entries
    public void ShowItemMenu(ItemData item)
    {
        if (item.entries.Length <= 0) return;
        LoreEntryData entry = item.entries[0];

        loreSpeaker.text = item.name;
        SetLore(entry);
        ShowMenu(0);
    }
    public void HideItemMenu()
    {
        HideMenu(0);
    }
    private void SetLore(LoreEntryData entry)
    {
        loreText.text = entry.text;
        itemImage.sprite = entry.image;
    }

    // Inventory
    public void ShowInventory()
    {
        ShowMenu(1);
    }
    public void HideInventory()
    {
        HideMenu(1);
    }

    public void GenerateInventory(Inventory inventory)
    {
        foreach((ItemType type, GameObject sub) in inventorySubsections)
        {
            int count = 0;
            Transform container = sub.transform.GetChild(1);
            ItemData[] items = inventory.Contents[type].Values.ToArray();

            // First use existing children
            foreach(Transform child in container)
            {
                ItemData item = count < items.Length ? items[count] : new ItemData();
                GameObject button = child.gameObject;
                SetInventoryItemButton(button, item);
                count++;

            }

            // Then instantiate more
            while (count < items.Length)
            {
                ItemData item = items[count];
                GameObject button = Instantiate(itemButtonPrefab);
                button.GetComponent<ButtonItem>().hud = this;
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
    }

    public void ShowTooltip(Interactable interactable)
    {
        if (tooltip == null || interactable == null) return;
        tooltipText.text = interactable.TooltipLabel;
        tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltip == null) return;
        tooltip.SetActive(false);
    }
}
