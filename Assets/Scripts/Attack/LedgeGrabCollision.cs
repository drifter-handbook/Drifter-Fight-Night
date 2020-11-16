using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LedgeGrabCollision : MonoBehaviour
{
	public PlayerMovement movement;
	public PlayerStatus status;
	BoxCollider2D ledgeBox;
    Coroutine insurance;
    // Start is called before the first frame update
    void Start()
    {
        ledgeBox = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame

    void OnTriggerEnter2D(Collider2D col){

    	if(col.gameObject.tag == "Ledge" && !col.GetComponent<HopUp>().locked && !status.HasEnemyStunEffect()){
    		UnityEngine.Debug.Log("LEDGE GRAB!");
    		movement.GrabLedge(col.gameObject.transform.position);
            if(insurance !=null)
            {
                StopCoroutine(insurance);
                insurance = StartCoroutine(makeSureItHappens(col));
            }
    	}

    }

    IEnumerator makeSureItHappens(Collider2D col){
        yield return new WaitForSeconds(.2f);

        movement.GrabLedge(col.gameObject.transform.position);
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
