using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;

public class EndgameImageHandler : MonoBehaviour
{
    
    public Sprite[] sprites;
    public GameObject[] winnerSetups;

    //public GameObject rightPanel;
    //public GameObject sillyImagePrefab;
    public GameObject playAgainButton;
    PlayerInput[] playerInputs;

    bool mouse = true;

    void Start()
    {
        playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput input in GameController.Instance.controls.Values)
        {
            input.SwitchCurrentActionMap("UI");
        }

        //isHost = GameController.Instance.IsHost;
        //Resources.UnloadUnusedAssets();

        // if(GameController.Instance.IsHost)
        // {

        // 	sync = GetComponent<NetworkSync>();

        // 	sync.SendNetworkMessage(new CharacterSelectSyncData() {charSelState = CharacterMenu.charSelStates});
        //     playAgainButton.SetActive(true);

        // }
        // else playAgainButton.SetActive(false);


        if( GameController.Instance.winnerOrder.Length ==0) UnityEngine.Debug.Log("NO CONTEST");
        for(int i = 0; i< GameController.Instance.winnerOrder.Length; i++)
        {

            UnityEngine.Debug.Log("Player " + (i + 1) + " came in " + GameController.Instance.winnerOrder[i] + "th place!");

            //Todo Cleanup
            if(GameController.Instance.winnerOrder[i] == 1)
                foreach (CharacterSelectState state in CharacterMenu.charSelStates)
                    if (state.PeerID == (i - 1))
                        setWinnerPic(state.PlayerType,CharacterMenu.ColorFromEnum[(PlayerColor)(state.PeerID+1)]);
        }
    }

    public void setWinnerPic(DrifterType type,Color color)
    {

        foreach(GameObject setup in winnerSetups)
        {
            if (setup.name.Contains(type.ToString()))
            {
                setup.SetActive(true);
                setup.transform.GetChild(0).GetComponent<Text>().color = color; //sets player Color on shadow text
            } else
            {
                setup.SetActive(false);
            }
        }
    }

    void FixedUpdate()
    {
        playerInputs = FindObjectsOfType<PlayerInput>();
        foreach(PlayerInput playerInput in playerInputs)
        {
            bool gamepadButtonPressed = false;
            if (Gamepad.current != null)
            {
                for (int i = 0; i < Gamepad.current.allControls.Count; i++)
                {
                    var c = Gamepad.current.allControls[i];
                    if (c is ButtonControl)
                    {
                        if (((ButtonControl)c).wasPressedThisFrame)
                        {
                            gamepadButtonPressed = true;
                        }
                    }
                }
            }

            if (playerInput != null && playerInput.currentActionMap.FindAction("Click").ReadValue<float>() > 0 || playerInput.currentActionMap.FindAction("RightClick").ReadValue<float>() > 0 || playerInput.currentActionMap.FindAction("MiddleClick").ReadValue<float>() > 0 && !mouse)
            {
                mouse = true;
                //Cursor.visible = true;
                EventSystem.current.SetSelectedGameObject(null);
                return;

            }
            else if ((Keyboard.current.anyKey.isPressed || gamepadButtonPressed) && mouse && (!(playerInput.currentActionMap.FindAction("Click").ReadValue<float>() > 0) || !(playerInput.currentActionMap.FindAction("RightClick").ReadValue<float>() > 0) || !(playerInput.currentActionMap.FindAction("MiddleClick").ReadValue<float>() > 0)))
            {
                EventSystem.current.SetSelectedGameObject(GameObject.Find("MainMenu"));
                mouse = false;
            }
        }

        //TODO: commenting out, figure out wtf to do with this.
        //if(Keyboard.current.escapeKey.wasPressedThisFrame) backToMain();  
    }

    public void backToMain()
    {
        GameController.Instance.GoToMainMenu();
    }

    public void playAgain()
    {
        GameController.Instance.BeginMatch();
        // if(.IsHost)GameController.Instance.host.SetScene("Character_Select_Rework");
        // else 
        // {
        //     GameController.Instance.CleanupNetwork();
        //     GameController.Instance.StartNetworkClient();
        // }
    }

    public void returnToCharacterSelect()
    {
        GameController.Instance.GoToCharacterSelect();
        // else 
        // {
        //     GameController.Instance.CleanupNetwork();
        //     GameController.Instance.StartNetworkClient();
        // }
    }

    public void Exit()
    {
        GameController.Instance.GoToMainMenu();
    }
}
