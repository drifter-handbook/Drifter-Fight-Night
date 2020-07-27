﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;

// Handles the menu logic flow and sends important stuff back to the game controller to disseminate
public class ViewManager : MonoBehaviour
{
    [SerializeField]
    #if UNITY_EDITOR
    [Help("All views must be registered by being children of this game object!", UnityEditor.MessageType.Warning)]
    #endif
    Transform startingMenu;
    
    GameController lucille = GameController.Instance;
    
    public string hostIP; // TODO: Don't hardcode an IP when we ship the game 🤔

    string currentView;
    Dictionary<string, Transform> views = new Dictionary<string, Transform>();

    void Awake()
    {
        views = new Dictionary<string, Transform>();
        if (views.Count <= 0) {
            foreach (var child in this.gameObject.GetComponentsInDirectChildren<Transform>())
            {
                child.gameObject.SetActive(false);

                if (views.ContainsKey(child.gameObject.name)) {
                    Debug.LogWarning("Views already contains key " + child.gameObject.name + "!");
                    continue;
                }

                views.Add(child.gameObject.name, child.transform);
            }
        }
        startingMenu.gameObject.SetActive(true);
        currentView = startingMenu.gameObject.name;
    }

    public Transform GetView(string name) {
        return views[name];
    }

    public void ShowView(string name){
        views[currentView].gameObject.SetActive(false);
        currentView = name;
        views[name].gameObject.SetActive(true);
    }

    public void SetIP(string ip) {
         lucille.hostIP = ip;
    }

    public void GoToCharacterSelect(bool isHost){
        lucille.isHost = isHost;
        lucille.ChooseYerDrifter();
    }

    // May be moved to game controller?
    public void Exit() {
        Application.Quit();
    }
}
