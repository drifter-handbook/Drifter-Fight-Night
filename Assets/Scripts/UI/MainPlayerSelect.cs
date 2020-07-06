using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainPlayerSelect : MonoBehaviour
{
    //Column then Row
    public List<CharacterSelectState> CharacterSelectState = new List<CharacterSelectState>() { new CharacterSelectState() };
    public bool readyToGo => CharacterSelectState.Count > 1 && CharacterSelectState.TrueForAll(x => x.locked);

    public Transform[] selectionObjects = new Transform[4];
    public GameObject[] lockedObjects = new GameObject[4];
    public GameObject GameStartButton;

    public GameObject PlayerProfiles;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        picturePosition();
        lockedPictures();
        ShowPlayerPictures();
        GameStartButton.SetActive(readyToGo);
        if (Input.GetKeyDown(KeyCode.Return) && readyToGo == true)
        {
            //Switch to the main game scene
            GetComponent<GameSyncManager>().StartGame();
        }
    }

    void picturePosition()
    {
        for (int i = 0; i < CharacterSelectState.Count; i++)
        {
            if (CharacterSelectState[i].y == 0 && CharacterSelectState[i].x == 0)
            {
                selectionObjects[i].position = new Vector2(-5.86f, 4.52f);
            }
            else if (CharacterSelectState[i].y == 0 && CharacterSelectState[i].x == 1)
            {
                selectionObjects[i].position = new Vector2(-1.78f, 4.52f);
            }
            else if (CharacterSelectState[i].y == 0 && CharacterSelectState[i].x == 2)
            {
                selectionObjects[i].position = new Vector2(3.25f, 4.52f);
            }
            else if (CharacterSelectState[i].y == 0 && CharacterSelectState[i].x == 3)
            {
                selectionObjects[i].position = new Vector2(7.18f, 4.52f);
            }
            else if (CharacterSelectState[i].y == 1 && CharacterSelectState[i].x == 0)
            {
                selectionObjects[i].position = new Vector2(-5.86f, 2.6f);
            }
            else if (CharacterSelectState[i].y == 1 && CharacterSelectState[i].x == 1)
            {
                selectionObjects[i].position = new Vector2(-1.78f, 2.6f);
            }
            else if (CharacterSelectState[i].y == 1 && CharacterSelectState[i].x == 2)
            {
                selectionObjects[i].position = new Vector2(3.25f, 2.6f);
            }
            else if (CharacterSelectState[i].y == 1 && CharacterSelectState[i].x == 3)
            {
                selectionObjects[i].position = new Vector2(7.18f, 2.6f);
            }
        }
    }

    void lockedPictures()
    {
        for(int i = 0; i < CharacterSelectState.Count; i++)
        {
            if (CharacterSelectState[i].locked == true)
            {
                lockedObjects[i].SetActive(true);
            }
            else
            {
                lockedObjects[i].SetActive(false);
            }
        }
    }

    void ShowPlayerPictures()
    {
        List<GameObject> pics = new List<GameObject>();
        foreach (Transform child in PlayerProfiles.transform)
        {
            pics.Add(child.gameObject);
        }
        for (int i = 0; i < pics.Count; i++)
        {
            pics[i].SetActive(i < CharacterSelectState.Count);
        }
    }

    void RecievePacket()
    {
        //Update Position of each player selection in selectionArray

        //Recieve from player Row/Column combination
        //Update Row/Column combination in 

        //selectionArray[playerNumb, 0] = playerRow
        //selectionArray[playerNumb, 1] = playerCol
    }
}
