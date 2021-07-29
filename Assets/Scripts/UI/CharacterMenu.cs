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

    float prevScreenTimer = 0;
    bool countingPrevScreen = false;


    //Old Stuff


    [Serializable]
    public class FightZone
    {
        public string sceneName;
    }

    public  List<FightZone> fightzones = new List<FightZone>();

    private GameObject clientCard;
    public GameObject Banner;

    private FightZone selectedFightzone;
    private int selectedFightzoneNum = 0;

    public class PlayerMenuEntry
    {
        public GameObject arrow;
        public GameObject characterCard;
    }
    List<PlayerMenuEntry> menuEntries = new List<PlayerMenuEntry>();

    bool stageSelect = false;

    NetworkSync sync;

    public static Dictionary<int,CharacterSelectState> charSelStates;

    public static CharacterMenu Instance => GameObject.FindGameObjectWithTag("CharacterMenu")?.GetComponent<CharacterMenu>();
    //public static CharacterSelectSyncData CharSelData = new CharSelData;
    
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

        charSelStates = new Dictionary<int,CharacterSelectState>();

        //
    
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


        //If no controllers are currently active, activate the primary controller
        if(GameController.Instance.controls.Count <1 || GameController.Instance.IsTraining)
            GameController.Instance.AssignInputAssest();

        foreach(InputActionAsset controller in GameController.Instance.availableControls)
        {
            GameObject PlayerInput = Instantiate(playerInputPrefab, transform.position, Quaternion.identity);
            PlayerInput.GetComponent<PlayerInput>().actions = controller;
        }

        //Populate a card for each active controller
        for(int i = -1; i < GameController.Instance.controls.Count-1; i++)
            AddCharSelState(i);

        if(GameController.Instance.IsTraining) AddCharSelState(0,DrifterType.Sandbag);

    }

    public Dictionary<int, int> GetPeerIDsToPlayerIDs()
    {
        Dictionary<int, int> peerIDsToPlayerIDs = new Dictionary<int, int>();
        foreach (CharacterSelectState state in charSelStates.Values)
        {
            peerIDsToPlayerIDs[state.PeerID] = (state.PeerID+1);
        }
        return peerIDsToPlayerIDs;
    }

    public void AddCharSelState(int peerID)
    {
        UnityEngine.Debug.Log(peerID);
        AddCharSelState(peerID,DrifterType.None);
    }

    public void AddCharSelState(int peerID, DrifterType drifter)
    {
        UnityEngine.Debug.Log(peerID);
        if(charSelStates.Count >= (GameController.Instance.IsTraining?2:GameController.MAX_PLAYERS))return;

        int[] drifterLoc = findDrifterMatrixPosition(drifter);
        GameObject cursor = GameController.Instance.host.CreateNetworkObject("CharacterCursor",characterRows[drifterLoc[0]][drifterLoc[1]].transform.position, transform.rotation);
        cursor.GetComponent<SpriteRenderer>().color = ColorFromEnum[(PlayerColor)(peerID+1)];

        charSelStates.Add(peerID,new CharacterSelectState()
        {
            PeerID = peerID,
            Cursor = cursor,
            PlayerType = drifter,
            x = drifterLoc[1],
            y = drifterLoc[0]
        });

        //Fix this for multiple input devices
        if(peerID != -1)
            GameController.Instance.host.Peers.Add(peerID);

        //SortCharSelState(charSelStates);
    }

    public void RemoveCharSelState(int peerID)
    {
        
        if(!charSelStates.ContainsKey(peerID))return;
    
        UnityEngine.Debug.Log("PEER REMOVED");

        Destroy(charSelStates[peerID].Cursor);
        charSelStates.Remove(peerID);

        // for (int i = 0; i < charSelStates.Count; i++)
        // {
        //     if (charSelStates[i].PeerID == peerID)
        //     {
                
        //         i--;
        //     }
        // }

        if(peerID != -1)
            GameController.Instance.host.Peers.Remove(peerID);

        //SortCharSelState(charSelStates);
    }

    // void SortCharSelState(List<CharacterSelectState> charSelStates)
    // {
    //     // sort by peer ID
    //     charSelStates.Sort((x, y) => x.PeerID.CompareTo(y.PeerID));
    //     for (int i = 0; i < charSelStates.Count; i++)
    //     {
    //         charSelStates[i].PlayerIndex = i;
    //     }
    // }

    //Finds the y-x positio of a certain drifter in the matrix and returns the values as an array
    private int[] findDrifterMatrixPosition(DrifterType drifter)
    {
        for(int y = 0; y < 3; y++)
        {
            for(int x = 0; x < 10; x++)
            {
                if(characterRows[y][x] != null && characterRows[y][x].GetComponent<CharacterSelectPortrait>().drifterType == drifter)
                    return new int[] {y,x};
            }
        }
        return new int[] {1,7};
    }

    void FixedUpdate()
    {
        SyncToCharSelectState();
    }


    void Update()
    {

        //Make real stage select


        //TODO make sure 
        //Create Character Select State when an inactive controller becomes active
        for(int i = 0; i < GameController.Instance.checkForNewControllers(); i++)
        {
            int peerID = -1;
            while(charSelStates.ContainsKey(peerID))
                peerID++;

            if(peerID < GameController.MAX_PLAYERS)AddCharSelState(peerID);
        }
    
    
        //Remove Character select state if a controller is disconnected
        List<int> toRemove = GameController.Instance.checkForRemoveControllers();
        if(toRemove.Count >0)
        {
            foreach(int peerID in toRemove)
                RemoveCharSelState(peerID);
        }

        //Return to title if the last player left
        //Maybe remove?
        if(GameController.Instance.controls.Count <1)
        {
            ReturnToTitle();
            return;
        }

        //Update input on each active char select state

        PlayerInputData input;

        int index = 0;
        foreach (KeyValuePair<int, CharacterSelectState> kvp in charSelStates)
        {
            //Link inputs to peer ids
            input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", kvp.Value.PeerID]);
            if (input != null)
                UpdateInput(charSelStates[kvp.Key], input);
            else if(GameController.Instance.controls.ContainsKey(kvp.Value.PeerID))
                UpdateInput(charSelStates[kvp.Key], NetworkPlayers.GetInput(GameController.Instance.controls[kvp.Value.PeerID]));
            index++;
        }

        //Return to title if the special button is helf for 1.5 consecutive seconds
        if(countingPrevScreen)
        {
            UnityEngine.Debug.Log(prevScreenTimer);
            prevScreenTimer += Time.deltaTime;
            if(prevScreenTimer > 1.5f) ReturnToTitle();
        }
        else
            prevScreenTimer = 0;

        countingPrevScreen = false;
      
        //If every player has selected a character, display the ready banner
        //If someone presses [pause], start the game
        if(everyoneReady() && stageSelect)
        {
            stageSelect = false;
            SelectFightzone("Training");
            UpdateFightzone();
            GameController.Instance.BeginMatch();
        }

        //Return to title if the last player leaves
        stageSelect = false;

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
        //If no previous input yet, populate it
        if(p_cursor.prevInput == null)
        {
           p_cursor.prevInput = input;
           return; 
        }
        
        //Wrap around the horizontal arrays
        if(p_cursor.prevInput.MoveX ==0 && input.MoveX != 0)
        {
            p_cursor.x = wrapIndex(p_cursor.x + (int)input.MoveX ,9);
        
            while(characterRows[p_cursor.y][p_cursor.x] == null)
                p_cursor.x = wrapIndex(p_cursor.x + (int)input.MoveX ,9);
        }
        
        //Wrap around the vertical arrays
        //Todo: make hanging edges work
        if(p_cursor.prevInput.MoveY ==0 && input.MoveY != 0)
        {
            p_cursor.y = wrapIndex(p_cursor.y + (int)input.MoveY ,2);

            while(characterRows[p_cursor.y][p_cursor.x] == null)
                p_cursor.y = wrapIndex(p_cursor.y + (int)input.MoveY ,2);
        }
        
        //Sets the cursor's location to that of the current character icon
        p_cursor.Cursor.transform.localPosition = characterRows[p_cursor.y][p_cursor.x].transform.position;
        
        //Select or deselelect on light press
        if(input.Light && !p_cursor.prevInput.Light)
        {
            DrifterType selected = characterRows[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().drifterType;
            p_cursor.PlayerType = (p_cursor.PlayerType == DrifterType.None || p_cursor.PlayerType != selected)?selected:DrifterType.None;
        }
        
        //Deselect on special press
        else if(input.Special && !p_cursor.prevInput.Special && p_cursor.PlayerType != DrifterType.None)
            p_cursor.PlayerType = DrifterType.None;

        //Return to previous screen if special is held
        if(input.Special && p_cursor.prevInput.Special)
            countingPrevScreen = true;

        //Remove this probably
        if(everyoneReady() && !p_cursor.prevInput.Pause && input.Pause)
            stageSelect = true;

        //Saves previous input
        p_cursor.prevInput = input;

    }


    //TODO Below Here



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
        // for (int i = 0; i < menuEntries.Count; i++)
        // {
        //     DrifterType drifter = charSelStates[i].PlayerType;
        // }
            
        
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

    // public void disableStages()
    // {
    //     stageMenu.SetActive(false);
    // }
    // public void enableStages()
    // {
    //     stageMenu.SetActive(true);
    // }

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

        foreach (CharacterSelectState selectState in charSelStates.Values)
        {
            if(selectState.PlayerType == DrifterType.None){
                Banner.SetActive(false);
                return false;
            }
        }
        if(charSelStates.Count >=2)Banner.SetActive(true);
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
            foreach (CharacterSelectState state in charSelStates.Values)
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
    public Dictionary<int,CharacterSelectState> charSelState = new Dictionary<int,CharacterSelectState>();
    public string stage;
}

public class CharacterSelectClientPacket : INetworkData
{
    public string Type { get; set; }
    public DrifterType drifter { get; set; }
}