using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TrainingDummyHandler : MonoBehaviour
{
	public enum buttonIcon
	{ UP, RIGHT, LEFT, DOWN, NORMAL, SPECIAL, THROW, GUARD, BYZANTINE, JUMP, UPLEFT, UPRIGHT, DOWNLEFT, DOWNRIGHT, DASH };

	public enum DummyReactionState
	{ NONE, WAIT_FOR_TRIGGER, WAIT_FOR_ACTIONABLE, REACTION };

	public enum DummyTrigger
	{ NONE, ON_HIT, ON_BLOCK, ON_WAKEUP, ON_HIT_OR_BLOCK};

	public enum DummyAction
	{ NONE, GUARD, JUMP, LIGHT, SPECIAL, DASH, SUPER, CONTROL, PLAYBACK };

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

	public GameObject recoridngIndicator;

	//Input buffer readout
	GameObject[] frameList = new GameObject[16];
	GameObject currentDisplay;
	bool displayInput = true;

	public DummyAction BaseAction = DummyAction.NONE;
	public DummyAction ReactionAction = DummyAction.NONE;
	public DummyTrigger ReactionTrigger = DummyTrigger.NONE;
	public DummyReactionState ReactionState = DummyReactionState.NONE;

	//Meter fill options
	bool fillMeter = false;
	bool emptyMeter = false;
	bool meterReset = false;
	int meterResetFrames = 0;

	//Record & Playback
	bool recording;
	string[] recordedSequence = new String[]{"0,0,0,0,0,0,0,0:69", null};
	string[] playbackBuffer = new String[]{"0,0,0,0,0,0,0,0:69", null};

	int currentInputFrameTime = 1;
	int playbackFrame;
	int playbackIndex;

	PlayerInputData playbackInput = new PlayerInputData();
	PlayerInputData prevFrameData = new PlayerInputData();
	

	void Start() {
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

		if(fillMeter) {
			Player.SetCharge(500);
			Dummy.SetCharge(500);
			Player.inspirationCharges = 3;
		}
		else if (emptyMeter)  {
			Player.SetCharge(0);
			Dummy.SetCharge(0);
		}
		else if(meterReset && meterResetFrames >0){
			meterResetFrames--;
			if(meterResetFrames == 0){
				Player.SetCharge(500);
				Dummy.SetCharge(500);
				Player.inspirationCharges = 3;
			}
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
			// //Record and saved Dummy input for replay
			// else if(Player.input[0].MoveX == 0 && Player.input[0].MoveY == 0){
			// 	if(playback)
			// 		playbackIndex = 0;
			// }
		}
			
		//Command Button in Dummy mode
		if(BaseAction == DummyAction.CONTROL && Dummy.input[0].Pause && Dummy.input[1].Pause && !Dummy.input[2].Pause && Dummy.input[0].MoveX == 0 && Dummy.input[0].MoveY == 0){
			if(recording == true){
				UnityEngine.Debug.Log("RECORDING STOPPED AND SAVED ;)");
				recording = false;
				recordFrame(Player.input[0],currentInputFrameTime);
				//Preserve null terminator to recorded sequence
				recordedSequence[511] = null;
			}
			else {
				UnityEngine.Debug.Log("RECORDING STARTED");
				recordedSequence = new string[512];
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

					if(currentFrameData.Dash) addButton(buttonIcon.DASH);

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
		if(BaseAction == DummyAction.CONTROL){
			Dummy.input[0] =  (PlayerInputData)Player.input[0].Clone();
			Player.input[0] = new PlayerInputData();
			//NetworkPlayers.Instance.UpdateInput(Player.gameObject);
			Player.UpdateFrame();
			Dummy.UpdateFrame();
		}

		else{
			//Update input from previosu frame so we can have access to the next frame's state to pick our inputs
			//Dummy.UpdateFrame();

			switch(ReactionState){
				case DummyReactionState.WAIT_FOR_TRIGGER:
					if( (ReactionTrigger == DummyTrigger.ON_WAKEUP && Dummy.knockedDown) ||
						(ReactionTrigger == DummyTrigger.ON_BLOCK && Dummy.status.HasEnemyStunEffect() && Dummy.guarding) ||
						(ReactionTrigger == DummyTrigger.ON_HIT && Dummy.status.HasEnemyStunEffect() && !Dummy.guarding) ||
						(ReactionTrigger == DummyTrigger.ON_HIT_OR_BLOCK && Dummy.status.HasEnemyStunEffect())){
							UnityEngine.Debug.Log("REACTION TRIGGERED");
							ReactionState = DummyReactionState.WAIT_FOR_ACTIONABLE;
							setDummyInputSequence(ReactionAction);
						}
						else
							playDummyInputFrame();
					break;
				case DummyReactionState.WAIT_FOR_ACTIONABLE:
					if(!Dummy.status.HasEnemyStunEffect()){
						UnityEngine.Debug.Log("REACTION PLAYED");
						ReactionState = DummyReactionState.REACTION;
						playbackIndex = 0;
						playDummyInputFrame();
					}
					break;
				case DummyReactionState.REACTION:
				case DummyReactionState.NONE:
				default:
					playDummyInputFrame();
					break;
			}

			Dummy.UpdateFrame();
			
		}
		recoridngIndicator.SetActive(recording);
	}

	//Ouput the new value of the Dropdown into Text
	void BaseDropdownValueChanged(Dropdown change) {
		Player.setTrainingDummy(false);
		//Dummy.setTrainingDummy(true);
		switch(change.value)
		{
			case 1:
				BaseAction = DummyAction.GUARD;
				break;
			case 2:
				BaseAction = DummyAction.JUMP;
				break;
			case 3:
				BaseAction = DummyAction.LIGHT;
				break;
			case 4:
				BaseAction = DummyAction.SPECIAL;
				break;
			case 5:
				BaseAction = DummyAction.CONTROL;
				clearBuffer();
				Player.setTrainingDummy(true);
				//Dummy.setTrainingDummy(false);
				break;
			case 6:
				BaseAction = DummyAction.PLAYBACK;
				break;
			case 0:
			default:
				BaseAction = DummyAction.NONE;
				break;
		};
		setDummyInputSequence(BaseAction);
	}

	void TriggerDropdownValueChanged(Dropdown change) {
		ReactionState = DummyReactionState.WAIT_FOR_TRIGGER;
		switch(change.value)
		{
			case 1:
				ReactionTrigger = DummyTrigger.ON_WAKEUP;
				break;
			case 2:
				ReactionTrigger = DummyTrigger.ON_HIT;
				break;
			case 3:
				ReactionTrigger = DummyTrigger.ON_BLOCK;
				break;
			case 4:
				ReactionTrigger = DummyTrigger.ON_HIT_OR_BLOCK;
				break;
			case 0:
			default:
				ReactionState = DummyReactionState.NONE;
				ReactionTrigger = DummyTrigger.NONE;
				break;
		};
	}

	void ReactionDropdownValueChanged(Dropdown change) {
		// resetFrames = 28;
		// reactionPlaybackState = DummyReactionState.NONE;
		switch(change.value)
		{
			case 1:
				ReactionAction = DummyAction.GUARD;
				break;
			case 2:
				ReactionAction = DummyAction.JUMP;
				break;
			case 3:
				ReactionAction = DummyAction.LIGHT;
				break;

			case 4:
				ReactionAction = DummyAction.SPECIAL;   
				break;
			case 5:
				ReactionAction = DummyAction.DASH;
				break;
			case 6:
				ReactionAction = DummyAction.SUPER;
				break;
			case 7:
				ReactionAction = DummyAction.PLAYBACK;
				//resetFrames = -1;
				break;
			case 0:
			default:
				ReactionAction = DummyAction.NONE;
				break;
		};
	}


	void MeterDropdownValueChanged(Dropdown change) {
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

	void GamespeedDropdownValueChanged(Dropdown change) {
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

	//Clears the input buffer readout
	void clearBuffer(){
		for(int i = 0; i < 16; i++){
			Destroy(frameList[i]);
			frameList[i] = null;
		}
		currentInputFrameTime = 0;
	}

	void playDummyInputFrame(){
		Dummy.input[0] = (PlayerInputData)playbackInput.Clone();
		playbackFrame--;
		if(playbackFrame == 0){
			getNextPlaybackFrame();
		}
	}

	void setDummyInputSequence(DummyAction action) {

		//PlayerInputData thisFrame;
		switch(action){
			case DummyAction.GUARD:
				playbackBuffer = new String[]{	"0,0,0,0,0,0,1,0,0:90", null};
				break;
			case DummyAction.JUMP:
				playbackBuffer = new String[]{	"0,0,1,0,0,0,0,0,0:10",
												"0,0,0,0,0,0,0,0,0:20", null};
				break;
			case DummyAction.LIGHT:
				playbackBuffer = new String[]{	"0,0,0,1,0,0,0,0,0:2",
												"0,0,0,0,0,0,0,0,0:10", null};
				break;
			case DummyAction.SPECIAL:
				playbackBuffer = new String[]{	"0,0,0,0,1,0,0,0,0:2",
												"0,0,0,0,0,0,0,0,0:10", null};
				break;
			case DummyAction.DASH:
				playbackBuffer = new String[]{"0,0,0,0,0,0,0,0,1:2", null};	
				break;
			case DummyAction.SUPER:
				Dummy.input[0] = new PlayerInputData(){Super = true};
				playbackBuffer = new String[]{	"0,0,0,0,0,1,0,0,0:2",
												"0,0,0,0,0,0,0,0,0:2", null};
				break;
			case DummyAction.CONTROL:
				//This case probably shouldn't ever get called?
				return;
				break;
			case DummyAction.PLAYBACK:
				playbackBuffer = (string[])recordedSequence.Clone();
				break;
			case DummyAction.NONE:
			default:
				playbackBuffer = new string[]{"0,0,0,0,0,0,0,0,0:60", null};
				break;
		}

		playbackIndex = 0;
		getNextPlaybackFrame();
	}

	void recordFrame(PlayerInputData data, int numFrames){
		recordedSequence[playbackIndex] = data.ToString() + ":" + numFrames.ToString();
		playbackIndex++;

		if(playbackIndex >= recordedSequence.Length) {
			UnityEngine.Debug.Log("MAX RECORDING LENGTH REACHED. STOPPING RECORDING");
			recording = false;
		}
	}

	void getNextPlaybackFrame(){
		//Return to head if max len reached.
		if(playbackBuffer[playbackIndex] == null || playbackIndex >= playbackBuffer.Length){
			//If the sequence was a reaction, go back to base state after frames run out
			if(ReactionState == DummyReactionState.REACTION){
				UnityEngine.Debug.Log("TRIGGER RESET");
				ReactionState = DummyReactionState.WAIT_FOR_TRIGGER;
				setDummyInputSequence(BaseAction);
			}
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
