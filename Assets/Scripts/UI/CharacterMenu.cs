using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColor
{
    RED, GOLD, GREEN, BLUE, PURPLE, MAGENTA, ORANGE, CYAN, GREY
}



// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour, INetworkMessageReceiver
{

    public static Dictionary<PlayerColor, Color> ColorFromEnum = new Dictionary<PlayerColor, Color>()
    {
        { PlayerColor.RED, new Color(1.0f, 0f, 0f) },
        { PlayerColor.GOLD, new Color(0.8f, 0.6f, 0f) },
        { PlayerColor.BLUE, new Color(0.075f, 0.702f, 0.906f) },
        { PlayerColor.GREEN, new Color(0.124f, 0.866f, 0.118f) },
        { PlayerColor.PURPLE, new Color(0.725f, 0.063f, 1.0f) },
        { PlayerColor.MAGENTA, new Color(1.0f, 0.063f, 0.565f) },
        { PlayerColor.ORANGE, new Color(1.0f, 0.55f, 0.165f) },
        { PlayerColor.CYAN, new Color(0.0f, 1.0f, 0.702f) },
        { PlayerColor.GREY, new Color(0.4f, 0.4f, 0.4f) }
    };


    // [Serializable]
    // public class PlayerSelectCursor
    // {
    //     public int x = 7;
    //     public int y = 1;
    //     public int peerID =-1;
    //     public GameObject cursorObject;
    // }

    public GameObject bottomPanel;
    //public GameObject rightPanel;
    public GameObject roomCode;

    //Character Matrix
    public GameObject[] topRow;
    public GameObject[] middleRow;
    public GameObject[] bottomRow;
    GameObject[][] characterRows = new GameObject[3][];

    public GameObject playerInputPrefab;

    NetworkSyncToHost syncFromClients;
   
    // [Serializable]
    // public class PlayerSelectFigurine
    // {
    //     public DrifterType drifter;
    //     public GameObject figurine;
    //     public Sprite image;
    // }

    [Serializable]
    public class FightZone
    {
        public string sceneName;
    }




    //public List<PlayerSelectFigurine> drifters;
    //Dictionary<DrifterType, PlayerSelectFigurine> figurines = new Dictionary<DrifterType, PlayerSelectFigurine>();
    public  List<FightZone> fightzones = new List<FightZone>();

    private GameObject clientCard;
    public GameObject stageMenu;

    private FightZone selectedFightzone;
    private int selectedFightzoneNum = 0;

    //determines how many player cards we can fit on a panel
    //private const int PANEL_MAX_PLAYERS = 2;

    //public GameObject arrowPrefab;

    //public GameObject forwardButton;
    //public GameObject backButton;

    //public GameObject selectedFigurine = null;

    public class PlayerMenuEntry
    {
        public GameObject arrow;
        public GameObject characterCard;
    }
    List<PlayerMenuEntry> menuEntries = new List<PlayerMenuEntry>();
    bool mouse = true;

    bool stageSelect = false;

    NetworkSync sync;

    public static List<CharacterSelectState> charSelStates;

    public static CharacterMenu Instance => GameObject.FindGameObjectWithTag("CharacterMenu")?.GetComponent<CharacterMenu>();
    //public static CharacterSelectSyncData CharSelData = new CharSelData;

    DrifterType currentDrifter = DrifterType.None;

    void Awake()
    {
        //UpdateFightzone();

        // if(GameController.Instance.IsOnline)
        // {
        //     if(GameController.Instance.IsHost ){
        //     GameObject.Find("RoomCodeValue").GetComponent<Text>().text = GameController.Instance.host.RoomKey;
        //     }

        //     if (PlayerPrefs.GetInt("HideRoomCode") > 0 || !GameController.Instance.IsHost)
        //     {
        //         roomCode.SetActive(false);
        //     }
        //     else
        //     {
        //         roomCode.SetActive(true);
        //     }
        // }
        // else
        //     roomCode.SetActive(false);

        
    }

    void Start()
    {

        characterRows[0] = topRow;
        characterRows[1] = middleRow;
        characterRows[2] = bottomRow;

        syncFromClients = GetComponent<NetworkSyncToHost>();

        charSelStates = new List<CharacterSelectState>();

        //AssignInputAssest();
    
        if(GameController.Instance.IsOnline)
        {
            sync = GetComponent<NetworkSync>();
            sync["charSelState"] = new CharacterSelectSyncData()
            {
                Type = typeof(CharacterSelectSyncData).Name
            };
            sync["location"] = false;

            charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        }
        GameController.Instance.host.Peers =  new List<int>();
        // add host



        foreach(InputActionAsset controller in GameController.Instance.availableControls)
        {
            GameObject PlayerInput = Instantiate(playerInputPrefab, transform.position, Quaternion.identity);
            PlayerInput.GetComponent<PlayerInput>().actions = controller;
        }

        
        if(GameController.Instance.IsTraining)
        {
            AddCharSelState(-1);
            AddCharSelState(0,DrifterType.Sandbag);
        }

        //Populate a card for each active controller
        else
        {
            for(int i = -1; i < GameController.Instance.controls.Length-1; i++)
            {
                AddCharSelState(i);
            }

        }
            
    }

    public Dictionary<int, int> GetPeerIDsToPlayerIDs()
    {
        Dictionary<int, int> peerIDsToPlayerIDs = new Dictionary<int, int>();
        foreach (CharacterSelectState state in charSelStates)
        {
            peerIDsToPlayerIDs[state.PeerID] = state.PlayerIndex;
        }
        return peerIDsToPlayerIDs;
    }

    public void AddCharSelState(int peerID)
    {
        AddCharSelState(peerID,DrifterType.None);
    }

    public void AddCharSelState(int peerID, DrifterType drifter)
    {

        GameObject cursor = GameController.Instance.host.CreateNetworkObject("CharacterCursor",characterRows[1][7].transform.position, transform.rotation);
        cursor.GetComponent<SpriteRenderer>().color = ColorFromEnum[(PlayerColor)(peerID+1)];

        charSelStates.Add(new CharacterSelectState()
        {
            PeerID = peerID,
            Cursor = cursor,
            PlayerType = drifter
        });

        //Fix this for multiple input devices
        if(peerID != -1)
            GameController.Instance.host.Peers.Add(peerID);

        SortCharSelState(charSelStates);
    }

    public void RemoveCharSelState(int peerID)
    {
        UnityEngine.Debug.Log("PEER REMOVED");
        for (int i = 0; i < charSelStates.Count; i++)
        {
            if (charSelStates[i].PeerID == peerID)
            {
                Destroy(charSelStates[i].Cursor);
                charSelStates.RemoveAt(i);
                i--;
            }
        }

        if(peerID != -1)
            GameController.Instance.host.Peers.Remove(peerID);

        SortCharSelState(charSelStates);
    }

    void SortCharSelState(List<CharacterSelectState> charSelStates)
    {
        // sort by peer ID
        charSelStates.Sort((x, y) => x.PeerID.CompareTo(y.PeerID));
        for (int i = 0; i < charSelStates.Count; i++)
        {
            charSelStates[i].PlayerIndex = i;
        }
    }

    void FixedUpdate()
    {
        SyncToCharSelectState();
    }



    void Update()
    {

        //Make real stage select
        if(everyoneReady())
        {
            SelectFightzone("Training");
            UpdateFightzone();
            GameController.Instance.BeginMatch();
        }

        //Create Character Select State when an inactive controller becomes active
        for(int i = 0; i < GameController.Instance.checkForNewControllers(); i++)
            AddCharSelState(GameController.Instance.host.Peers.Count);
        

        //Remove Character select state if a controller is disconnected
        int removeIndex = GameController.Instance.checkForRemoveControllers();
        if(removeIndex >= 0)
        {
            RemovePlayerCard(removeIndex);
            RemoveCharSelState(removeIndex-1);
        }

        //Update input on each active char select state
        PlayerInputData input = NetworkPlayers.GetInput(GameController.Instance.controls[0]);
        UpdateInput(charSelStates[0], input);
        foreach (int peerID in GameController.Instance.host.Peers)
        {
            //Link inputs to peer ids
            input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", peerID]);
            if (input != null)
                UpdateInput(charSelStates[peerID+1], input);
            else
                UpdateInput(charSelStates[peerID+1], NetworkPlayers.GetInput(GameController.Instance.controls[peerID+1]));
        }
    }

    //Circular Array Helper
    private int wrapIndex(int curr, int max)
    {
        if(curr > max) return 0;
        else if(curr < 0) return max;
        else return curr;
    }


    //Updates input commands for a given cursor object
    public void UpdateInput(CharacterSelectState p_cursor,PlayerInputData input)
    {
        if(p_cursor.prevInput == null)
        {
           p_cursor.prevInput = input;
           return; 
        }
        
        if(p_cursor.prevInput.MoveX ==0 && input.MoveX != 0)
        {
            p_cursor.x = wrapIndex(p_cursor.x + (int)input.MoveX ,9);
        
            while(characterRows[p_cursor.y][p_cursor.x] == null)
                p_cursor.x = wrapIndex(p_cursor.x + (int)input.MoveX ,9);
        }
        
        if(p_cursor.prevInput.MoveY ==0 && input.MoveY != 0)
        {
            p_cursor.y = wrapIndex(p_cursor.y + (int)input.MoveY ,2);

            while(characterRows[p_cursor.y][p_cursor.x] == null)
                p_cursor.y = wrapIndex(p_cursor.y + (int)input.MoveY ,2);
        }
        
        p_cursor.Cursor.transform.position = characterRows[p_cursor.y][p_cursor.x].transform.position;
        
        //Select or deselelect on light press
        if(input.Light && !p_cursor.prevInput.Light)
        {
            DrifterType selected = characterRows[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().drifterType;
            p_cursor.PlayerType = (p_cursor.PlayerType == DrifterType.None || p_cursor.PlayerType != selected)?selected:DrifterType.None;
        }
        
        //Deselect on special press
        else if(input.Special && !p_cursor.prevInput.Special && p_cursor.PlayerType != DrifterType.None)
            p_cursor.PlayerType = DrifterType.None;


        p_cursor.prevInput = input;

    }



    public void SyncToCharSelectState()
    {
        // add cards if needed
        for (int i = menuEntries.Count; i < charSelStates.Count; i++)
            AddPlayerCard();

        // remove cards if needed
        for (int i = charSelStates.Count; i < menuEntries.Count; i++)
        {
            RemovePlayerCard();
            i--;
        }
        // set cards
        for (int i = 0; i < menuEntries.Count; i++)
        {
            DrifterType drifter = charSelStates[i].PlayerType;
        }
            
        
        // set stage
        if (!GameController.Instance.IsHost)
        {
            SelectFightzone(NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).stage);
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

        // GameObject charCard = CharacterCard.CreatePlayerCard(ColorFromEnum[(PlayerColor)index]);
        // entry.characterCard = charCard;

        // charCard.GetComponent<Animator>().SetBool("present", true);

        // Transform card = charCard.transform;

        // Transform parent = bottomPanel.transform;//(index < PANEL_MAX_PLAYERS) ? leftPanel.transform : rightPanel.transform;

        // card.SetParent(parent, false);

        // if (GameController.Instance.IsHost)
        // {
        //     // TODO: search for player index with peer ID -1
        //     GameObject myCard = menuEntries[0].characterCard;
        //     if (myCard == charCard && GameController.Instance.IsHost)
        //     {
        //         CharacterCard.EnableKickPlayers(card, false);
        //     }
        //     else
        //     {
        //         //TODO: Call function to add listener for kick button click.
        //         Button kickPlayer = CharacterCard.EnableKickPlayers(card, GameController.Instance.IsHost);
        //         int cardIndex = menuEntries.IndexOf(entry);
        //         // kickPlayer.onClick.AddListener(() => GameController.Instance.GetComponent<NetworkHost>().ForceKick(cardIndex));
        //     }
        // }
    }

    public void RemovePlayerCard()
    {
        int index = menuEntries.Count - 1;
        Transform parent = bottomPanel.transform;//index < PANEL_MAX_PLAYERS ? leftPanel.transform : rightPanel.transform;
        Destroy(menuEntries[index].characterCard);
        menuEntries.RemoveAt(index);
    }


    public void RemovePlayerCard(int index)
    {
        Transform parent = bottomPanel.transform;//index < PANEL_MAX_PLAYERS ? leftPanel.transform : rightPanel.transform;
        Destroy(menuEntries[index].characterCard);
        menuEntries.RemoveAt(index);
    }

    public void SelectFightzone(string s)
    {
        selectedFightzoneNum = fightzones.FindIndex(x => x.sceneName == s);
        UpdateFightzone();

        //CharSelData.charSelState = charSelStates;
        GameController.Instance.selectedStage = selectedFightzone.sceneName;
        //Cursor.visible = false;
        GameController.Instance.BeginMatch();
    }

    public void UpdateFightzone()
    {
        if (selectedFightzoneNum < 0)
        {
            return;
        }
        selectedFightzone = fightzones[selectedFightzoneNum];

        if (stageSelect)
        {
            GameController.Instance.selectedStage = selectedFightzone.sceneName;
            if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
            {
                NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).stage = selectedFightzone.sceneName;
            }
        }
    }

    public void HeadToLocationSelect()
    {

        if (stageSelect)
        {
            //So you're the host?
            //LET'S GO TO THE GAME!
            GameController.Instance.BeginMatch();
            return;
        }

        stageMenu.SetActive(true);

        //EventSystem.current.SetSelectedGameObject(GameObject.Find("Training"));

        //forwardButton.GetComponent<Button>().interactable = false;

        stageSelect =  true;

        if (GameController.Instance.IsHost)
        {
            if(GameController.Instance.IsOnline)sync["location"] = stageSelect;
            //GetComponent<SyncAnimatorStateHost>().SetState("BoardMove");

        }

        //List<DrifterType> pickedTypes = new List<DrifterType>();

        // foreach (Animator card in rightPanel.GetComponentsInChildren<Animator>())
        // {
        //    card.GetComponent<Animator>().SetBool("present", false);
        //     pickedTypes.Add(getDrifterTypeFromString(card.transform.GetChild(1).GetComponent<Text>().text));
        // }

        // foreach (Animator card in leftPanel.GetComponentsInChildren<Animator>())
        // {
        //     card.GetComponent<Animator>().SetBool("present", false);
        //     pickedTypes.Add(getDrifterTypeFromString(card.transform.GetChild(1).GetComponent<Text>().text));
        // }

        // foreach (PlayerSelectFigurine drifter in drifters)
        // {
        //     drifter.figurine.GetComponent<Figurine>().TurnArrowOff();
        //     //drifter.figurine.GetComponent<Animator>().SetBool("present", false);
        //     drifter.figurine.GetComponent<Button>().interactable = false;
        // }
        UpdateFightzone();
    }


    public void HeadToCharacterSelect()
    {

        if (GameController.Instance.IsHost)
        {
            stageSelect =  true;

            if(GameController.Instance.IsOnline)sync["location"] = stageSelect;
            GetComponent<SyncAnimatorStateHost>().SetState("BoardMoveBack");

        }

        //forwardButton.GetComponent<Button>().interactable = true;

        stageSelect = false;

        // foreach (Animator card in rightPanel.GetComponentsInChildren<Animator>())
        // {
        //     card.GetComponent<Animator>().SetBool("present", true);
        // }

        // foreach (Animator card in leftPanel.GetComponentsInChildren<Animator>())
        // {
        //     card.GetComponent<Animator>().SetBool("present", true);
        // }

        // foreach (PlayerSelectFigurine drifter in drifters)
        // {
        //     //drifter.figurine.GetComponent<Animator>().SetBool("present", true);
        //     drifter.figurine.GetComponent<Button>().interactable = true;

        // }

        //selectedFigurine.GetComponent<Figurine>().TurnArrowOn();
        //selectedFigurine.GetComponent<Button>().enabled = true;
        //EventSystem.current.SetSelectedGameObject(selectedFigurine);
        //selectedFigurine.GetComponent<Button>().enabled = false;
        //EventSystem.current.SetSelectedGameObject(forwardButton);

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

    public void disableStages()
    {
        stageMenu.SetActive(false);
    }
    public void enableStages()
    {
        stageMenu.SetActive(true);
    }

    // public void BackButton(){
    //     UnityEngine.Debug.Log("BACK PRESSED");
    //     if(stageSelect){
    //         HeadToCharacterSelect();
    //     }
    //     else if(selectedFigurine != null){
        	
    //     	selectedFigurine.GetComponent<Button>().enabled = true;
    //         currentDrifter = DrifterType.None;
    //         SelectDrifter("None");
    //         EventSystem.current.SetSelectedGameObject(backButton);
            
    //     }

    //     else{
    //         ReturnToTitle();
    //     }
    // }

    public bool everyoneReady()
    {
        foreach (CharacterSelectState selectState in charSelStates)
        {
            if(selectState.PlayerType == DrifterType.None){
                return false;
            }
        }
        return charSelStates.Count >=2;
    }

    public void ReturnToTitle()
    {
        //TODO: C
        if (GameController.Instance.GetComponent<NetworkClient>() != null)
            GameController.Instance.CleanupNetwork();


        if (GameController.Instance.GetComponent<NetworkHost>() != null)
            GameController.Instance.CleanupNetwork();
            
        GameController.Instance.Load("MenuScene");
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        CharacterSelectClientPacket selectCharacter = NetworkUtils.GetNetworkData<CharacterSelectClientPacket>(message.contents);
        if (selectCharacter != null)
        {
            foreach (CharacterSelectState state in charSelStates)
            {
                if (state.PeerID == message.peerId)
                {
                    state.PlayerType = selectCharacter.drifter;
                }
            }
        }
    }
    public void setStateWrapper(string state)
    {
        GetComponent<SyncAnimatorStateHost>().SetState(state);
    }
}



public class CharacterSelectSyncData : INetworkData
{
    public string Type { get; set; }
    public List<CharacterSelectState> charSelState = new List<CharacterSelectState>();
    public string stage;
}

public class CharacterSelectClientPacket : INetworkData
{
    public string Type { get; set; }
    public DrifterType drifter { get; set; }
}