using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class LucillePortal : MonoBehaviour
{

	public GameObject drifter;

	public int size = 1;

	public float myPriority;
	public SyncAnimatorStateHost anim;

	void Start()
	{
		anim = GetComponent<SyncAnimatorStateHost>();
		myPriority = size;
	}


	// void OnTriggerEnter2D(Collider2D collider)
	// {
	// 	if(!GameController.Instance.IsHost)return;

	// 	UnityEngine.Debug.Log("HIT");

	// 	if(collider.gameObject.tag == "Lucille_Portal")
	// 	{
	// 		LucillePortal merging_Portal = collider.GetComponent<LucillePortal>();

	// 		if(merging_Portal.drifter == drifter)
	// 		{
	// 			if(merging_Portal.HandleMerge(myPriority))
	// 			{
	// 				size += 1;
	// 				anim.SetState("Rift_" + size);

	// 			}

	// 		}
	// 		else anim.SetState("Rift_Decay_" + size); 
	// 	}
	// }


	void OnTriggerEnter2D(Collider2D collider)
	{
		if(!GameController.Instance.IsHost)return;

		HitboxCollision hitbox = collider.gameObject.GetComponent<HitboxCollision>();

		if(collider.gameObject.tag == "Lucille_Portal")
		{
			LucillePortal merging_Portal = collider.GetComponent<LucillePortal>();

			if(merging_Portal.drifter == drifter)
			{
				myPriority = size + UnityEngine.Random.Range(0f, 100f) / 100f;

				if(myPriority > merging_Portal.myPriority && merging_Portal.size !=3)
				{
					
					size++;
					if(size >3) size=3;

					anim.SetState("Rift_" + size);

				}
				else
				{
					anim.SetState("Rift_Decay_" + size);
					drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(this.gameObject);

				} 

			}
			
		}


		try
		{
			if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact" )
			{
				GetComponent<Rigidbody2D>().velocity = new Vector3(hitbox.Facing * ((hitbox.OverrideData.AngleOfImpact < 45f && hitbox.OverrideData.AngleOfImpact > -30f)?35f:0f),(hitbox.OverrideData.AngleOfImpact > 45f ?35f:(hitbox.OverrideData.AngleOfImpact > 20?0:-35f )),0);

				float moveDirection = hitbox.OverrideData.AngleOfImpact;

				// if((moveDirection > 45f && moveDirection < 135f))anim.SetState("Move_up");
				// else if(hitbox.Facing > 0)anim.SetState("Move_Right");
				// else anim.SetState("Move_Left");

				foreach (HitboxCollision portalHitbox in GetComponentsInChildren<HitboxCollision>(true))
				{
					portalHitbox.AttackID -= 3;
					portalHitbox.Facing = hitbox.Facing;
				}

				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
			}

			else if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Detonate")
			{
				drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(this.gameObject);
				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
			}

		}

		catch(NullReferenceException)
		{
			return;
		}
	}

	public void detonate()
	{
		if(!GameController.Instance.IsHost)return;
		anim.SetState("Rift_Detonate_" + size);
	}

	public void decay()
	{
		if(!GameController.Instance.IsHost)return;
		anim.SetState("Rift_Decay_" + size);
	}
}
