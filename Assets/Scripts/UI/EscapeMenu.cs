﻿using System.Collections;
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

    void Update()
    {
        if(!GameController.Instance.IsPaused && GameController.Instance.canPause)
        {
            foreach(PlayerInput input in GameController.Instance.controls.Values)
            {
                if(input.currentActionMap.FindAction("Menu").ReadValue<float>()>0)
                {
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
        GameController.Instance.EndMatch();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
