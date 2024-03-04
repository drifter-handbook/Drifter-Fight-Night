using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityGGPO;

public class NetworkPlayers : MonoBehaviour
{

	NetworkSyncToHost syncFromClients;

	public List<GameObject> spawnPoints;

	public GameObject playerInputPrefab;

	public GameObject stage;

	public int rollbackFrames = 10;

	DrifterRollbackFrame[,] rollbackTest;

	//Dictionary<int, GameObject> clientPlayers = new Dictionary<int, GameObject>();

	[NonSerialized]
	public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

	public static NetworkPlayers Instance => GameObject.FindGameObjectWithTag("NetworkPlayers")?.GetComponent<NetworkPlayers>();

	// Start is called before the first frame update
	void Start() {
		stage = GameController.Instance.CreatePrefab(GameController.Instance.selectedStage);
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>().getParalax();

		//populate spawn points
		spawnPoints = new List<GameObject>();
		for(int i = 0; i <4; i++)
			spawnPoints.Add(GameObject.Find("SpawnPoint" + i));

		//syncFromClients = GetComponent<NetworkSyncToHost>();

		// create players
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values)
			CreatePlayer(charSel.PeerID);


		rollbackTest = new DrifterRollbackFrame[rollbackFrames,2];

		//br.makeLobby();//InitializeRollbackSession();
	}

	// Update is called once per frame
	void FixedUpdate() {

		Physics2D.Simulate(1f/60f);

		// PlayerInputData input;

		int q = 0;

		DrifterRollbackFrame[] rollback2 = new DrifterRollbackFrame[CharacterMenu.charSelStates.Values.Count];

		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values) {
			//Link inputs to peer ids
			// input = NetworkUtils.GetNetworkData<PlayerInputData>(syncFromClients["input", charSel.PeerID]);
			// if (input != null)
			// 	rollback2[q] = UpdateInput(players[charSel.PeerID], input);
			// else 
			if(GameController.Instance.controls.ContainsKey(charSel.PeerID))
				 rollback2[q] = UpdateInput(players[charSel.PeerID], GetInput(GameController.Instance.controls[charSel.PeerID]));
				
			else
				 rollback2[q] = UpdateInput(players[charSel.PeerID]);

			q++;

		}

		for (int i = rollbackFrames -2; i >= 0; i--) {
			rollbackTest[i + 1,0] = rollbackTest[i,0];
			rollbackTest[i + 1,1] = rollbackTest[i,1];

		}

		rollbackTest[0,0] = rollback2[0];
		rollbackTest[0,1] = rollback2[1];

		//Physics2D.Simulate(1f/60f);
	}

	GameObject CreatePlayer(int peerID) {
		DrifterType drifter = DrifterType.None;
		foreach (CharacterSelectState state in CharacterMenu.charSelStates.Values) {
			if (state.PeerID == peerID)
				drifter = state.PlayerType;
		}

		//Same here
		GameObject obj = GameController.Instance.CreatePrefab(drifter.ToString().Replace("_", " "),
			spawnPoints[(peerID +1) % spawnPoints.Count].transform.position, Quaternion.identity);
		obj.GetComponent<Drifter>().SetColor((peerID +1));

		if(GameController.Instance.controls.ContainsKey(peerID))obj.GetComponent<Drifter>().playerInputController = GameController.Instance.controls[peerID];
		obj.GetComponent<Drifter>().SetPeerId(peerID);
		obj.GetComponent<PlayerMovement>().setFacing((peerID+1 % 2) * 2 - 1);
		players[peerID] = obj;
		return obj;
	}

	public DrifterRollbackFrame UpdateInput(GameObject player, PlayerInputData input, bool updateDummy = false) {
		if (player == null)
			return null;

		Drifter playerDrifter = player.GetComponent<Drifter>();

		if(playerDrifter.isTrainingDummy() && !updateDummy) {
			playerDrifter.input[0] = input;
			return null;
		}

		//UnityEngine.Debug.Log(playerDrifter);

		for (int i = player.GetComponent<Drifter>().input.Length - 2; i >= 0; i--)
			playerDrifter.input[i + 1] = (PlayerInputData)playerDrifter.input[i].Clone();

		playerDrifter.input[0] = input;
		playerDrifter.UpdateFrame();

		return playerDrifter.SerializeFrame();

	}

	//If no input is recieved, assume a player kept doing what they were doing last frame
	public DrifterRollbackFrame UpdateInput(GameObject player, bool updateDummy = false) {
		return UpdateInput(player, player.GetComponent<Drifter>().input[0], updateDummy);
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

		input.Pause = playerInputAction.FindAction("Start").ReadValue<float>()>0;

		input.Menu = playerInputAction.FindAction("Menu").ReadValue<float>()>0;

		return input;
	}

	public void rollemback() {
		rollemback(rollbackFrames);

	}

	public void rollemback(int frames){
		int z = 0;
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values){
		
			players[charSel.PeerID].GetComponent<Drifter>().DeserializeFrame(rollbackTest[frames -1,z]);
			rollbackTest[0,z] = rollbackTest[frames -1,z];
			z++;
		}
	}

	
}


