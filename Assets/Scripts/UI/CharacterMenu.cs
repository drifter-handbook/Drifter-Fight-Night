﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour
{
    // Make a note of these so we don't lose them to the source control gods
    // (You will have to modify the alpha value in inspector because -\_(o.o)_/-)
    // public List<Color> colorList = new List<Color> {
    //     new Color(255,   0,   0),
    //     new Color(255, 204,   0),
    //     new Color( 32, 221,  30),
    //     new Color( 19, 179, 231),
    //     new Color(185,  16, 255),
    //     new Color(255,  16, 144),
    //     new Color(255, 140,  42),
    //     new Color(  0, 255, 221),
    // };

    List<PlayerColor> colorList = new List<PlayerColor>{
        PlayerColor.RED,
        PlayerColor.GOLD,
        PlayerColor.GREEN,
        PlayerColor.BLUE,
        PlayerColor.PURPLE,
        PlayerColor.MAGENTA,
        PlayerColor.ORANGE,
        PlayerColor.CYAN
    };

    private int clientPlayerID = 0; //the person looking at the screen

    public GameObject leftPanel;
    public GameObject rightPanel;

    private int connectedPlayers = 1; //or maybe 0, but I hope we can assume 1
    private int maxPlayers = 8; //the # of players allowed in the lobby

    private List<PlayerData> playerDataList = new List<PlayerData>();

    public GameObject[] figurines;
    public Sprite[] images;
    private GameObject clientArrow;
    private GameObject clientCard;
    //determines how many player cards we can fit on a panel
    private const int PANEL_MAX_PLAYERS = 4;

    public GameObject arrowPrefab;

    public void debugClientIncrementPlayerID()
    {
        clientPlayerID++;
    }

    public void changeClientPlayerID(int num)
    {
        clientPlayerID = num;
    }

    public PlayerData getPlayerData(int id)
    {
        foreach (PlayerData playerData in playerDataList)
        {
            if (playerData.PlayerID == clientPlayerID)
            {
                return playerData;
            }
        }
        return new PlayerData();//no please don't hit this
    }

    //try to add player, return false if over max
    public bool AddPlayerCard(PlayerData player)
    {
        if (connectedPlayers > maxPlayers) return false;

        player.PlayerColor = colorList[0];
        player.PlayerIndex = connectedPlayers - 1;
        playerDataList.Add(player);

        // TODO: Show specific character based on selection

        player.PlayerColor = colorList[0];
        GameObject charCard = CharacterCard.CreatePlayerCard(player.getColorFromEnum());
        player.characterCard = charCard;
        Transform card = charCard.transform;
        colorList.RemoveAt(0);

        Transform parent = (connectedPlayers <= PANEL_MAX_PLAYERS) ?
            leftPanel.transform : rightPanel.transform;

        card.SetParent(parent, false);

        connectedPlayers++;

        return true;
    }

    public void RemovePlayerCard(int index) //start at 0 plz
    {
        if (connectedPlayers <= 0)
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

        colorList.Add(getPlayerData(index).PlayerColor);

        Destroy(parent.GetChild(index % PANEL_MAX_PLAYERS).gameObject);
        connectedPlayers--;
    }

    public void selectDrifter(int drifterIndex)
    {
        PlayerData myData = getPlayerData(clientPlayerID);
        if (clientArrow != null)
        {
            Destroy(clientArrow);
        }

        //spawn arrow on the correct object
        clientArrow = Instantiate(arrowPrefab.gameObject, new Vector3(0, 0, 0), new Quaternion());
        clientArrow.transform.SetParent(figurines[drifterIndex].transform);
        clientArrow.transform.localPosition = new Vector3(0, 0, 0);
        clientArrow.transform.localScale = new Vector3(1, 1, 1);
        clientArrow.GetComponent<Image>().color = myData.getColorFromEnum();
        if (myData.characterCard != null)
        {
            CharacterCard.SetCharacter(myData.characterCard.transform, images[drifterIndex], fetchCharacterName(drifterIndex));
        }

    }

    public string fetchCharacterName(int charID)
    {
        switch (charID)
        {
            case 0: return "Swordfrog";
            case 1: return "Nero";
            case 2: return "Lady Parhelion";
            case 3: return "Rykke";
            case 4: return "Space Jame";
            default: return "???";
        }
    }
}