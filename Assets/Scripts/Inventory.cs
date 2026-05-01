using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Inventory
{
    private Dictionary<ItemType, Dictionary<int, ItemData>> data;

    public Dictionary<ItemType, Dictionary<int, ItemData>> Contents { get { return data; } }

    public Inventory()
    {
        data = new Dictionary<ItemType, Dictionary<int, ItemData>>();
        data.Add(ItemType.Artifact, new Dictionary<int, ItemData>());
        data.Add(ItemType.Language, new Dictionary<int, ItemData>());
        data.Add(ItemType.Memory, new Dictionary<int, ItemData>());
    }

    public bool AddItem(ItemData item)
    {
        if (item.entries?.Length <= 0) return false;
        ItemType type = item.type;
        return data[type].TryAdd(item.id, item);
    }

    public ItemData GetItemByID(ItemType type, int id)
    {
        data.TryGetValue(type, out Dictionary<int, ItemData> subInventory);
        if (subInventory == null) return new ItemData();
        subInventory.TryGetValue(id, out ItemData item);
        return item;
    }
    public ItemData GetItemByIndex(ItemType type, int index)
    {
        data.TryGetValue(type, out Dictionary<int, ItemData> subInventory);
        if (subInventory == null) return new ItemData();
        ItemData[] array = subInventory.Values.ToArray();
        if (index >= array.Length) return new ItemData();
        return array[index];
    }
}