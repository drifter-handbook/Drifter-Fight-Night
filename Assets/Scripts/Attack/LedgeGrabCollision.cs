using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabCollision : MonoBehaviour
{
	public PlayerMovement movement;
	public PlayerStatus status;
	BoxCollider2D ledgeBox;
	//Coroutine insurance;
	// Start is called before the first frame update
	void Start() {
		ledgeBox = GetComponent<BoxCollider2D>();
	}

	// Update is called once per frame

	void OnTriggerStay2D(Collider2D col){

		if(col.gameObject.tag == "Ledge" && col.GetComponent<HopUp>().ledgeLock != LedgeLockState.Locked && !status.HasEnemyStunEffect() && !movement.ledgeHanging){
			movement.GrabLedge(col.gameObject.transform.position);
		}

	}

	void OnTriggerExit2D(Collider2D col){

		if(col.gameObject.tag == "Ledge" && movement.ledgeHanging){
			movement.DropLedge();
		}
	}
}
