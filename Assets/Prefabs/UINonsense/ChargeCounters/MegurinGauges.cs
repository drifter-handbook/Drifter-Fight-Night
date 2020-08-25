using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinGauges : MonoBehaviour
{

	public MegurinMasterHit megurin;
	public GameObject ice;
	public GameObject wind;
	public GameObject lightning;

 	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
    	ice.transform.localScale = new Vector2(megurin.iceCharge/50f, 1f);
    	if(ice.transform.localScale.x ==1)
    	{
    		ice.GetComponent<SpriteRenderer>().color = new Color(1f,1f,.5f);
    	}
    	else{
    		ice.GetComponent<SpriteRenderer>().color = Color.white;
    	}
    	wind.transform.localScale = new Vector2(megurin.windCharge/50f,1f);
    	if(wind.transform.localScale.x ==1)
    	{
    		wind.GetComponent<SpriteRenderer>().color = new Color(1f,1f,.5f);
    	}
    	else{
    		wind.GetComponent<SpriteRenderer>().color = Color.white;
    	}
    	lightning.transform.localScale = new Vector2(megurin.lightningCharge/50f,1f);
    	if(lightning.transform.localScale.x ==1)
    	{
    		lightning.GetComponent<SpriteRenderer>().color = new Color(1f,1f,.5f);
    	}
    	else{
    		lightning.GetComponent<SpriteRenderer>().color = Color.white;
    	}

    }

}