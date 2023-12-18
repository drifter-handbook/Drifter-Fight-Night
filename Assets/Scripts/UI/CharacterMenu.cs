using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum PlayerColor {
    RED, 
    GOLD, 
    GREEN, 
    BLUE, 
    PURPLE, 
    MAGENTA, 
    ORANGE, 
    CYAN, 
    GREY, 
    WHITE, 
    BLACK, 
    DARK_GREY
}

public enum BattleStage {
    None,
    Random,
    Training,
    Moosejaw,
    Mendys,
    Treefell,
    Driftwood,
    Neo_Tokyo,
    Amber_River,
    Hadal_Keep
}

public enum CharacterMenuState {
    CharSelect,                                 //In character select screen
    AllCharsSelected,                           //In character select, all players have selected a character
    TransitionToStageSelect,                    //Start pressed with all characters selected, moving to stage select
    TransitionToCharSelectFromStageSelect,      //Return to character select from stage select
    StageSelect,                                //In stage select screen
    AllStagesSelected,                          //In stage select, all stages have been selected
    GameStart                                   //Transition to gameplay
}

[Serializable]

// Shows players and selected character [View]
public class CharacterMenu : MonoBehaviour, INetworkMessageReceiver {
    //-------------------------------------------------------------
    // START OF ITEMS ACCESSIBLE FROM SCENE COMPONENT
    //-------------------------------------------------------------
    public GameObject roomCode;

    //Character Matrix
    public GameObject[] topRow;
    public GameObject[] middleRow;
    public GameObject[] bottomRow;
    GameObject[][] characterRows = new GameObject[3][];

    //Stage Matrix
    public GameObject[] topStageRow;
    public GameObject[] middleStageRow;
    public GameObject[] bottomStageRow;
    GameObject[][] stageRows = new GameObject[3][];

    public GameObject playerInputPrefab;
    public GameObject Banner;
    public Image[] backArrows;
    //-------------------------------------------------------------
    // END OF ITEMS ACCESSIBLE FROM SCENE COMPONENT
    //-------------------------------------------------------------

    public static Dictionary<PlayerColor, Color> ColorFromEnum = new Dictionary<PlayerColor, Color>() {
        { PlayerColor.RED, new Color(1.0f, 0f, 0f) },
        { PlayerColor.GOLD, new Color(.9f, 0.75f, 0f) },
        { PlayerColor.BLUE, new Color(0.075f, 0.702f, 0.906f) },
        { PlayerColor.GREEN, new Color(0.124f, 0.866f, 0.118f) },
        { PlayerColor.PURPLE, new Color(0.725f, 0.063f, 1.0f) },
        { PlayerColor.MAGENTA, new Color(1.0f, 0.063f, 0.565f) },
        { PlayerColor.ORANGE, new Color(1.0f, 0.55f, 0.165f) },
        { PlayerColor.CYAN, new Color(0.0f, 1.0f, 0.702f) },
        { PlayerColor.WHITE, new Color(.9f, .9f, .9f) },
        { PlayerColor.GREY, new Color(0.7f, 0.7f, 0.7f) },
        { PlayerColor.DARK_GREY, new Color(0.5f, 0.5f, 0.5f)},
        { PlayerColor.BLACK, new Color(0.3f, 0.3f, 0.3f) }
    };

    NetworkSyncToHost syncFromClients;
    NetworkSync sync;

    float prevScreenTimer = 0;
    bool countingPrevScreen = false;

    Dictionary<int,GameObject> playerCards = new Dictionary<int,GameObject>();
    public static Dictionary<int, CharacterSelectState> charSelStates;

    CharacterMenuState phase = CharacterMenuState.CharSelect;

    public static CharacterMenu Instance => GameObject.FindGameObjectWithTag("CharacterMenu")?.GetComponent<CharacterMenu>();
    
