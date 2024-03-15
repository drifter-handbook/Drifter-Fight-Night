using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.InputSystem.UI;

public class EscapeMenu : UIMenuManager {
    public GameObject escapeMenu;
    public GameObject rebindMenuButtons;

    //This menu should onyl be accessible in local games (probably)
    //Doesnt need its own serialize/deserialize becasue its state is entirely based on other components

    public static EscapeMenu Instance { get; private set; }

    void Awake() {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public void UpdateFrame(PlayerInputData[] inputs) {
        //skip this function in online games
        if (!GameController.Instance.IsPaused && GameController.Instance.canPause) {
            for (int i = 0; i < inputs.Length; i++) {
                if (inputs[i].Menu) {
                    GameController.Instance.toggleInputSystem(true);
                    InputSystemUIInputModule uiInputModule = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
                    //Only the player who pressed pause gets menu privs
                    uiInputModule.actionsAsset = GameController.Instance.controls[i].actions;
                    TogglePauseMenuPanel();
                    InitializeRebindMenuControlScheme(GameController.Instance.controls[i]);
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
            GameController.Instance.controlGroup = ControlGroup.Controls;
            GameController.Instance.toggleInputSystem(false);
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

    public void InitializeRebindMenuControlScheme(PlayerInput input) {
        foreach (var child in rebindMenuButtons.GetComponentsInDirectChildren<RebindButton>()) {
            child.InitializeBindingControlScheme(input);
        }
    }
}
