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
    RED, GOLD, GREEN, BLUE, PURPLE, MAGENTA, ORANGE, CYAN, GREY, WHITE, BLACK
}


[Serializable]
public enum BattleStage
{
    None,Random,Training,Moosejaw,Wendys,Treefell,Driftwood,Neotokyo,Amberriver,Hadalkeep
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
        { PlayerColor.GREY, new Color(0.6f, 0.6f, 0.6f) },
        { PlayerColor.WHITE, new Color(0.9f, 0.9f, 0.9f) },
        { PlayerColor.BLACK, new Color(0.1f, 0.1f, 0.1f) }
    };

    public GameObject bottomPanel;
    //public GameObject rightPanel;
    public GameObject roomCode;

    //Character Matrix
    public GameObject[] topRow;
    public GameObject[] middleRow;
    public GameObject[] bottomRow;
    GameObject[][] characterRows = new GameObject[3][];


    public GameObject[] topStageRow;
    public GameObject[] middleStageRow;
    public GameObject[] bottomStageRow;
    GameObject[][] stageRows = new GameObject[3][];

    public GameObject playerInputPrefab;

    NetworkSyncToHost syncFromClients;

    float prevScreenTimer = 0;
    bool countingPrevScreen = false;

    private GameObject playerCardPrefab;
    public GameObject Banner;

    Dictionary<int,GameObject> playerCards = new Dictionary<int,GameObject>();

    //0 - charactewr select
    //1 - all characters selected
    //2 - start pressed with all characters selected/ moving to stage select
    //3 - stage select
    //4 - all stages selected
    //5 - game is starting 
    //6 - returning to character select fromstage select
    int phase = 0;

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


        stageRows[0] = topStageRow;
        stageRows[1] = middleStageRow;
        stageRows[2] = bottomStageRow;

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

        //Delay this by 1/2 a second
        StartCoroutine(delayJoining());

        //If no controllers are currently active, activate the primary controller
        // if(GameController.Instance.IsTraining || GameController.Instance.controls.Count <1 )
        //     GameController.Instance.AssignInputAssest();

        foreach(int peer in GameController.Instance.controls.Keys)
        {
            AddCharSelState(peer);
        }

        //Populate a card for each active controller
        // for(int i = -1; i < GameController.Instance.controls.Count-1; i++)
        //     AddCharSelState(i);

        if(GameController.Instance.IsTraining) AddCharSelState(8,DrifterType.Sandbag);

    }
    IEnumerator delayJoining()
    {
        yield return new WaitForSeconds(.25f);
        GameController.Instance.EnableJoining();
    }

    public Dictionary<int, int> GetPeerIDsToPlayerIDs()
    {
        Dictionary<int, int> peerIDsToPlayerIDs = new Dictionary<int, int>();
        foreach (CharacterSelectState charSelState in charSelStates.Values)
        {
            peerIDsToPlayerIDs[charSelState.PeerID] = (charSelState.PeerID+1);
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
        if(charSelStates.Count >= (GameController.Instance.IsTraining?2:GameController.Instance.maxPlayerCount))return;

        int[] drifterLoc = findDrifterMatrixPosition(drifter);
        GameObject cursor = GameController.Instance.host.CreateNetworkObject("CharacterCursor",characterRows[drifterLoc[0]][drifterLoc[1]].transform.position, transform.rotation);
        cursor.GetComponent<SpriteRenderer>().color = ColorFromEnum[(PlayerColor)(peerID+1)];

        charSelStates.Add(peerID,new CharacterSelectState()
        {
            PeerID = peerID,
            Cursor = cursor,
            PlayerType = drifter,
            x = drifterLoc[1],
            y = drifterLoc[0],
            StageType = BattleStage.None,
        });

        GameObject card = GameController.Instance.host.CreateNetworkObject("CharacterSelectCard",new Vector2(-20 + 13.5f * ((peerID +1) % 4),-9), transform.rotation);

        card.transform.SetParent(gameObject.transform , false);

        playerCards.Add(peerID,card);

        //Fix this for multiple input devices
        if(peerID != -1)
            GameController.Instance.host.Peers.Add(peerID);
    }

    public void RemoveCharSelState(int peerID)
    {
        if(!charSelStates.ContainsKey(peerID))
        {
            UnityEngine.Debug.Log("PEER NOT FOUND FOR REMOVAL: " + peerID);

            return;

        }
    
        UnityEngine.Debug.Log("PEER REMOVED");

        Destroy(charSelStates[peerID].Cursor);
        charSelStates.Remove(peerID);

        Destroy(playerCards[peerID]);
        playerCards.Remove(peerID);


        //if(peerID != -1)
            

        //Return to main menu if not in multiplayer
        checkReturnToMenuConditions();
    }

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

        //Only allow adding and removing players before characters are locked in

        //Return to title if the last player left
        //Maybe remove?
        checkReturnToMenuConditions();

        //Update input on each active char select state

        PlayerInputData input;

        int index = 0;
        List<int> peersToRemove = new List<int>();
        foreach (KeyValuePair<int, CharacterSelectState> kvp in charSelStates)
        {
            //Link inputs to peer ids
            bool remove = false;
            input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", kvp.Value.PeerID]);
            if (input != null)
                remove = UpdateInput(charSelStates[kvp.Key], input);
                    
            else if(GameController.Instance.controls.ContainsKey(kvp.Value.PeerID))
                remove = UpdateInput(charSelStates[kvp.Key], NetworkPlayers.GetInput(GameController.Instance.controls[kvp.Value.PeerID]));

            if(remove)peersToRemove.Add(kvp.Value.PeerID);
            index++;
        }

        foreach(int peerToRemove in peersToRemove)
            GameController.Instance.removeUserByPeer(peerToRemove);

        //Return to title if the special button is helf for 1.5 consecutive seconds
        if(countingPrevScreen)
        {
            prevScreenTimer += Time.deltaTime;
            if(prevScreenTimer > 1.5f) 
                if(phase < 3)ReturnToTitle();
                else 
                {
                    phase = 3;
                    prevScreenTimer = 0;
                }
        }
        else
            prevScreenTimer = 0;

        countingPrevScreen = false;
    

    //0 - charactewr select
    //1 - all characters selected
    //2 - start pressed with all characters selected/ moving to stage select
    //3 - returning to character select fromstage select
    //4 - stage select
    //5 - all stages selected
    //6 - game is starting 
    

        if(GameController.Instance.IsOnline)sync["location"] = phase;

        switch(phase)
        {
            case 0:
                if(checkCharacterSelectReadiness())
                    phase = 1;
                break;

            case 1:
                if(!checkCharacterSelectReadiness())
                    phase = 0;
                break;

            case 2:
                GameController.Instance.DisableJoining();
                gameObject.transform.position = new Vector2(0,18);
                foreach(CharacterSelectState charSelState in charSelStates.Values)
                {
                    if(charSelState.PeerID <8)
                    {
                        charSelState.x = 3;
                        charSelState.y = 0;
                        charSelState.Cursor.transform.position = stageRows[0][3].transform.position;
                    }
                    else 
                        charSelState.Cursor.transform.position = characterRows[1][2].transform.position;
                }
                phase = 4;
                break;

            case 3:
                gameObject.transform.position = Vector2.zero;
                GameController.Instance.EnableJoining();
                foreach(CharacterSelectState charSelState in charSelStates.Values)
                {
                    int[] arr = findDrifterMatrixPosition(charSelState.PlayerType);
                    charSelState.x = arr[1];
                    charSelState.y = arr[0];
                    charSelState.Cursor.transform.position = characterRows[arr[0]][arr[1]].transform.position;
                }
                phase = 0;
                break;
                
            case 4:
            if(checkStageSelectReadiness())
                    phase = 5;
                break;
                
            case 5:
                if(!checkStageSelectReadiness())
                    phase = 4;
                break;  

            case 6:
                List<BattleStage> randomStage = new List<BattleStage>();
                foreach(CharacterSelectState charSelState in charSelStates.Values)
                {

                    //Random Character sync
                    if(charSelState.PlayerType == DrifterType.Random)
                        charSelState.PlayerType = (DrifterType)UnityEngine.Random.Range(3,DrifterType.GetValues(typeof( DrifterType)).Length-1);

                    //Populate stage list
                    if(charSelState.StageType != BattleStage.None && charSelState.PeerID < 8)
                    {
                        //Add a random non none, random, training stage to the list
                        if(charSelState.StageType == BattleStage.Random)
                             randomStage.Add((BattleStage)UnityEngine.Random.Range(4,BattleStage.GetValues(typeof(BattleStage)).Length-1));
                        else
                            randomStage.Add(charSelState.StageType);
                    }
                        
                }

                string selectedStage = randomStage[UnityEngine.Random.Range(0,(randomStage.Count -1))].ToString();
                GameController.Instance.selectedStage = selectedStage;

                // if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
                // {
                //     NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).stage = selectedFightzone.sceneName;
                // }
                GameController.Instance.BeginMatch();
                break;
            default:
                break;

        }
    }

    //Circular Array Helper
    private int wrapIndex(int curr, int max)
    {
        if(curr >= max) return 0;
        else if(curr < 0) return (max-1);
        else return curr;
    }


    //Updates input commands for a given cursor object
    public bool UpdateInput(CharacterSelectState p_cursor,PlayerInputData input)
    {
        //If no previous input yet, populate it
        if(p_cursor.prevInput == null)
        {
           p_cursor.prevInput = input;
           return false; 
        }
        
        GameObject[][] matrix = (phase >2)?stageRows:characterRows;

        //Wrap around the horizontal arrays
        if(p_cursor.prevInput.MoveX ==0 && input.MoveX != 0)
        {
            p_cursor.x = wrapIndex(p_cursor.x + (int)input.MoveX ,matrix[0].Length);
        
            while(matrix[p_cursor.y][p_cursor.x] == null)
                p_cursor.x = wrapIndex(p_cursor.x + (int)input.MoveX ,matrix[0].Length);
        }
        
        //Wrap around the vertical arrays
        //Todo: make hanging edges work
        if(p_cursor.prevInput.MoveY ==0 && input.MoveY != 0)
        {
            p_cursor.y = wrapIndex(p_cursor.y + (int)input.MoveY ,matrix.Length);

            while(matrix[p_cursor.y][p_cursor.x] == null)
                p_cursor.y = wrapIndex(p_cursor.y + (int)input.MoveY ,matrix.Length);
        }
        
        //Sets the cursor's location to that of the current character icon
        p_cursor.Cursor.transform.localPosition = matrix[p_cursor.y][p_cursor.x].transform.position;
        
        //Select or deselelect on light press
        if(input.Light && !p_cursor.prevInput.Light && phase <2)
        {
            DrifterType selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().drifterType;
            p_cursor.PlayerType = (p_cursor.PlayerType == DrifterType.None || p_cursor.PlayerType != selected)?selected:DrifterType.None;
        }
        else if(input.Light && !p_cursor.prevInput.Light && phase >=3)
        {
            BattleStage selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().StageType;
            p_cursor.StageType = (p_cursor.StageType == BattleStage.None || p_cursor.StageType != selected)?selected:BattleStage.None;
        }
        
        //Deselect on special press
        else if(input.Special && !p_cursor.prevInput.Special && p_cursor.PlayerType != DrifterType.None && phase < 2)
            p_cursor.PlayerType = DrifterType.None;

        else if(input.Special && !p_cursor.prevInput.Special && p_cursor.StageType != BattleStage.None && phase <3 && phase <6 )
            p_cursor.StageType = BattleStage.None;

        //Return to previous screen if special is held
        if(input.Special && p_cursor.prevInput.Special)
            countingPrevScreen = true;

        //Remove this probably
        if(!p_cursor.prevInput.Pause && input.Pause && phase == 1)
            phase = 2;
        else if(!p_cursor.prevInput.Pause && input.Pause && phase == 5)
            phase = 6;

        //Saves previous input
        p_cursor.prevInput = input;

        return input.Menu && (phase == 0 || phase == 1);

    }

    //Checks for conditions that should return players to the main menu
    //Add to this later for other single player modes
    void checkReturnToMenuConditions()
    {
        // if(GameController.Instance.playerCount <1 && GameController.Instance.IsTraining)
        //     ReturnToTitle();
    }

    //Checks to make sure each player has selected a character
    bool checkCharacterSelectReadiness()
    {

        foreach (CharacterSelectState charSelState in charSelStates.Values)
        {
            if(charSelState.PlayerType == DrifterType.None){
                Banner.SetActive(false);
                return false;
            }
        }

        //phase = true;
        if(charSelStates.Count >=2 && !Banner.activeInHierarchy)Banner.SetActive(true);
        else if(charSelStates.Count <2 && Banner.activeInHierarchy)Banner.SetActive(false);

        return charSelStates.Count >=2;
    }

    //checks if each active player has selected a stage
    bool checkStageSelectReadiness()
    {
        foreach (CharacterSelectState charSelState in charSelStates.Values)
        {
            if(charSelState.StageType == BattleStage.None && charSelState.PeerID < 8)
            {
                Banner.SetActive(false);
                return false;
            }
        }
        if(charSelStates.Count >=2)Banner.SetActive(true);
        return charSelStates.Count >=2;
    }

    //Backs out to the tile screen and disconnects clients if there plaeyr is host
    public void ReturnToTitle()
    {
        GameController.Instance.removeAllPeers();
        //TODO: C
        if (GameController.Instance.GetComponent<NetworkClient>() != null)
            GameController.Instance.CleanupNetwork();


        if (GameController.Instance.GetComponent<NetworkHost>() != null)
            GameController.Instance.CleanupNetwork();
            
        GameController.Instance.Load("MenuScene");
    }


    //TODO Below Here



    public void SyncToCharSelectState()
    {
        // add cards if needed
        for (int i = playerCards.Count; i < charSelStates.Count; i++)
            AddPlayerCard();

        // remove cards if needed
        for (int i = charSelStates.Count; i < playerCards.Count; i++)
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
            //SelectFightzone(NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).stage);
        }
    }

    //try to add player, return false if over max
    public void AddPlayerCard()
    {
        // if (playerCards.Count >= GameController.maxPlayerCount)
        // {
        //     return;
        // }
        

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
        return DrifterType.None;
    }


    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        CharacterSelectClientPacket selectCharacter = NetworkUtils.GetNetworkData<CharacterSelectClientPacket>(message.contents);
        if (selectCharacter != null)
        {
            foreach (CharacterSelectState charSelState in charSelStates.Values)
            {
                if (charSelState.PeerID == message.peerId)
                {
                    charSelState.PlayerType = selectCharacter.drifter;
                }
            }
        }
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