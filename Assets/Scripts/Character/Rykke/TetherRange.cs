using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetherRange : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector3 TetherPoint;
    public GameObject g_obj;

    void OnTriggerStay2D(Collider2D col)
    {
        if(col.gameObject.tag == "Ledge")
        {
            TetherPoint = col.gameObject.transform.position; 
            g_obj = col.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if(col.gameObject.tag == "Ledge")
        {
            TetherPoint = Vector3.zero;
            g_obj = null;
        }
    }
}
