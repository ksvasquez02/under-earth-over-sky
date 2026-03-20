using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

[CustomEditor(typeof(Interactable))]
public class ItemEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        return new PropertyField(serializedObject.FindProperty("itemData"));
    }
}
