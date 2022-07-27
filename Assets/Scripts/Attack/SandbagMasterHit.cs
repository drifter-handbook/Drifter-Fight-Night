﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandbagMasterHit : MasterHit
{
	bool dust = false;
	GameObject sandblast;
	int dustCount = 7;

	override protected void UpdateMasterHit()
    {
        base.UpdateMasterHit();

        if(status.HasEnemyStunEffect() || movement.ledgeHanging)
			dust = false;
		else if(dust)
		{
			if(dustCount > 3f)
			{
				dustCount = 0;
				movement.spawnJuiceParticle(transform.position + new Vector3(movement.Facing * -1.4f,2.7f,0), MovementParticleMode.SmokeTrail);
			}
        	dustCount +=1;
		}

    }

	public void Setdust()
	{
		dust = true;
		
		GameObject ring = GameController.Instance.host.CreateNetworkObject("LaunchRing", transform.position,  transform.rotation);
		ring.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		dustCount = 3;
	}
	public void disableDust()
	{
		dust = false;
	}

	public new void returnToIdle()
    {
        base.returnToIdle();
        dust = false;
    }

	public override void clearMasterhitVars()
	{
		base.clearMasterhitVars();
		dust = false;
	}

	public void Neutral_Special()
	{
		if(!isHost)return;

		
		if(sandblast!= null) Destroy(sandblast);
		sandblast = host.CreateNetworkObject("Sandblast", transform.position + new Vector3(1.5f * movement.Facing, 2.5f), transform.rotation);
		sandblast.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in sandblast.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            
            hitbox.Facing = movement.Facing;
        }
		sandblast.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
		sandblast.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 25f,0,0);
	}

	public void Ground_Down()
	{
		if(!isHost)return;

		

		GameObject sandspear1 = host.CreateNetworkObject("Sandspear", transform.position + new Vector3(1.2f * movement.Facing, 1.3f,1), transform.rotation);
		GameObject sandspear2 = host.CreateNetworkObject("Sandspear", transform.position + new Vector3(-1.5f * movement.Facing, 1.3f,-1), transform.rotation);
		sandspear1.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		sandspear2.transform.localScale = new Vector3(-10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in sandspear1.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            
            hitbox.Facing = movement.Facing;
        }
        foreach (HitboxCollision hitbox in sandspear2.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            
            hitbox.Facing = movement.Facing;
        }
		sandspear1.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
		sandspear2.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
		sandspear1.transform.SetParent(drifter.gameObject.transform);
		sandspear2.transform.SetParent(drifter.gameObject.transform);
	}

}
