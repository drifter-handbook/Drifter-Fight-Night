using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

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

    string currentView;
    Dictionary<string, Transform> views = new Dictionary<string, Transform>();
    bool mouse = true;

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    //public GameObject roomCodeBox;

    void Awake()
    {
        views = new Dictionary<string, Transform>();
        if (views.Count <= 0)
        {
            foreach (var child in this.gameObject.GetComponentsInDirectChildren<Transform>())
            {
                child.gameObject.SetActive(false);

                if (views.ContainsKey(child.gameObject.name))
                {
                    Debug.LogWarning("Views already contains key " + child.gameObject.name + "!");
                    continue;
                }
                views.Add(child.gameObject.name, child.transform);
            }
        }
        startingMenu.gameObject.SetActive(true);
        currentView = startingMenu.gameObject.name;
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
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (currentView == "Mode Menu")
            {
                Application.Quit();
            }
            else
            {
                mouse = false;
                ShowView("Mode Menu");
            }
        }

        if(Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed && !mouse)
        {
            mouse = true;
            //Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

        }
        else if(Keyboard.current.anyKey.isPressed && mouse && (!Mouse.current.leftButton.isPressed || !Mouse.current.rightButton.isPressed || !Mouse.current.middleButton.isPressed)){
            mouse = false;
            //Cursor.visible = false;
            switch (currentView){
            	case "Host Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Host"));
                    break;
                case "Join Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join"));
                    break;
                case "Online Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Online"));
                    break;
                case "Local Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Local"));
                    break;
                case "Mode Menu":
                    ShowView("Mode Menu");
                    break;
                case "Settings Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Settings"));
                    break;
                default:
                    break;
            }
        }

    }

    public Transform GetView(string name)
    {
        return views[name];
    }

    public void ShowView(string name)
    {
        
        views[currentView].gameObject.SetActive(false);
        currentView = name;
        views[name].gameObject.SetActive(true);


        // if(roomCodeBox.activeSelf && PlayerPrefs.GetInt("HideRoomCode") > 0)
        // {
        //     roomCodeBox.GetComponent<InputField>().contentType = InputField.ContentType.Password;
        // } else if (roomCodeBox.activeSelf && PlayerPrefs.GetInt("HideRoomCode") == 0){
        //     roomCodeBox.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
        // }

         if (name == "Local Menu" && !mouse)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find("Training"));
        }

        if (name == "Online Menu" && !mouse)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find("Host"));
        }
        if(name == "Mode Menu" && !mouse)
            EventSystem.current.SetSelectedGameObject(GameObject.Find("Local"));

        if (name == "Join Menu")
        {
            if(!mouse)EventSystem.current.SetSelectedGameObject(GameObject.Find("Back Join"));
            // if (PlayerPrefs.GetInt("HideTextInput") > 0)
            // {
            //     savedIPObject.GetComponent<InputField>().contentType = InputField.ContentType.Password;
            // } else
            // {
            //     savedIPObject.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
            // }

            // if (PlayerPrefs.GetString("savedIP") != null)
            // {
            //     savedIPObject.GetComponent<InputField>().text = PlayerPrefs.GetString("savedIP");
            // }
        }

        if (name == "Host Menu")
        {
            if(!mouse)EventSystem.current.SetSelectedGameObject(GameObject.Find("Host Button"));

            roomNameObject.GetComponent<InputField>().text = GameController.Instance.Username;
        }

        if(name == "Settings Menu")
        {
            UnityEngine.Debug.Log("Update toggles");
            UpdateToggles();
            if(!mouse)EventSystem.current.SetSelectedGameObject(GameObject.Find("Back"));
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

    public void setRoomName()
    {

        GameController.Instance.Username = roomNameObject.GetComponent<InputField>().text;

    }

    public void toggleDynamicCamera()
    {

        PlayerPrefs.SetInt("dynamicCamera",toggle1.isOn?1:0);

    }
  
}
