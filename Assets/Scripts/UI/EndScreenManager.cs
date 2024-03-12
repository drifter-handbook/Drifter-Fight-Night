using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class EndScreenManager : UIMenuManager {
	
	public Sprite[] sprites;
	public GameObject[] winnerSetups;
	public GameObject playAgainButton;

	public static EndScreenManager Instance { get; private set; }

	void Awake(){
		if (Instance != null && Instance != this) 
			Destroy(gameObject);
		else 
			Instance = this;
	}

	void Start() {

		//Set input to UI action map (was at gameplay action map until reaching this screen) and set initial selected game object.
		GameController.Instance.toggleInputSystem(true);
		
		EventSystem.current.SetSelectedGameObject(GameObject.Find("MainMenu"));

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
		//TODO
		//Each player present needs to be able to act in this screen to vote on the next action

		//call to base class.
		PlayerInput[] playerInputs = FindObjectsOfType<PlayerInput>();
		foreach (PlayerInput playerInput in playerInputs) {
			UpdateActivePlayerInputs(playerInput);
		}

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
	public void backToMain() {
		//Disconnect associated peer immediately
		GameController.Instance.GoToMainMenu();
	}

	public void playAgain() {
		//Vote to play again with curetn characters here
		GameController.Instance.BeginMatch();
		//NOTE: used to have some network cleanup and start client calls here (see mouse removal revision for commented out code if necessary)
	}

	public void returnToCharacterSelect() {
		//Return to character select with all players who stayed
		GameController.Instance.GoToCharacterSelect();
		//NOTE: used to have some network cleanup and start client calls here (see mouse removal revision for commented out code if necessary)
	}

	public override void Exit() {
		//Ragequit button lmao
		GameController.Instance.GoToMainMenu();
	}
}
