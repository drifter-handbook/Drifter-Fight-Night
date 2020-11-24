using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopKillBox : KillBox
{


	void OnTriggerExit2D(Collider2D other){

		//Do nothing

	}

	void OnTriggerStay2D(Collider2D other){

		if(other.gameObject.GetComponent<PlayerStatus>() != null && other.gameObject.GetComponent<PlayerStatus>().HasEnemyStunEffect()){
			killPlayer(other);
		}

	}
    
}
