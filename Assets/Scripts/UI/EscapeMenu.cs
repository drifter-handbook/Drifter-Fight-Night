using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.EventSystems;

public class EscapeMenu : MonoBehaviour
{
    public GameObject escapeMenu;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Add back joystick support
        //Input.GetKeyDown("joystick button 7")
        if (GameController.Instance.controls.FindActionMap("PlayerKeyboard").FindAction("Pause").triggered)
        {
            GameController.Instance.IsPaused = true;
            ToggleMenu();
        }
        if(!escapeMenu.activeSelf)
        {
            GameController.Instance.IsPaused = false;
        }
    }

    public void ToggleMenu()
    {
        escapeMenu.SetActive(!(escapeMenu.activeSelf));
        EventSystem.current.SetSelectedGameObject(GameObject.Find("Continue"));
    }

    public void ReturnToTitle()
    {
            GameController.Instance.EndMatch(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