    void Awake() {
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

    void Start() {
        characterRows[0] = topRow;
        characterRows[1] = middleRow;
        characterRows[2] = bottomRow;
        stageRows[0] = topStageRow;
        stageRows[1] = middleStageRow;
        stageRows[2] = bottomStageRow;
        syncFromClients = GetComponent<NetworkSyncToHost>();
        charSelStates = new Dictionary<int,CharacterSelectState>();
    
        if(GameController.Instance.IsOnline) {
            sync = GetComponent<NetworkSync>();
            sync["charSelState"] = new CharacterSelectSyncData() {
                Type = typeof(CharacterSelectSyncData).Name
            };
            sync["location"] = false;

            charSelStates = NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).charSelState;
        }
        GameController.Instance.host.Peers =  new List<int>();
        // add host

        //Delay this by 1/2 a second
        StartCoroutine(delayJoining());

        if (GameController.Instance.IsTraining)
            AddCharSelState(8, GameController.Instance.Trainee);

        GameController.Instance.removeAllUIPeers();
    }

    IEnumerator delayJoining() {
        yield return new WaitForSeconds(.25f);
        GameController.Instance.EnableJoining();
    }

    public Dictionary<int, int> GetPeerIDsToPlayerIDs() {
        Dictionary<int, int> peerIDsToPlayerIDs = new Dictionary<int, int>();

        foreach (CharacterSelectState charSelState in charSelStates.Values)
            peerIDsToPlayerIDs[charSelState.PeerID] = (charSelState.PeerID+1);

        return peerIDsToPlayerIDs;
    }

    public void AddCharSelState(int peerID) {
        AddCharSelState(peerID,DrifterType.None);
    }

    public void AddCharSelState(int peerID, DrifterType drifter) {
        int maxPlayer = GameController.Instance.maxPlayerCount;
        bool training = GameController.Instance.IsTraining;

        if (charSelStates.Count >= (training ? 2 : maxPlayer))
            return;

        int[] drifterLoc = findDrifterMatrixPosition(drifter);
        GameObject cursor = GameController.Instance.host.CreateNetworkObject("CharacterCursor",characterRows[drifterLoc[0]][drifterLoc[1]].transform.position, transform.rotation);
        cursor.GetComponent<SpriteRenderer>().color = ColorFromEnum[(PlayerColor)(peerID+1)];

        GameObject card = GameController.Instance.host.CreateNetworkObject("CharacterSelectCard",new Vector2(-20 + 13.5f * ((peerID +1) % 4),-9), transform.rotation);

        charSelStates.Add(peerID,new CharacterSelectState()
        {
            PeerID = peerID,
            Cursor = cursor,
            PlayerType = drifter,
            x = drifterLoc[1],
            y = drifterLoc[0],
            StageType = BattleStage.None,
        });

        card.transform.SetParent(gameObject.transform , false);
        card.GetComponent<CharacterCard>().SetCharacter(drifter);
        playerCards.Add(peerID,card);

        //TODO: Fix this for multiple input devices
        if(peerID != -1)
            GameController.Instance.host.Peers.Add(peerID);
    }

    public void RemoveCharSelState(int peerID) {
        if(!charSelStates.ContainsKey(peerID)) {
            UnityEngine.Debug.Log("PEER NOT FOUND FOR REMOVAL: " + peerID);
            return;
        }
    
        UnityEngine.Debug.Log("PEER REMOVED");
        Destroy(charSelStates[peerID].Cursor);
        charSelStates.Remove(peerID);
        Destroy(playerCards[peerID]);
        playerCards.Remove(peerID);
    }

    //Finds the y-x positio of a certain drifter in the matrix and returns the values as an array
    private int[] findDrifterMatrixPosition(DrifterType drifter) {
        for(int y = 0; y < 3; y++) {
            for(int x = 0; x < 10; x++) {
                if(characterRows[y][x] != null && characterRows[y][x].GetComponent<CharacterSelectPortrait>().drifterType == drifter)
                    return new int[] { y, x };
            }
        }
        return new int[] {1,7};
    }

