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

	public static CombatManager Instance => GameObject.FindGameObjectWithTag("CombatManager")?.GetComponent<CombatManager>();

	void Start(){
		Drifters = new Drifter[CharacterMenu.charSelStates.Length];

		CreateStage(GameController.Instance.selectedStage);
		// create players
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates)
			CreatePlayer(charSel.PeerID, charSel.PlayerType);

		if(GameController.Instance.IsTraining) 
			playerUnlockFrame = 1;
	}

	public void UpdateFrame(PlayerInputData[] inputs){
		Physics2D.Simulate(1f/60f);

		for(int i = 0; i < Drifters.Length; i++){
			for (int j = Drifters[i].input.Length - 2; j >= 0; j--)
				Drifters[i].input[j + 1] = (PlayerInputData)Drifters[i].input[j].Clone();

			Drifters[i].input[0] = inputs[i];
			if(!Drifters[i].isTrainingDummy()) Drifters[i].UpdateFrame();
		}

		if(playerUnlockFrame >0){
			playerUnlockFrame--;
			if(playerUnlockFrame <= 0)
				unlockPlayers();
		}
	}

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

	void CreatePlayer(int peerID, DrifterType drifter) {

		//Same here
		GameObject obj = GameController.Instance.CreatePrefab(drifter.ToString().Replace("_", " "),
			spawnPoints[(peerID +1) % spawnPoints.Count].transform.position, Quaternion.identity, peerID);
		obj.GetComponent<Drifter>().SetColor((peerID +1));

		if(GameController.Instance.controls.ContainsKey(peerID))obj.GetComponent<Drifter>().playerInputController = GameController.Instance.controls[peerID];
		obj.GetComponent<Drifter>().SetPeerId(peerID);
		obj.GetComponent<PlayerMovement>().setFacing(-1 *((peerID+1 % 2) * 2 - 1));
		Drifters[peerID] = obj.GetComponent<Drifter>();
	}

	public void unlockPlayers(){
		foreach (CharacterSelectState charSel in CharacterMenu.charSelStates) {
			if(charSel!= null && GameController.Instance.controls.ContainsKey(charSel.PeerID))
				 Drifters[charSel.PeerID].setTrainingDummy(false);
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