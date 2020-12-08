using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColor
{
    RED, GOLD, GREEN, BLUE, PURPLE, MAGENTA, ORANGE, CYAN
}



// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour, INetworkMessageReceiver
{
    public GameObject movesetOverlay;

    public static Dictionary<PlayerColor, Color> ColorFromEnum = new Dictionary<PlayerColor, Color>()
    {
        { PlayerColor.RED, new Color(1.0f, 0f, 0f) },
        { PlayerColor.BLUE, new Color(0.075f, 0.702f, 0.906f) },
        { PlayerColor.GOLD, new Color(0.8f, 0.6f, 0f) },
        { PlayerColor.GREEN, new Color(0.124f, 0.866f, 0.118f) },
        { PlayerColor.PURPLE, new Color(0.725f, 0.063f, 1.0f) },
        { PlayerColor.MAGENTA, new Color(1.0f, 0.063f, 0.565f) },
        { PlayerColor.ORANGE, new Color(1.0f, 0.55f, 0.165f) },
        { PlayerColor.CYAN, new Color(0.0f, 1.0f, 0.702f) }
    };

    public GameObject leftPanel;
    public GameObject rightPanel;
    public GameObject roomCode;

   
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
        public string sceneName;
    }

    public List<PlayerSelectFigurine> drifters;
    Dictionary<DrifterType, PlayerSelectFigurine> figurines = new Dictionary<DrifterType, PlayerSelectFigurine>();
    public  List<FightZone> fightzones = new List<FightZone>();

    private GameObject clientCard;

    public GameObject stageMenu;

    public Sprite noImage;

    private FightZone selectedFightzone;
    private int selectedFightzoneNum = 0;

    //determines how many player cards we can fit on a panel
    private const int PANEL_MAX_PLAYERS = 4;

    public GameObject arrowPrefab;

    public GameObject forwardButton;
    public GameObject backButton;

    public GameObject selectedFigurine = null;

    public class PlayerMenuEntry
    {
        public GameObject arrow;
        public GameObject characterCard;
    }
    List<PlayerMenuEntry> menuEntries = new List<PlayerMenuEntry>();
    bool mouse = true;

    bool stageSelect = false;

    NetworkSync sync;

    public static CharacterMenu Instance => GameObject.FindGameObjectWithTag("CharacterMenu")?.GetComponent<CharacterMenu>();
    public static CharacterSelectSyncData CharSelData;

    DrifterType currentDrifter = DrifterType.None;

    void Awake()
    {
        foreach (PlayerSelectFigurine drifter in drifters)
        {
            figurines[drifter.drifter] = drifter;
            //drifter.figurine.GetComponent<Animator>().SetBool("present", true);
        }

        UpdateFightzone();

        if(GameController.Instance.IsHost){
            GameObject.Find("RoomCodeValue").GetComponent<Text>().text = GameController.Instance.host.RoomKey;
        }

        if (PlayerPrefs.GetInt("HideRoomCode") > 0 || !GameController.Instance.IsHost)
        {
            roomCode.SetActive(false);
        }
        else
        {
            roomCode.SetActive(true);
        }
    }

    void Start()
    {
        sync = GetComponent<NetworkSync>();
        sync["charSelState"] = new CharacterSelectSyncData()
        {
            Type = typeof(CharacterSelectSyncData).Name
        };
        sync["location"] = false;
        // add host
        AddCharSelState(-1);
    }

    public Dictionary<int, int> GetPeerIDsToPlayerIDs()
    {
        Dictionary<int, int> peerIDsToPlayerIDs = new Dictionary<int, int>();
        List<CharacterSelectState> charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        foreach (CharacterSelectState state in charSelStates)
        {
            peerIDsToPlayerIDs[state.PeerID] = state.PlayerIndex;
        }
        return peerIDsToPlayerIDs;
    }

    public void AddCharSelState(int peerID)
    {
        List<CharacterSelectState> charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        charSelStates.Add(new CharacterSelectState()
        {
            PeerID = peerID
        });
        SortCharSelState(charSelStates);
    }

    public void RemoveCharSelState(int peerID)
    {
        List<CharacterSelectState> charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        for (int i = 0; i < charSelStates.Count; i++)
        {
            if (charSelStates[i].PeerID == peerID)
            {
                charSelStates.RemoveAt(i);
                i--;
            }
        }
        SortCharSelState(charSelStates);
    }

    void SortCharSelState(List<CharacterSelectState> charSelStates)
    {
        // sort by peer ID
        CharSelData = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]);
        charSelStates.Sort((x, y) => x.PeerID.CompareTo(y.PeerID));
        for (int i = 0; i < charSelStates.Count; i++)
        {
            charSelStates[i].PlayerIndex = i;
        }
    }

    void FixedUpdate()
    {
        SyncToCharSelectState();
        transform.Find("ReadyButton").gameObject.SetActive(GameController.Instance.IsHost);

         if(Input.GetAxis("Mouse X")!=0 || Input.GetAxis("Mouse X")<0 && !mouse)
        {
            mouse = true;
            EventSystem.current.SetSelectedGameObject(null);

        }
        if((Input.anyKey || Input.GetAxis ("Horizontal") !=0 || Input.GetAxis ("Vertical") != 0) && mouse && (!Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))){
            mouse = false;
            EventSystem.current.SetSelectedGameObject(everyoneReady() && GameController.Instance.IsHost ?(stageSelect?GameObject.Find("Training"):forwardButton):GameObject.Find("Random Figurine"));
        }

        Cursor.visible = mouse;
    }

    void Update(){
        if(everyoneReady() && !stageSelect)forwardButton.GetComponent<Button>().interactable = true;
        else forwardButton.GetComponent<Button>().interactable = false;

        //Press B or esc to bo back a screen

        if((Input.GetKeyDown("joystick button 1") || Input.GetKeyDown(KeyCode.Escape))){
            UnityEngine.Debug.Log("BACK DETECTED");
            BackButton();
        }
    }

    public void SyncToCharSelectState()
    {
        // add cards if needed
        List<CharacterSelectState> charSelState = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        for (int i = menuEntries.Count; i < charSelState.Count; i++)
        {
            AddPlayerCard();
        }
        // remove cards if needed
        for (int i = charSelState.Count; i < menuEntries.Count; i++)
        {
            RemovePlayerCard();
            i--;
        }
        // set cards
        for (int i = 0; i < menuEntries.Count; i++)
        {
            DrifterType drifter = charSelState[i].PlayerType;
            
            if(drifter != DrifterType.None){
                CharacterCard.SetCharacter(menuEntries[i].characterCard.transform, figurines[drifter].image, drifter.ToString().Replace("_", " "));
            }
            else
            {
                CharacterCard.SetCharacter(menuEntries[i].characterCard.transform, noImage, "SELECT DRIFTER");
            }
            
        }
        // set arrow color and visibility
        foreach (PlayerSelectFigurine drifter in drifters)
        {
            CharacterSelectState state = charSelState.Find(x => x.PlayerIndex == GameController.Instance.PlayerID);
            if (drifter.figurine != null && !stageSelect)
            {
                drifter.figurine.GetComponent<Figurine>().SetColor(ColorFromEnum[(PlayerColor)state.PlayerIndex]);
                if (drifter.drifter == charSelState[state.PlayerIndex].PlayerType)
                {
                    drifter.figurine.GetComponent<Figurine>().TurnArrowOn();
                }
                else
                {
                    drifter.figurine.GetComponent<Figurine>().TurnArrowOff();
                }
            }
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

        GameObject charCard = CharacterCard.CreatePlayerCard(ColorFromEnum[(PlayerColor)index]);
        entry.characterCard = charCard;

        charCard.GetComponent<Animator>().SetBool("present", true);

        Transform card = charCard.transform;

        Transform parent = (index < PANEL_MAX_PLAYERS) ?
            leftPanel.transform : rightPanel.transform;

        card.SetParent(parent, false);

        if (GameController.Instance.IsHost)
        {
            // TODO: search for player index with peer ID -1
            GameObject myCard = menuEntries[0].characterCard;
            if (myCard == charCard)
            {
                CharacterCard.EnableKickPlayers(card, false);
            }
            else
            {
                //TODO: Call function to add listener for kick button click.
                Button kickPlayer = CharacterCard.EnableKickPlayers(card, GameController.Instance.IsHost);
                int cardIndex = menuEntries.IndexOf(entry);
                // kickPlayer.onClick.AddListener(() => GameController.Instance.GetComponent<NetworkHost>().ForceKick(cardIndex));
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

        GameController.Instance.selectedStage = selectedFightzone.sceneName;
        Cursor.visible = false;
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
            if (GameController.Instance.IsHost)
            {
                NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).stage = selectedFightzone.sceneName;
            }
        }
    }

    public void SelectRandomDrifter()
    {

    	//Update sound clip in here
    	switch(UnityEngine.Random.Range(0, 9)){
    		case 0:
    			SelectDrifter("Nero");
    			break;
    		case 1:
    			SelectDrifter("Orro");
    			break;
    		case 2:
    			SelectDrifter("Swordfrog");
    			break;
    		case 3:
    			SelectDrifter("Megurin");
    			break;
    		case 4:
    			SelectDrifter("Ryyke");
    			break;
    		case 5:
    			SelectDrifter("Lady_Parhelion");
    			break;
    		case 6:
    			SelectDrifter("Bojo");
    			break;
    		case 7:
    			SelectDrifter("Spacejam");
    			break;
            case 8:
                SelectDrifter("Lucille");
                break;    
    		default:
    			SelectDrifter("Spacejam");
    			break;						
    	}

    }

    public void SelectDrifter(string drifterString)
    {
        currentDrifter = Drifter.DrifterTypeFromString(drifterString);
        if (GameController.Instance.IsHost)
        {
            List<CharacterSelectState> charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
            foreach (CharacterSelectState state in charSelStates)
            {
                if (state.PeerID == -1)
                {
                    state.PlayerType = currentDrifter;
                }
            }
        }
        else
        {
            sync.SendNetworkMessage(new CharacterSelectClientPacket()
            {
                drifter = currentDrifter
            });
        }

        if(selectedFigurine!=null)selectedFigurine.GetComponent<Button>().enabled = true;
        if(drifterString !="None")
        {
            selectedFigurine = figurines[currentDrifter].figurine;
            selectedFigurine.GetComponent<Button>().enabled = false;
        }
        else
        {
            selectedFigurine = null;

        }
        
        EventSystem.current.SetSelectedGameObject(backButton);

        if(everyoneReady())
        {
            forwardButton.GetComponent<Button>().interactable = true;
            if(!mouse)EventSystem.current.SetSelectedGameObject(forwardButton);
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

        EventSystem.current.SetSelectedGameObject(GameObject.Find("Training"));

        forwardButton.GetComponent<Button>().interactable = false;

        stageSelect =  true;

        if (GameController.Instance.IsHost)
        {
            sync["location"] = stageSelect;
            GetComponent<SyncAnimatorStateHost>().SetState("BoardMove");

        }
        // if (!GameController.Instance.IsHost)
        // {
        //     //forwardButton.GetComponent<Animator>().SetBool("present", false);
        // }

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
            drifter.figurine.GetComponent<Figurine>().TurnArrowOff();
            //drifter.figurine.GetComponent<Animator>().SetBool("present", false);
            drifter.figurine.GetComponent<Button>().interactable = false;
        }
        UpdateFightzone();
    }

    public void HeadToCharacterSelect()
    {

        if (GameController.Instance.IsHost)
        {
            stageSelect =  true;

            sync["location"] = stageSelect;
            GetComponent<SyncAnimatorStateHost>().SetState("BoardMoveBack");

        }
        
        stageMenu.SetActive(false);

        forwardButton.GetComponent<Button>().interactable = true;

        stageSelect = false;

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
            //drifter.figurine.GetComponent<Animator>().SetBool("present", true);
            drifter.figurine.GetComponent<Button>().interactable = true;

        }

        selectedFigurine.GetComponent<Figurine>().TurnArrowOn();
        selectedFigurine.GetComponent<Button>().enabled = true;
        EventSystem.current.SetSelectedGameObject(selectedFigurine);
        selectedFigurine.GetComponent<Button>().enabled = false;
        EventSystem.current.SetSelectedGameObject(forwardButton);

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
        int drifterMoves = 0;
        switch(currentDrifter)
        {
            case DrifterType.Nero:
                drifterMoves = 1;
                break;
            case DrifterType.Orro:
                drifterMoves = 2;
                break;
            case DrifterType.Lady_Parhelion:
                drifterMoves = 3;
                break;
            case DrifterType.Bojo:
                drifterMoves = 4;
                break;
            case DrifterType.Megurin:
                drifterMoves = 5;
                break;
            case DrifterType.Ryyke:
                drifterMoves = 6;
                break;
            case DrifterType.Swordfrog:
                drifterMoves = 7;
                break;

        }
        movesetOverlay.GetComponentInChildren<TutorialSwapper>().SelectDrifter(drifterMoves);
    }

    public void BackButton(){
        UnityEngine.Debug.Log("BACK PRESSED");
        if(stageSelect){
            HeadToCharacterSelect();
        }
        else if(selectedFigurine != null){
        	
        	selectedFigurine.GetComponent<Button>().enabled = true;
            currentDrifter = DrifterType.None;
            SelectDrifter("None");
            EventSystem.current.SetSelectedGameObject(backButton);
            

        }

        else{
            ReturnToTitle();
        }
    }

    public bool everyoneReady(){
        List<CharacterSelectState> charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        foreach (CharacterSelectState selectState in charSelStates)
        {
            if(selectState.PlayerType == DrifterType.None){
                return false;
            }
        }
        return true;
    }

    public void ReturnToTitle()
    {
        //TODO: C
        if (GameController.Instance.GetComponent<NetworkClient>() != null)
        {
            GameController.Instance.CleanupNetwork();
        }

        if (GameController.Instance.GetComponent<NetworkHost>() != null)
        {
            GameController.Instance.CleanupNetwork();
        }
        GameController.Instance.Load("MenuScene");
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        CharacterSelectClientPacket selectCharacter = NetworkUtils.GetNetworkData<CharacterSelectClientPacket>(message.contents);
        if (selectCharacter != null)
        {
            List<CharacterSelectState> charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
            foreach (CharacterSelectState state in charSelStates)
            {
                if (state.PeerID == message.peerId)
                {
                    state.PlayerType = selectCharacter.drifter;
                }
            }
        }
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