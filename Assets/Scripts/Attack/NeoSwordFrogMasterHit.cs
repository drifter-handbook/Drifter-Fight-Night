﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{

    float chargeTime = 0;

    Coroutine kunaiShoot;

    void Update()
    {
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            if(kunaiShoot != null)StopCoroutine(kunaiShoot);
            if(drifter.GetCharge() > 0)drifter.SetCharge(0);
        }
    }

    public void charge_W_Neutral(int grantCharge)
    {
         if(!isHost)return;
         if(chargeAttackPesistent("W_Neutral_Fire") !=0)return;
         else if(grantCharge >=1)
         {
            drifter.IncrementCharge();
            if(drifter.GetCharge()>=3)
            {
                Empowered = true;
                returnToIdle();
            }
        }
    }


    public void neutralSpecialProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Empowered = false;
        //Fire an arrow if Swordfrog has a charge
        kunaiShoot = StartCoroutine(fireKunaiNeutral());

    }

    IEnumerator fireKunaiNeutral()
    {


        int projnum = drifter.GetCharge() * 2;

        while(projnum >= 0)
        {
            yield return new WaitForSeconds(framerateScalar * .3f);
            GameObject arrow = host.CreateNetworkObject("Arrow", transform.position + new Vector3(0, 3f + projnum * .1f, 0), transform.rotation);
            arrow.transform.localScale = new Vector3(10f * facing, 10f, 1f);
            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * (65f - 2 * projnum), (projnum *5 -5f));
            foreach (HitboxCollision hitbox in arrow.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            projnum--;

            attacks.SetupAttackID(DrifterAttackType.W_Neutral);
            if(projnum%2 ==0)drifter.DecrementCharge();
        }
        if(drifter.GetCharge() < 0)drifter.SetCharge(0);
        yield break;

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


