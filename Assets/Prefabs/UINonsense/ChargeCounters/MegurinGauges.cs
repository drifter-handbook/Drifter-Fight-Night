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
    	wind.transform.localScale = new Vector2(megurin.windCharge/50f,1f);
    	lightning.transform.localScale = new Vector2(megurin.lightningCharge/50f,1f);

    }

}