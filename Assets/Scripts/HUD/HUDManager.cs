using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(HUDDialogue))]
[RequireComponent(typeof(HUDInventory))]
[RequireComponent(typeof(ControlManager))]
public class HUDManager : MonoBehaviour
{
    public GameObject menu;
    private readonly List<GameObject> menuPanels = new();

    public GameObject lorePanel;
    private TextMeshProUGUI loreSpeaker;
    private TextMeshProUGUI loreText;
    private Image itemImage;
    private Queue<LoreEntryData> queuedLore;


    private readonly List<Interactable> activeTooltips =  new();
    private Sprite tooltipIcon;

    private HUDInventory inventoryManager;
    private HUDDialogue dialogueManager;
    private ControlManager controlManager;

    private Player player;
    private MenuState state;
    private int previousState = -1;

    public MenuState State { get { return state; } }
    public List<GameObject> MenuPanels { get { return menuPanels; } }

    #region Init
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<Player>();
        inventoryManager = GetComponent<HUDInventory>();
        dialogueManager = GetComponent<HUDDialogue>();
        controlManager = GetComponent<ControlManager>();

        if (lorePanel != null)
        {
            Transform lorePanelDialogue = lorePanel.transform.GetChild(0);
            Transform lorePanelImageCon = lorePanel.transform.GetChild(1);
            loreSpeaker = lorePanelDialogue.GetChild(0).GetComponent<TextMeshProUGUI>();
            loreText = lorePanelDialogue.GetChild(1).GetComponent<TextMeshProUGUI>();
            itemImage = lorePanelImageCon.GetComponentInChildren<Image>();
            menuPanels.Insert((int)MenuState.Lore, lorePanel);
            lorePanel.SetActive(false);
        }

        if (inventoryManager.Initialize())
            inventoryManager.PopulateInventory(player.Inventory);

        tooltipIcon = controlManager.GetBindingIcon("Player/Interact");
        controlManager.ActiveDeviceChanged += UpdateTooltipIcon;
    }
    #endregion

    #region Menu
    // Global Menu
    public bool ShowMenu(MenuState stateId, bool sub = false)
    {
        if ((int)stateId >= menuPanels.Count) return false;
        bool isAlreadyActive = menuPanels[(int)stateId].activeSelf;
        state = stateId;
        previousState = -1;

        foreach (GameObject go in menuPanels)
        {
            if (sub && go.activeInHierarchy) previousState = menuPanels.IndexOf(go);
            go.SetActive(false);
        }
        menuPanels[(int)stateId].SetActive(true);
        menu.SetActive(true);
        player.LockPlayer();

        return !isAlreadyActive;
    }
    public bool ShowMenu(int state, bool sub = false)
    {
        return ShowMenu((MenuState)state, sub);
    }

    public void HideMenu()
    {
        menu.SetActive(false);
        state = MenuState.None;
        previousState = -1;
    }
    public void HideMenu(int stateId)
    {
        if (previousState > 0 && previousState != stateId)
            ShowMenu(previousState);
        else if (menuPanels[stateId].activeSelf)
            HideMenu();
    }
    public void HideMenu(MenuState state)
    {
        HideMenu((int)state);
    }
    #endregion

    #region LoreEntry
    // Lore Entries
    public void PopulateLore(ItemData item)
    {
        if (item.entries == null || item.entries.Length <= 0) return;
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
    #endregion

    #region Inventory
    // Inventory
    public void PopulateInventory(Inventory inventory)
    {
        inventoryManager.PopulateInventory(inventory);
    }
    #endregion

    #region Dialogue
    // Dialogue
    public void ShowDialogue(Dialoguer dia)
    {
        dialogueManager.ShowDialogue(dia);
    }
    #endregion

    #region Tooltips
    // Tooltips
    public void ShowTooltip(Interactable interactable)
    {
        interactable.ShowTooltip(tooltipIcon);
        if (!activeTooltips.Contains(interactable)) activeTooltips.Add(interactable);
    }
    public void HideTooltip(Interactable interactable)
    {
        interactable.HideTooltip();
        activeTooltips.Remove(interactable);
    }
    public void UpdateTooltipIcon()
    {
        tooltipIcon = controlManager.GetBindingIcon("Player/Interact");
        foreach (Interactable inter in activeTooltips)
        {
            inter.ShowTooltip(tooltipIcon);
        }
    }
    #endregion
}

public enum MenuState
{
    None = -1,
    Lore = 0,
    Inventory = 1,
}
