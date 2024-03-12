using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class EscapeMenu : UIMenuManager {
    public GameObject escapeMenu;

    void Update() {
        if(!GameController.Instance.IsPaused && GameController.Instance.canPause) {
            foreach(PlayerInput input in GameController.Instance.controls.Values) {
                if(input.currentActionMap.FindAction("Menu").ReadValue<float>()>0) {
                    input.SwitchCurrentActionMap("UI");
                    InputSystemUIInputModule uiInputModule = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
                    uiInputModule.actionsAsset = input.actions;
                    ToggleMenu();
                    return;
                }
            }
        }
    }

    public void ToggleMenu() {
        bool isPaused = !GameController.Instance.IsPaused;
        GameController.Instance.IsPaused = isPaused;
        escapeMenu.SetActive(isPaused);
        if (!isPaused) {
            foreach (PlayerInput input in GameController.Instance.controls.Values) {
                input.SwitchCurrentActionMap("Controls");
            }
            ClearMenus();
        }
        else {
            InitializeMenus();
        }
    }

    public void ReturnToTitle() {
        ToggleMenu();
        GameController.Instance.IsPaused = false;
        GameController.Instance.EndMatch();
    }
}
