// https://forum.unity.com/threads/help-how-do-you-set-up-a-gamemanager.131170/
// https://wiki.unity3d.com/index.php/Toolbox

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Assertions;
using UnityEngine.InputSystem.Users;
using UnityEngine.InputSystem;
using GameAnalyticsSDK;
using UnityEngine.EventSystems;
using System.IO;



public enum ControlGroup
{
	UI,
	Controls
}


[DisallowMultipleComponent]

public class GameController : MonoBehaviour
{
	public enum VolumeType
	{
		MASTER,
		MUSIC,
		SFX
	}

	public enum GameState
	{
		MENU,
		CHARACTER_SELECT,
		COMBAT,
		ENDSCREEN
	}

	public float[] volume = { -1f, -1f, -1f };

	//* Serialized members
	[Header("Check box if hosting")]

	public bool IsTraining;
	public BattleStage selectedStage;
	public int[] winnerOrder;

	public GameState gameState = GameState.CHARACTER_SELECT; 

	bool clearingPeers = false;

	public bool canPause = false;
	private bool _IsPaused = false;
	public ControlGroup controlGroup = ControlGroup.UI;

	public bool IsPaused {
		get { return _IsPaused;}
		set {
			_IsPaused = value;

			//toggleInputSystem(value);

			Time.timeScale = _IsPaused?0f:_GameSpeed;
		}
	}

	private float _GameSpeed = 1f;
	public float GameSpeed {
		get { return _GameSpeed;}
		set {
			_GameSpeed = value;

			if(!_IsPaused) Time.timeScale = _GameSpeed;
		}
	}

	public int maxPlayerCount {
		get{ return inputManager.maxPlayerCount;}
	}

	
	[NonSerialized]
	public PlayerInputManager inputManager;
	public static GameController Instance { get; private set; }

	public Dictionary<int,PlayerInput> controls = new Dictionary<int,PlayerInput>();

	public List<GameObject> NetworkTypePrefabs = new List<GameObject>();

	public List<int> Peers = new List<int>();

	public DFNGameManager GGPO;

	void Awake() {
		if (Instance != null && Instance != this)
			Destroy(gameObject);

		else 
			Instance = this;
		
		inputManager = GetComponent<PlayerInputManager>();
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		aggregatePrefabs("Assets/Resources/");
		inputManager.EnableJoining();
	}

	//-------------------------------------------------------------
	// START OF USER MANAGEMENT
	//-------------------------------------------------------------

	public void addUser(PlayerInput playerInput) {
		int peerID = 0;
		while (controls.ContainsKey(peerID))
			peerID++;
		
		controls.Add(peerID, playerInput);

		if (FindObjectOfType<MainMenuScreensManager>() == null && FindObjectOfType<EndScreenManager>() == null)	{
			playerInput.SwitchCurrentActionMap("Controls");
			controlGroup = ControlGroup.Controls;
			FindObjectOfType<CharacterMenu>()?.AddCharSelState(peerID);
		}

		else {
			controlGroup = ControlGroup.UI;
			playerInput.SwitchCurrentActionMap("UI");
		}
		

		if(IsTraining) FindObjectOfType<CharacterMenu>()?.AddCharSelState(9,DrifterType.Sandbag);

		playerInput.ActivateInput();
		DontDestroyOnLoad(playerInput);
	}

	public void removeUserByPeer(int peerID) {
		if(!controls.ContainsKey(peerID)) {
			UnityEngine.Debug.Log("PEER ID " + peerID +" ATTEMPTED TO BE REMOVED BUT WAS NOT FOUND");
			return;
		}
		
		controls[peerID].DeactivateInput();
		//inputManager.Un
		Destroy(controls[peerID].gameObject);
		controls.Remove(peerID);
		if (FindObjectOfType<MainMenuScreensManager>() == null && FindObjectOfType<EndScreenManager>() == null) {
			FindObjectOfType<CharacterMenu>()?.RemoveCharSelState(peerID);
		}
		Peers.Remove(peerID);
		if(!clearingPeers && IsTraining && controls.Count == 0) {
			EnableJoining();
		}
	}

	public void removeAllPeers() {
		clearingPeers = true;
		List<int> peersToRemove = new List<int>();
		foreach(int peerID in controls.Keys)
			peersToRemove.Add(peerID);
		foreach(int peer in peersToRemove)
			removeUserByPeer(peer);
		clearingPeers = false;
	}

	public void removeAllUIPeers() {
		foreach (int peer in controls.Keys) {
			controls[peer].DeactivateInput();
			Destroy(controls[peer].gameObject);
			controls.Remove(peer);
			Peers.Remove(peer);
		}
	}

	 //Wrap enable method
	public void EnableJoining() {
		inputManager.EnableJoining();
	}
	//Wrap Disable method
	public void DisableJoining() {
		inputManager.DisableJoining();
	}

	public bool CanJoin() {
		return inputManager.joiningEnabled;
	}

	//-------------------------------------------------------------
	// START OF GAMESTATE MANAGEMENT
	//-------------------------------------------------------------

