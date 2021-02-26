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
	bool canMerge  = true;
	Rigidbody2D rb;

	float speed = 30f;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		anim = GetComponent<SyncAnimatorStateHost>();
		myPriority = size;
	}

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(!GameController.Instance.IsHost)return;

		HitboxCollision hitbox = collider.gameObject.GetComponent<HitboxCollision>();

		if(collider.gameObject.tag == "Lucille_Portal" && collider.GetComponent<LucillePortal>().drifter == drifter)
		{
			canMerge = true;
			LucillePortal merging_Portal = collider.GetComponent<LucillePortal>();

			myPriority +=size + UnityEngine.Random.Range(0f,.100f);

			merging_Portal.contest(myPriority,this);

		}

		try
		{
			if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact" )
			{
				rb.velocity = speed * new Vector3(hitbox.Facing * ((hitbox.OverrideData.AngleOfImpact < 45f && hitbox.OverrideData.AngleOfImpact > -30f)?1f:0f),(hitbox.OverrideData.AngleOfImpact > 45f ?1f:(hitbox.OverrideData.AngleOfImpact > 20?0:-1f )),0);

				float moveDirection = hitbox.OverrideData.AngleOfImpact;

				myPriority = -3;

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
				drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(gameObject,true);
				detonate();
				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
			}

		}

		catch(NullReferenceException)
		{
			return;
		}
	}

	public void contest(float enemyPriority, LucillePortal other)
	{
		if(!canMerge)return;
		other.canMerge = false;

		myPriority += size + UnityEngine.Random.Range(0f,.100f);


		if(myPriority < enemyPriority)
		{
			other.grow(size);
			decay();
			drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(gameObject);
		}
		else 
		{
			grow(other.size);
			other.decay();
			drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(other.gameObject);
		}
		
	}

	public void grow(int growthIncrement)
	{
		UnityEngine.Debug.Log(size + " " +  growthIncrement);
		size += growthIncrement;
		rb.drag = size -.5f ;
		speed = 30f - 3*size; 
		anim.SetState("Rift_" + size);
	}

	public void detonate()
	{
		if(!GameController.Instance.IsHost)return;
		rb.velocity = Vector2.zero;
		anim.SetState("Rift_Detonate_" + size);
	}

	public void decay()
	{
		if(!GameController.Instance.IsHost)return;
		rb.velocity = Vector2.zero;
		anim.SetState("Rift_Decay_" + size);
	}
}
