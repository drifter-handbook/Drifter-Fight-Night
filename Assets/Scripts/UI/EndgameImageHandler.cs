using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.InputSystem.Controls;

public class EndgameImageHandler : UIMenuManager {
    
    public Sprite[] sprites;
    public GameObject[] winnerSetups;
    public GameObject playAgainButton;

    void Start() {
        PlayerInput[] playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput input in GameController.Instance.controls.Values) {
            input.SwitchCurrentActionMap("UI");
        }

        EventSystem.current.SetSelectedGameObject(GameObject.Find("MainMenu"));

        //NOTE: used to have some isHost and sendNetworkMessage logic here (see mouse removal revision for commented out code if necessary)

        if ( GameController.Instance.winnerOrder.Length == 0) UnityEngine.Debug.Log("NO CONTEST");
        for(int i = 0; i< GameController.Instance.winnerOrder.Length; i++) {

            UnityEngine.Debug.Log("Player " + (i + 1) + " came in " + GameController.Instance.winnerOrder[i] + "th place!");

            //Todo Cleanup
            if(GameController.Instance.winnerOrder[i] == 1)
                foreach (CharacterSelectState state in CharacterMenu.charSelStates.Values)
                    if (state.PeerID == (i - 1))
                        setWinnerPic(state.PlayerType,CharacterMenu.ColorFromEnum[(PlayerColor)(state.PeerID+1)]);
        }
    }

    public void FixedUpdate() {
        PlayerInput[] playerInputs = FindObjectsOfType<PlayerInput>();
        foreach (PlayerInput playerInput in playerInputs) {
            UpdateActivePlayerInputs(playerInput);
        }
    }

    public void setWinnerPic(DrifterType type,Color color) {
        foreach(GameObject setup in winnerSetups) {
            if (setup.name.Contains(type.ToString())) {
                setup.SetActive(true);
                setup.transform.GetChild(0).GetComponent<Text>().color = color; //sets player Color on shadow text
            } 
            else {
                setup.SetActive(false);
            }
        }
    }
    public void backToMain() {
        GameController.Instance.GoToMainMenu();
    }

    public void playAgain() {
        GameController.Instance.BeginMatch();
        //NOTE: used to have some network cleanup and start client calls here (see mouse removal revision for commented out code if necessary)
    }

    public void returnToCharacterSelect() {
        GameController.Instance.GoToCharacterSelect();
        //NOTE: used to have some network cleanup and start client calls here (see mouse removal revision for commented out code if necessary)
    }

    public override void Exit() {
        GameController.Instance.GoToMainMenu();
    }
}
