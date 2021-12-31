using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandbagMasterHit : MasterHit
{
	bool rolling = false;
	GameObject sandblast;
	int dustCount = 7;
	new void FixedUpdate()
	{
		base.FixedUpdate();
		if(status.HasEnemyStunEffect() || movement.ledgeHanging)
			rolling = false;
		else if(rolling && movement.grounded)
		{
			if(dustCount > 7f)
			{
				dustCount = 0;
				movement.spawnKickoffDust();
			}
			rb.velocity = new Vector2(movement.Facing * 33f * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
        	status.saveXVelocity(movement.Facing *33f);
        	dustCount +=1;
		}
	}

	public void SetRolling()
	{
		rolling = true;
		dustCount = 7;
	}

	new public void clearMasterhitVars()
	{
		base.clearMasterhitVars();
		rolling = false;
	}

	public void Neutral_Special()
	{
		if(!isHost)return;

		facing = movement.Facing;
		if(sandblast!= null) Destroy(sandblast);
		sandblast = host.CreateNetworkObject("Sandblast", transform.position + new Vector3(1.5f * movement.Facing, 2.5f), transform.rotation);
		sandblast.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in sandblast.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
		sandblast.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
		sandblast.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 25f,0,0);
	}

	public void Ground_Down()
	{
		if(!isHost)return;

		facing = movement.Facing;

		GameObject sandspear1 = host.CreateNetworkObject("Sandspear", transform.position + new Vector3(1.2f * movement.Facing, 1.3f,1), transform.rotation);
		GameObject sandspear2 = host.CreateNetworkObject("Sandspear", transform.position + new Vector3(-1.5f * movement.Facing, 1.3f,-1), transform.rotation);
		sandspear1.transform.localScale = new Vector3(10f * facing, 10f , 1f);
		sandspear2.transform.localScale = new Vector3(-10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in sandspear1.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        foreach (HitboxCollision hitbox in sandspear2.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
		sandspear1.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
		sandspear2.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
		sandspear1.transform.SetParent(drifter.gameObject.transform);
		sandspear2.transform.SetParent(drifter.gameObject.transform);
	}

}
