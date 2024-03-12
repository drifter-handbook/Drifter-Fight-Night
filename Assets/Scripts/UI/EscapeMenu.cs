using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class EscapeMenu : UIMenuManager {
    public GameObject escapeMenu;

    void Update() {
        //check to see if we should open up the pause menu. If so, set the input to UI action map and toggle the panel.
        if(!GameController.Instance.IsPaused && GameController.Instance.canPause) {
            foreach(PlayerInput input in GameController.Instance.controls.Values) {
                if(input.currentActionMap.FindAction("Menu").ReadValue<float>()>0) {
                    input.SwitchCurrentActionMap("UI");
                    InputSystemUIInputModule uiInputModule = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
                    uiInputModule.actionsAsset = input.actions;
                    TogglePauseMenuPanel();
                    return;
                }
            }
        }
    }

    public void TogglePauseMenuPanel() {
        //Set associated vars depending on if turning pause menu on/off
        //Set input to gameplay action map if closing pause menu.
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
        TogglePauseMenuPanel();
        GameController.Instance.IsPaused = false;
        GameController.Instance.EndMatch();
    }
}
