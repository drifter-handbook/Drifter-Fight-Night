﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// TODO: Rename to Menu Manager
// Handles the menu logic flow and sends important stuff back to the game controller to disseminate
public class ViewManager : MonoBehaviour
{
    [SerializeField]
#if UNITY_EDITOR
    [Help("All views must be registered by being children of this game object!", UnityEditor.MessageType.Warning)]
#endif
    Transform startingMenu;

    public GameObject hostIP;
    bool foundIP = false;
    public GameObject savedIPObject;

    string currentView;
    Dictionary<string, Transform> views = new Dictionary<string, Transform>();

    public Toggle toggle1;
    public Toggle toggle2;
    public Toggle toggle3;

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

        if (hostIP.activeSelf && PlayerPrefs.GetInt("HideIP") > 0)
        {
            hostIP.GetComponent<InputField>().contentType = InputField.ContentType.Password;
        }
        else if (hostIP.activeSelf && PlayerPrefs.GetInt("HideIP") == 0)
        {
            hostIP.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
        }

    }

    public void UpdateToggles()
    {
        toggle1.onValueChanged.RemoveAllListeners();
        toggle2.onValueChanged.RemoveAllListeners();
        toggle3.onValueChanged.RemoveAllListeners();

        toggle1.isOn = PlayerPrefs.GetInt("HideIP") > 0;
        toggle2.isOn = PlayerPrefs.GetInt("HidePing") > 0;
        toggle3.isOn = PlayerPrefs.GetInt("HideTextInput") > 0;
        //   ^ toggles the code too. Why? idk, unity makes interesting decisions sometimes


        toggle1.onValueChanged.AddListener(delegate {
            toggleIP();
        });

        toggle2.onValueChanged.AddListener(delegate {
            togglePing();
        });

        toggle3.onValueChanged.AddListener(delegate {
            toggleTextInput();
        });
    }

    private void Update()
    {
        
        if (!foundIP && GameController.Instance.GetComponent<IPWebRequest>().complete)
        {
            string holepunch_ip = Resources.Load<TextAsset>("Config/server_ip").text.Trim();
            hostIP.GetComponent<InputField>().text = $"{GameController.Instance.GetComponent<IPWebRequest>().result.ToString()}:{UDPHolePuncher.GetLocalIP(holepunch_ip, 6970).GetAddressBytes()[3]}";
            foundIP = true;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentView == "Main Menu")
            {
                Application.Quit();
            }
            else
            {
                ShowView("Main Menu");
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


        if(hostIP.activeSelf && PlayerPrefs.GetInt("HideIP") > 0)
        {
            hostIP.GetComponent<InputField>().contentType = InputField.ContentType.Password;
        } else if (hostIP.activeSelf && PlayerPrefs.GetInt("HideIP") == 0){
            hostIP.GetComponent<InputField>().contentType = InputField.ContentType.Standard;
        }

        if (name == "Join Menu")
        {
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
        }
    }

    public void SetIP(string ip)
    {
        if (currentView == "Join Menu")
        {
            string[] ip_id = ip.Split(':');
            GameController.Instance.hostIP = ip_id[0];
            GameController.Instance.HostID = int.Parse(ip_id[1]);
            PlayerPrefs.SetString("savedIP", ip);
            PlayerPrefs.Save();
        }
    }

    public void GoToCharacterSelect(bool isHost)
    {
        SetIP(savedIPObject.GetComponent<InputField>().text);
        GameController.Instance.IsHost = isHost;
        if (isHost)
        {
            //if we're hosting, lets grab our own IP
        }
        GameController.Instance.ChooseYerDrifter();

    }

    // May be moved to game controller?
    public void Exit()
    {
        Application.Quit();
    }


    public void toggleIP()
    {
        if(PlayerPrefs.GetInt("HideIP") == 0) { PlayerPrefs.SetInt("HideIP", 1); }
        else{ PlayerPrefs.SetInt("HideIP", 0); }
        PlayerPrefs.Save();
    }

    public void togglePing()
    {
        if (PlayerPrefs.GetInt("HidePing") == 0) { PlayerPrefs.SetInt("HidePing", 1); }
        else { PlayerPrefs.SetInt("HidePing", 0); }
        PlayerPrefs.Save();
    }

    public void toggleTextInput()
    {
        if (PlayerPrefs.GetInt("HideTextInput") == 0) { PlayerPrefs.SetInt("HideTextInput", 1); }
        else { PlayerPrefs.SetInt("HideTextInput", 0); }
        PlayerPrefs.Save();
    }
  
}
