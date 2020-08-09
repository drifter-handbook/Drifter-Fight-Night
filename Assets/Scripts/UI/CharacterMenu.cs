using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColor
{
    RED, GOLD, GREEN, BLUE, PURPLE, MAGENTA, ORANGE, CYAN
}

// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour
{
    public static Dictionary<PlayerColor, Color> ColorFromEnum = new Dictionary<PlayerColor, Color>()
    {
        { PlayerColor.RED, new Color(1.0f, 0f, 0f) },
        { PlayerColor.GOLD, new Color(1.0f, 0.8f, 0f) },
        { PlayerColor.GREEN, new Color(0.124f, 0.866f, 0.118f) },
        { PlayerColor.BLUE, new Color(0.075f, 0.702f, 0.906f) },
        { PlayerColor.PURPLE, new Color(0.725f, 0.063f, 1.0f) },
        { PlayerColor.MAGENTA, new Color(1.0f, 0.063f, 0.565f) },
        { PlayerColor.ORANGE, new Color(1.0f, 0.55f, 0.165f) },
        { PlayerColor.CYAN, new Color(0.0f, 1.0f, 0.702f) }
    };

    public GameObject leftPanel;
    public GameObject rightPanel;

    [Serializable]
    public class PlayerSelectFigurine
    {
        public DrifterType drifter;
        public GameObject figurine;
        public Sprite image;
    }
    public List<PlayerSelectFigurine> drifters;
    Dictionary<DrifterType, PlayerSelectFigurine> figurines = new Dictionary<DrifterType, PlayerSelectFigurine>();

    private GameObject clientCard;

    //determines how many player cards we can fit on a panel
    private const int PANEL_MAX_PLAYERS = 4;

    public GameObject arrowPrefab;

    public class PlayerMenuEntry
    {
        public GameObject arrow;
        public GameObject characterCard;
    }
    List<PlayerMenuEntry> menuEntries = new List<PlayerMenuEntry>();

    void Awake()
    {
        foreach (PlayerSelectFigurine drifter in drifters)
        {
            figurines[drifter.drifter] = drifter;
        }
    }

    void FixedUpdate()
    {
        SyncToCharSelectState();
    }

    public void SyncToCharSelectState()
    {
        // add cards if needed
        for (int i = menuEntries.Count; i < GameController.Instance.CharacterSelectStates.Count; i++)
        {
            AddPlayerCard();
        }
        // remove cards if needed
        for (int i = GameController.Instance.CharacterSelectStates.Count; i < menuEntries.Count; i++)
        {
            RemovePlayerCard();
            i--;
        }
        int index = GameController.Instance.LocalPlayer.PlayerIndex;
        // set cards
        for (int i = 0; i < menuEntries.Count; i++)
        {
            DrifterType drifter = GameController.Instance.CharacterSelectStates[i].PlayerType;
            CharacterCard.SetCharacter(menuEntries[i].characterCard.transform, figurines[drifter].image, drifter.ToString().Replace("_", " "));
        }
        // set arrow color and visibility
        foreach (PlayerSelectFigurine drifter in drifters)
        {
            if (drifter.figurine != null)
            {
                drifter.figurine.GetComponent<Figurine>().SetColor(ColorFromEnum[(PlayerColor)index]);
                if (drifter.drifter == GameController.Instance.CharacterSelectStates[index].PlayerType)
                {
                    drifter.figurine.GetComponent<Figurine>().TurnArrowOn();
                }
                else
                {
                    drifter.figurine.GetComponent<Figurine>().TurnArrowOff();
                }
            }
        }
    }

    //try to add player, return false if over max
    public void AddPlayerCard()
    {
        if (menuEntries.Count >= GameController.MAX_PLAYERS)
        {
            return;
        }
        PlayerMenuEntry entry = new PlayerMenuEntry();
        int index = menuEntries.Count;
        menuEntries.Add(entry);

        GameObject charCard = CharacterCard.CreatePlayerCard(ColorFromEnum[(PlayerColor)index]);
        entry.characterCard = charCard;

        Transform card = charCard.transform;

        Transform parent = (index < PANEL_MAX_PLAYERS) ?
            leftPanel.transform : rightPanel.transform;

        card.SetParent(parent, false);
    }

    public void RemovePlayerCard()
    {
        int index = menuEntries.Count - 1;
        Transform parent = index < PANEL_MAX_PLAYERS ? leftPanel.transform : rightPanel.transform;
        Destroy(menuEntries[index].characterCard);
        menuEntries.RemoveAt(index);
    }

    public void SelectDrifter(string drifterString)
    {
        DrifterType drifter = (DrifterType)Enum.Parse(typeof(DrifterType), drifterString.Replace(" ", "_"));
        int index = GameController.Instance.LocalPlayer.PlayerIndex;
        if (index < 0)
        {
            return;
        }
        PlayerMenuEntry myCard = menuEntries[GameController.Instance.LocalPlayer.PlayerIndex];
        if (myCard.characterCard != null)
        {
            CharacterCard.SetCharacter(myCard.characterCard.transform, figurines[drifter].image, drifter.ToString().Replace("_", " "));
        }
        GameController.Instance.CharacterSelectStates[index].PlayerType = drifter;
    }
}
