using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TongueLash : MonoBehaviour
{

	public Drifter drifter;
	public string state;

    void OnTriggerEnter2D(Collider2D collision)
    {

    	if(state != "" && (collision.gameObject.tag =="Ground" || collision.gameObject.tag =="Platform"))
    	{
    		drifter.PlayAnimation(state);
    	}

    }
}
