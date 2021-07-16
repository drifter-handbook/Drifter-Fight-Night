using System.Collections;
using System.Collections.Generic;
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
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown("joystick button 7"))
        {
            ToggleMenu();
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
