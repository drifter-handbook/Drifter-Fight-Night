using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.IO;

public class CombatManager : MonoBehaviour {

	public GameObject playerInputPrefab;

	GameObject stage;
	BattleStage stageName;
	List<GameObject> spawnPoints;

	public int playerUnlockFrame = 111;
	public Drifter[] Drifters;

	public static CombatManager Instance { get; private set; }

	void Awake(){
		if (Instance != null && Instance != this) 
			Destroy(gameObject);
		else 
			Instance = this;
	}

	void Start(){

		Drifters = new Drifter[CharacterMenu.charSelStates.Length];

		//Create combat stage
		CreateStage(GameController.Instance.selectedStage);
		// create players
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates)
			if(charSel!=null) CreatePlayer(charSel.PeerID, charSel.PlayerType);

		//Ignore countdown and unlock players after 1 frame in training mode
		if(GameController.Instance.IsTraining) 
			playerUnlockFrame = 1;
	}

	//-------------------------------------------------------------
	// START OF DRIFTER AND STAGE MANAGEMENT
	//-------------------------------------------------------------

	//Spawns the stage prefab combat will take place on
	void CreateStage(BattleStage p_stageName) {
		if(stage != null) Destroy(stage);

		stageName = p_stageName;
		stage = GameController.Instance.CreatePrefab(p_stageName.ToString(), 0);
		GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>().getParalax();

		//populate spawn points
		spawnPoints = new List<GameObject>();
		for(int i = 0; i <4; i++)
			spawnPoints.Add(GameObject.Find("SpawnPoint" + i));

	}

	//Creates a Drifter player character for a given Peer
	void CreatePlayer(int peerID, DrifterType drifter) {
		GameObject obj = GameController.Instance.CreatePrefab(drifter.ToString().Replace("_", " "),
			spawnPoints[peerID % spawnPoints.Count].transform.position, Quaternion.identity, peerID);
		obj.GetComponent<Drifter>().SetColor(peerID);

		if(GameController.Instance.controls.ContainsKey(peerID))obj.GetComponent<Drifter>().playerInputController = GameController.Instance.controls[peerID];
		obj.GetComponent<Drifter>().SetPeerId(peerID);
		obj.GetComponent<PlayerMovement>().setFacing(-2 * (peerID % 2) + 1);
		Drifters[peerID] = obj.GetComponent<Drifter>();
	}

	//Activates players inputs once the countdown has finished
	public void unlockPlayers(){
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates) {
			if(charSel != null && GameController.Instance.controls.ContainsKey(charSel.PeerID))
				 Drifters[charSel.PeerID].setTrainingDummy(false);
		}
	}

	//-------------------------------------------------------------
	// START OF SERIALIZATION AND UPDATE GAMELOOP
	//-------------------------------------------------------------

	public void UpdateFrame(PlayerInputData[] inputs){
		if(GameController.Instance.IsPaused) return;
		Physics2D.Simulate(1f/60f);

		for(int i = 0; i < Drifters.Length; i++){
			if(Drifters[i] == null) continue;
			for (int j = Drifters[i].input.Length - 2; j >= 0; j--)
				Drifters[i].input[j + 1] = (PlayerInputData)Drifters[i].input[j].Clone();

			//Avoids breaking on training dummy; TODO do this in a less hacky way
			if(inputs.Length > i) Drifters[i].input[0] = inputs[i];
			
			if(!Drifters[i].isTrainingDummy()) {
				Drifters[i].UpdateFrame();
			}
		}

		if(playerUnlockFrame >0){
			playerUnlockFrame--;
			if(playerUnlockFrame <= 0)
				unlockPlayers();
		}
	}

	public void Serialize(BinaryWriter bw) {
		 bw.Write(Drifters.Length);
		 for (int i = 0; i < Drifters.Length; ++i) {
		 	bw.Write((int)Drifters[i].drifterType);
			Drifters[i].Serialize(bw);
		 }
		 bw.Write((int)stageName);
		 bw.Write(playerUnlockFrame);
	}

	public void Deserialize(BinaryReader br) {
		int length = br.ReadInt32();
		if (length != Drifters.Length) {
			UnityEngine.Debug.Log("INCONSISTENT NUMBER OF DRIFTERS IN COMBAT");
			
			//Clear old drifter list
			foreach(Drifter drifter in Drifters)
				Destroy(drifter.gameObject);
			
			//Populate a new one based on the binary stream
			Drifters = new Drifter[length];
			for(int i = 0; i < Drifters.Length; ++i){
				DrifterType drifter = (DrifterType)br.ReadInt32();
				CreatePlayer(i,drifter);
				Drifters[i].Deserialize(br);
			}
		}
		else{
			for (int i = 0; i < Drifters.Length; ++i) {
				//Read drifter type bit. (not used in this case)
				br.ReadInt32();
				Drifters[i].Deserialize(br);
			}
		}

		BattleStage StageName = (BattleStage)br.ReadInt32();
		if(stageName != StageName)
			CreateStage(StageName);

		playerUnlockFrame = br.ReadInt32();
		
	}
}