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

[DisallowMultipleComponent]
public class GameController : MonoBehaviour
{
	public enum VolumeType
	{
		MASTER,
		MUSIC,
		SFX
	};

	public enum GameState
	{
		MENU,
		CHARACTER_SELECT,
		COMBAT,
		ENDSCREEN
	};

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

	void Awake()
	{
		if (Instance != null && Instance != this) {
			Destroy(gameObject);
		}
		else {
			Instance = this;
		}
		inputManager = GetComponent<PlayerInputManager>();
		DontDestroyOnLoad(gameObject);
	}

	void Start() {
		//GameAnalytics.Initialize();
		//GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "startGame");
		// this is horrid practice please dont do this but
		// if(IsOnline)
		// {
		//     string server = Resources.Load<TextAsset>("Config/server_ip").text;
		//     if (IPAddress.TryParse(server, out IPAddress address))
		//     {
		//     }
		//     else
		//     {
		//         IPHostEntry host = Dns.GetHostEntry(server);
		//         address = host.AddressList[0];
		//     }
		//     NatPunchServer = new IPEndPoint(address, NatPunchServer.Port);
		//     MatchmakingServer = new IPEndPoint(address, MatchmakingServer.Port);
		// }
		aggregatePrefabs("Assets/Resources/");
	}

	public void addUser(PlayerInput playerInput) {
		int peerID = -1;
		while(controls.ContainsKey(peerID))
			peerID++;
		controls.Add(peerID,playerInput);

		FindObjectOfType<CharacterMenu>()?.AddCharSelState(peerID);

		playerInput.ActivateInput();

		if(IsTraining) FindObjectOfType<CharacterMenu>()?.AddCharSelState(8,DrifterType.Sandbag);

		DontDestroyOnLoad(playerInput);

        aggregatePrefabs("Assets/Resources/");
        inputManager.EnableJoining();
        //AssignInputAssest();

        //AssignInputAssest();
        
    }

    public void addUser(PlayerInput playerInput)
    {
        int peerID = -1;
        while (controls.ContainsKey(peerID))
        {
            peerID++;
        }
        controls.Add(peerID, playerInput);

        if (FindObjectOfType<ViewManager>() == null)
        {
            playerInput.SwitchCurrentActionMap("Controls");
            FindObjectOfType<CharacterMenu>()?.AddCharSelState(peerID);
        }
        else 
        {
            playerInput.SwitchCurrentActionMap("UI");
        }

        playerInput.ActivateInput();
        DontDestroyOnLoad(playerInput);

        if (controls.Count >= 1 && IsTraining)
        {
            DisableJoining();
        }
    }

    public void removeUserByPeer(int peerID)
    {
        if(!controls.ContainsKey(peerID))
        {
            UnityEngine.Debug.Log("PEER ID " + peerID +" ATTEMPTED TO BE REMOVED BUT WAS NOT FOUND");
            return;
        }
        
        controls[peerID].DeactivateInput();
        //inputManager.Un
        Destroy(controls[peerID].gameObject);
        controls.Remove(peerID);
        if (FindObjectOfType<ViewManager>() == null)
        {
            FindObjectOfType<CharacterMenu>()?.RemoveCharSelState(peerID);
        }
        if(peerID != -1) host.Peers.Remove(peerID);
        if(!clearingPeers && IsTraining && controls.Count == 0)
        {
            EnableJoining();
        }
    }

	// Only the host gets to see this guy
	public void BeginMatch() {

		canPause = true;
		toggleInuptSystem(false);
		gameState = GameState.COMBAT;
		//GameSpeed = 1f;
		SceneManager.LoadScene("Combat");
	}

	public void EndMatch() {
		canPause = false;
		IsPaused = false;

		//Add delay here

    public void toggleInputSystem(bool ui)
    {
        List<int> inputsToToggle = new List<int>();
        foreach(int peerID in controls.Keys)
            inputsToToggle.Add(peerID);
		toggleInuptSystem(true);
		gameState = GameState.ENDSCREEN;
		 SceneManager.LoadScene("Endgame");
	}

	public void GoToCharacterSelect(){
		UnityEngine.Debug.Log("LOAD SCENE");
		removeAllPeers();
		gameState = GameState.CHARACTER_SELECT;
		SceneManager.LoadScene("Character_Select_Rework");
	}

	public void GoToMainMenu(){
		removeAllPeers();
		gameState = GameState.MENU;
		 SceneManager.LoadScene("MenuScene");
	}

	public void UpdateSFXVolume(float val)
	{
		AudioSource source = GetComponent<AudioSource>();
		source.volume = val;
	}

	//Populates the Network Prefabs list in Lucille Johnson
	private void aggregatePrefabs(string basePath)
	{

		//string[] networkPrefabs = Directory.GetFiles(basePath,"*.prefab",SearchOption.AllDirectories);

		UnityEngine.Object[] networkPrefabs = Resources.LoadAll("", typeof(GameObject));

		for(int i = 0; i < networkPrefabs.Length; i++)

		   NetworkTypePrefabs.Add((GameObject)networkPrefabs[i]);

		UnityEngine.Debug.Log("Added " + NetworkTypePrefabs.Count + " Prefabs to the Network Prefab List");

	}

	public GameObject CreatePrefab(string networkType, int peerId = 0)
	{
		GameObject obj = Instantiate(NetworkTypePrefabs.Find(x => x.name == networkType));
		obj.name = networkType + "_" + peerId;
		return obj;
	}

	public GameObject CreatePrefab(string networkType, Vector3 position, Quaternion rotation, int peerId = 0)
	{
		GameObject obj = Instantiate(NetworkTypePrefabs.Find(x => x.name == networkType),position,rotation);

		obj.name = networkType + "_" + peerId;
		return obj;
	}

	public void Serialize(BinaryWriter bw) {
		
		//bw.Write(clearingPeers);

		bw.Write((int)selectedStage);

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
