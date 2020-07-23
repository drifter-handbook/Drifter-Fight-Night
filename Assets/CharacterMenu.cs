using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour
{
    // Make a note of these so we don't lose them to the source control gods
    // (You will have to modify the alpha value in inspector because -\_(o.o)_/-)
    // public List<Color> playerColorPool = new List<Color> {
    //     new Color(255,   0,   0, 1f),
    //     new Color(255, 204,   0, 1f),
    //     new Color( 32, 221,  30, 1f),
    //     new Color( 19, 179, 231, 1f),
    //     new Color(185,  16, 255, 1f),
    //     new Color(255,  16, 144, 1f),
    //     new Color(255, 140,  42, 1f),
    //     new Color(  0, 255, 221, 1f),
    // };

    public List<Color> playerColorPool = new List<Color>();

    public List<GameObject> figurines;
    public GameObject leftPanel;
    public GameObject rightPanel;

    public GameObject characterCardPrefab;

    private int connectedPlayers = 1; //or maybe 0, but I hope we can assume 1
    private int maxPlayers = 8; //the # of players allowed in the lobby

    private const int PANEL_MAX_PLAYERS = 4; //determines how many player cards we can fit on a panel

    //try to add player, return false if over max
    public bool AddPlayerCard()
    {
        if(connectedPlayers > maxPlayers)
        {
            return false;
        }

        // TODO: Show specific character based on selection
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

    public void RemovePlayerCard(int index) //start at 0 plz
    {
        if(connectedPlayers <= 0)
        {
            //then you're using the debug menu
            //and you should not
            return;
        }

        //FIXME: Will delete from left panel first when there are 8 characters - probs cause we remove at 0 and don't shift stuff from r to l
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
        AddPlayerCard();
    }

}
