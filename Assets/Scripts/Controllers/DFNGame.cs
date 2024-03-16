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
	public const int INPUT_JUMP 	= (1 << 0);
	public const int INPUT_LIGHT 	= (1 << 1);
	public const int INPUT_SPECIAL 	= (1 << 2);
	public const int INPUT_SUPER 	= (1 << 3);
	public const int INPUT_GUARD 	= (1 << 4);
	public const int INPUT_GRAB 	= (1 << 5);
	public const int INPUT_DASH 	= (1 << 6);
	public const int INPUT_PAUSE 	= (1 << 7);
	public const int INPUT_MENU		= (1 << 8);
	public const int INPUT_LEFT 	= (1 << 9);
	public const int INPUT_RIGHT	= (1 << 10);
	public const int INPUT_UP 		= (1 << 11);
	public const int INPUT_DOWN		= (1 << 12);
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
		GameController.Instance.UpdateFrame(ParseDrifterInputs(inputsLong));
	}
	public PlayerInputData[] ParseDrifterInputs(long[] inputsLong) {
		PlayerInputData[] inputsParsed = new PlayerInputData[inputsLong.Length];
		for(int i = 0; i < inputsLong.Length; i++){
			GGPORunner.LogGame($"parsing drifter {i} inputs: {inputsLong[i]}.");
			inputsParsed[i] = new PlayerInputData();
			inputsParsed[i].Jump = ((inputsLong[i] & INPUT_JUMP) !=0);
			inputsParsed[i].Light = ((inputsLong[i] & INPUT_LIGHT) !=0);
			inputsParsed[i].Special = ((inputsLong[i] & INPUT_SPECIAL) !=0);
			inputsParsed[i].Super = ((inputsLong[i] & INPUT_SUPER) !=0);
			inputsParsed[i].Guard = ((inputsLong[i] & INPUT_GUARD) !=0);
			inputsParsed[i].Grab = ((inputsLong[i] & INPUT_GRAB) !=0);
			inputsParsed[i].Dash = ((inputsLong[i] & INPUT_DASH) !=0);
			inputsParsed[i].Pause = ((inputsLong[i] & INPUT_PAUSE) !=0);
			inputsParsed[i].Menu = ((inputsLong[i] & INPUT_MENU) !=0);
			if((inputsLong[i] & INPUT_RIGHT) != 0)
				inputsParsed[i].MoveX = 1;
			else if((inputsLong[i] & INPUT_LEFT) != 0)
				inputsParsed[i].MoveX = -1;
			if((inputsLong[i] & INPUT_UP) !=0)
				inputsParsed[i].MoveY = 1;
			else if((inputsLong[i] & INPUT_DOWN) != 0)
				inputsParsed[i].MoveY = -1;
		}
		return inputsParsed;
	}

	public long ReadInputs(int id) {
		long input = 0;
		InputActionMap playerInputAction;
		if(GameController.Instance.controls.ContainsKey(id)) playerInputAction = GameController.Instance.controls[id].currentActionMap;
		//Return empty input if key is not present
		else
			return input;

		switch(GameController.Instance.controlGroup){
			case ControlGroup.UI:
				UnityEngine.Debug.Log("UI MODE; IMPLEMENT ME");
				Vector2 nav = playerInputAction.FindAction("Navigate").ReadValue<Vector2>();
				if(nav.x > 0)
					input |= INPUT_RIGHT;
				if(nav.x < 0)
					input |= INPUT_LEFT;
				if(nav.y > 0)
					input |= INPUT_UP;
				if(nav.y < 0)
					input |= INPUT_DOWN;

				if(playerInputAction.FindAction("Submit").ReadValue<float>() > 0)
					input |= INPUT_LIGHT;

				if(playerInputAction.FindAction("Cancel").ReadValue<float>() > 0)
					input |= INPUT_SPECIAL;

				return input;

			case ControlGroup.Controls:
			default:
				//Binary Buttons
				if(playerInputAction.FindAction("Jump").ReadValue<float>() > 0)
					input |= INPUT_JUMP;
				if(playerInputAction.FindAction("Light").ReadValue<float>() > 0)
					input |= INPUT_LIGHT;
				if(playerInputAction.FindAction("Special").ReadValue<float>() > 0)
					input |= INPUT_SPECIAL;
				if(playerInputAction.FindAction("Super").ReadValue<float>() > 0)
					input |= INPUT_SUPER;
				if(playerInputAction.FindAction("Guard 1").ReadValue<float>() > 0)
					input |= INPUT_GUARD;
				if(playerInputAction.FindAction("Grab").ReadValue<float>() > 0)
					input |= INPUT_GRAB;
				if(playerInputAction.FindAction("Dash").ReadValue<float>() > 0)
					input |= INPUT_DASH;
				if(playerInputAction.FindAction("Start").ReadValue<float>() > 0)
					input |= INPUT_PAUSE;
				if(playerInputAction.FindAction("Menu").ReadValue<float>() > 0)
					input |= INPUT_MENU;
				//Directional Movement
				if((int)playerInputAction.FindAction("Right").ReadValue<float>() > 0)
					input |= INPUT_RIGHT;
				if((int)playerInputAction.FindAction("Left").ReadValue<float>() > 0)
					input |= INPUT_LEFT;
				if((int)playerInputAction.FindAction("Up").ReadValue<float>() > 0)
					input |= INPUT_UP;
				if((int)playerInputAction.FindAction("Down").ReadValue<float>() > 0)
					input |= INPUT_DOWN;
				return input;
		}
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
