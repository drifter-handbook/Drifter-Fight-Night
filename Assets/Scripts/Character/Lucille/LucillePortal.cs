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

	float speed = 28f;

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

		if(collider.gameObject.tag == "Lucille_Portal" && canMerge && collider.GetComponent<LucillePortal>().drifter == drifter)
		{
			LucillePortal merging_Portal = collider.GetComponent<LucillePortal>();

			myPriority += size;

			merging_Portal.contest(myPriority,this);

		}

		try
		{
			if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact" )
			{

				float verticalMag = Mathf.Sin(hitbox.OverrideData.AngleOfImpact * Mathf.PI/180f);
				float horizontalMag = Mathf.Cos(hitbox.OverrideData.AngleOfImpact * Mathf.PI/180f);

				UnityEngine.Debug.Log(hitbox.OverrideData.AngleOfImpact);

				UnityEngine.Debug.Log("Y : " + verticalMag);
				UnityEngine.Debug.Log("X : " + horizontalMag);

				bool moveHorizontally = horizontalMag > .5f || horizontalMag <-.5f;
				bool moveVertically = verticalMag > .5f || verticalMag < -.5f;


				transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * hitbox.Facing, Mathf.Abs(transform.localScale.y) * Mathf.Sign(verticalMag));

				rb.velocity = speed * new Vector3(moveHorizontally?hitbox.Facing * .707f:0, moveVertically?Mathf.Sign(verticalMag) * .707f:0,0);

				if(moveHorizontally && moveVertically) anim.SetState("Diagonal_" + size);
				else if(moveHorizontally)  anim.SetState("Horizontal_" + size);
				else if(moveVertically)  anim.SetState("Vertical_" + size);

				myPriority = -3;


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
		if(myPriority < enemyPriority)
		{
			other.grow(size);
			decay();
			drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(gameObject);
		}

		// else 
		// {
		// 	grow(other.size);
		// 	other.decay();
		// 	drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(other.gameObject);
		// }
		
	}

	public void grow(int growthIncrement)
	{

		UnityEngine.Debug.Log(size + " " +  growthIncrement);
		size += growthIncrement;
		rb.drag = size -.5f ;
		speed = 30f - 2*size; 

		myPriority = size;

		if(size < 4 && growthIncrement <3) anim.SetState("Grow_" + size);
		else anim.SetState("Rift_Detonate_3");
		
	}

	public void detonate()
	{
		if(!GameController.Instance.IsHost)return;

		foreach (HitboxCollision portalHitbox in GetComponentsInChildren<HitboxCollision>(true))portalHitbox.AttackID -= 9;
		
		canMerge = false;
		rb.velocity = Vector2.zero;
		anim.SetState("Rift_Detonate_" + size);
	}

	public void decay()
	{
		if(!GameController.Instance.IsHost)return;
		canMerge = false;
		rb.velocity = Vector2.zero;
		anim.SetState("Rift_Decay_" + size);
	}
}
