using System;
using UnityEngine;

[Serializable]
public struct ItemData
{
    public int id;
    public ItemType type;
    public string name;
    public Sprite image;
    public string desc;
    public LoreEntryData[] entries;
}

[Serializable]
public struct LoreEntryData
{
    public string title;
    public Sprite image;
    public string text;
}

public enum ItemType
{
    Artifact,
    Language,
    Memory
}