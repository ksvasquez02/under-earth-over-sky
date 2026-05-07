using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityIS = UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    #region Variables
    public enum DeviceType
    {
        Keyboard,
        Gamepad,
        Xbox,
        Playstation,
        Switch
    }

    private const string CTRL_ICONS = "Assets/Data/InputIcons.json";

    [SerializeField]
    private PlayerInput _playerInput;
    private DeviceType activeDevice = DeviceType.Keyboard;
    private Dictionary<string, BindingNameData> bindingIconData = new();

    public event Action ActiveDeviceChanged;

    public PlayerInput PlayerInput { get { return _playerInput; } }
    public DeviceType ActiveDevice { get { return activeDevice; } }
    public string ActiveDeviceScheme
    {
        get { return activeDevice == DeviceType.Keyboard ? "Keyboard&Mouse" : "Gamepad"; }
    }
    #endregion

    #region Structs
    [Serializable]
    struct BindingNameDataWrapper
    {
        public BindingNameData[] data;
        public BindingNameDataWrapper(BindingNameData[] data)
        {
            this.data = data;
        }
    }

    [Serializable]
    struct BindingNameData
    {
        public string BUTTON;
        public string XBOX;
        public string PLAYSTATION;
        public string SWITCH;
        public string STEAM;
        public string MOUSE;
        public string KEYBOARD;

        public string GetIconName(ControlManager.DeviceType deviceType)
        {
            Dictionary<ControlManager.DeviceType, string> names = new()
        {
            { DeviceType.Gamepad, XBOX },
            { DeviceType.Xbox, XBOX },
            { DeviceType.Playstation, PLAYSTATION ?? XBOX },
            { DeviceType.Keyboard, KEYBOARD ?? MOUSE }
        };

            if (names.TryGetValue(deviceType, out var name) && name != "")
            {
                return name;
            }
            return null;
        }

        public static Dictionary<ControlManager.DeviceType, string> DevicePrefix = new()
        {
            { DeviceType.Gamepad, "xbox" },
            { DeviceType.Xbox, "xbox" },
            { DeviceType.Playstation, "playstation" },
            { DeviceType.Keyboard, "keyboard" }
        };
    }
    #endregion

    void Awake()
    {
        if (_playerInput == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            _playerInput = player.GetComponent<PlayerInput>();
        }
        LoadBindingIconNames();
        InputSystem.onActionChange += OnActionPerformed;
    }

    private void OnActionPerformed(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction action = (InputAction)obj;
            InputControl control = action.activeControl;

            DeviceType newDevice = DeviceType.Keyboard;

            if (control.device is Keyboard)
            {
                newDevice = DeviceType.Keyboard;
            }

            if (control.device is Gamepad)
            {
                newDevice = DeviceType.Gamepad;

                if (control.device is UnityIS.XInput.XInputController)
                    newDevice = DeviceType.Xbox;
                if (control.device is UnityIS.DualShock.DualShockGamepad)
                    newDevice = DeviceType.Playstation;
                if (control.device is UnityIS.Switch.SwitchProControllerHID)
                    newDevice = DeviceType.Switch;
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
        InputAction action = _playerInput.actions.FindAction(actionName);

        string scheme = ActiveDeviceScheme ?? _playerInput.currentControlScheme;
        int bindIndex = action.GetBindingIndex(group: scheme);

        return action.bindings[bindIndex];
    }

    #region ButtonIcons
    public Sprite GetBindingIcon(string actionName)
    {
        string[] paths = GetBindingIconPaths(actionName);
        return LoadBindingIconSprite(paths);
    }

    public string[] GetBindingIconPaths(string actionName)
    {
        string controlPath = GetActionControlPath(actionName);
        string sheetPath = GetActiveDeviceSheetPath();
        string alias = GetBindingIconName(controlPath);
        string[] paths = { sheetPath, alias };
        return paths;
    }

    private string GetActionControlPath(string actionName)
    {
        InputActionAsset actions = _playerInput.actions;
        InputActionMap map = _playerInput.currentActionMap;

        if (actions == null || map == null) return "";

        InputAction action = _playerInput.actions.FindAction(actionName);

        string scheme = ActiveDeviceScheme ?? _playerInput.currentControlScheme;
        int bindIndex = action.GetBindingIndex(group: scheme);
        if (bindIndex < 0) return "";

        string display = action.GetBindingDisplayString(bindIndex, out string deviceLayout, out string controlPath);

        return controlPath;
    }

    private string GetActiveDeviceSheetPath()
    {
        string assetPath = "Assets/Sprites/UI/ButtonPrompts/";
        string deviceSheet = "";

        switch (activeDevice)
        {
            case DeviceType.Keyboard:
                deviceSheet = "keyboard-&-mouse_sheet_default.png";
                break;
            case DeviceType.Gamepad:
            case DeviceType.Xbox:
                deviceSheet = "xbox-series_sheet_default.png";
                break;
            case DeviceType.Playstation:
                deviceSheet = "playstation-series_sheet_default.png";
                break;
        }

        return assetPath + deviceSheet;
    }

    private string GetBindingIconName(string controlPath)
    {
        string[] parts = controlPath.Split('/');

        string device = BindingNameData.DevicePrefix[activeDevice];

        // Look for the full path first, then the first part
        string path = bindingIconData.ContainsKey(controlPath) ? controlPath : parts[0];
        if (bindingIconData.TryGetValue(path, out BindingNameData data))
        {
            string icon = data.GetIconName(activeDevice);
            if (path != controlPath && parts.Length > 1) icon = $"{icon}_{parts[1]}";
            return $"{device}_{icon}";
        }
        // Not found, no special override
        return $"{device}_{controlPath}";
    }

    public Sprite LoadBindingIconSprite(string[] paths)
    {
        string sheetPath = paths[0];
        string subPath = paths[1];

        Sprite[] sheet = AssetDatabase.LoadAllAssetsAtPath(sheetPath).OfType<Sprite>().ToArray();
        Dictionary<string, Sprite> dic = sheet.ToDictionary(spr => spr.name);

        if (dic.TryGetValue(subPath, out Sprite sprite))
        {
            //Debug.Log($"Loaded sprite at \"{sheetPath}\" named \"{subPath}\"");
            return sprite;
        }
        return null;
    }

    private void LoadBindingIconNames()
    {
        TextAsset jsonGamepad = AssetDatabase.LoadAssetAtPath<TextAsset>(CTRL_ICONS);
        string textGamepad = jsonGamepad.text;
        BindingNameDataWrapper cpwGamepad = JsonUtility.FromJson<BindingNameDataWrapper>(textGamepad);
        bindingIconData = cpwGamepad.data.ToDictionary(d => d.BUTTON);
    }
    #endregion
}