    void Update() {
        bool isBeforeStageSelect = (phase == CharacterMenuState.CharSelect || phase == CharacterMenuState.AllCharsSelected || phase == CharacterMenuState.TransitionToStageSelect);

        //Update input on each active char select state
        PlayerInputData input;

        List<int> peersToRemove = new List<int>();
        foreach (KeyValuePair<int, CharacterSelectState> kvp in charSelStates) {
            //Link inputs to peer ids
            bool remove = false;
            input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", kvp.Value.PeerID]);

            if (input != null)
                remove = UpdateInput(charSelStates[kvp.Key], input);    
            else if(GameController.Instance.controls.ContainsKey(kvp.Value.PeerID))
                remove = UpdateInput(charSelStates[kvp.Key], NetworkPlayers.GetInput(GameController.Instance.controls[kvp.Value.PeerID]));

            if (remove)
                peersToRemove.Add(kvp.Value.PeerID);
        }

        foreach (int peerToRemove in peersToRemove)
            GameController.Instance.removeUserByPeer(peerToRemove);

        //Return to title if the special button is helf for 1.5 consecutive seconds
        if(countingPrevScreen) {
            prevScreenTimer += Time.deltaTime;
            if(prevScreenTimer > 1.5f) {
                if (isBeforeStageSelect)
                    ReturnToTitle();
                else {
                    phase = CharacterMenuState.TransitionToCharSelectFromStageSelect;
                    prevScreenTimer = 0;
                }
            }
        }
        else
            prevScreenTimer = 0;

        foreach (Image backArrow in backArrows)
            backArrow.fillAmount = prevScreenTimer / 2.3f;

        countingPrevScreen = false;

        if (GameController.Instance.IsOnline)
            sync["location"] = (int)phase;

        switch(phase) {
            case CharacterMenuState.CharSelect: {
                    phase = checkCharacterSelectReadiness() ? CharacterMenuState.AllCharsSelected : phase;
                    break;
                }
            case CharacterMenuState.AllCharsSelected: {
                    phase = !checkCharacterSelectReadiness() ? CharacterMenuState.CharSelect : phase;
                    break;
                }
            case CharacterMenuState.TransitionToStageSelect: {
                    GameController.Instance.DisableJoining();
                    gameObject.transform.position = new Vector2(0, 18);
                    foreach (CharacterSelectState charSelState in charSelStates.Values) {
                        if (charSelState.PeerID < 8) {
                            charSelState.x = 3;
                            charSelState.y = 0;
                            charSelState.Cursor.transform.position = stageRows[0][3].transform.position;
                        }
                        else
                            charSelState.Cursor.transform.position = characterRows[1][2].transform.position;
                    }
                    phase = CharacterMenuState.StageSelect;
                    break;
                }
            case CharacterMenuState.TransitionToCharSelectFromStageSelect: {
                    gameObject.transform.position = Vector2.zero;
                    GameController.Instance.EnableJoining();
                    foreach (CharacterSelectState charSelState in charSelStates.Values) {
                        int[] arr = findDrifterMatrixPosition(charSelState.PlayerType);
                        charSelState.x = arr[1];
                        charSelState.y = arr[0];
                        charSelState.Cursor.transform.position = characterRows[arr[0]][arr[1]].transform.position;
                    }
                    phase = CharacterMenuState.CharSelect;
                    break;
                }
            case CharacterMenuState.StageSelect: {
                    phase = checkStageSelectReadiness() ? CharacterMenuState.AllStagesSelected : phase;
                    break;
                }             
            case CharacterMenuState.AllStagesSelected: {
                    phase = !checkStageSelectReadiness() ? CharacterMenuState.StageSelect : phase;
                    break;
                }

            case CharacterMenuState.GameStart: {
                    List<BattleStage> randomStage = new List<BattleStage>();
                    foreach (CharacterSelectState charSelState in charSelStates.Values) {
                        //Random Character sync
                        if (charSelState.PlayerType == DrifterType.Random)
                            charSelState.PlayerType = (DrifterType)UnityEngine.Random.Range(3, DrifterType.GetValues(typeof(DrifterType)).Length - 1);

                        //Populate stage list
                        if (charSelState.StageType != BattleStage.None && charSelState.PeerID < 8) {
                            //Add a random non none, random, training stage to the list
                            if (charSelState.StageType == BattleStage.Random)
                                randomStage.Add((BattleStage)UnityEngine.Random.Range(4, BattleStage.GetValues(typeof(BattleStage)).Length - 1));
                            else
                                randomStage.Add(charSelState.StageType);
                        }
                    }

                    string selectedStage = randomStage[UnityEngine.Random.Range(0, (randomStage.Count - 1))].ToString();
                    GameController.Instance.selectedStage = selectedStage;

                    // if (GameController.Instance.IsHost && GameController.Instance.IsOnline)
                    // {
                    //     NetworkUtils.GetNetworkData<CharacterSelectSyncData>(sync["charSelState"]).stage = selectedFightzone.sceneName;
                    // }
                    GameController.Instance.BeginMatch();
                    break;
                }
            default:
                break;

        }
    }

