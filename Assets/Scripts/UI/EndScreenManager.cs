using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.IO;

public enum EndgameVote{
	EMPTY_SLOT,
	NO_SELECTION,
	PLAY_AGAIN,
	CHARACTER_SELECT,
	STAGE_SELECT,
	DISCONNECT
}

public class EndScreenManager : UIMenuManager {
	
	public Sprite[] sprites;
	public GameObject[] winnerSetups;
	public GameObject playAgainButton;

	public GameObject[] buttons;

	EndgameVote[] EndgameVotes;

	public static EndScreenManager Instance { get; private set; }

	void Awake(){
		if (Instance != null && Instance != this) 
			Destroy(gameObject);
		else 
			Instance = this;
	}

	void Start() {

		
		if(EndgameVotes == null) SetUpVote();
		//Set input to UI action map (was at gameplay action map until reaching this screen) and set initial selected game object.
		//GameController.Instance.toggleInputSystem(true);
		
		//EventSystem.current.SetSelectedGameObject(GameObject.Find("MainMenu"));

		//NOTE: used to have some isHost and sendNetworkMessage logic here (see mouse removal revision for commented out code if necessary)

		if ( GameController.Instance.winnerOrder.Length == 0) UnityEngine.Debug.Log("NO CONTEST");
		for(int i = 0; i< GameController.Instance.winnerOrder.Length; i++) {

			UnityEngine.Debug.Log("Player " + (i + 1) + " came in " + GameController.Instance.winnerOrder[i] + "th place!");

			//Todo Cleanup
			if(GameController.Instance.winnerOrder[i] == 1)
				foreach (CharacterSelectState state in CharacterMenu.charSelStates)
					if (state.PeerID == (i - 1))
						setWinnerPic(state.PlayerType,CharacterMenu.ColorFromEnum[(PlayerColor)(state.PeerID+1)]);
		}
	}

	public void UpdateFrame(PlayerInputData[] inputs) {

		if(EndgameVotes == null) SetUpVote();

		foreach(CharacterSelectState css in CharacterMenu.charSelStates){
			if(css == null || css.PeerID > 8) break;
			if(css.prevInput.MoveX == 0 && inputs[css.PeerID].MoveX != 0){
				css.x += inputs[css.PeerID].MoveX;
				if(css.x < 0) css.x = buttons.Length - 1;
				if(css.x >= buttons.Length) css.x = 0;	
			}
			css.Cursor.transform.localPosition = buttons[css.x].transform.position;

			if ((inputs[css.PeerID].Light && !css.prevInput.Light) ||
				(inputs[css.PeerID].Pause && !css.prevInput.Pause)){ 
				//Probably grab the button type from the button instead?
				EndgameVotes[css.PeerID] = (EndgameVote)(2 + css.x);
				UnityEngine.Debug.Log(EndgameVotes[css.PeerID] );
			}
			css.prevInput = inputs[css.PeerID];
		}
		GoToNextScreen();

	}

	void SetUpVote() {
		EndgameVotes = new EndgameVote[CharacterMenu.charSelStates.Length];

		foreach(CharacterSelectState css in CharacterMenu.charSelStates){
			if(css != null && css.PeerID < 8){
				css.Cursor =  GameController.Instance.CreatePrefab("CharacterCursor",buttons[0].transform.position, transform.rotation);
				css.x = 0;
				css.Cursor.GetComponent<SpriteRenderer>().color = CharacterMenu.ColorFromEnum[(PlayerColor)css.PeerID];
				EndgameVotes[css.PeerID] = EndgameVote.NO_SELECTION;
			}
		}
	}

	void GoToNextScreen(){
		bool returnToCharacterSelect = false;

		foreach(EndgameVote vote in EndgameVotes){
			switch(vote){
				case EndgameVote.NO_SELECTION:
				//A player has not made a selection yet, so no action will be taken yet
					return;
				case EndgameVote.DISCONNECT:
				//Disconnect a player from the game who is done playing, then set their slot to empty when theyre gone
					backToMain(); //Replace this with actual DC code
					return;
					//break;
				case EndgameVote.STAGE_SELECT:
					UnityEngine.Debug.Log("IMPLEMENT ME");
					returnToCharacterSelect = true;
					break;
				case EndgameVote.CHARACTER_SELECT:
				//A player wants to change characters or stages
					returnToCharacterSelect = true;
					break;
				case EndgameVote.PLAY_AGAIN:
				//Player is ready to play again
				case EndgameVote.EMPTY_SLOT:
				//Ignore empty CSS slots
				default:
					break;
			}
		}

		if(returnToCharacterSelect) GameController.Instance.GoToCharacterSelect();
		else GameController.Instance.BeginMatch();
		EndgameVotes = null;
	}

	public void setWinnerPic(DrifterType type,Color color) {
		foreach(GameObject setup in winnerSetups) {
			if (setup.name.Contains(type.ToString())) {
				setup.SetActive(true);
				setup.transform.GetChild(0).GetComponent<Text>().color = color; //sets player Color on shadow text
			} 
			else {
				setup.SetActive(false);
			}
		}
	}
	void DisconnectPlayerFromGame(int PeerID){
		//Disconnect user from GGPO game here
		GameController.Instance.removeUserByPeer(PeerID);
		GameController.Instance.GoToMainMenu();
	}

	public void backToMain() {
		//Disconnect associated peer immediately
		EndgameVotes = null;
		GameController.Instance.GoToMainMenu();
	}

	public override void Exit() {
		//Ragequit button lmao
		GameController.Instance.GoToMainMenu();
	}

	public void Serialize(BinaryWriter bw) {

		// bw.Write(countingPrevScreen);
		// bw.Write(prevScreenTimer);
		// bw.Write((int)phase);

		// if(EndgameVotes != null){
		// 	bw.Write(true);
		// 	foreach(EndgameVote vote in EndgameVotes)
		// 		bw.Write((int)vote);
		// }
		// else
		// 	bw.Write(false);
		
		
		// for(int i = 0; i < CharacterMenu.charSelStates.Length; i++){
		// 	if(CharacterMenu.charSelStates[i] !=  null){
		// 		bw.Write(true);
		// 		CharacterMenu.charSelStates[i].Serialize(bw);
		// 	}
		// }
	}

	public void Deserialize(BinaryReader br) {

		// // countingPrevScreen = br.ReadBoolean();
		// // prevScreenTimer = br.ReadInt32();
		// // phase = (CharacterMenuState)br.ReadInt32();
		// if(br.ReadBoolean()){

		// }

	
		// for(int i = 0; i < CharacterMenu.charSelStates.Length; i++){
		// 	if(br.ReadBoolean()){
		// 		if(CharacterMenu.charSelStates[i] ==  null) CharacterMenu.Instance.AddCharSelState(i,DrifterType.None);
		// 		CharacterMenu.charSelStates[i].Deserialize(br);
		// 	}
		// 	else if(CharacterMenu.charSelStates[i] !=  null) CharacterMenu.Instance.RemoveCharSelState(i);
		// }
	}
}
