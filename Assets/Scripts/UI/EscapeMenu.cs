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
        // //TODO: Add back joystick support
        // //Input.GetKeyDown("joystick button 7")
        // if (GameController.Instance.controls[0].FindActionMap("PlayerKeyboard").FindAction("Pause").triggered)
        // {
        //     
        // }


       

        foreach(KeyValuePair<int, InputActionAsset> kvp in GameController.Instance.controls)
        {
            if(kvp.Value.FindActionMap("PlayerKeyboard").FindAction("Menu").triggered)
            {
                GameController.Instance.IsPaused = true;
                ToggleMenu();
                break;
            }
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
