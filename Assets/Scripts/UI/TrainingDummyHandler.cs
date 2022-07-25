using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingDummyHandler : MonoBehaviour
{
	public Drifter Dummy;
	public Dropdown m_Dropdown;

    //-1    no option
    //0     dash
    //1     jump
    //2     jab
    //3     random

    int wakeupMode = -1;

	int option;

	void Start()
    {
        //Add listener for when the value of the Dropdown changes, to take action
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
    }

    void FixedUpdate()
    {

        if(wakeupMode >=0 && Dummy.knockedDown)
        {
            switch(wakeupMode)
            {
                case 0:
                    Dummy.input[0] = new PlayerInputData(){MoveX = Dummy.movement.Facing};
                    Dummy.input[2] = new PlayerInputData(){MoveX = Dummy.movement.Facing};
                    break;
                case 1:
                    Dummy.input[0] = new PlayerInputData(){Jump = true};
                    break;
                case -1:
                default:
                    break;
            }
        }

        if(wakeupMode >=0 &&(Dummy.movement.dashing || Dummy.movement.jumping ))
        {
            Dummy.input[0] = new PlayerInputData();
            Dummy.input[1] = new PlayerInputData();
            Dummy.input[2] = new PlayerInputData();
        }

    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(Dropdown change)
    {
        wakeupMode = -1;
    	switch(change.value)
    	{
    		case 0:
    			Dummy.input[0] = new PlayerInputData();
    			break;
    		case 1:
    			Dummy.input[0] = new PlayerInputData(){Guard = true};
    			break;
    		case 2:
    			Dummy.input[0] = new PlayerInputData(){Jump = true};
    			break;
            case 3:
                wakeupMode = 0;
                break;
            case 4:
                wakeupMode = 1;
                break;
    		case 5:

    			break;
    		default:
    			break;
    	};
    }
}
