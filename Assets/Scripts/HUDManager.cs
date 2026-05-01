using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public GameObject menu;
    public GameObject focusPanel;
    private GameObject itemDialoguePanel;
    private TextMeshProUGUI itemDiaSpeaker;
    private TextMeshProUGUI itemDiaText;
    private Image itemImage;

    public GameObject tooltip;
    private TextMeshProUGUI tooltipText;
    private TextMeshProUGUI tooltipKey;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (focusPanel != null)
        {
            itemDialoguePanel = focusPanel.transform.GetChild(0).gameObject;
            itemDiaSpeaker = itemDialoguePanel.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
            itemDiaText = itemDialoguePanel.transform.GetChild(1).gameObject.GetComponent<TextMeshProUGUI>();
            itemImage = focusPanel.transform.GetChild(1).gameObject.GetComponent<Image>();
        }
        if (tooltip != null)
        {
            TextMeshProUGUI[] ttTexts = tooltip.GetComponentsInChildren<TextMeshProUGUI>();
            tooltipText = ttTexts[0];
            tooltipKey = ttTexts[1];
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowItemMenu(ItemData item)
    {
        if (item.entries.Length <= 0) return;
        LoreEntryData entry = item.entries[0];

        itemDiaSpeaker.text = item.name;
        SetLore(entry);

        menu.SetActive(true);
    }

    //public void ShowItemMenu(ItemData[] items)
    //{
    //    if (items.Length <= 0) return;
    //    ItemData item = items[0];

    //    if (item.entries.Length <= 0) return;
    //    LoreEntryData entry = item.entries[0];

    //    SetLore(item, entry);

    //    menu.SetActive(true);
    //}

    public void HideItemMenu()
    {
        menu.SetActive(false);
    }

    private void SetLore(LoreEntryData entry)
    {
        itemDiaText.text = entry.text;
        itemImage.sprite = entry.image;
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
