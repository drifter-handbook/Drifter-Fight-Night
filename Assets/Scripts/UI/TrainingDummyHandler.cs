using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrainingDummyHandler : MonoBehaviour
{
	public Drifter Dummy;
	public Dropdown m_Dropdown;

	int option;

	void Start()
    {
        //Add listener for when the value of the Dropdown changes, to take action
        m_Dropdown.onValueChanged.AddListener(delegate {
            DropdownValueChanged(m_Dropdown);
        });
    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(Dropdown change)
    {
    	switch(change.value)
    	{
    		case 0:
    			Dummy.input[0] = new PlayerInputData();
    			break;
    		case 1:
    			Dummy.input[0] = new PlayerInputData(){Guard = true};
    			Dummy.input[1] = new PlayerInputData(){Guard = true};
    			break;
    		case 2:
    			Dummy.input[0] = new PlayerInputData(){Jump = true};
    			break;
    		case 4:
    			//Do this later :)
    			UnityEngine.Debug.Log("Bitch you thought");
    			Dummy.input[0] = new PlayerInputData();
    			break;
    		default:
    			break;
    	};
    }
}
