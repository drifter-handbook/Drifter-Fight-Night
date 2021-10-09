using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherRange : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 TetherPoint;

    void OnTriggerStay2D(Collider2D col)
    {
        if(col.gameObject.tag == "Ledge")
        {
            
            TetherPoint = col.gameObject.transform.position; 
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.tag == "Ledge")
        {
            TetherPoint = Vector3.zero;
        }
    }
}
