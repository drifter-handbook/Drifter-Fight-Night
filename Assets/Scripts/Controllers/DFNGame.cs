using SharedGame;
using UnityGGPO;
using System;
using System.Collections.Generic;
using System.IO;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public struct DFNGame : IGame {
	public int Framenumber { get; private set; }
	public int Checksum => GetHashCode();
	public int numPlayers;

	public void Serialize(BinaryWriter bw) {
		bw.Write(Framenumber);
		GameController.Instance.Serialize(bw);
	}
	public void Deserialize(BinaryReader br) {
		Framenumber = br.ReadInt32();
		GameController.Instance.Deserialize(br);
	}
	public NativeArray<byte> ToBytes() {
		using (var memoryStream = new MemoryStream()) {
			using (var writer = new BinaryWriter(memoryStream)) {
				Serialize(writer);
			}
			return new NativeArray<byte>(memoryStream.ToArray(), Allocator.Persistent);
		}
	}
	public void FromBytes(NativeArray<byte> bytes) {
		using (var memoryStream = new MemoryStream(bytes.ToArray())) {
			using (var reader = new BinaryReader(memoryStream)) {
				Deserialize(reader);
			}
		}
	}
	/*
	 * InitGameState --
	 *
	 * Initialize our game state.
	 */
	public DFNGame(int num_players) {
		Framenumber = 0;
		//Link this to GameController
		numPlayers = num_players;
		UnityEngine.Debug.Log("New Game made");
	}

	public void Update(long[] inputsLong, int disconnect_flags) {
		Framenumber++;
		for (int i = 0; i < numPlayers; i++) {
			if ((disconnect_flags & (1 << i)) != 0) {
				//GetShipAI(i);
			}
			else {
				
			}
		}
		GameController.Instance.UpdateFrame(NetworkControls.ParseDrifterInputs(inputsLong));
	}

	public long ReadInputs(int id) {

		if(GameController.Instance.controls.ContainsKey(id)) 
			return GameController.Instance.controls[id].getInputs();
		//Return empty input if key is not present
		else
			return 0;
	}
	public void LogInfo(string str){
	}

	public void FreeBytes(NativeArray<byte> data) {
		if (data.IsCreated) {
			data.Dispose();
		}
	}

	public override int GetHashCode() {
		int hashCode = -1214587014;
		hashCode = hashCode * -1521134295 + Framenumber.GetHashCode();
		hashCode = hashCode * -1521134295 + GameController.Instance.GetHashCode();
		return hashCode;
	}
}
