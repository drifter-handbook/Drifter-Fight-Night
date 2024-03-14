using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

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
public class CharacterMenu : MonoBehaviour {
	//-------------------------------------------------------------
	// START OF ITEMS ACCESSIBLE FROM SCENE COMPONENT
	//-------------------------------------------------------------
	public GameObject roomCode;

	//Character Matrix
	public GameObject[] topRow;
	public GameObject[] middleRow;
	public GameObject[] bottomRow;
	static GameObject[][] characterRows = new GameObject[3][];

	//Stage Matrix
	public GameObject[] topStageRow;
	public GameObject[] middleStageRow;
	public GameObject[] bottomStageRow;
	static GameObject[][] stageRows = new GameObject[3][];

	public GameObject playerInputPrefab;

	public GameObject Banner;

	public Image[] BackArrows;

	const int PEER_REMOVAL_TIME = 30;
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


	static int prevScreenTimer = 0;
	static bool countingPrevScreen = false;
	public CharacterMenuState phase = CharacterMenuState.CharSelect;
	
	public static CharacterSelectState[] charSelStates;

	Dictionary<int,GameObject> playerCards = new Dictionary<int,GameObject>();
	public static CharacterMenu Instance { get; private set; }
	
	void Awake() {
		if (Instance != null && Instance != this) 
			Destroy(gameObject);
		else 
			Instance = this;
	}

	void Start() {

		characterRows[0] = topRow;
		characterRows[1] = middleRow;
		characterRows[2] = bottomRow;
		stageRows[0] = topStageRow;
		stageRows[1] = middleStageRow;
		stageRows[2] = bottomStageRow;

		//Peer ID 9 is always the training dummy
		//charSelStates = new CharacterSelectState[10];

		initializeCharacterSelect();
	}

	//Deprecate this
	public Dictionary<int, int> GetPeerIDsToPlayerIDs() {
		Dictionary<int, int> peerIDsToPlayerIDs = new Dictionary<int, int>();

		foreach (CharacterSelectState charSelState in charSelStates)
			peerIDsToPlayerIDs[charSelState.PeerID] = charSelState.PeerID;

		return peerIDsToPlayerIDs;
	}

	public void AddCharSelState(int peerID) {
		AddCharSelState(peerID,DrifterType.None);
	}

	public void initializeCharacterSelect() {

		if(charSelStates == null){
			UnityEngine.Debug.Log("RESET");
			charSelStates = new CharacterSelectState[10];
			GameController.Instance.Peers =  new List<int>();
			GameController.Instance.removeAllUIPeers();
		}  
		else{
			for(int i = 0 ;i < charSelStates.Length; i++)
				if(charSelStates[i] != null)
					ConfigureCharSelState(charSelStates[i].PeerID,charSelStates[i].PlayerType);
		}

		GameController.Instance.EnableJoining();
	}

	public void setCharacterSelectPhase(CharacterMenuState p_phase){
		phase = p_phase;
	}

	public void ResetCharacterSelect() {
		foreach(int peerID in GameController.Instance.Peers)
			RemoveCharSelState(peerID);
		charSelStates = null;
	}

	public void ConfigureCharSelState(int peerID, DrifterType drifter){
		int[] drifterLoc = findDrifterMatrixPosition(drifter);
		GameObject cursor = GameController.Instance.CreatePrefab("CharacterCursor",characterRows[drifterLoc[0]][drifterLoc[1]].transform.position, transform.rotation);
		cursor.GetComponent<SpriteRenderer>().color = ColorFromEnum[(PlayerColor)peerID];

		GameObject card = GameController.Instance.CreatePrefab("CharacterSelectCard",new Vector2(-20 + 13.5f * (peerID % 4),-9), transform.rotation);

		charSelStates[peerID] = new CharacterSelectState(){
			PeerID = peerID,
			Cursor = cursor,
			PlayerType = drifter,
			x = drifterLoc[1],
			y = drifterLoc[0],
			removalTimer = PEER_REMOVAL_TIME,
			StageType = BattleStage.None,
			GameStandings = -1,
		};

		card.transform.SetParent(gameObject.transform , false);
		card.GetComponent<CharacterCard>().SetCharacter(drifter);
		playerCards.Add(peerID,card);
	}

	public void AddCharSelState(int peerID, DrifterType drifter) {
		int maxPlayer = GameController.Instance.maxPlayerCount;
		bool training = GameController.Instance.IsTraining;

		if (charSelStates[peerID] != null)
			return;

		ConfigureCharSelState(peerID,drifter);

		GameController.Instance.Peers.Add(peerID);
	}

