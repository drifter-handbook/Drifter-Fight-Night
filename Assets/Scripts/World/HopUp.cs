using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum LedgeLockState{
	Open, Locked, Tethered
}

public class HopUp : MonoBehaviour {
	public LedgeLockState ledgeLock = LedgeLockState.Open;
	PlayerMovement lockingPlayer;

	void OnTriggerStay2D(Collider2D col) {
		if (col.gameObject.tag == "LedgeGrabBox") {
			switch(ledgeLock){
				case(LedgeLockState.Open):
					ledgeLock = LedgeLockState.Locked;
					lockingPlayer = col.gameObject.GetComponent<LedgeGrabCollision>().movement;
					break;
				case(LedgeLockState.Tethered):
					if(col.gameObject.GetComponent<LedgeGrabCollision>().movement != lockingPlayer){
						forceDrop();
						ledgeLock = LedgeLockState.Locked;
						lockingPlayer = col.gameObject.GetComponent<LedgeGrabCollision>().movement;
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
		}
	}

	public void forceDrop() {
		if(lockingPlayer != null){
			lockingPlayer.JumpFromLedge();
			ledgeLock = LedgeLockState.Open;
		}
		
	}
}