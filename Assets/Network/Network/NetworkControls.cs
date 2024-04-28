using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.InputSystem;
using SharedGame;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

public class NetworkControls : NetworkBehaviour{

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

	[NonSerialized]
	public PlayerInput inputObject;
	[NonSerialized]
	public int peerId;
	[NonSerialized]
	[SyncVar]
	public string connection = "local";
	[SyncVar]
	long input = 0;
	[SyncVar (hook = nameof(SetReady))]
	public bool Ready = false;

	void Awake(){
		inputObject = gameObject.GetComponent<PlayerInput>();
	}

	void Start(){
		if(isLocalPlayer){
			SetupConnection();
		}
		SetUpPeer();
	}

	void OnDestroy() {
		GameController.Instance.removeUserByPeer(peerId);
	}

	public static PlayerInputData[] ParseDrifterInputs(long[] inputsLong) {
		PlayerInputData[] inputsParsed = new PlayerInputData[inputsLong.Length];
		for(int i = 0; i < inputsLong.Length; i++){
			//GGPORunner.LogGame($"parsing drifter {i} inputs: {inputsLong[i]}.");
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

	[Command(requiresAuthority = false)]
	public void ReadyUp(){
		Ready =! Ready;
		UnityEngine.Debug.Log("Player " + peerId + " is " + (Ready?"READY":"NOT READY"));
	}

	public void SetUpPeer(){
		peerId = GameController.Instance.addUser(this);
	}

	void SetReady(bool before, bool after){
		Ready = after;
		GameObject.Find("Views").GetComponent<OnlineMenuManager>().setPips();
	}

	
	void SetupConnection() {
    	var host = Dns.GetHostEntry(Dns.GetHostName());
    	foreach (var ip in host.AddressList)
        	if (ip.AddressFamily == AddressFamily.InterNetwork){
        		populateData(ip.ToString());
				return;
        	}
    	throw new Exception("No network adapters with an IPv4 address in the system!");
	}
	[Command(requiresAuthority = false)]
	void populateData(string connectionData){
		connection = connectionData;
	}

	public long getInputs(){

		if(isLocalPlayer || !GameController.Instance.IsOnline){
			input = 0;
			InputActionMap playerInputAction = inputObject.currentActionMap;

			switch(GameController.Instance.controlGroup){
				case ControlGroup.UI:
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

					break;

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
					break;
			}
		}
		return input;
	}

}
