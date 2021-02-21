using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

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

    string currentView;
    Dictionary<string, Transform> views = new Dictionary<string, Transform>();
    bool mouse = true;

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

    public GameObject roomCodeBox;

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
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentView == "Main Menu")
            {
                Application.Quit();
            }
            else
            {
                mouse = false;
                ShowView("Main Menu");
            }
        }

        if((Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2)) && !mouse)
        {
            mouse = true;
            Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

        }
        else if(Input.anyKey && mouse && (!Input.GetMouseButton(0) || !Input.GetMouseButton(1) || !Input.GetMouseButton(2))){
            mouse = false;
            Cursor.visible = false;
            switch (currentView){
                case "Matchmaking Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Host"));
                    break;
                case "Join Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Input Field"));
                    break;
                case "Main Menu":
                    ShowView("Matchmaking Menu");
                    break;
                case "Settings Menu":
                    EventSystem.current.SetSelectedGameObject(GameObject.Find("Back"));
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


        if(roomCodeBox.activeSelf && PlayerPrefs.GetInt("HideRoomCode") > 0)
        {
            roomCodeBox.GetComponent<InputField>().contentType = InputField.ContentType.Password;
        } else if (roomCodeBox.activeSelf && PlayerPrefs.GetInt("HideRoomCode") == 0){
            roomCodeBox.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
        }

        if (name == "Matchmaking Menu" && !mouse)
        {
            EventSystem.current.SetSelectedGameObject(GameObject.Find("Host"));
        }
        if(name == "Main Menu" && !mouse)
            EventSystem.current.SetSelectedGameObject(GameObject.Find("Play"));

        if (name == "Join Menu")
        {
            if(!mouse)EventSystem.current.SetSelectedGameObject(GameObject.Find("Join Button"));
            if (PlayerPrefs.GetInt("HideTextInput") > 0)
            {
                savedIPObject.GetComponent<InputField>().contentType = InputField.ContentType.Password;
            } else
            {
                savedIPObject.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
            }

            if (PlayerPrefs.GetString("savedIP") != null)
            {
                savedIPObject.GetComponent<InputField>().text = PlayerPrefs.GetString("savedIP");
            }
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

    public void toggleDynamicCamera()
    {

        PlayerPrefs.SetInt("dynamicCamera",toggle1.isOn?1:0);

    }
  
}