[Serializable]
public class PlayerInputData :INetworkData, ICloneable, IEquatable<PlayerInputData> {
	public string Type { get; set; }
	public int MoveX;
	public int MoveY;
	public bool Jump;
	public bool Light;
	public bool Special;
	public bool Super;
	public bool Guard;
	public bool Pause;
	public bool Grab;
	public bool Menu;

	public object Clone() {
		return new PlayerInputData() {
			Type = Type,
			MoveX = MoveX,
			MoveY = MoveY,
			Jump = Jump,
			Light = Light,
			Special = Special,
			Super = Super,
			Guard = Guard,
			Pause = Pause,
			Grab = Grab,
			Menu = Menu,
		};
	}

	public bool Equals(PlayerInputData other){
		if(other == null) return false;
		if( ReferenceEquals(this, other)) return true;

		return(
			MoveX == other.MoveX &&
			MoveY == other.MoveY && 
			Jump == other.Jump &&
			Light == other.Light &&
			Special == other.Special &&
			Super == other.Super &&
			Guard == other.Guard &&
			Grab == other.Grab 
			);
	   
	}

	public bool isEmpty(){
		return(
			MoveX == 0 &&
			MoveY == 0 && 
			Jump == false &&
			Light == false &&
			Special == false &&
			Super == false &&
			Guard == false &&
			Grab == false 
			);
	}

	public override String ToString(){
		return 
			MoveX 					+ "," +
			MoveY 					+ "," + 
			(Jump		? "1":"0") 	+ "," +
			(Light		? "1":"0") 	+ "," +
			(Special	? "1":"0") 	+ "," +
			(Super		? "1":"0") 	+ "," +
			(Guard		? "1":"0") 	+ "," +
			(Grab		? "1":"0");

	}

	public static PlayerInputData FromString(String data){
		string[] buttons = data.Split(',');
		if(buttons.Length <8) return new PlayerInputData();

		return new PlayerInputData{
			MoveX 		= Int32.Parse(buttons[0]),
			MoveY 		= Int32.Parse(buttons[1]),
			Jump		= buttons[2].Equals("1"),	
			Light		= buttons[3].Equals("1"),
			Special		= buttons[4].Equals("1"),
			Super		= buttons[5].Equals("1"),
			Guard		= buttons[6].Equals("1"),
			Grab		= buttons[7].Equals("1")
		};
	}

	public void CopyFrom(PlayerInputData data) {
		Type = data.Type;
		MoveX = data.MoveX;
		MoveY = data.MoveY;
		Jump = data.Jump;
		Light = data.Light;
		Special = data.Special;
		Super = data.Super;
		Guard = data.Guard;
		Pause = data.Pause;
		Grab = data.Grab;
		Menu = data.Menu;
	}
}
