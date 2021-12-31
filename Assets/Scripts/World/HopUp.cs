using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class HopUp : MonoBehaviour
{

    public bool locked = false;
    Collider2D lockingPlayer;

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.tag == "LedgeGrabBox" && !locked)
        {
            locked = true;
            lockingPlayer = col;

        }
    }

    void OnTriggerExit2D(Collider2D col){
        if(col == lockingPlayer && locked){
            //StartCoroutine(Unlock());
            locked = false;
        }
    }

    IEnumerator Unlock(){
        yield return new WaitForSeconds(.083333f);
        locked = false;
    }
}