    //Circular Array Helper
    private int WrapIndex(int curr, int max) {
        if (curr >= max) 
            return 0;
        else if (curr < 0) 
            return (max - 1);
        else 
            return curr;
    }

    //Updates input commands for a given cursor object
    public bool UpdateInput(CharacterSelectState p_cursor,PlayerInputData input) {
        bool isInStageSelect = (phase != CharacterMenuState.CharSelect && phase != CharacterMenuState.AllCharsSelected && phase != CharacterMenuState.TransitionToStageSelect);
        bool isInCharacterSelect = (phase == CharacterMenuState.CharSelect || phase == CharacterMenuState.AllCharsSelected);
        bool isBeforeStageSelect = (isInCharacterSelect || phase == CharacterMenuState.TransitionToStageSelect);

        //If no previous input yet, populate it
        if (p_cursor.prevInput == null) {
           p_cursor.prevInput = input;
           return false; 
        }

        GameObject[][] matrix = (isInStageSelect) ? stageRows : characterRows;

        //Wrap around the horizontal arrays
        if(p_cursor.prevInput.MoveX ==0 && input.MoveX != 0) {
            p_cursor.x = WrapIndex(p_cursor.x + (int)input.MoveX ,matrix[0].Length);
        
            while(matrix[p_cursor.y][p_cursor.x] == null)
                p_cursor.x = WrapIndex(p_cursor.x + (int)input.MoveX, matrix[0].Length);
        }
        
        //Wrap around the vertical arrays
        //Todo: make hanging edges work
        if(p_cursor.prevInput.MoveY ==0 && input.MoveY != 0) {
            p_cursor.y = WrapIndex(p_cursor.y + (int)input.MoveY ,matrix.Length);

            while(matrix[p_cursor.y][p_cursor.x] == null)
                p_cursor.y = WrapIndex(p_cursor.y + (int)input.MoveY, matrix.Length);
        }
        
        //Sets the cursor's location to that of the current character icon
        p_cursor.Cursor.transform.localPosition = matrix[p_cursor.y][p_cursor.x].transform.position;

        //Select or deselelect on light press
        if (input.Light && !p_cursor.prevInput.Light && isInCharacterSelect) {
            DrifterType selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().drifterType;
            p_cursor.PlayerType = (p_cursor.PlayerType == DrifterType.None || p_cursor.PlayerType != selected)?selected:DrifterType.None;
            playerCards[p_cursor.PeerID].GetComponent<CharacterCard>().SetCharacter(p_cursor.PlayerType);
        }
        else if(input.Light && !p_cursor.prevInput.Light && isInStageSelect) {
            BattleStage selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().StageType;
            p_cursor.StageType = (p_cursor.StageType == BattleStage.None || p_cursor.StageType != selected)?selected:BattleStage.None;
            //This might need some work if it needs to be more flashy
            playerCards[p_cursor.PeerID].GetComponent<CharacterCard>().SetStage(matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().portrait.sprite);
        }
        else if(input.Special && !p_cursor.prevInput.Special && p_cursor.PlayerType != DrifterType.None && isInCharacterSelect)
            p_cursor.PlayerType = DrifterType.None; //Deselect on special press
        else if(input.Special && !p_cursor.prevInput.Special && p_cursor.StageType != BattleStage.None && isBeforeStageSelect && phase != CharacterMenuState.GameStart)
            p_cursor.StageType = BattleStage.None; //Deselect on special press

        //Return to previous screen if special is held
        if(input.Special && p_cursor.prevInput.Special)
            countingPrevScreen = true;

        //Remove this probably
        if(!p_cursor.prevInput.Pause && input.Pause && phase == CharacterMenuState.AllCharsSelected)
            phase = CharacterMenuState.TransitionToStageSelect;
        else if(!p_cursor.prevInput.Pause && input.Pause && phase == CharacterMenuState.AllStagesSelected)
            phase = CharacterMenuState.GameStart;

        //Saves previous input
        p_cursor.prevInput = input;

        return input.Menu && (phase == 0 || phase == CharacterMenuState.AllCharsSelected);
    }