	public void RemoveCharSelState(int peerID) {
		if(charSelStates[peerID] == null) {
			UnityEngine.Debug.Log("PEER NOT FOUND FOR REMOVAL: " + peerID);
			return;
		}
	
		UnityEngine.Debug.Log("PEER REMOVED");
		Destroy(charSelStates[peerID].Cursor);
		charSelStates[peerID] = null;
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

	public void UpdateFrame(PlayerInputData[] inputs) {

		bool isBeforeStageSelect = (phase == CharacterMenuState.CharSelect || phase == CharacterMenuState.AllCharsSelected || phase == CharacterMenuState.TransitionToStageSelect);

		UpdateInput(inputs);

		//Return to title if the special button is helf for 1.5 consecutive seconds
		if(countingPrevScreen) {
			prevScreenTimer++;
			if(prevScreenTimer > 90) {
				if (isBeforeStageSelect)
					GameController.Instance.GoToMainMenu();
				else {
					phase = CharacterMenuState.TransitionToCharSelectFromStageSelect;
					prevScreenTimer = 0;
				}
			}
		}
		else
			prevScreenTimer = 0;

		foreach (Image backArrow in BackArrows)
			backArrow.fillAmount = prevScreenTimer / (2.3f * 60f);

		countingPrevScreen = false;

		//State Machine for handling transitions between phases of character selection
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
					foreach (CharacterSelectState charSelState in charSelStates) {
						if(charSelState == null) continue;
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
					foreach (CharacterSelectState charSelState in charSelStates) {
						if(charSelState == null) continue;
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
				foreach (CharacterSelectState charSelState in charSelStates) {
					if(charSelState == null) continue;
					UnityEngine.Debug.Log(charSelState);
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

				GameController.Instance.selectedStage = randomStage[UnityEngine.Random.Range(0,(randomStage.Count -1))];

				GameController.Instance.BeginMatch();
				phase = CharacterMenuState.CharSelect;
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
	void UpdateInput(PlayerInputData[] input) {
		bool isInStageSelect = (phase != CharacterMenuState.CharSelect && phase != CharacterMenuState.AllCharsSelected && phase != CharacterMenuState.TransitionToStageSelect);
		bool isInCharacterSelect = (phase == CharacterMenuState.CharSelect || phase == CharacterMenuState.AllCharsSelected);
		bool isBeforeStageSelect = (isInCharacterSelect || phase == CharacterMenuState.TransitionToStageSelect);

		if(charSelStates == null) return;

		for(int j = 0; j < charSelStates.Length; j++){
			CharacterSelectState p_cursor = charSelStates[j];

			//Skip empty states and the dummy
			if(p_cursor == null || j == 9) continue;

			//If no previous input yet, populate it
			if (p_cursor.prevInput == null) {
			   p_cursor.prevInput = input[j];
			   return; 
			}

			GameObject[][] matrix = (isInStageSelect) ? stageRows : characterRows;

			//Wrap around the horizontal arrays
			if(p_cursor.prevInput.MoveX ==0 && input[j].MoveX != 0) {
				p_cursor.x = WrapIndex(p_cursor.x + (int)input[j].MoveX ,matrix[0].Length);
			
				while(matrix[p_cursor.y][p_cursor.x] == null)
					p_cursor.x = WrapIndex(p_cursor.x + (int)input[j].MoveX, matrix[0].Length);
			}
			
			//Wrap around the vertical arrays
			//Todo: make hanging edges work
			if(p_cursor.prevInput.MoveY ==0 && input[j].MoveY != 0) {
				p_cursor.y = WrapIndex(p_cursor.y + (int)input[j].MoveY ,matrix.Length);

				while(matrix[p_cursor.y][p_cursor.x] == null)
					p_cursor.y = WrapIndex(p_cursor.y + (int)input[j].MoveY, matrix.Length);
			}
			
			//Sets the cursor's location to that of the current character icon
			if(p_cursor.Cursor != null) p_cursor.Cursor.transform.localPosition = matrix[p_cursor.y][p_cursor.x].transform.position;

			//Select or deselelect on light press
			if (input[j].Light && !p_cursor.prevInput.Light && isInCharacterSelect) {
				DrifterType selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().drifterType;
				p_cursor.PlayerType = (p_cursor.PlayerType == DrifterType.None || p_cursor.PlayerType != selected)?selected:DrifterType.None;
				playerCards[p_cursor.PeerID].GetComponent<CharacterCard>().SetCharacter(p_cursor.PlayerType);
			}
			else if(input[j].Light && !p_cursor.prevInput.Light && isInStageSelect) {
				BattleStage selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().StageType;
				p_cursor.StageType = (p_cursor.StageType == BattleStage.None || p_cursor.StageType != selected)?selected:BattleStage.None;
				//This might need some work if it needs to be more flashy
				playerCards[p_cursor.PeerID].GetComponent<CharacterCard>().SetStage(matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().portrait.sprite);
			}
			//Select dummy character on super press if n training mode
			else if(GameController.Instance.IsTraining && input[j].Super && !p_cursor.prevInput.Super && isInCharacterSelect) {
				DrifterType selected = matrix[p_cursor.y][p_cursor.x].GetComponent<CharacterSelectPortrait>().drifterType;
				charSelStates[9].PlayerType = selected;
				playerCards[9].GetComponent<CharacterCard>().SetCharacter(charSelStates[9].PlayerType);
				charSelStates[9].x = p_cursor.x;
				charSelStates[9].y = p_cursor.y;
				charSelStates[9].Cursor.transform.position = characterRows[p_cursor.y][p_cursor.x].transform.position;
			}
			
			//Deselect on special press
			else if(input[j].Special && !p_cursor.prevInput.Special && p_cursor.PlayerType != DrifterType.None && isInStageSelect)
				p_cursor.PlayerType = DrifterType.None;

			//Return to previous screen if special is held
			if(input[j].Special && p_cursor.prevInput.Special)
				countingPrevScreen = true;

			//Remove this probably
			if(!p_cursor.prevInput.Pause && input[j].Pause && phase == CharacterMenuState.AllCharsSelected)
				phase = CharacterMenuState.TransitionToStageSelect;
			else if(!p_cursor.prevInput.Pause && input[j].Pause && phase == CharacterMenuState.AllStagesSelected)
				phase = CharacterMenuState.GameStart;

			//Saves previous input
			p_cursor.prevInput = input[j];

			if(input[j].Menu && (phase == 0 || phase == CharacterMenuState.AllCharsSelected)){
				p_cursor.removalTimer++;
				if(p_cursor.removalTimer == PEER_REMOVAL_TIME)
					GameController.Instance.removeUserByPeer(j);
			}
			else
				p_cursor.removalTimer = 0;
		}
	}

	//Checks to make sure each player has selected a character
	bool checkCharacterSelectReadiness() {
		int playersReady = 0;
		if(charSelStates == null) return false;
		foreach (CharacterSelectState charSelState in charSelStates) {
			if(charSelState == null) continue;
			if(charSelState != null && charSelState.PlayerType != DrifterType.None) 
				playersReady++;
		}
			
		
		if (playersReady >= 2 && !Banner.activeInHierarchy)
			Banner.SetActive(true);
		else if (playersReady < 2 && Banner.activeInHierarchy)
			Banner.SetActive(false);

		return playersReady >=2;
	}

	//checks if each active player has selected a stage
	bool checkStageSelectReadiness() {
		int playersReady = 0;
		foreach (CharacterSelectState charSelState in charSelStates) {
			if(charSelState == null) continue;
			if(charSelState.StageType != BattleStage.None || charSelState.PeerID >= 8) 
				playersReady++;
		}

		if (playersReady >= 2 && !Banner.activeInHierarchy)
			Banner.SetActive(true);
		else if (playersReady < 2 && Banner.activeInHierarchy)
			Banner.SetActive(false);

		return playersReady >=2;
	}

	public DrifterType getDrifterTypeFromString(string name) {
		foreach(DrifterType drifter in Enum.GetValues(typeof(DrifterType))) {
			if(drifter.ToString() == name.Replace(" ", "_"))
				return drifter;
		}
		return DrifterType.None;
	}


	public void Serialize(BinaryWriter bw) {

		bw.Write(countingPrevScreen);
		bw.Write(prevScreenTimer);
		bw.Write((int)phase);
	
		for(int i = 0; i < charSelStates.Length; i++){
			if(charSelStates[i] !=  null){
				bw.Write(true);
				charSelStates[i].Serialize(bw);
			}
		}
	}

	public void Deserialize(BinaryReader br) {

		countingPrevScreen = br.ReadBoolean();
		prevScreenTimer = br.ReadInt32();
		phase = (CharacterMenuState)br.ReadInt32();
	
		for(int i = 0; i < charSelStates.Length; i++){
			if(br.ReadBoolean()){
				if(charSelStates[i] ==  null) AddCharSelState(i,DrifterType.None);
				charSelStates[i].Deserialize(br);
			}
			else if(charSelStates[i] !=  null) RemoveCharSelState(i);
		}
	}
}