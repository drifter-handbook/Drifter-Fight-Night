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

    public void ToggleMenu()
    {
        escapeMenu.SetActive(!(escapeMenu.activeSelf));
        EventSystem.current.SetSelectedGameObject(GameObject.Find("Continue"));
    }

    public void ReturnToTitle()
    {
        GameController.Instance.IsPaused = false;
        GameController.Instance.EndMatch(0);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
