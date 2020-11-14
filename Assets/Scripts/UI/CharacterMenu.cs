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
    public GameObject movesetOverlay;

    public static Dictionary<PlayerColor, Color> ColorFromEnum = new Dictionary<PlayerColor, Color>()
    {
        { PlayerColor.RED, new Color(1.0f, 0f, 0f) },
        { PlayerColor.GOLD, new Color(0.8f, 0.6f, 0f) },
        { PlayerColor.GREEN, new Color(0.124f, 0.866f, 0.118f) },
        { PlayerColor.BLUE, new Color(0.075f, 0.702f, 0.906f) },
        { PlayerColor.PURPLE, new Color(0.725f, 0.063f, 1.0f) },
        { PlayerColor.MAGENTA, new Color(1.0f, 0.063f, 0.565f) },
        { PlayerColor.ORANGE, new Color(1.0f, 0.55f, 0.165f) },
        { PlayerColor.CYAN, new Color(0.0f, 1.0f, 0.702f) }
    };

    public GameObject leftPanel;
    public GameObject rightPanel;
    public GameObject ipPanel;

   
    [Serializable]
    public class PlayerSelectFigurine
    {
        public DrifterType drifter;
        public GameObject figurine;
        public Sprite image;
    }

    [Serializable]
    public class FightZone
    {
        public GameObject fightzone;
        public Sprite fightzonePreview;
        public string fightzoneName;
        public string sceneName;
    }

    public List<PlayerSelectFigurine> drifters;
    Dictionary<DrifterType, PlayerSelectFigurine> figurines = new Dictionary<DrifterType, PlayerSelectFigurine>();
    public  List<FightZone> fightzones = new List<FightZone>();

    private GameObject clientCard;

    private FightZone selectedFightzone;
    private int selectedFightzoneNum = 0;

    public Image fightZonePreview;
    public Text fightZoneLabel;

    //determines how many player cards we can fit on a panel
    private const int PANEL_MAX_PLAYERS = 4;

    public GameObject arrowPrefab;

    public GameObject forwardButton;
    public GameObject backButton;

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
            drifter.figurine.GetComponent<Animator>().SetBool("present", true);
        }

        forwardButton.GetComponent<Animator>().SetBool("present", true);
        UpdateFightzone();

        if (PlayerPrefs.GetInt("HideIP") > 0)
        {
            ipPanel.SetActive(false);
        }
        else
        {
            ipPanel.SetActive(true);
        }
    }

    void FixedUpdate()
    {
        SyncToCharSelectState();
        transform.Find("ReadyButton").gameObject.SetActive(GameController.Instance.IsHost);

        if (GameController.Instance.LocalPlayer.PlayerIndex < 0)
        {
            ReturnToTitle();
        }
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
        // set cards
        for (int i = 0; i < menuEntries.Count; i++)
        {
            DrifterType drifter = GameController.Instance.CharacterSelectStates[i].PlayerType;
            
            if(drifter != DrifterType.None){
                CharacterCard.SetCharacter(menuEntries[i].characterCard.transform, figurines[drifter].image, drifter.ToString().Replace("_", " "));
            }
            
        }
        // set arrow color and visibility
        int index = GameController.Instance.LocalPlayer.PlayerIndex;
        if (index < 0)
        {
            return;
        }
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

        charCard.GetComponent<Animator>().SetBool("present", true);

        Transform card = charCard.transform;

        Transform parent = (index < PANEL_MAX_PLAYERS) ?
            leftPanel.transform : rightPanel.transform;

        card.SetParent(parent, false);

        if (GameController.Instance.IsHost)
        {
            GameObject myCard = menuEntries[GameController.Instance.LocalPlayer.PlayerIndex].characterCard;
            if (myCard == charCard)
            {
                CharacterCard.EnableKickPlayers(card, false);
            }
            else
            {
                //TODO: Call function to add listener for kick button click.
                Button kickPlayer = CharacterCard.EnableKickPlayers(card, GameController.Instance.IsHost);
                int cardIndex = menuEntries.IndexOf(entry);
                kickPlayer.onClick.AddListener(() => GameController.Instance.GetComponent<NetworkHost>().ForceKick(cardIndex));
            }
        }
    }

    public void RemovePlayerCard()
    {
        int index = menuEntries.Count - 1;
        Transform parent = index < PANEL_MAX_PLAYERS ? leftPanel.transform : rightPanel.transform;
        Destroy(menuEntries[index].characterCard);
        menuEntries.RemoveAt(index);
    }

    public void SelectFightzone(string s)
    {
        selectedFightzoneNum = fightzones.FindIndex(x => x.sceneName == s);
        UpdateFightzone();
    }

    public void nextFightzone()
    {
        selectedFightzoneNum++;
        if(selectedFightzoneNum >= fightzones.Count)
        {
            selectedFightzoneNum = 0;
        }
        UpdateFightzone();
    }

    public void UpdateFightzone()
    {
        selectedFightzone = fightzones[selectedFightzoneNum];
        fightZonePreview.sprite = selectedFightzone.fightzonePreview;
        fightZoneLabel.text = selectedFightzone.fightzoneName;
        if (GetComponent<Animator>().GetBool("location"))
        {
            GameController.Instance.selectedStage = selectedFightzone.sceneName;
        }
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


    public void HeadToLocationSelect()
    {

        if (this.GetComponent<Animator>().GetBool("location"))
        {
            //So you're the host?
            //LET'S GO TO THE GAME!
            GameController.Instance.BeginMatch();
            return;
        }



        this.GetComponent<Animator>().SetBool("location", true);
        if (!GameController.Instance.IsHost)
        {
            forwardButton.GetComponent<Animator>().SetBool("present", false);
        }

        List<DrifterType> pickedTypes = new List<DrifterType>();

        foreach (Animator card in rightPanel.GetComponentsInChildren<Animator>())
        {
           card.GetComponent<Animator>().SetBool("present", false);
            pickedTypes.Add(getDrifterTypeFromString(card.transform.GetChild(1).GetComponent<Text>().text));
        }

        foreach (Animator card in leftPanel.GetComponentsInChildren<Animator>())
        {
            card.GetComponent<Animator>().SetBool("present", false);
            pickedTypes.Add(getDrifterTypeFromString(card.transform.GetChild(1).GetComponent<Text>().text));
        }

        foreach (PlayerSelectFigurine drifter in drifters)
        {
            if (!pickedTypes.Contains(drifter.drifter))
            {
                drifter.figurine.GetComponent<Animator>().SetBool("present", false);
            }
        }
        UpdateFightzone();
    }

    public void HeadToCharacterSelect()
    {

        if (!GameController.Instance.IsHost)
        {
            //non-hosts don't get to start the game, so bring me back!
            forwardButton.GetComponent<Animator>().SetBool("present", true);
        }


        this.GetComponent<Animator>().SetBool("location", false);

        foreach (Animator card in rightPanel.GetComponentsInChildren<Animator>())
        {
            card.GetComponent<Animator>().SetBool("present", true);
        }

        foreach (Animator card in leftPanel.GetComponentsInChildren<Animator>())
        {
            card.GetComponent<Animator>().SetBool("present", true);
        }

        foreach (PlayerSelectFigurine drifter in drifters)
        {
            if (!drifter.figurine.GetComponent<Animator>().GetBool("present"))
            {
                drifter.figurine.GetComponent<Animator>().SetBool("present", true);
            }
        }
    }

    public DrifterType getDrifterTypeFromString(string name)
    {
        foreach(DrifterType drifter in Enum.GetValues(typeof(DrifterType)))
        {
            if(drifter.ToString() == name.Replace(" ", "_"))
            {
                return drifter;
            }
        }
        return DrifterType.Bojo;
    }


    public void ShowMovesetForDrifter()
    {
        movesetOverlay.gameObject.SetActive(true);
        movesetOverlay.GetComponentInChildren<TutorialSwapper>().SelectDrifter(0);
    }

    public void ReturnToTitle()
    {
        //TODO: C
        if (GameController.Instance.GetComponent<NetworkClient>() != null)
        {
            GameController.Instance.GetComponent<NetworkClient>().ExitToMain();
        }

        if (GameController.Instance.GetComponent<NetworkHost>() != null)
        {
            GameController.Instance.GetComponent<NetworkHost>().ExitToMain();
        }
        GameController.Instance.Load("MenuScene");
    }

}
