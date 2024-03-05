using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingDummyHandler : MonoBehaviour
{
	public enum buttonIcon
	{ UP, RIGHT, LEFT, DOWN, NORMAL, SPECIAL, THROW, GUARD, BYZANTINE, JUMP, UPLEFT, UPRIGHT, DOWNLEFT, DOWNRIGHT };

	public Drifter Dummy;
	public Drifter Player;
	public Dropdown d_Dropdown;
	public Dropdown t_Dropdown;
	public Dropdown r_Dropdown;
	public Dropdown s_Dropdown;
	public Dropdown b_Dropdown;
	public Dropdown g_Dropdown;

	public GameObject inputList;
	public Sprite[] images;

	//Input buffer readout
	GameObject[] frameList = new GameObject[16];
	GameObject currentDisplay;
	bool displayInput = true;

	//Dummy Trigger
	bool onHit = false;
	bool onBlock = false;
	bool onWakeup = false;
	bool resetFlag = true;
	int reset = 0;
	int resetFrames = 28;

	//Meter fill options
	bool fillMeter = false;
	bool emptyMeter = false;
	bool meterReset = false;
	int meterResetFrames = 0;

	//Record & Playback
	string[] playbackBuffer = new String[]{"0,0,0,0,0,0,0,0:69", null};
	bool controlDummy;
	bool recording;
	bool playback;
	// bool triggerPlayback;
	// bool playbackReaction;
	PlayerInputData playbackInput;
	int playbackFrame;
	int playbackIndex;


	PlayerInputData[] baseState     = new PlayerInputData[]
		{
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData()
		};
	PlayerInputData[] reactionState = new PlayerInputData[]
		{
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData()
		};

	PlayerInputData prevFrameData = new PlayerInputData();
	int currentInputFrameTime = 1;

	void Start()
	{
		//Add listener for when the value of the Dropdown changes, to take action
		d_Dropdown.onValueChanged.AddListener(delegate {
			BaseDropdownValueChanged(d_Dropdown);
		});

		t_Dropdown.onValueChanged.AddListener(delegate {
			TriggerDropdownValueChanged(t_Dropdown);
		});

		r_Dropdown.onValueChanged.AddListener(delegate {
			ReactionDropdownValueChanged(r_Dropdown);
		});

		s_Dropdown.onValueChanged.AddListener(delegate {
			MeterDropdownValueChanged(s_Dropdown);
		});

		b_Dropdown.onValueChanged.AddListener(delegate {
			BufferDropdownValueChanged(b_Dropdown);
		});

		g_Dropdown.onValueChanged.AddListener(delegate {
			GamespeedDropdownValueChanged(g_Dropdown);
		});
	}

	void FixedUpdate() {
		if(!GameController.Instance.IsTraining) return;

		if(Player == null || Dummy == null) return;
		
		//Meter Settings

		if(fillMeter) Player.SetCharge(500);
		else if (emptyMeter)  Player.SetCharge(0);
		else if(meterReset && meterResetFrames >0){
			meterResetFrames--;
			if(meterResetFrames == 0)
				Player.SetCharge(500);
		}

		if(meterReset && Dummy.status.HasEnemyStunEffect()){
			meterResetFrames = 200;
		}

		//Command Button

		if(Player.input[0].Pause && Player.input[1].Pause && !Player.input[2].Pause){
			
			if(Player.input[0].MoveY > 0 && Player.input[0].MoveX ==0)
				clearBuffer();
			else if(Player.input[0].MoveY < 0 && Player.input[0].MoveX ==0)
				Dummy.transform.position = new Vector3(0,4);
			else if(Player.input[0].MoveY <0 && Player.input[0].MoveX < 0){
				Dummy.movement.setFacing(1);
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[0].transform.position;
			}
			else if(Player.input[0].MoveY <0 && Player.input[0].MoveX > 0){
				Dummy.movement.setFacing(-1);
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[1].transform.position;
			}
			else if(Player.input[0].MoveX < 0){
				Dummy.movement.setFacing(1);
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[2].transform.position;
			}
			else if(Player.input[0].MoveX > 0){
				Dummy.movement.setFacing(-1);
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[3].transform.position;
			}
			//Record and saved Dummy input for replay
			else if(Player.input[0].MoveX == 0 && Player.input[0].MoveY == 0){
				if(playback)
					playbackIndex = 0;
			}
		}
			
		//Command Button in Dummy mode
		if(controlDummy && Dummy.input[0].Pause && Dummy.input[1].Pause && !Dummy.input[2].Pause && Dummy.input[0].MoveX == 0 && Dummy.input[0].MoveY == 0){
			if(recording == true){
				UnityEngine.Debug.Log("RECORDING STOPPED AND SAVED ;)");
				recording = false;
				recordFrame(Player.input[0],currentInputFrameTime);
			}
			else {
				UnityEngine.Debug.Log("RECORDING STARTED");
				playbackBuffer = new string[512];
				playbackIndex = 0;
				currentInputFrameTime = 0;
				recording = true;
			}
			
		}

		//Input buffer display
		if(displayInput || recording){

			PlayerInputData currentFrameData = Player.input[0];

			if(currentFrameData.Equals(prevFrameData)) {
				if(currentInputFrameTime < 999) currentInputFrameTime++;
				if(currentDisplay != null && !currentFrameData.isEmpty())currentDisplay.GetComponentInChildren<TextMeshProUGUI>().text = currentInputFrameTime.ToString();

			}
			else{
				if(recording)
					recordFrame(prevFrameData,currentInputFrameTime);

				currentInputFrameTime = 1;

				if(!currentFrameData.isEmpty()){

					currentDisplay = addButtonFrame();
					currentDisplay.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = currentInputFrameTime.ToString();

					if(currentFrameData.Light && currentFrameData.Special) addButton(buttonIcon.THROW);
					else {
						if(currentFrameData.Light) addButton(buttonIcon.NORMAL);
						if(currentFrameData.Special) addButton(buttonIcon.SPECIAL);
					}
					if(currentFrameData.Guard) addButton(buttonIcon.GUARD);
					if(currentFrameData.Super) addButton(buttonIcon.BYZANTINE);
					if(currentFrameData.Jump) addButton(buttonIcon.JUMP);

					if(currentFrameData.MoveX > 0 && currentFrameData.MoveY >0) addButton(buttonIcon.UPRIGHT);
					else if(currentFrameData.MoveX < 0 && currentFrameData.MoveY >0) addButton(buttonIcon.UPLEFT);
					else if(currentFrameData.MoveX > 0 && currentFrameData.MoveY <0) addButton(buttonIcon.DOWNRIGHT);
					else if(currentFrameData.MoveX < 0 && currentFrameData.MoveY <0) addButton(buttonIcon.DOWNLEFT);
					else if(currentFrameData.MoveY > 0) addButton(buttonIcon.UP);
					else if(currentFrameData.MoveY < 0) addButton(buttonIcon.DOWN);
					else if(currentFrameData.MoveX > 0) addButton(buttonIcon.RIGHT);
					else if(currentFrameData.MoveX < 0) addButton(buttonIcon.LEFT);

					//clear the oldest frame
					if(frameList[15] != null) Destroy(frameList[15]);

					for (int i = frameList.Length - 2; i >= 0; i--)
						frameList[i + 1] = frameList[i];
			
					frameList[0] = currentDisplay;
					
				}
				prevFrameData = currentFrameData;
			}
		}

		//Do this by swapping input systems when that is easier
		if(controlDummy){
			NetworkPlayers.Instance.UpdateInput(Dummy.gameObject, (PlayerInputData)Player.input[0].Clone(), true);
			Player.input[0] = new PlayerInputData();
			NetworkPlayers.Instance.UpdateInput(Player.gameObject, true);
		}
		//else if(playback || playbackReaction){
		else if(playback){
			NetworkPlayers.Instance.UpdateInput(Dummy.gameObject, (PlayerInputData)playbackInput.Clone(), true);
			playbackFrame--;

			if(playbackFrame == 0){
				getNextPlaybackFrame();
			}

		}
		//Hard set dummy inputs
		else {
			//NetworkPlayers.Instance.UpdateInput(Dummy.gameObject, true);
			if(onWakeup && Dummy.knockedDown)
			{
				if(resetFlag)setDummyInput(reactionState);
				resetFlag = false;
				reset = resetFrames;
				
			}
			if(onBlock && Dummy.status.HasEnemyStunEffect() && Dummy.guarding)
			{
			
				if(resetFlag) setDummyInput(reactionState);
				resetFlag = false;
				reset = resetFrames;
				
			}

			if(onHit && Dummy.status.HasEnemyStunEffect())
			{
				if(resetFlag) setDummyInput(reactionState);
				resetFlag = false;
				reset = resetFrames;	
			}

			 if((onHit || onBlock)  && !Dummy.status.HasEnemyStunEffect() && reset == 0)
			 	setDummyInput(baseState);

			Dummy.UpdateFrame();

			if(reset > 0) {
				reset--;
				if(reset == 0) {
					resetFlag = true;
					setDummyInput(baseState);
					Dummy.SetCharge(300);
				}
			}
		}

	}

	//Ouput the new value of the Dropdown into Text
	void BaseDropdownValueChanged(Dropdown change)
	{
		baseState = new PlayerInputData[]
		{
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData()
		};
		controlDummy = false;
		recording = false;
		playback = false;
		// triggerPlayback = false;
		// playbackReaction = false;
		Player.setTrainingDummy(false);
		switch(change.value)
		{
			case 1:
				Dummy.input[0] = new PlayerInputData(){Guard = true};
				Dummy.input[1] = new PlayerInputData(){Guard = true};
				baseState[0] = new PlayerInputData(){Guard = true};
				baseState[1] = new PlayerInputData(){Guard = true};
				break;
			case 2:
				Dummy.input[0] = new PlayerInputData(){Jump = true};
				baseState[0] = new PlayerInputData(){Jump = true};
				break;
			case 3:
				Dummy.input[0] = new PlayerInputData(){Light = true};
				baseState[0] = new PlayerInputData(){Light = true};
				Dummy.input[1] = new PlayerInputData(){Light = true};
				baseState[1] = new PlayerInputData(){Light = true};
				break;
			case 4:
				Dummy.input[0] = new PlayerInputData(){Special = true};
				baseState[0] = new PlayerInputData(){Special = true};
				Dummy.input[1] = new PlayerInputData(){Special = true};
				baseState[1] = new PlayerInputData(){Special = true};
				break;

			case 5:
				controlDummy = true;
				clearBuffer();
				Player.setTrainingDummy(true);
				break;

			case 6:
				playback = true;
				playbackIndex = 0;
				getNextPlaybackFrame();
				break;

			case 0:
				Dummy.input[0] = new PlayerInputData();
				Dummy.input[1] = new PlayerInputData();
				baseState[0] = new PlayerInputData();
				baseState[1] = new PlayerInputData();
				break;

			default:
				break;
		};
	}

	//Ouput the new value of the Dropdown into Text
	void TriggerDropdownValueChanged(Dropdown change)
	{
		onWakeup = false;
		onHit = false;
		onBlock = false;
		resetFlag = true;
		switch(change.value)
		{
			case 1:
				onWakeup = true;
				break;
			case 2:
				onHit = true;
				break;
			case 3:
				onBlock = true;
				break;
			case 0:
			default:
				break;
		};
	}

	//Ouput the new value of the Dropdown into Text
	void ReactionDropdownValueChanged(Dropdown change)
	{
		reactionState = new PlayerInputData[]
		{
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData(),
			new PlayerInputData()
		};

		resetFrames = 28;
		// triggerPlayback = false;
		// playbackReaction = false;
		switch(change.value)
		{
			case 1:
				reactionState[0] = new PlayerInputData(){Guard = true};
				resetFrames = 60;
				break;
			case 2:
				reactionState[0] = new PlayerInputData(){Jump = true};
				break;
			case 3:
				reactionState[0] = new PlayerInputData(){Light = true};
				reactionState[1] = new PlayerInputData(){Light = true};
				break;

			case 4:
				reactionState[0] = new PlayerInputData(){Special = true};
				reactionState[1] = new PlayerInputData(){Special = true};     
				break;

			case 5:
				reactionState[0] = new PlayerInputData(){MoveX = Dummy.movement.Facing};
				reactionState[2] = new PlayerInputData(){MoveX = Dummy.movement.Facing};
				resetFrames = 10;
				break;

			case 6:
				reactionState[0] = new PlayerInputData(){Super = true};
				reactionState[1] = new PlayerInputData(){Super = true};
				//resetFrames = 5;
				break;

			// case 7:
			// 	triggerPlayback = true;
			// 	resetFrames = -1;
			// 	getNextPlaybackFrame();
			// 	break;

			case 0:
				reactionState[0] = new PlayerInputData();
				reactionState[1] = new PlayerInputData();
				break;
			default:
				break;
		};
	}

	void MeterDropdownValueChanged(Dropdown change)
	{
		fillMeter = false;
		emptyMeter = false;
		meterReset = false;
		switch(change.value)
		{
			case 1:
				fillMeter = true;
				break;
			case 2:
				meterReset = true;
				Player.SetCharge(500);
				break;
			case 3:
				emptyMeter = true;
				break;
			case 0:
			default:
				break;
		};
	}

	void GamespeedDropdownValueChanged(Dropdown change)
	{
		switch(change.value)
		{
			case 1:
				GameController.Instance.GameSpeed = .5f;
				break;
			case 2:
				GameController.Instance.GameSpeed = .25f;
				break;
			case 3:
				GameController.Instance.GameSpeed = .1f;
				break;
			case 0:
			default:
				GameController.Instance.GameSpeed = 1f;
				break;
		};
	}

	void clearBuffer(){
		for(int i = 0; i < 16; i++){
			Destroy(frameList[i]);
			frameList[i] = null;
		}
		currentInputFrameTime = 0;

	}

	void BufferDropdownValueChanged(Dropdown change) {
		switch(change.value)
		{
			case 1:
				displayInput = false;
				prevFrameData = new PlayerInputData();
				clearBuffer();
				break;
			case 0:
			default:
				displayInput = true;
				break;
		};
	}

	void setDummyInput(PlayerInputData[] p_input) {
		// if(triggerPlayback)playbackReaction = true;
		// else
			for(int i = 0; i < p_input.Length; i++)
				Dummy.input[i] = p_input[i];
	}

	void recordFrame(PlayerInputData data, int numFrames){
		playbackBuffer[playbackIndex] = data.ToString() + ":" + numFrames.ToString();
		playbackIndex++;

		if(playbackIndex >= playbackBuffer.Length) {
			UnityEngine.Debug.Log("MAX RECORDING LENGTH REACHED. STOPPING RECORDING");
			recording = false;
		}
	}

	void getNextPlaybackFrame(){
		//Return to head if max len reached.
		if(playbackBuffer[playbackIndex] == null || playbackIndex >= playbackBuffer.Length){
			// if(triggerPlayback) {
			// 	playbackReaction = false;
			// 	reset = 0;
			// }
			playbackIndex = 0;
		}

		string[] frame = playbackBuffer[playbackIndex].Split(':');

		playbackInput = PlayerInputData.FromString(frame[0]);
		playbackFrame = Int32.Parse(frame[1]);
		playbackIndex++;
		
	}

	GameObject addButton(buttonIcon icon){
		GameObject button = GameController.Instance.CreatePrefab("InputFrameButton", transform.position, transform.rotation);
		button.GetComponent<Image>().sprite = images[(int)icon];
		button.transform.SetParent(currentDisplay.transform.GetChild(0));
		button.transform.localScale = new Vector3(1, 1, 1) ;
		return button;
	}

	GameObject addButtonFrame(){
		GameObject display = GameController.Instance.CreatePrefab("InputFrameDisplay", transform.position, transform.rotation);
		display.transform.SetParent(inputList.transform);
		display.transform.localScale = new Vector3(1, 1, 1) ;
		return display;
	}
}
