using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndgameImageHandler : MonoBehaviour
{
    
    public Sprite[] sprites;
    public GameObject[] winnerSetups;

    //public GameObject rightPanel;
    //public GameObject sillyImagePrefab;
    public GameObject playAgainButton;

    NetworkSync sync;

    bool mouse = false;

    void Start()
    {
        //isHost = GameController.Instance.IsHost;
        Resources.UnloadUnusedAssets();

        if(GameController.Instance.IsHost)
        {

        	sync = GetComponent<NetworkSync>();

        	sync.SendNetworkMessage(new CharacterSelectSyncData() {charSelState = CharacterMenu.charSelStates});
            playAgainButton.SetActive(true);

        }
        else playAgainButton.SetActive(false);


        if( GameController.Instance.winnerOrder.Length ==0) UnityEngine.Debug.Log("NO CONTEST");
        for(int i = 0; i< GameController.Instance.winnerOrder.Length; i++)
        {

            UnityEngine.Debug.Log("Player " +  GameController.Instance.winnerOrder[i] + " came in " + (i + 1) + "th place!");

            //Todo Cleanup
            if(i == 0)
            {
                foreach (CharacterSelectState state in CharacterMenu.charSelStates.Values)
                {
                    if (state.PeerID == (i - 1))
                    {
                        UnityEngine.Debug.Log(state.PlayerType);
                        setWinnerPic(state.PlayerType,CharacterMenu.ColorFromEnum[(PlayerColor)(state.PeerID+1)]);
                    }
                }

            }
        }
    }

    // public void playWinnerAudio(int winnerIndex)
    // {
    //     gameObject.GetComponent<MultiSound>().PlayAudio(winnerIndex);
    // }

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

    void Update()
    {

        if(Keyboard.current.escapeKey.wasPressedThisFrame) backToMain();
        
        if((Mouse.current.leftButton.isPressed || Mouse.current.rightButton.isPressed || Mouse.current.middleButton.isPressed) && !mouse)
        {
            mouse = true;
            //Cursor.visible = true;
            EventSystem.current.SetSelectedGameObject(null);

        }
        else if(Keyboard.current.anyKey.isPressed && mouse && (!Mouse.current.leftButton.isPressed || !Mouse.current.rightButton.isPressed || !Mouse.current.middleButton.isPressed)){
            EventSystem.current.SetSelectedGameObject(GameObject.Find("MainMenu"));
            mouse = false;
        }
     
    }

    public void backToMain()
    {

        GameController.Instance.CleanupNetwork();
        SceneManager.LoadSceneAsync("MenuScene");

    }

    public void playAgain()
    {
        if(GameController.Instance.IsHost)GameController.Instance.host.SetScene("Character_Select_Rework");
        else 
        {
            GameController.Instance.CleanupNetwork();
            GameController.Instance.StartNetworkClient();
        }
    }

    public void Exit()
    {
        if (GameController.Instance.GetComponent<NetworkClient>() != null)
        {
            SceneManager.LoadScene("MenuScene");
        }

        if (GameController.Instance.GetComponent<NetworkHost>() != null)
        {
            SceneManager.LoadScene("MenuScene");
        }
    }
}
