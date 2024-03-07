using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LedgeLockState{
	Open, Locked, Tethered
}

public class HopUp : MonoBehaviour {
	public LedgeLockState ledgeLock = LedgeLockState.Open;
	public PlayerMovement lockingPlayer;

	void OnTriggerStay2D(Collider2D col) {
		if (col.gameObject.tag == "LedgeGrabBox") {
			switch(ledgeLock){
				case(LedgeLockState.Open):
					PlayerMovement candidate = col.gameObject.GetComponent<LedgeGrabCollision>().movement;
					if(candidate.canGrabLedge()){
						ledgeLock = LedgeLockState.Locked;
						lockingPlayer = candidate;
					}
					break;
				case(LedgeLockState.Tethered):
					forceDrop();
					ledgeLock = LedgeLockState.Locked;
					lockingPlayer = col.gameObject.GetComponent<LedgeGrabCollision>().movement;
					break;
				case(LedgeLockState.Locked):
					if(!lockingPlayer.ledgeHanging){
						ledgeLock = LedgeLockState.Open;
						lockingPlayer = null;
					}
					break;
				default:
					break;
			}
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		if(col.gameObject.tag == "LedgeGrabBox" && col.gameObject.GetComponent<LedgeGrabCollision>().movement == lockingPlayer && ledgeLock != LedgeLockState.Open){
			ledgeLock = LedgeLockState.Open;
			lockingPlayer = null;
		}
	}

	public void forceDrop() {
		if(lockingPlayer != null){
			lockingPlayer.JumpFromLedge();
			ledgeLock = LedgeLockState.Open;
		}
		
	}
}