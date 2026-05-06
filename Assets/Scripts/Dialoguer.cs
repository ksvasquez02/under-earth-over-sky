using UnityEngine;
using System;
using System.Collections.Generic;

public class Dialoguer : MonoBehaviour
{
    public bool isActive = true;
    public List<DialogueEntryData> entries = new();
}

[Serializable]
public struct DialogueEntryData
{
    public string title;
    public string text;
    public float time;
}
