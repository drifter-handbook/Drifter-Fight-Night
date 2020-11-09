using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabCollision : MonoBehaviour
{
	public PlayerMovement movement;
	BoxCollider2D ledgeBox;
    // Start is called before the first frame update
    void Start()
    {
        ledgeBox = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame

    void OnTriggerEnter2D(Collider2D col){

    	if(col.gameObject.tag == "Ledge" && !col.GetComponent<HopUp>().locked){
    		UnityEngine.Debug.Log("LEDGE GRAB!");
    		movement.GrabLedge(col.gameObject.transform.position);
    	}

    }

    void OnTriggerExit2D(Collider2D col){

    	if(col.gameObject.tag == "Ledge"){
    		UnityEngine.Debug.Log("LEDGE DROPPED!");
    		movement.DropLedge();

    		StartCoroutine(FlicerLedgebox());
    	}
    }

    IEnumerator FlicerLedgebox(){
    	ledgeBox.enabled = false;
    	yield return new WaitForSeconds(.5f);
    	ledgeBox.enabled = true;
    }
}
