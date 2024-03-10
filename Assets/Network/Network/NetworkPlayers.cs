using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

//using UnityGGPO;

public class NetworkPlayers : MonoBehaviour
{
	public List<GameObject> spawnPoints;

	public GameObject playerInputPrefab;

	public GameObject stage;

	public int playerUnlockFrame = 111;


	[NonSerialized]
	public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

	public static NetworkPlayers Instance => GameObject.FindGameObjectWithTag("NetworkPlayers")?.GetComponent<NetworkPlayers>();

	// Start is called before the first frame update
	void Start() {
		
		CreateStage(GameController.Instance.selectedStage);
		//syncFromClients = GetComponent<NetworkSyncToHost>();

		// create players
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values)
			CreatePlayer(charSel.PeerID);


		if(GameController.Instance.IsTraining) 
			playerUnlockFrame = 1;
	}

	// Update is called once per frame
	void FixedUpdate() {
		AdvanceFrame();
		if(playerUnlockFrame >0){
			playerUnlockFrame--;
			if(playerUnlockFrame <= 0)
				unlockPlayers();
		}
		
	}

	public void AdvanceFrame(){
		Physics2D.Simulate(1f/60f);

		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values) {
			if(GameController.Instance.controls.ContainsKey(charSel.PeerID))
				 UpdateInput(players[charSel.PeerID], GetInput(GameController.Instance.controls[charSel.PeerID]));
			else
				 UpdateInput(players[charSel.PeerID]);
		}
	}

	void CreateStage(BattleStage stageName) {

		if(stage != null) Destroy(stage);
		stage = GameController.Instance.CreatePrefab(stageName.ToString(), 0);
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>().getParalax();

		//populate spawn points
		spawnPoints = new List<GameObject>();
		for(int i = 0; i <4; i++)
			spawnPoints.Add(GameObject.Find("SpawnPoint" + i));

	}

	GameObject CreatePlayer(int peerID) {
		DrifterType drifter = DrifterType.None;
		foreach (CharacterSelectState state in CharacterMenu.charSelStates.Values) {
			if (state.PeerID == peerID)
				drifter = state.PlayerType;
		}

		//Same here
		GameObject obj = GameController.Instance.CreatePrefab(drifter.ToString().Replace("_", " "),
			spawnPoints[(peerID +1) % spawnPoints.Count].transform.position, Quaternion.identity, peerID);
		obj.GetComponent<Drifter>().SetColor((peerID +1));

		if(GameController.Instance.controls.ContainsKey(peerID))obj.GetComponent<Drifter>().playerInputController = GameController.Instance.controls[peerID];
		obj.GetComponent<Drifter>().SetPeerId(peerID);
		obj.GetComponent<PlayerMovement>().setFacing(-1 *((peerID+1 % 2) * 2 - 1));
		players[peerID] = obj;
		return obj;
	}

	public void unlockPlayers(){
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values) {
			if(GameController.Instance.controls.ContainsKey(charSel.PeerID))
				 players[charSel.PeerID].GetComponent<Drifter>().setTrainingDummy(false);
		}
	}




	public void UpdateInput(GameObject player, PlayerInputData input) {
		if (player == null)
			return;

		Drifter playerDrifter = player.GetComponent<Drifter>();

		//UnityEngine.Debug.Log(playerDrifter);

		for (int i = player.GetComponent<Drifter>().input.Length - 2; i >= 0; i--)
			playerDrifter.input[i + 1] = (PlayerInputData)playerDrifter.input[i].Clone();

		playerDrifter.input[0] = input;

		if(!playerDrifter.isTrainingDummy()) playerDrifter.UpdateFrame();

		
		//return playerDrifter.SerializeFrame();

	}

	//If no input is recieved, assume a player kept doing what they were doing last frame
	public void UpdateInput(GameObject player) {
		UpdateInput(player, player.GetComponent<Drifter>().input[0]);
	}

	public static PlayerInputData GetInput(PlayerInput playerInput) {
		InputActionMap playerInputAction = playerInput.currentActionMap;
		PlayerInputData input = new PlayerInputData();

		// get player input
		input.Jump = playerInputAction.FindAction("Jump").ReadValue<float>() > 0 || playerInputAction.FindAction("Jump Alt").ReadValue<float>() > 0;
		input.Light = playerInputAction.FindAction("Light").ReadValue<float>() > 0;
		input.Special = playerInputAction.FindAction("Special").ReadValue<float>() > 0;
		input.Super = playerInputAction.FindAction("Super").ReadValue<float>() > 0;
		input.Guard = playerInputAction.FindAction("Guard 1").ReadValue<float>() > 0;
		input.MoveX = (int)playerInputAction.FindAction("Horizontal").ReadValue<float>();
		input.MoveY = (int)playerInputAction.FindAction("Vertical").ReadValue<float>();
		input.Grab = playerInputAction.FindAction("Grab").ReadValue<float>() > 0;
		input.Dash = playerInputAction.FindAction("Dash").ReadValue<float>() > 0;

		input.Pause = playerInputAction.FindAction("Start").ReadValue<float>()>0;

		input.Menu = playerInputAction.FindAction("Menu").ReadValue<float>()>0;

		return input;
	}	
}