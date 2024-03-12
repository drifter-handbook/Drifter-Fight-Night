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
public class ViewManager : UIMenuManager
{
    public GameObject savedIPObject;
    public GameObject roomNameObject;

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    PlayerInput[] playerInputs;

    [SerializeField]
    public InputSystemUIInputModule uiInputModule;
    void Awake()
    {
        InitializeMenus();
        mouse = true;
    }
    public override void UpdateToggles()
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

    void FixedUpdate()
    {
        playerInputs = FindObjectsOfType<PlayerInput>();
        foreach(PlayerInput playerInput in playerInputs)
        {
            if (playerInput != null && playerInput.currentActionMap.FindAction("Cancel").triggered)
            {
                if (activeMenu == UIMenuType.MainMenu)
                {
                    Application.Quit();
                    return;
                }
                else
                {
                    Debug.Log("Pressed back time to die: " + menuFlowHistory[menuFlowHistory.Count - 1] + "\n");
                    mouse = false;
                    ReturnToPriorMenu();
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
                EventSystem.current.SetSelectedGameObject(null);
                return;

            }
            else if ((Keyboard.current.anyKey.isPressed || gamepadButtonPressed) && mouse && (!(playerInput.currentActionMap.FindAction("Click").ReadValue<float>() > 0) || !(playerInput.currentActionMap.FindAction("RightClick").ReadValue<float>() > 0) || !(playerInput.currentActionMap.FindAction("MiddleClick").ReadValue<float>() > 0)))
            {
                mouse = false;
                SetSelectedGameObjectAfterMouseDisabled();
            }
        }
    }

    public void togglePing() {
        if (PlayerPrefs.GetInt("HidePing") == 0) { PlayerPrefs.SetInt("HidePing", 1); }
        else { PlayerPrefs.SetInt("HidePing", 0); }
        PlayerPrefs.Save();
    }

    public void saveRoomCode() {
        PlayerPrefs.SetString("savedIP",savedIPObject.GetComponent<InputField>().text);
    }

    // public void setRoomName()
    // {

    //     GameController.Instance.Username = roomNameObject.GetComponent<InputField>().text;

    // }

    public void toggleDynamicCamera() {
        PlayerPrefs.SetInt("dynamicCamera",toggle1.isOn?1:0);
    }
  
}
