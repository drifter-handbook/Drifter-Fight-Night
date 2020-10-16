using System.Collections;
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


    private void Update()
    {
        
        if (!foundIP && GameController.Instance.GetComponent<IPWebRequest>().complete)
        {
            string holepunch_ip = Resources.Load<TextAsset>("Config/server_ip").text.Trim();
            hostIP.GetComponent<InputField>().text = $"{GameController.Instance.GetComponent<IPWebRequest>().result.ToString()}:{UDPHolePuncher.GetLocalIP(holepunch_ip, 6970).GetAddressBytes()[3]}";
            foundIP = true;
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

        if (savedIPObject == null && name == "Join Menu")
        {
            savedIPObject = GameObject.Find("InputField");
            if (PlayerPrefs.GetString("savedIP") != null)
                savedIPObject.GetComponent<InputField>().text = PlayerPrefs.GetString("savedIP");
        }
    }

    public void SetIP(string ip)
    {
        string[] ip_id = ip.Split(':');
        GameController.Instance.hostIP = ip_id[0];
        GameController.Instance.HostID = int.Parse(ip_id[1]);
        PlayerPrefs.SetString("savedIP", ip);
        PlayerPrefs.Save();
    }

    public void GoToCharacterSelect(bool isHost)
    {
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
}
