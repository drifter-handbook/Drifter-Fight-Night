using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterCannonMasterHit : MasterHit
{

    public void SairExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(1.6f * facing,2f,0);
        
        GameObject orroSplosion = host.CreateNetworkObject("ExplosionSide", transform.position + pos, transform.rotation);
        orroSplosion.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.AttackData = attacks.Attacks[attacks.AttackType];
            hitbox.Active = true;
            hitbox.Facing = facing;
       }
    }


    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0f,35f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 30f,5f);
    }
}


