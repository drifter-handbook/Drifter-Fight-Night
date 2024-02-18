using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

public class EscapeMenu : MonoBehaviour
{
    public GameObject escapeMenu;
    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if(!GameController.Instance.IsPaused && GameController.Instance.canPause)
        {
            foreach(PlayerInput input in GameController.Instance.controls.Values)
            {
                if(input.currentActionMap.FindAction("Menu").ReadValue<float>()>0)
                {
                    InputSystemUIInputModule uiInputModule = GameObject.Find("EventSystem")?.GetComponent<InputSystemUIInputModule>();
                    uiInputModule.actionsAsset = input.actions;
                    ToggleMenu();
                    return;
                }

            }
        }        
    }

    public void ToggleMenu()
    {
        escapeMenu.SetActive(!(escapeMenu.activeSelf));
        GameController.Instance.IsPaused = !GameController.Instance.IsPaused;
        EventSystem.current.SetSelectedGameObject(GameObject.Find("Continue"));
    }

    public void ReturnToTitle()
    {
        ToggleMenu();
        GameController.Instance.IsPaused = false;
        GameController.Instance.EndMatch(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
