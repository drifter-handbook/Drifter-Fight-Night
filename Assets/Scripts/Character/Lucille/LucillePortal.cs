using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class LucillePortal : MonoBehaviour
{

	public GameObject drifter;

	void OnTriggerEnter2D(Collider2D collider)
    {
        if(!GameController.Instance.IsHost)return;

        HitboxCollision hitbox = collider.gameObject.GetComponent<HitboxCollision>();

    	if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact")
    	{
    		GetComponent<Rigidbody2D>().velocity = new Vector3(hitbox.Facing * ((hitbox.OverrideData.AngleOfImpact < 45f && hitbox.OverrideData.AngleOfImpact > -45f)?35f:0f),((hitbox.OverrideData.AngleOfImpact > 45f || hitbox.OverrideData.AngleOfImpact < -45f)?35f:0f),0);

    		GetComponent<SyncAnimatorStateHost>().SetState("Move_up");

    		GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
    	}

    }
    
}
