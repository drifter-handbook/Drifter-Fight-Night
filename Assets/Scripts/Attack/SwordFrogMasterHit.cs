using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFrogMasterHit : MasterHit
{

    float chargeTime = 0;

    void Update()
    {

        //Generate a new arrow every 3 seconds
        if(drifter.Charge < 3)
        {
            chargeTime += Time.deltaTime;
            if(chargeTime >= 3f)
            {
                drifter.SetAnimatorBool("HasCharge",true);
                drifter.Charge++;
                chargeTime = 0;
            }
        }
        
    }

    //Neutral W

    public void fireCrossbow()
    {
        facing = movement.Facing;

        //Fire an arrow if Swordfrog has a charge
        if(drifter.Charge >0){
            drifter.Charge--;
            if (GameController.Instance.IsHost)
            {
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
        }

        //Spawn a smoke puff for juice
        if (GameController.Instance.IsHost)
        {
            GameObject poof = host.CreateNetworkObject("MovementParticle", transform.position + new Vector3(facing * 4f, 3.8f, 0), transform.rotation);
            poof.GetComponent<JuiceParticle>().mode = MovementParticleMode.SmokeTrail;
        }

        //Update charge count
        if(drifter.Charge ==0){
            drifter.SetAnimatorBool("HasCharge",false);
        }

    }
 
    //Down W, Counter Logic (Gaming)

    public void counter()
    {
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.SetAnimatorBool("Empowered",true);
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        }
    }

    public void hitCounter()
    {
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        StartCoroutine(resetCounter());
        
    }

    IEnumerator resetCounter()
    {
        yield return new WaitForSeconds(.3f);
        drifter.SetAnimatorBool("Empowered",false);
    }

    //Roll Methods

    public override void roll()
    {
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }


     public override void rollGetupStart()
     {
        applyEndLag(1);
        rb.velocity = new Vector3(facing * -5f,40f,0);
    }

    public override void rollGetupEnd()
    {
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 30f,5f);
    }
}


