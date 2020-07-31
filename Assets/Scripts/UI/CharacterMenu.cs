using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour
{
    // Make a note of these so we don't lose them to the source control gods
    // (You will have to modify the alpha value in inspector because -\_(o.o)_/-)
    // public List<Color> colorList = new List<Color> {
    //     new Color(255,   0,   0, 1f),
    //     new Color(255, 204,   0, 1f),
    //     new Color( 32, 221,  30, 1f),
    //     new Color( 19, 179, 231, 1f),
    //     new Color(185,  16, 255, 1f),
    //     new Color(255,  16, 144, 1f),
    //     new Color(255, 140,  42, 1f),
    //     new Color(  0, 255, 221, 1f),
    // };

    public List<Color> colorList = new List<Color>();

    public GameObject leftPanel;
    public GameObject rightPanel;

    private int connectedPlayers = 1; //or maybe 0, but I hope we can assume 1
    private int maxPlayers = 8; //the # of players allowed in the lobby

    //determines how many player cards we can fit on a panel
    private const int PANEL_MAX_PLAYERS = 4; 

    //try to add player, return false if over max
    public bool AddPlayerCard()
    {
        if(connectedPlayers > maxPlayers) return false;

        // TODO: Show specific character based on selection
        Transform card = CharacterCard.CreatePlayerCard(colorList[0]);
        colorList.RemoveAt(0);

        Transform parent = (connectedPlayers <= PANEL_MAX_PLAYERS) ? 
            leftPanel.transform : rightPanel.transform;

        card.SetParent(parent);

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

        // FIXME: Will delete from left panel first when there are 8 characters - 
        // probs cause we remove at 0 and don't shift stuff from r to l
        Transform parent = (index <= PANEL_MAX_PLAYERS && 
            leftPanel.transform.childCount >= index) ? leftPanel.transform 
            : rightPanel.transform;

        colorList.Add(parent.GetChild(index).transform.GetChild(0).gameObject.GetComponent<Image>().color); //this line still hurts my bones
        //we should probably just assign color to a "player" data structure but I'm lazy rn
        Destroy(parent.GetChild(index % PANEL_MAX_PLAYERS).gameObject);
        connectedPlayers--;
    }

    public void debugAddPlayer()
    {
        //buttons apparently need void returns, ignore me
        AddPlayerCard();
    }

}
