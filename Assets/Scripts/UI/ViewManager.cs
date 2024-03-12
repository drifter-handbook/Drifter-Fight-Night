using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.InputSystem.Controls;

// TODO: Rename to a name specific to initial main menu flow before Character Select
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
    void Awake() {
        InitializeMenus();
    }
    public override void UpdateToggles() {
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

    void FixedUpdate() {
        playerInputs = FindObjectsOfType<PlayerInput>();
        foreach(PlayerInput playerInput in playerInputs) {
            if (playerInput != null && playerInput.currentActionMap.FindAction("Cancel").triggered) {
                if (activeMenu == UIMenuType.MainMenu) {
                    Application.Quit();
                    return;
                }
                else {
                    Debug.Log("Pressed back time to die: " + menuFlowHistory[menuFlowHistory.Count - 1] + "\n");
                    ReturnToPriorMenu();
                    return;
                }
            }
            UpdateActivePlayerInputs(playerInput);
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
