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

	public GameObject inputList;

	GameObject[] frameList = new GameObject[16];

	public Sprite[] images;

	GameObject currentDisplay;

	bool onHit = false;
	bool onBlock = false;
	bool onWakeup = false;

	bool fillMeter = false;
	bool emptyMeter = false;
	bool meterReset = false;

	bool displayInput = true;

	bool controlDummy;

	bool record;

	int reset = 0;

	int meterResetFrames = 0;

	int resetFrames = 28;
	bool resetFlag = true;

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
	}

	void FixedUpdate() {
		if(!GameController.Instance.IsTraining) return;
		if(controlDummy){
			Dummy.input[0] = (PlayerInputData)Player.input[0].Clone();
			Player.input[0] = new PlayerInputData();
		}
		else{
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

			if((onHit || onBlock)  && !Dummy.status.HasEnemyStunEffect() && reset <=0 )
				 setDummyInput(baseState);

			if(reset > 0)
			{
				reset--;
				if(reset <=0)
				{
					resetFlag = true;
					setDummyInput(baseState);
					Dummy.SetCharge(300);
				}
			}
		}
		
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

		if(Player != null && Player.input[0].Pause && Player.input[1].Pause && !Player.input[2].Pause){
			
			if(Player.input[0].MoveY > 0 && Player.input[0].MoveX ==0)
				clearBuffer();
			else if(Player.input[0].MoveY < 0 && Player.input[0].MoveX ==0)
				Dummy.transform.position = new Vector3(0,5);
			else if(Player.input[0].MoveY <0 && Player.input[0].MoveX > 0)
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[0].transform.position;
			else if(Player.input[0].MoveY <0 && Player.input[0].MoveX < 0)
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[1].transform.position;
			else if(Player.input[0].MoveX > 0)
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[2].transform.position;
			else if(Player.input[0].MoveX < 0)
				Dummy.transform.position = NetworkPlayers.Instance.spawnPoints[3].transform.position;

			
		}

		//Input buffer display
		if(displayInput && Player != null){

			PlayerInputData currentFrameData = Player.input[0];

			if(currentFrameData.Equals(prevFrameData)) {
				if(currentInputFrameTime < 999) currentInputFrameTime++;
				if(currentDisplay != null)currentDisplay.GetComponentInChildren<TextMeshProUGUI>().text = currentInputFrameTime.ToString();

			}
			else if(!currentFrameData.isEmpty()){
				currentInputFrameTime = 1;
				currentDisplay = addButtonFrame();

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

				//if(currentFrameData.MoveX == 0 && currentFrameData.MoveY == 0) addButton(buttonIcon.NONE);

				currentDisplay.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = "1";

				//clear the oldest frame
				if(frameList[15] != null) Destroy(frameList[15]);

				for (int i = frameList.Length - 2; i >= 0; i--)
            		frameList[i + 1] = frameList[i];
        
        		frameList[0] = currentDisplay;
        		prevFrameData = currentFrameData;
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

	void clearBuffer(){
		for(int i = 0; i < 16; i++){
			Destroy(frameList[i]);
			frameList[i] = null;
		}
		currentInputFrameTime = 0;

	}

	void BufferDropdownValueChanged(Dropdown change)
	{
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

	void setDummyInput(PlayerInputData[] p_input)
	{
		for(int i = 0; i < p_input.Length; i++)
		{
			Dummy.input[i] = p_input[i];
		}
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
