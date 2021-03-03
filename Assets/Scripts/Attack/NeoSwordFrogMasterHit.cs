using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{

    float chargeTime = 0;

    void Update()
    {
               
    }

    public void charge_W_Neutral(int grantCharge)
    {
         if(!isHost)return;
         if(chargeAttackSingleUse("W_Neutral_Fire") == 1)drifter.SetCharge(0);
         else if(grantCharge >=1)
         {
            drifter.IncrementCharge();
            if(grantCharge ==2)playState("W_Neutral_Fire");
        }
    }


    public void neutralSpecialProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;

        //Fire an arrow if Swordfrog has a charge
        StartCoroutine(fireKunai());

    }

    IEnumerator fireKunai()
    {
        while(drifter.GetCharge() >= 0)
        {
            yield return new WaitForSeconds(framerateScalar * .5f);
            GameObject arrow = host.CreateNetworkObject("Arrow", transform.position + new Vector3(0, 3f + drifter.GetCharge() * .1f, 0), transform.rotation);
            arrow.transform.localScale = new Vector3(10f * facing, 10f, 1f);
            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing *( 60f - drifter.GetCharge() * 3.5f), drifter.GetCharge() * 4f - 3f);
            foreach (HitboxCollision hitbox in arrow.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            attacks.SetupAttackID(DrifterAttackType.W_Neutral);
            drifter.DecrementCharge();
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


