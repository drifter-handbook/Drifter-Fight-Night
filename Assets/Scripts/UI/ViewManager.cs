using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Controls;

// TODO: Rename to Menu Manager
// Handles the menu logic flow and sends important stuff back to the game controller to disseminate
public class ViewManager : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [Help("All views must be registered by being children of this game object!", UnityEditor.MessageType.Warning)]
#endif
    public Transform startingMenu;

    public GameObject savedIPObject;
    public GameObject roomNameObject;

    UIMenuType currentView;
    Dictionary<UIMenuType, Transform> views = new Dictionary<UIMenuType, Transform>();
    bool mouse = true;

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    PlayerInput[] playerInputs;
    public List<UIMenuType> menuFlowHistory = new List<UIMenuType>();

    [SerializeField]
    public InputSystemUIInputModule uiInputModule;
    void Awake()
    {
        mouse = true;
        views = new Dictionary<UIMenuType, Transform>();
        if (views.Count <= 0)
        {
            foreach (var child in this.gameObject.GetComponentsInDirectChildren<Transform>())
            {
                child.gameObject.SetActive(false);

                if (views.ContainsKey(child.gameObject.GetComponent<UIMenu>().currentMenu))
                {
                    Debug.LogWarning("Views already contains key " + child.gameObject.name + "!");
                    continue;
                }
                views.Add(child.gameObject.GetComponent<UIMenu>().currentMenu, child.transform);
            }
        }
        startingMenu.gameObject.SetActive(true);
        currentView = startingMenu.gameObject.GetComponent<UIMenu>().currentMenu;

        //Change this later to back out to the previous menu instead
        ShowUIMenuTypeView(UIMenuType.MainMenu);
        menuFlowHistory.Add(UIMenuType.MainMenu);
    }
    public void UpdateToggles()
    {
        toggle1.onValueChanged.RemoveAllListeners();
        toggle2.onValueChanged.RemoveAllListeners();
        toggle3.onValueChanged.RemoveAllListeners();

        toggle1.isOn = PlayerPrefs.GetInt("dynamicCamera") > 0;
        toggle2.isOn = PlayerPrefs.GetInt("HidePing") > 0;
        toggle3.isOn = PlayerPrefs.GetInt("HideTextInput") > 0;
        //   ^ toggles the code too. Why? idk, unity makes interesting decisions sometimes

        toggle2.onValueChanged.AddListener(delegate {
            togglePing();
        });
    }

    void Update()
    {
        playerInputs = FindObjectsOfType<PlayerInput>();
        foreach(PlayerInput playerInput in playerInputs)
        {
            if (playerInput != null && playerInput.currentActionMap.FindAction("Cancel").triggered)
            {
                if (currentView == UIMenuType.MainMenu)
                {
                    Application.Quit();
                    return;
                }
                else
                {
                    Debug.Log("Pressed back time to die: " + menuFlowHistory[menuFlowHistory.Count - 1] + "\n");
                    mouse = false;
                    menuFlowHistory.Remove(menuFlowHistory[menuFlowHistory.Count - 1]);
                    ShowUIMenuTypeView(menuFlowHistory[menuFlowHistory.Count - 1]);
                    return;
                }
            }

            bool gamepadButtonPressed = false;
            if (Gamepad.current != null)
            {
                for (int i = 0; i < Gamepad.current.allControls.Count; i++)
                {
                    var c = Gamepad.current.allControls[i];
                    if (c is ButtonControl)
                    {
                        if (((ButtonControl)c).wasPressedThisFrame)
                        {
                            gamepadButtonPressed = true;
                        }
                    }
                }
            }

            //In the process of trying to remove hacks, I added another hack. Feels bad man.
            //UI Input Module is bad and should feel bad. It only knows how to handle one playerInput mapping at a time.
            //To get around this and allow all controllers to navigate initial UI menus before Character Select, we detect
            //the input type and force set that player input map to the UI Input Module so Unity's bad single player-only UI system
            //pretends like it is successfully working with multiple controllers.

            //This limitation also means we should have the key rebinding menu in Character Select, NOT the ViewManager screens.
            if((playerInput.currentControlScheme == "Gamepad" && gamepadButtonPressed) || (playerInput.currentControlScheme == "Keyboard" && Keyboard.current.anyKey.isPressed))
            {
                uiInputModule.actionsAsset = playerInput.actions;
            }

            if (playerInput != null && playerInput.currentActionMap.FindAction("Click").ReadValue<float>() > 0 || playerInput.currentActionMap.FindAction("RightClick").ReadValue<float>() > 0 || playerInput.currentActionMap.FindAction("MiddleClick").ReadValue<float>() > 0 && !mouse)
            {
                mouse = true;
                //Cursor.visible = true;
                EventSystem.current.SetSelectedGameObject(null);
                return;

            }
            else if ((Keyboard.current.anyKey.isPressed || gamepadButtonPressed) && mouse && (!(playerInput.currentActionMap.FindAction("Click").ReadValue<float>() > 0) || !(playerInput.currentActionMap.FindAction("RightClick").ReadValue<float>() > 0) || !(playerInput.currentActionMap.FindAction("MiddleClick").ReadValue<float>() > 0)))
            {
                mouse = false;
                //Cursor.visible = false;
                switch (currentView)
                {
                    case UIMenuType.MainMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Main Play")); break;
                    case UIMenuType.ModeMenu: ShowUIMenuTypeView(UIMenuType.ModeMenu); break;
                    case UIMenuType.LocalMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Local")); break;
                    case UIMenuType.OnlineMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Online")); break;
                    case UIMenuType.HostMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Host")); break;
                    case UIMenuType.JoinMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join")); break;
                    case UIMenuType.SettingsMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
                    case UIMenuType.RebindMenu: EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
                    default:
                        {
                            Debug.Log("Unsupported UIMenuType " + currentView + "\n");
                        }
                        break;
                }
                return;
            }
        }
    }

    public Transform GetView(UIMenuType name)
    {
        return views[name];
    }

    [com.llamagod.EnumAction(typeof(UIMenuType))]
    public void SetView(int view)
    {
        ShowUIMenuTypeView((UIMenuType)view);
        menuFlowHistory.Add((UIMenuType)view);
    }

    [com.llamagod.EnumAction(typeof(UIMenuType))]
    public void SetViewBack(int view)
    {
        menuFlowHistory.Remove(menuFlowHistory[menuFlowHistory.Count - 1]);
        ShowUIMenuTypeView((UIMenuType)view);
    }

    public void ShowUIMenuTypeView(UIMenuType name)
    { 
        views[currentView].gameObject.SetActive(false);
        currentView = name;
        views[name].gameObject.SetActive(true);
        // if(roomCodeBox.activeSelf && PlayerPrefs.GetInt("HideRoomCode") > 0)
        // {
        //     roomCodeBox.GetComponent<InputField>().contentType = InputField.ContentType.Password;
        // }
        // else if (roomCodeBox.activeSelf && PlayerPrefs.GetInt("HideRoomCode") == 0)
        // {
        //     roomCodeBox.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
        // }

        switch(name)
        {
            case UIMenuType.MainMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Main Play"));  break;
            case UIMenuType.ModeMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Local"));      break;
            case UIMenuType.OnlineMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Host"));     break;
            case UIMenuType.RebindMenu: if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings")); break;
            case UIMenuType.LocalMenu:
                {
                    if (Debug.isDebugBuild)
                    {
                        if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Training"));
                    }
                    else
                    {
                        if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Fight Night"));
                    }
                    break;
                }
            case UIMenuType.HostMenu:
                {
                    if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Host Button"));
                    roomNameObject.GetComponent<InputField>().text = GameController.Instance.Username;
                    break;
                }
            case UIMenuType.JoinMenu:
                {
                    if (!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join"));
                    // savedIPObject.GetComponent<InputField>().contentType = PlayerPrefs.GetInt("HideTextInput") > 0 ? InputField.ContentType.Password : InputField.ContentType.Standard;
                    // if (PlayerPrefs.GetString("savedIP") != null)
                    // {
                    //     savedIPObject.GetComponent<InputField>().text = PlayerPrefs.GetString("savedIP");
                    // }
                    break;
                }
            case UIMenuType.SettingsMenu:
                {
                    UpdateToggles();
                    if(!mouse) EventSystem.current.SetSelectedGameObject(GameObject.Find("IPToggle"));
                    break;
                }
            default:
                {
                    Debug.Log("Unhandled UIMenuType type" + name + "\n");
                    break;
                }
        }
        
    }

    // May be moved to game controller?
    public void Exit()
    {
        Application.Quit();
    }

    public void togglePing()
    {
        if (PlayerPrefs.GetInt("HidePing") == 0) { PlayerPrefs.SetInt("HidePing", 1); }
        else { PlayerPrefs.SetInt("HidePing", 0); }
        PlayerPrefs.Save();
    }

    public void saveRoomCode()
    {

        PlayerPrefs.SetString("savedIP",savedIPObject.GetComponent<InputField>().text);

    }

    // public void setRoomName()
    // {

    //     GameController.Instance.Username = roomNameObject.GetComponent<InputField>().text;

    // }

    public void toggleDynamicCamera()
    {

        PlayerPrefs.SetInt("dynamicCamera",toggle1.isOn?1:0);

    }
  
}
