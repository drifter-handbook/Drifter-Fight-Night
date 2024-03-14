using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using TMPro;

public class RebindButton : MonoBehaviour {
    [SerializeField]
    public InputActionReference actionReference;
    public TextMeshProUGUI mappingString;
    private static Action<InputUser, InputUserChange, InputDevice> userChangeCallback;
    string bindingName = "";

    public void RegisterRebindCallbacks(bool register) {
        if(register) {
            userChangeCallback = OnUserChange;
            InputUser.onChange += userChangeCallback;
        }
        else {
            InputUser.onChange -= userChangeCallback;
        }
    }

    public void RemapButtonClicked()
    {
        actionReference.action.Disable();
        var rebindOperation = actionReference.action.PerformInteractiveRebinding().WithBindingGroup(bindingName).Start();
        rebindOperation.OnComplete(operation => {
            actionReference.action.Enable();
        });
    }

    public void UpdateMappingString(string name) {
        bindingName = name;
        mappingString.text = actionReference.action.GetBindingDisplayString();
    }

    public void OnUserChange(InputUser user, InputUserChange change, InputDevice device)
    {
        if(user.controlScheme.HasValue)
        {
            UpdateMappingString(user.controlScheme.Value.name);
        }
    }
}
