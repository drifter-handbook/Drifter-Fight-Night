using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;
using TMPro;

public class RebindButton : MonoBehaviour {
    [SerializeField]
    public InputAction action;
    public TextMeshProUGUI labelString;
    public TextMeshProUGUI mappingString;
    string bindingName = "";

    public void InitializeBindingControlScheme(PlayerInput input) {
        bindingName = input.currentControlScheme;
        mappingString.text = action.GetBindingDisplayString();
    }

    public void RemapButtonClicked()
    {
        action.Disable();
        var rebindOperation = action.PerformInteractiveRebinding().WithBindingGroup(bindingName).Start();
        rebindOperation.OnComplete(operation => {
            action.Enable();
            mappingString.text = action.GetBindingDisplayString();
        });
    }
}
