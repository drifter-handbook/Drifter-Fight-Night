using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{
 	GameObject slowfield;

	//Down W, Counter Logic (Gaming)

    public void counter()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.PlayAnimation("W_Down_Success");
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.2f);
        }
    }


    public void spawnSlowZone()
    {

    	if(!isHost)return;

        Vector3 pos = new Vector3(0f, 4.5f, 0f);
        //TODO Add delete animation here
        if (slowfield)Destroy(slowfield);
        slowfield = host.CreateNetworkObject("myth_slowfield", transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in slowfield.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID + 150;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

        
        

        slowfield.GetComponent<MultihitZoneProjectile>().attacks = attacks;
    }

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,75f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
