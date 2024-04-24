using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIEffectsManager : MonoBehaviour{

	public void WipeIn(){
		 GameObject.Find("Wipe").GetComponent<Animator>().Play("WipeIn");
	}

	public void WipeOut(){
		GameObject.Find("Wipe").GetComponent<Animator>().Play("WipeOut");
	}
    
}
