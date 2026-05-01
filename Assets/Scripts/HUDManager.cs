using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HUDManager : MonoBehaviour
{
    public GameObject menu;
    private List<GameObject> menuPanels = new List<GameObject>();

    public GameObject lorePanel;
    private TextMeshProUGUI loreSpeaker;
    private TextMeshProUGUI loreText;
    private Image itemImage;
    private Queue<LoreEntryData> queuedLore;

    public GameObject inventoryPanel;
    private Dictionary<ItemType, GameObject> inventorySubsections;
    public GameObject itemButtonPrefab;

    public GameObject tooltip;
    private TextMeshProUGUI tooltipText;
    private TextMeshProUGUI tooltipKey;

    public GameObject diaPanel;
    private TextMeshProUGUI diaSpeaker;
    private TextMeshProUGUI diaText;
    private Dialoguer currentDia;
    private Queue<string> queuedDias = new Queue<string>();

    private Player player;
    private int state;
    private int previousState = -1;

    public int State { get { return state; } } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();

        if (lorePanel != null)
        {
            GameObject lorePanelDialogue = lorePanel.transform.GetChild(0).gameObject;
            loreSpeaker = lorePanelDialogue.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            loreText = lorePanelDialogue.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
            itemImage = lorePanel.transform.GetChild(1).GetComponent<Image>();
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
        if (diaPanel != null)
        {
            diaSpeaker = diaPanel.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            diaText = diaPanel.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
        }
    }

    // Global Menu
    public bool ShowMenu(int stateId, bool sub = false)
    {
        bool isAlreadyActive = menuPanels[stateId].activeSelf;
        state = stateId;
        previousState = -1;

        foreach (GameObject go in menuPanels)
        {
            if (sub && go.activeInHierarchy) previousState = menuPanels.IndexOf(go);
            go.SetActive(false);
        }
        menuPanels[stateId].SetActive(true);
        menu.SetActive(true);
        player.LockPlayer();

        return !isAlreadyActive;
    }
    public bool ShowMenu(MenuState state, bool sub = false)
    {
        return ShowMenu((int)state, sub);
    }

    public void HideMenu()
    {
        menu.SetActive(false);
        state = -1;
        previousState = -1;
    }
    public void HideMenu(int stateId)
    {
        if (previousState > 0 && previousState != stateId)
        {
            ShowMenu(previousState);
        }
        else if (menuPanels[stateId].activeSelf)
        {
            HideMenu();
        }
    }
    public void HideMenu(MenuState state)
    {
        HideMenu((int)state);
    }

    // Lore Entries
    public void PopulateLore(ItemData item)
    {
        if (item.entries?.Length <= 0) return;
        queuedLore = new Queue<LoreEntryData>(item.entries);
        LoreEntryData first = queuedLore.Dequeue();

        loreSpeaker.text = item.name;
        SetLore(first);
        ShowMenu(MenuState.Lore, true);
    }
    public bool AdvanceLore()
    {
        if (queuedLore.Count <= 0) return false;
        LoreEntryData next = queuedLore.Dequeue();
        SetLore(next);
        return true;
    }
    private void SetLore(LoreEntryData entry)
    {
        loreText.text = entry.text;
        itemImage.sprite = entry.image;
    }

    // Inventory
    public void PopulateInventory(Inventory inventory)
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

    // Tooltips
    public void ShowTooltip(Interactable interactable)
    {
        if (tooltip == null || interactable == null) return;
        tooltipText.text = interactable.TooltipLabel;
        if (tooltipText.text == "") HideTooltip();
        tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltip == null) return;
        tooltip.SetActive(false);
    }

    public void ShowDialogue(Dialoguer dia)
    {
        if (!dia.isActive) return;
        currentDia = dia;

        diaSpeaker.text = dia.title;

        queuedDias = new Queue<string>(dia.entries);
        string first = queuedDias.Dequeue();
        diaText.text = first;

        diaPanel.SetActive(true);
        dia.isActive = false;
        dia.OnTimerOff += AdvanceDialogue;
        dia.StartFadeTimer();
    }
    public void AdvanceDialogue()
    {
        if (queuedDias.Count <= 0) {
            currentDia.OnTimerOff -= AdvanceDialogue;
            HideDialogue();
            return;
        }
        string next = queuedDias.Dequeue();
        diaText.text = next;
        currentDia.StartFadeTimer();
    }
    public void HideDialogue()
    {
        currentDia = null;
        diaPanel.SetActive(false);
    }
}

public enum MenuState
{
    Lore,
    Inventory
}
