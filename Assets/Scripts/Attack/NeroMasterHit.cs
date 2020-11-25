using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroMasterHit : MasterHit
{
    public Animator anim;
    int dashDistance = 25;

    public void RecoveryThrowSpear()
    {
        // jump upwards and create spear projectile
        if(!isHost)return;
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * 35f);
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        
            GameObject neroSpear = host.CreateNetworkObject("NeroSpear", transform.position, transform.rotation);
            foreach (HitboxCollision hitbox in neroSpear.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            
        }
    }


    //Neutral W  logic


    public  void neutralWInitialize()
    {
        if(!isHost)return;
        facing = movement.Facing;
        dashDistance = 25;
    }

     public void neutralWCharge(int cancelable)
     {
        if(!isHost)return;
        movement.gravityPaused= true;        
        rb.gravityScale = 5f;
        if(cancelable != 0)
        {
            if(TransitionFromChanneledAttack()) return;
            if(drifter.input.MoveX != 0 || drifter.input.Special || dashDistance>=55) drifter.SetAnimatorBool("HasCharge",true);
        }

        dashDistance += 3;
     }

     public void neutralWDash()
     {
        if(!isHost)return;
        rb.velocity = new Vector3( facing * dashDistance, 0);
        movement.gravityPaused= true;
        rb.gravityScale = 0;

        attacks.SetupAttackID(DrifterAttackType.W_Neutral);

        drifter.SetAnimatorBool("HasCharge",false);
    }



    //Counter Logic

    public void counter()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.SetAnimatorBool("Empowered",true);
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        }
        

    }
    public void hitCounter()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        StartCoroutine(resetCounter());
        
    }

    IEnumerator resetCounter(){
        yield return new WaitForSeconds(.3f);
        drifter.SetAnimatorBool("Empowered",false);
    }

    //Inhereted Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }

    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1f);
        rb.velocity = new Vector3(0,75f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 35f,0f);
    }
}
