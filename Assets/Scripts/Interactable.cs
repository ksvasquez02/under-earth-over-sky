using System.Collections.Generic;
using System.Reflection.Emit;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    public bool isActive = true;
    public ItemData itemData;

    public bool tooltipActive = false;
    private TextMeshProUGUI tooltip;

    public string TooltipLabel { get { return GetItemTypeLabel(itemData.type); } } 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Canvas canvas = GetComponentInChildren<Canvas>();
        if (canvas == null) return;
        tooltip = canvas.GetComponentInChildren<TextMeshProUGUI>();
        if (tooltip == null) return;
        tooltip.text = GetItemTypeLabel(itemData.type);
        ToggleTooltip(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (tooltip == null) return;
    }

    public bool ToggleTooltip(bool enable)
    {
        //Debug.Log($"Toggling toolip {(enable ? "on" : "off")}: {tooltip.text}");
        if (tooltip != null)
        {
            tooltipActive = enable;
            tooltip.gameObject.SetActive(enable);
            return true;
        }
        return false;
    }

    private static string GetItemTypeLabel(ItemType type)
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