	//Begin a GGPO Local Game
	public void StartGGPO() {
		GGPO.StartLocalGame();
	}

	//End a GGPO Game
	public void StopGGPO(){
		GGPO.StopGame();
	}

	// Move to Combat Scene and core gameplay loop
	public void BeginMatch() {
		UnityEngine.Debug.Log("BEGIN COMBAT");
		canPause = true;
		toggleInputSystem(false);
		gameState = GameState.COMBAT;
		//GameSpeed = 1f;
		SceneManager.LoadScene("Combat");
	}

	//Finish a Combat round and move to Endgame
	public void EndMatch() {
		canPause = false;
		IsPaused = false;

		//Add delay here
		//toggleInputSystem(true);
		gameState = GameState.ENDSCREEN;
		SceneManager.LoadScene("Endgame");
	}

	//Move to Character Select from the main menu or Endgame
	public void GoToCharacterSelect(){
		UnityEngine.Debug.Log("LOAD CHARACTER SELECT");
		removeAllPeers();
		gameState = GameState.CHARACTER_SELECT;
		SceneManager.LoadScene("Character_Select_Rework");
	}

	//Returns to the DFN Main Menu
	public void GoToMainMenu(){
		UnityEngine.Debug.Log("LOAD MAIN MENU");
		removeAllPeers();
		StopGGPO();
		gameState = GameState.MENU;
		EnableJoining();
		SceneManager.LoadScene("MenuScene");
	}

	public void toggleInputSystem(bool ui) {
		controlGroup = (ui ? ControlGroup.UI : ControlGroup.Controls);
		foreach(PlayerInput input in controls.Values) 
			input.SwitchCurrentActionMap(ui?"UI":"Controls");
	}

	public void UpdateSFXVolume(float val) {
		AudioSource source = GetComponent<AudioSource>();
		source.volume = val;
	}

	//-------------------------------------------------------------
	// START OF ITEMS PREFAB AGGREGATION AND CREATION
	//-------------------------------------------------------------

	//Populates the Network Prefabs list in Lucille Johnson
	private void aggregatePrefabs(string basePath) {
		UnityEngine.Object[] networkPrefabs = Resources.LoadAll("", typeof(GameObject));

		for(int i = 0; i < networkPrefabs.Length; i++)

		   NetworkTypePrefabs.Add((GameObject)networkPrefabs[i]);

		UnityEngine.Debug.Log("Added " + NetworkTypePrefabs.Count + " Prefabs to the Network Prefab List");

	}

	public GameObject CreatePrefab(string networkType, int peerId = 0) {
		UnityEngine.Debug.Log(networkType);
		GameObject obj = Instantiate(NetworkTypePrefabs.Find(x => x.name == networkType));
		obj.name = networkType + "_" + peerId;
		return obj;
	}

	public GameObject CreatePrefab(string networkType, Vector3 position, Quaternion rotation, int peerId = 0) {
		GameObject obj = Instantiate(NetworkTypePrefabs.Find(x => x.name == networkType),position,rotation);

		obj.name = networkType + "_" + peerId;
		return obj;
	}

	//-------------------------------------------------------------
	// START OF SERIALIZATION AND UPDATE GAMELOOP
	//-------------------------------------------------------------

	public void UpdateFrame(PlayerInputData[] inputs){
		switch(gameState){
			case GameState.CHARACTER_SELECT:
				CharacterMenu.Instance?.UpdateFrame(inputs);
				break;
			case GameState.COMBAT:
				EscapeMenu.Instance?.UpdateFrame(inputs);
				CombatManager.Instance?.UpdateFrame(inputs);
				TrainingDummyHandler.Instance?.UpdateFrame();
				break;
			case GameState.ENDSCREEN:
				EndScreenManager.Instance?.UpdateFrame(inputs);
				break;
			default:
				break;
		 }  
	}

	public void Serialize(BinaryWriter bw) {
		
		//bw.Write(clearingPeers);

		bw.Write((int)selectedStage);
		bw.Write((int)controlGroup);

		bw.Write((int)gameState);
		switch(gameState){
			case GameState.CHARACTER_SELECT:
				CharacterMenu.Instance.Serialize(bw);
				break;
			case GameState.COMBAT:
				CombatManager.Instance.Serialize(bw);
				break;
			case GameState.ENDSCREEN:
				break;
			default:
				break;
		 }         
	}

	public void Deserialize(BinaryReader br) {

		//clearingPeers = br.ReadBoolean();

		selectedStage = (BattleStage)br.ReadInt32();
		controlGroup = (ControlGroup)br.ReadInt32();

		gameState = (GameState)br.ReadInt32();
		switch(gameState){
			case GameState.CHARACTER_SELECT:
				CharacterMenu.Instance.Deserialize(br);
				break;
			case GameState.COMBAT:
				CombatManager.Instance.Deserialize(br);
				break;
			case GameState.ENDSCREEN:
				break;
			default:
				break;
		 } 		
	}
}
