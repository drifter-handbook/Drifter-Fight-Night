using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ViewManager : MonoBehaviour
{
    [SerializeField]
    #if UNITY_EDITOR
    [Help("All views must be registered by being children of this game object!", UnityEditor.MessageType.Warning)]
    #endif
    Transform startingMenu;

    string currentView;
    Dictionary<string, Transform> views = new Dictionary<string, Transform>();

    void Awake()
    {
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

    // May be moved to game controller?
    public void Exit() {
        Application.Quit();
    }

}
