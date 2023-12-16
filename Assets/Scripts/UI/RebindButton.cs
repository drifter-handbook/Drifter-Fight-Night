using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class RebindButton : MonoBehaviour {
    [SerializeField]
    public InputActionReference actionReference;
    public Text mappingString;

    public void Awake() {
        UpdateMappingString();
    }

    public void RemapButtonClicked()
    {
        actionReference.action.Disable();
        var rebindOperation = actionReference.action.PerformInteractiveRebinding().Start();
        rebindOperation.OnComplete(operation => {
            actionReference.action.Enable();
            UpdateMappingString();
        });
    }

    public void UpdateMappingString() {
        mappingString.text = actionReference.action.GetBindingDisplayString();
    }
}
