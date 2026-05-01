using System;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isActive = true;
    public List<ItemData> items = new List<ItemData>();
    public int stage = 0;

    public ItemData CurrentItem
    {
        get
        {
            if (items == null || stage >= items.Count) return new ItemData();
            return items[stage];
        }
    }
    public string TooltipLabel
    {
        get
        { 
            return stage < items.Count ? GetItemTypeLabel(CurrentItem.type) : "";
        }
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
