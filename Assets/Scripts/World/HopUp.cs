using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HopUp : MonoBehaviour
{

    public bool locked = false;
    Collider2D lockingPlayer;

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "LedgeGrabBox" && !locked)
        {
        	UnityEngine.Debug.Log("CORNER LOCKED");
            locked = true;
            lockingPlayer = col;

        }
    }

    void OnTriggerExit2D(Collider2D col){
        if(col == lockingPlayer && locked){
            StartCoroutine(Unlock());
        }
    }

    IEnumerator Unlock(){
        yield return new WaitForSeconds(.5f);
        UnityEngine.Debug.Log("CORNER UNLOCKED!");
        locked = false;
    }
}