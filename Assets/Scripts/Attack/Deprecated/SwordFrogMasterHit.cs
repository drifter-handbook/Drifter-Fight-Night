﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFrogMasterHit : MasterHit
{

    float chargeTime = 0;

    void Update()
    {
        if(!isHost)return;
        //Generate a new arrow every 3 seconds
        if(drifter.GetCharge() < 3)
        {
            chargeTime += Time.deltaTime;
            if(chargeTime >= 3f)
            {
                Empowered = true;
                drifter.IncrementCharge();
                chargeTime = 0;
            }
        }
        
    }

    //Neutral W

    public void fireCrossbow()
    {
        if(!isHost)return;
        facing = movement.Facing;

        //Fire an arrow if Swordfrog has a charge
        if(drifter.GetCharge() >0){
            drifter.DecrementCharge();
            
            GameObject arrow = host.CreateNetworkObject("Arrow", transform.position + new Vector3(0, 3.8f, 0), transform.rotation);
            arrow.transform.localScale = new Vector3(7.5f * facing, 7.5f, 1f);
            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * 60f, 5f);
            foreach (HitboxCollision hitbox in arrow.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            
        }

        //Spawn a smoke puff for juice

        GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.SmokeTrail, transform.position + new Vector3(facing * 4f, 3.8f, 0), transform.rotation.eulerAngles.z,new Vector2(1, 1));
        

        //Update charge count
        if(drifter.GetCharge() ==0){
            Empowered = false;
        }

    }

    //Down W, Counter Logic (Gaming)

    public void counter()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.PlayAnimation("W_Down_Success");
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
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


