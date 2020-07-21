using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterMenu : MonoBehaviour
{

    public List<Color> playerColorPool = new List<Color>();

    public List<GameObject> figurines;
    public GameObject leftPanel;
    public GameObject rightPanel;


    public GameObject characterCardPrefab;



    private int connectedPlayers = 1; //or maybe 0, but I hope we can assume 1
    private int maxPlayers = 8; //the # of players allowed in the lobby


    private const int PANEL_MAX_PLAYERS = 4; //determines how many player cards we can fit on a panel
    
    void Start()
    {
        
    }


    //try to add player, return false if over max
    public bool addPlayer()
    {
        if(connectedPlayers > maxPlayers)
        {
            return false;
        }

        GameObject card = Instantiate(characterCardPrefab);
        if (connectedPlayers <= PANEL_MAX_PLAYERS)
        {
            card.transform.SetParent(leftPanel.transform);
            card.transform.localScale = new Vector3(1, 1, 1);
            card.transform.localPosition = new Vector3(card.transform.localPosition.x, card.transform.localPosition.y, 0);
          
        } else
        {
            card.transform.SetParent(rightPanel.transform);
            card.transform.localScale = new Vector3(1, 1, 1);
            card.transform.localPosition = new Vector3(card.transform.localPosition.x, card.transform.localPosition.y, 0);
        }

        if (playerColorPool.Count > 0)
        {
            card.transform.GetChild(0).GetComponent<Image>().color = playerColorPool[0];
            playerColorPool.Remove(playerColorPool[0]);
        }

        connectedPlayers++;

        return true;
    }

    public void removePlayer(int index) //start at 0 plz
    {
        if(connectedPlayers <= 0)
        {
            //then you're using the debug menu
            //and you should not
            return;
        }

        if (index <= PANEL_MAX_PLAYERS && leftPanel.transform.childCount >= index)
        {

            playerColorPool.Insert(0, leftPanel.transform.GetChild(index).transform.GetChild(0).gameObject.GetComponent<Image>().color); //this line hurts my bones
            //we should probably just assign color to a "player" data structure but I'm lazy rn
            Destroy(leftPanel.transform.GetChild(index).gameObject);
            connectedPlayers--;
        }
        else
        {
            playerColorPool.Insert(0,leftPanel.transform.GetChild(index).transform.GetChild(0).gameObject.GetComponent<Image>().color);
            Destroy(rightPanel.transform.GetChild(index-PANEL_MAX_PLAYERS).gameObject);
            connectedPlayers--;
        }
    }

    public void debugAddPlayer()
    {
        //buttons apparently need void returns, ignore me
        addPlayer();
    }

}
