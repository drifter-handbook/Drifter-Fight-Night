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

    public void InitializeBindingControlScheme(PlayerInput input) {
        bindingName = input.currentControlScheme;
        mappingString.text = actionReference.action.GetBindingDisplayString();
    }

    public void RemapButtonClicked()
    {
        actionReference.action.Disable();
        var rebindOperation = actionReference.action.PerformInteractiveRebinding().WithBindingGroup(bindingName).Start();
        rebindOperation.OnComplete(operation => {
            actionReference.action.Enable();
            mappingString.text = actionReference.action.GetBindingDisplayString();
        });
    }
}
