using System;
using UnityEngine;

[Serializable]
public struct ItemData
{
    public int id;
    public string name;
    public Sprite image;
    public string description;
}

enum ItemTypes
{
    Artifact,
    Language,
    Memory
}