using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Users;

public class RebindMenu : MonoBehaviour {
    public GameObject buttonPrefab;
    public Transform parent;
    public InputActionAsset inputActionAsset;

    // Start is called before the first frame update
    void Start() {
        foreach (InputAction action in inputActionAsset.FindActionMap("Controls").actions) {
            GameObject obj = Instantiate(buttonPrefab, new Vector3(0, 0, 0), new Quaternion(0, 0, 0, 0), parent);
            Debug.Log("Action name: " + action.name + ", action: " + action);
            RebindButton button = obj.GetComponent<RebindButton>();
            button.action = action;
            button.labelString.SetText(action.name);
        }

        foreach (RebindButton gameObject in parent.GetComponentsInChildren<RebindButton>()) {
            if(gameObject.name != "Back") {
                gameObject.InitializeBindingControlScheme(FindObjectOfType<UIMenuManager>().activePlayerInput);
            }
        }
    }

    private void OnDestroy() {
        foreach (RebindButton gameObject in parent.GetComponentsInChildren<RebindButton>()) {
            Destroy(gameObject);
        }
    }
}