    //Checks to make sure each player has selected a character
    bool checkCharacterSelectReadiness() {
        foreach (CharacterSelectState charSelState in charSelStates.Values) {
            if(charSelState.PlayerType == DrifterType.None) {
                Banner.SetActive(false);
                return false;
            }
        }

        if (charSelStates.Count >= 2 && !Banner.activeInHierarchy)
            Banner.SetActive(true);
        else if (charSelStates.Count < 2 && Banner.activeInHierarchy)
            Banner.SetActive(false);

        return charSelStates.Count >=2;
    }

    //checks if each active player has selected a stage
    bool checkStageSelectReadiness() {
        foreach (CharacterSelectState charSelState in charSelStates.Values) {
            if(charSelState.StageType == BattleStage.None && charSelState.PeerID < 8) {
                Banner.SetActive(false);
                return false;
            }
        }

        if (charSelStates.Count >= 2) {
            Banner.SetActive(true);
            return true;
        }
        return false;
    }

    //Backs out to the tile screen and disconnects clients if their player is host
    public void ReturnToTitle() {
        GameController.Instance.removeAllPeers();

        if (GameController.Instance.GetComponent<NetworkClient>() != null)
            GameController.Instance.CleanupNetwork();
        if (GameController.Instance.GetComponent<NetworkHost>() != null)
            GameController.Instance.CleanupNetwork();
  
        GameController.Instance.Load("MenuScene");
    }

    public DrifterType getDrifterTypeFromString(string name) {
        foreach(DrifterType drifter in Enum.GetValues(typeof(DrifterType))) {
            if(drifter.ToString() == name.Replace(" ", "_"))
                return drifter;
        }
        return DrifterType.None;
    }

    public void ReceiveNetworkMessage(NetworkMessage message) {
        CharacterSelectClientPacket selectCharacter = NetworkUtils.GetNetworkData<CharacterSelectClientPacket>(message.contents);
        if (selectCharacter != null) {
            foreach (CharacterSelectState charSelState in charSelStates.Values) {
                if (charSelState.PeerID == message.peerId)
                    charSelState.PlayerType = selectCharacter.drifter;
            }
        }
    }
}

//Network Functions
public class CharacterSelectSyncData : INetworkData {
    public string Type { get; set; }
    public Dictionary<int,CharacterSelectState> charSelState = new Dictionary<int,CharacterSelectState>();
    public string stage;
}

public class CharacterSelectClientPacket : INetworkData {
    public string Type { get; set; }
    public DrifterType drifter { get; set; }
}