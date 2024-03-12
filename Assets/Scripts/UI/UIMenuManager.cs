using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Controls;

public class UIMenuManager : MonoBehaviour {
#if UNITY_EDITOR
    [Help("Menu Item States and Menu Game Objects must be the same size and have valid objects!", UnityEditor.MessageType.Warning)]
#endif

    [SerializeField]
    public UIMenuType startingMenu;
    [SerializeField]
    public List<UIMenuType> menuItemStates;
    [SerializeField]
    public List<GameObject> menuGameObjects;

    [HideInInspector]
    Dictionary<UIMenuType, GameObject> menuList = new Dictionary<UIMenuType, GameObject>();
    [HideInInspector]
    public UIMenuType activeMenu = UIMenuType.Invalid;
    [HideInInspector]
    public List<UIMenuType> menuFlowHistory = new List<UIMenuType>();
    
    public void InitializeMenus() {
        //HACK: Could not figure out serializable dictionaries in a reasonable time, so doing this nonsense.
        //Afterwards, initialize the first default menu view.
        if(menuItemStates.Count != menuGameObjects.Count) {
            Debug.LogWarning("Size mismatch between Menu Item States and Menu Game Objects, fatal error!");
            return;
        }

        for (int index = 0; index < menuItemStates.Count; index++) {
            if (menuGameObjects.Count > index) {
                menuList.Add(menuItemStates[index], menuGameObjects[index]);
            }
        }
        ShowUIMenuTypeView(startingMenu);
        menuFlowHistory.Add(startingMenu);
    }

    public void ClearMenus() {
        activeMenu = UIMenuType.Invalid;
        menuFlowHistory.Clear();
        menuList.Clear();
    }

    [com.llamagod.EnumAction(typeof(UIMenuType))]
    public void SetView(int view) {
        ShowUIMenuTypeView((UIMenuType)view);
        menuFlowHistory.Add((UIMenuType)view);
    }

    [com.llamagod.EnumAction(typeof(UIMenuType))]
    public void SetViewBack(int view) {
        //user pressed the back button, so remove option in menuFlowHistory rather than add to it.
        menuFlowHistory.Remove(menuFlowHistory[menuFlowHistory.Count - 1]);
        ShowUIMenuTypeView((UIMenuType)view);
    }

    public Transform GetView(UIMenuType name) {
        return menuList[name].transform;
    }

    public void ReturnToPriorMenu() {
        //use the stored flow history to go to the previous menu view.
        menuFlowHistory.Remove(menuFlowHistory[menuFlowHistory.Count - 1]);
        ShowUIMenuTypeView(menuFlowHistory[menuFlowHistory.Count - 1]);
    }

    public void ShowUIMenuTypeView(UIMenuType name) {
        //swap to new menu view defined by parameter "name" and set the selected game object. Disable the prior view if valid. 
        if (!menuList.ContainsKey(name)) {
            Debug.LogWarning("Invalid Menu for Screen, fatal error! Menu " + name + "!");
            return;
        }

        if (activeMenu != UIMenuType.Invalid && menuList.ContainsKey(activeMenu)) {
            menuList[activeMenu].gameObject.SetActive(false);
        }
        activeMenu = name;

        menuList[activeMenu].gameObject.SetActive(true);

        switch (name) {
            case UIMenuType.MainMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Main Play")); break;
            case UIMenuType.ModeMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Local")); break;
            case UIMenuType.OnlineMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Host")); break;
            case UIMenuType.RebindMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
            case UIMenuType.LocalMenu: EventSystem.current.SetSelectedGameObject(Debug.isDebugBuild ? GameObject.Find("Training") : GameObject.Find("Fight Night")); break;
            case UIMenuType.HostMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Host Button")); break;
            case UIMenuType.JoinMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join")); break;
            case UIMenuType.InGamePauseMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Continue")); break;
            case UIMenuType.InGameSettingsMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back")); break;
            case UIMenuType.SettingsMenu: {
                    UpdateToggles();
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("IPToggle"));
                    break;
            }
            default: {
                    Debug.Log("Unhandled UIMenuType type" + name + "\n");
                    break;
            }
        }
    }

    public void UpdateActivePlayerInputs(PlayerInput playerInput) {
        //check for any gamepad input. If it exists, adjust UI input assigment. See explanation below.
        bool gamepadButtonPressed = false;
        if (Gamepad.current != null) {
            for (int i = 0; i < Gamepad.current.allControls.Count; i++) {
                var c = Gamepad.current.allControls[i];
                if (c is ButtonControl) {
                    if (((ButtonControl)c).wasPressedThisFrame) {
                        gamepadButtonPressed = true;
                        break;
                    }
                }
            }
        }

        //In the process of trying to remove hacks, I added another hack. Feels bad man.
        //UI Input Module is bad and should feel bad. It only knows how to handle one playerInput mapping at a time.
        //To get around this and allow all controllers to navigate initial UI menus before Character Select, we detect
        //the input type and force set that player input map to the UI Input Module so Unity's bad single player-only UI system
        //pretends like it is successfully working with multiple controllers.

        //This limitation also means we should have the key rebinding menu in Character Select, NOT the MainMenuScreensManager screens.
        if ((playerInput.currentControlScheme == "Gamepad" && gamepadButtonPressed) || (playerInput.currentControlScheme == "Keyboard" && Keyboard.current.anyKey.isPressed)) {
            InputSystemUIInputModule uiInputModule = FindObjectOfType<InputSystemUIInputModule>();
            uiInputModule.actionsAsset = playerInput.actions;
        }
    }

    public virtual void Exit() {
        Application.Quit();
    }

    public virtual void UpdateToggles() {
        //intentionally empty, overridden in MainMenuScreensManager so the menuManager logic can be used for other scenes.
    }
}
