using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class Interactable : MonoBehaviour
{
    [SerializeField]
    private bool isActive = true;
    [SerializeField]
    private List<ItemData> items = new();
    [SerializeField]
    private int stage = 0;

    [SerializeField]
    private GameObject tooltip;
    private TextMeshProUGUI tooltipText;
    private Image tooltipImage;

    public ItemData CurrentItem
    {
        get
        {
            if (items == null || stage >= items.Count) return new ItemData();
            return items[stage];
        }
    }
    public int Stage { get { return stage; }  set { stage = value; } }
    public bool IsComplete { get { return stage >= items.Count; } }
    public string TooltipLabel { get { return IsComplete ? "" : GetTypeLabel(CurrentItem.type); } }

    private void Awake()
    {
        if (tooltip != null)
        {
            tooltipText = tooltip.GetComponentInChildren<TextMeshProUGUI>();
            Transform keyContainer = tooltip.transform.GetChild(1);
            tooltipImage = keyContainer.GetChild(0).GetComponent<Image>();
            tooltip.SetActive(false);
        }
    }

    public void ShowTooltip(Sprite icon = null)
    {
        if (!isActive || tooltip == null) return;

        tooltipText.text = TooltipLabel;
        if (icon != null) tooltipImage.sprite = icon;

        if (tooltipText.text == "") HideTooltip();
        else tooltip.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltip == null) return;
        tooltip.SetActive(false);
    }

    private static string GetTypeLabel(ItemType type)
    {
        switch (type)
        {
            case ItemType.Artifact:
                return "Examine";
            case ItemType.Memory:
                return "Recall memory";
            default:
                return "Interact";
        }
    }
}
