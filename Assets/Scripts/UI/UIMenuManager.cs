using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

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
    [HideInInspector]
    public bool mouse = false;
    public void InitializeMenus() {
        for (int index = 0; index < menuItemStates.Count; index++) {
            if (menuGameObjects.Count > index) {
                menuList.Add(menuItemStates[index], menuGameObjects[index]);
            }
            else {
                Debug.LogWarning("Size mismatch! Index " + index + "!");
            }
        }
        //Change this later to back out to the previous menu instead
        ShowUIMenuTypeView(startingMenu);
        menuFlowHistory.Add(startingMenu);
    }

    public void ClearMenus() {
        activeMenu = UIMenuType.Invalid;
        menuFlowHistory.Clear();
        menuList.Clear();
    }

    public virtual void UpdateToggles() {
        //intentionally empty, overridden in viewManager so the menuManager logic can be used for other scenes.
    }

    public Transform GetView(UIMenuType name) {
        return menuList[name].transform;
    }

    [com.llamagod.EnumAction(typeof(UIMenuType))]
    public void SetView(int view) {
        ShowUIMenuTypeView((UIMenuType)view);
        menuFlowHistory.Add((UIMenuType)view);
    }

    [com.llamagod.EnumAction(typeof(UIMenuType))]
    public void SetViewBack(int view) {
        menuFlowHistory.Remove(menuFlowHistory[menuFlowHistory.Count - 1]);
        ShowUIMenuTypeView((UIMenuType)view);
    }

    public void ReturnToPriorMenu() {
        menuFlowHistory.Remove(menuFlowHistory[menuFlowHistory.Count - 1]);
        ShowUIMenuTypeView(menuFlowHistory[menuFlowHistory.Count - 1]);
    }

    public void ShowUIMenuTypeView(UIMenuType name) {
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
            case UIMenuType.MainMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Main Play")); break;
            case UIMenuType.ModeMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Local")); break;
            case UIMenuType.OnlineMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Host")); break;
            case UIMenuType.RebindMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
            case UIMenuType.LocalMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(Debug.isDebugBuild ? GameObject.Find("Training") : GameObject.Find("Fight Night")); break;
            case UIMenuType.HostMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Host Button")); break;
            case UIMenuType.JoinMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join")); break;
            case UIMenuType.InGamePauseMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Continue")); break;
            case UIMenuType.InGameSettingsMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Back")); break;
            case UIMenuType.SettingsMenu: {
                    UpdateToggles();
                    if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("IPToggle"));
                    break;
                }
            default: {
                    Debug.Log("Unhandled UIMenuType type" + name + "\n");
                    break;
                }
        }

    }

    public void SetSelectedGameObjectAfterMouseDisabled() {
        switch (activeMenu) {
            case UIMenuType.MainMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Main Play")); break;
            case UIMenuType.ModeMenu: ShowUIMenuTypeView(UIMenuType.ModeMenu); break;
            case UIMenuType.LocalMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Local")); break;
            case UIMenuType.OnlineMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Online")); break;
            case UIMenuType.HostMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Host")); break;
            case UIMenuType.JoinMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join")); break;
            case UIMenuType.SettingsMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
            case UIMenuType.RebindMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
            default: {
                    Debug.Log("Unsupported UIMenuType " + activeMenu + "\n");
                }
                break;
        }
        return;
    }

    public void Exit() {
        Application.Quit();
    }
}
