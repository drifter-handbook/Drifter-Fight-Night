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
                    input.SwitchCurrentActionMap("UI");
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
        bool isPaused = GameController.Instance.IsPaused;
        if (isPaused)
        {
            foreach (PlayerInput input in GameController.Instance.controls.Values)
            {
                input.SwitchCurrentActionMap("Controls");
            }
        }

        escapeMenu.SetActive(!isPaused);
        GameController.Instance.IsPaused = !isPaused;
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
