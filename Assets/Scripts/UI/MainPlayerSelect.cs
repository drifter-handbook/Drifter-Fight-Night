using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainPlayerSelect : MonoBehaviour
{
    int numberOfPlayers = 1;
    //Column then Row
    public int[,] selectionArray = new int[,]{{ 1,1 }, { 1, 1 }, { 1, 1 }, { 1, 1 }};
    public bool[] locked = new bool[4];
    public bool readyToGo = false;
    public Transform[] selectionObjects = new Transform[4];
    public GameObject[] lockedObjects = new GameObject[4];
    public GameObject GameStartButton;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        picturePosition();
        checkIfReady();
        lockedPictures();
        if (Input.GetKeyDown(KeyCode.Return) && readyToGo == true)
            //Switch to the main game scene
            SceneManager.LoadScene("NetworkTestScene");
    }

    void picturePosition()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (selectionArray[i, 0] == 1 && selectionArray[i, 1] == 1)
            {
                selectionObjects[i].position = new Vector2(-5.86f, 4.52f);
            }
            else if (selectionArray[i, 0] == 1 && selectionArray[i, 1] == 2)
            {
                selectionObjects[i].position = new Vector2(-1.78f, 4.52f);
            }
            else if (selectionArray[i, 0] == 1 && selectionArray[i, 1] == 3)
            {
                selectionObjects[i].position = new Vector2(3.25f, 4.52f);
            }
            else if (selectionArray[i, 0] == 1 && selectionArray[i, 1] == 4)
            {
                selectionObjects[i].position = new Vector2(7.18f, 4.52f);
            }
            else if (selectionArray[i, 0] == 2 && selectionArray[i, 1] == 1)
            {
                selectionObjects[i].position = new Vector2(-5.86f, 2.6f);
            }
            else if (selectionArray[i, 0] == 2 && selectionArray[i, 1] == 2)
            {
                selectionObjects[i].position = new Vector2(-1.78f, 2.6f);
            }
            else if (selectionArray[i, 0] == 2 && selectionArray[i, 1] == 3)
            {
                selectionObjects[i].position = new Vector2(3.25f, 2.6f);
            }
            else if (selectionArray[i, 0] == 2 && selectionArray[i, 1] == 4)
            {
                selectionObjects[i].position = new Vector2(7.18f, 2.6f);
            }
        }
    }

    void lockedPictures()
    {
        for(int i = 0; i < numberOfPlayers; i++)
        {
            if (locked[i] == true)
            {
                lockedObjects[i].SetActive(true);
            }
            else
            {
                lockedObjects[i].SetActive(false);
            }
        }
    }

    void checkIfReady()
    {
        for (int i = 0; i < numberOfPlayers; i++)
        {
            if (locked[i] == false)
            {
                readyToGo = false;
                GameStartButton.SetActive(false);
                return;
            }
        }
        readyToGo = true;
        GameStartButton.SetActive(true);
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
