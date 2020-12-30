using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterCannonMasterHit : MasterHit
{

    float boostTime = 1.3f;



    public void SairExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(1.6f * facing,2f,0);
        
        GameObject explosion = host.CreateNetworkObject("ExplosionSide", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.AttackData = attacks.Attacks[attacks.AttackType];
            hitbox.Active = true;
            hitbox.Facing = facing;
       }
    }


    public void SideWExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(-.7f * facing,2.7f,0);
        
        GameObject explosion = host.CreateNetworkObject("ExplosionSide", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(-10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.AttackData = attacks.Attacks[attacks.AttackType];
            hitbox.Active = true;
            hitbox.Facing = facing;
       }
    }

    public void applyLandingLag()
    {
        movement.canLandingCancel = true;
        boostTime = 1.3f;
    }

     public void upWGlide()
    {
        if(!isHost)return;

        if(TransitionFromChanneledAttack())return;
     

        else if(drifter.input.MoveY <0 || movement.grounded)
        {
            returnToIdle();
        }
        else
        {
            movement.updateFacing();
            rb.velocity = new Vector2(Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)? drifter.input.MoveX * 20f:(.6f*20f)),rb.velocity.x,.75f),(drifter.input.MoveY >0?Mathf.Lerp(35f,rb.velocity.y,.45f):rb.velocity.y));
            if(drifter.input.MoveY > 0)
            {
                drifter.PlayAnimation("W_Up_Loop");
                boostTime -= .1f;
            } 
            else drifter.PlayAnimation("W_Up_Idle");
            if(boostTime <=0)drifter.PlayAnimation("W_Up_End");
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


