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

	//public int rollbackFrames = 10;

	//Dictionary<int, GameObject> clientPlayers = new Dictionary<int, GameObject>();

	[NonSerialized]
	public Dictionary<int, GameObject> players = new Dictionary<int, GameObject>();

	public static NetworkPlayers Instance => GameObject.FindGameObjectWithTag("NetworkPlayers")?.GetComponent<NetworkPlayers>();

	// Start is called before the first frame update
	void Start() {
		stage = GameController.Instance.CreatePrefab(GameController.Instance.selectedStage,0);

		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>().getParalax();

		//populate spawn points
		spawnPoints = new List<GameObject>();
		for(int i = 0; i <4; i++)
			spawnPoints.Add(GameObject.Find("SpawnPoint" + i));

		//syncFromClients = GetComponent<NetworkSyncToHost>();

		// create players
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates.Values)
			CreatePlayer(charSel.PeerID);


		if(GameController.Instance.IsTraining) 
			playerUnlockFrame = 1;
			
		//rollbackTest = new DrifterRollbackFrame[rollbackFrames,2];

		//br.makeLobby();//InitializeRollbackSession();
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
	public bool Dash;
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
			Dash = Dash,
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
			Grab == other.Grab &&
			Dash == other.Dash
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
			Grab == false &&
			Dash == false
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
			(Grab		? "1":"0")+ "," +
			(Dash		? "1":"0");

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
			Grab		= buttons[7].Equals("1"),
			Dash		= buttons[8].Equals("1")
		};
	}


	public void Serialize(BinaryWriter bw){
		bw.Write(MoveX);
		bw.Write(MoveY);
		bw.Write(Jump);
		bw.Write(Light);
		bw.Write(Special);
		bw.Write(Super);
		bw.Write(Guard);
		bw.Write(Grab);
		bw.Write(Dash);
		bw.Write(Pause);
		bw.Write(Menu);
	}

	 public void Deserialize(BinaryReader br) {  
		MoveX = br.ReadInt32();
		MoveY = br.ReadInt32();
		Jump = br.ReadBoolean();
		Light = br.ReadBoolean();
		Special = br.ReadBoolean();
		Super = br.ReadBoolean();
		Guard = br.ReadBoolean();
		Grab = br.ReadBoolean();
		Dash = br.ReadBoolean();
		Pause = br.ReadBoolean();
		Menu = br.ReadBoolean();	
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
		Dash = data.Dash;
		Pause = data.Pause;
		Grab = data.Grab;
		Menu = data.Menu;
	}
}
