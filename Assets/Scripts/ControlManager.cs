using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

public class ControlManager : MonoBehaviour
{
    public enum DeviceType
    {
        KeyboardMouse,
        Gamepad,
        GamepadXbox,
        GamepadPS
    }

    [SerializeField]
    private PlayerInput _pI;
    private DeviceType activeDevice = DeviceType.KeyboardMouse;

    public event Action ActiveDeviceChanged;

    public PlayerInput PlayerInput { get { return _pI; } }
    public DeviceType ActiveDevice { get { return activeDevice; } }
    public string ActiveDeviceScheme
    {
        get
        {
            switch (ActiveDevice)
            {
                case DeviceType.KeyboardMouse: return "Keyboard&Mouse";
                case DeviceType.Gamepad:
                case DeviceType.GamepadXbox:
                case DeviceType.GamepadPS: return "Gamepad";
            }
            return null;
        }
    }

    void Awake()
    {
        if (_pI == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _pI = player.GetComponent<PlayerInput>();
        }
        InputSystem.onActionChange += OnActionPerformed;
    }

    private void OnActionPerformed(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction action = (InputAction)obj;
            InputControl control = action.activeControl;

            DeviceType newDevice = DeviceType.KeyboardMouse;

            if (control.device is Keyboard)
            {
                newDevice = DeviceType.KeyboardMouse;
            }

            if (control.device is Gamepad)
            {
                newDevice = DeviceType.Gamepad;

                if (control.device is XInputController)
                {
                    newDevice = DeviceType.GamepadXbox;
                }

                if (control.device is DualShockGamepad)
                {
                    newDevice = DeviceType.GamepadPS;
                }
            }

            if (activeDevice != newDevice)
            {
                activeDevice = newDevice;
                ActiveDeviceChanged?.Invoke();
            }
        }
    }

    public InputBinding GetBinding(string actionName)
    {
        InputActionAsset actions = _pI.actions;
        string schemeName = _pI.currentControlScheme;
        InputAction action = _pI.actions.FindAction(actionName);

        int bindIndex = action.GetBindingIndex(group: schemeName);
        InputBinding deviceBinding = action.bindings[bindIndex];
        return deviceBinding;
    }

    public Sprite GetBindingIcon(string actionName)
    {
        string[] paths = GetBindingIconPath(actionName);
        return LoadBindingIconSprite(paths);
    }

    public string[] GetBindingIconPath(string actionName)
    {
        string controlPath = GetActionControlPath(actionName);
        string sheetPath = GetDeviceSheetPath();
        string alias = GetBindingAlias(controlPath);
        string[] paths = { sheetPath, alias };
        return paths;
    }

    private string GetActionControlPath(string actionName)
    {
        InputActionAsset actions = _pI.actions;
        InputActionMap map = _pI.currentActionMap;

        if (actions == null || map == null) return "";

        InputAction action = _pI.actions.FindAction(actionName);

        string scheme = ActiveDeviceScheme ?? _pI.currentControlScheme;
        int bindIndex = action.GetBindingIndex(group: scheme);
        if (bindIndex < 0) return "";
                
        string binding = action.GetBindingDisplayString(bindIndex, out string deviceLayout, out string controlPath);

        return controlPath;
    }

    private string GetDeviceSheetPath()
    {
        string assetPath = "Assets/Sprites/UI/ButtonPrompts/";
        string deviceSheet = "";

        switch (activeDevice)
        {
            case DeviceType.KeyboardMouse:
                deviceSheet = "keyboard-&-mouse_sheet_default.png";
                break;
            case DeviceType.Gamepad:
            case DeviceType.GamepadXbox:
                deviceSheet = "xbox-series_sheet_default.png";
                break;
            case DeviceType.GamepadPS:
                deviceSheet = "playstation-series_sheet_default.png";
                break;
        }

        return assetPath + deviceSheet;
    }

    private string GetBindingAlias(string controlPath)
    {
        string device = "";

        switch (activeDevice)
        {
            case DeviceType.KeyboardMouse:
                device = "keyboard_";
                break;
            case DeviceType.Gamepad:
            case DeviceType.GamepadXbox:
                device = "xbox_";
                break;
            case DeviceType.GamepadPS:
                device = "playstation_";
                break;
        }

        string icon = "";
        switch (controlPath)
        {
            case "buttonSouth":
            case "buttonEast":
            case "buttonWest":
            case "buttonNorth":
                icon = GetFaceButtonAlias(controlPath);
                break;
            default:
                icon = controlPath.Replace('/', '_');
                break;
        }
        return device + icon;
    }

    private string GetFaceButtonAlias(string button)
    {
        Dictionary<DeviceType,string> d = new();

        switch (button)
        {
            case "buttonSouth":
                d.Add(DeviceType.Gamepad, "a");
                d.Add(DeviceType.GamepadXbox, "a");
                d.Add(DeviceType.GamepadPS, "cross");
                break;
            case "buttonEast":
                d.Add(DeviceType.Gamepad, "b");
                d.Add(DeviceType.GamepadXbox, "b");
                d.Add(DeviceType.GamepadPS, "circle");
                break;
            case "buttonWest":
                d.Add(DeviceType.Gamepad, "x");
                d.Add(DeviceType.GamepadXbox, "x");
                d.Add(DeviceType.GamepadPS, "square");
                break;
            case "buttonNorth":
                d.Add(DeviceType.Gamepad, "y");
                d.Add(DeviceType.GamepadXbox, "y");
                d.Add(DeviceType.GamepadPS, "triangle");
                break;
            default: break;
        }

        return $"button_color_{d[activeDevice]}";
    }

    public Sprite LoadBindingIconSprite(string[] paths)
    {
        string sheetPath = paths[0];
        string subPath = paths[1];

        Sprite[] sheet = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
        Dictionary<string, Sprite> dic = sheet.ToDictionary(spr => spr.name);

        if (dic.TryGetValue(subPath, out Sprite sprite))
        {
            Debug.Log($"Loaded sprite at \"{sheetPath}\" named \"{subPath}\"");
            return sprite;
        }
        return null;
    }
}
