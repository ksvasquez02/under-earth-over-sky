using UnityEngine;

public class ButtonItem : MonoBehaviour
{
    public HUDManager hud;
    public ItemData item;

    public void DisplayItem()
    {
        if (hud == null) return;
        hud.PopulateLore(item);
    }
}
