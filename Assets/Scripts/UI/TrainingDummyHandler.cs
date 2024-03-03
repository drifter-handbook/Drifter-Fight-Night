using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingDummyHandler : MonoBehaviour
{
	public Drifter Dummy;
	public Drifter Player;
	public Dropdown d_Dropdown;
	public Dropdown t_Dropdown;
	public Dropdown r_Dropdown;
	public Dropdown s_Dropdown;

	bool onHit = false;
	bool onBlock = false;
	bool onWakeup = false;

	bool fillMeter = false;
	bool emptyMeter = false;
	bool meterReset = false;

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

	int option;

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
	}

	void FixedUpdate() {

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

	void setDummyInput(PlayerInputData[] p_input)
	{
		for(int i = 0; i < p_input.Length; i++)
		{
			Dummy.input[i] = p_input[i];
		}
	}

	void controlDummy()
	{
		
	}
}
