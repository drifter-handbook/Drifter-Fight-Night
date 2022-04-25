using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class LucillePortal : NonplayerHurtboxHandler
{

	public GameObject drifter;

	public int size = 1;

	public float myPriority;
	public SyncAnimatorStateHost anim;

	Collider2D grabPoint;

	bool canMerge  = true;

	float speed = 28f;

	new void Start()
    {
        base.Start();
		anim = GetComponent<SyncAnimatorStateHost>();
		myPriority = size;
		if(size == -1)
			canMerge = false;

	}

	new void FixedUpdate(){
		base.FixedUpdate();

        if(grabPoint !=null && grabPoint.enabled)
            transform.position = grabPoint.bounds.center;
     
        else
        	grabPoint = null;
       
	}

	public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {

        if (GameController.Instance.IsHost && hurtbox.owner == hitbox.parent && !oldAttacks.ContainsKey(attackID))
        {
        	GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
        	grabPoint = hitbox.gameObject.GetComponent<Collider2D>();
        	return 1; 

        }

        return -3;
    }

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(!GameController.Instance.IsHost)return;

		HitboxCollision hitbox = collider.gameObject.GetComponent<HitboxCollision>();

		if(collider.gameObject.tag == "KillzoneTop")
		{
			decay();
			return;
		}

		if(collider.gameObject.tag == "Lucille_Portal" && canMerge && collider.GetComponent<LucillePortal>().drifter == drifter)
		{
			LucillePortal merging_Portal = collider.GetComponent<LucillePortal>();

			if(merging_Portal.canMerge)
			{
				myPriority += size;
				merging_Portal.contest(myPriority,this);
			}
		}

		try
		{
			if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact" && size != -1)
			{

				int direction = ((hitbox.OverrideData.AngleOfImpact > 90 && hitbox.OverrideData.AngleOfImpact < 270) ? -1: 1 ) * hitbox.Facing;

				float verticalMag = Mathf.Sin(hitbox.OverrideData.AngleOfImpact * Mathf.PI/180f);
				float horizontalMag = Mathf.Cos(hitbox.OverrideData.AngleOfImpact * Mathf.PI/180f);

				bool moveHorizontally = horizontalMag > .5f || horizontalMag <-.5f;
				bool moveVertically = verticalMag > .5f || verticalMag < -.5f;


				transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * direction, Mathf.Abs(transform.localScale.y) * Mathf.Sign(verticalMag));

				rb.velocity = speed * new Vector3((moveHorizontally?direction * .707f:0) + ((!moveHorizontally && verticalMag <0)? .4f * direction:0f), (moveVertically?Mathf.Sign(verticalMag) * .707f:0) * ((!moveHorizontally && verticalMag <0)? 2f:1f),0);

				if(moveHorizontally && moveVertically) anim.SetState("Diagonal_" + size);
				else if(moveHorizontally)  anim.SetState("Horizontal_" + size);
				else if(moveVertically)  anim.SetState("Vertical_" + size);

				myPriority = -3;


				foreach (HitboxCollision portalHitbox in GetComponentsInChildren<HitboxCollision>(true))
				{
					portalHitbox.AttackID -= 3;
					portalHitbox.Facing = direction;
				}

				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
			}
			else if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact" && size == -1)
			{
				decay();
				drifter.GetComponentInChildren<LucilleMasterHit>().SpawnRift(transform.position);

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
		if(myPriority < enemyPriority && myPriority != -1)
		{
			decay();
			drifter.GetComponentInChildren<LucilleMasterHit>().breakRift(gameObject);
			other.grow(size);
		}
		
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

		foreach (HitboxCollision portalHitbox in GetComponentsInChildren<HitboxCollision>(true))portalHitbox.AttackID -= UnityEngine.Random.Range(5,10);
		
		canMerge = false;
		rb.velocity = Vector2.zero;
		UnityEngine.Debug.Log("Rift_Detonate_" + size);
		anim.SetState("Rift_Detonate_" + size);
		Shake.startShakeCoroutine(.16f, size/2f);
	}

	public void decay()
	{
		if(!GameController.Instance.IsHost)return;
		canMerge = false;
		rb.velocity = Vector2.zero;
		anim.SetState("Rift_Decay_" + size);
	}
}
