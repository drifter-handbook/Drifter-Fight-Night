using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterCannonMasterHit : MasterHit
{

    float boostTime = 1.3f;
    bool jumpGranted = false;

    void Update()
    {
    	if(!isHost)return;
    	if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
    	{
    		Empowered = false;
            jumpGranted = false;
    		SetCharge(1);
    	}

        if(jumpGranted && movement.grounded)jumpGranted = false;


    }

    public void SairExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(1.9f * facing,3.3f,0);
        
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
        Vector3 pos = new Vector3(-1.5f * facing,2.7f,0);
        
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

    public void wallBounce()
    {
        if(!isHost)return;

        if(movement.wallSliding != Vector3.zero)
        {
            rb.velocity = new Vector2(movement.Facing * -15f,30f);
            drifter.PlayAnimation("W_Side_End_Early");
            if(!jumpGranted && movement.currentJumps <= movement.numberOfJumps -1) movement.currentJumps++;
            jumpGranted = true;
            GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Restitution,rb.position + new Vector2(facing * .5f,0), (facing > 0)?90:-90,Vector3.one);
            unpauseGravity();
        }
    }

    public void UpAirExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(-.4f* facing,2f,0);
        
        GameObject explosion = host.CreateNetworkObject("UairExplosion", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f* facing, 10f, 1f);
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

    public void DownSpecialBomb()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(-.5f * facing,2.7f,0);
        
        GameObject grenade = host.CreateNetworkObject("DCGenade", transform.position + pos, transform.rotation);
        grenade.transform.localScale = new Vector3(10f * facing, 10f , 1f);

        grenade.GetComponent<Rigidbody2D>().velocity = new Vector2(20* facing,25);

        foreach (HitboxCollision hitbox in grenade.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
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

        if(cancelAttack())return;
     

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

    //W_Neutral

    public void handleRanchStartup()
    {
    	if(!isHost)return;
    	if(drifter.GetCharge() > 1) drifter.PlayAnimation("W_Neutral_" + drifter.GetCharge());
    }

    public void SetCharge(int charge)
    {
    	if(!isHost)return;
    	drifter.SetCharge(charge);
    	Empowered = (charge == 3);

    	drifter.WalkStateName = Empowered?"Walk_Ranch":"Walk";
        drifter.GroundIdleStateName = Empowered?"Idle_Ranch":"Idle";
        drifter.JumpStartStateName = Empowered?"Jump_Start_Ranch":"Jump_Start";
        drifter.AirIdleStateName = Empowered?"Hang_Ranch":"Hang";
    }

    public void neutralWCharge()
     {
        if(!isHost)return;
        chargeAttackPesistent("W_Neutral_Fire");
     }

     public void FireRanchProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(1f * facing,2.7f,0);
        
        GameObject ranch = host.CreateNetworkObject("Ranch" + drifter.GetCharge(), transform.position + pos, transform.rotation);
        ranch.transform.localScale = new Vector3(10f * facing, 10f , 1f);

        rb.velocity = new Vector2((drifter.GetCharge() - 1) * -15f* facing,0);
        
        if(drifter.GetCharge() < 3)ranch.GetComponent<Rigidbody2D>().velocity = new Vector2((drifter.GetCharge() == 1?40f:20f)* facing,0);

        SetCharge(1);

        foreach (HitboxCollision hitbox in ranch.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
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
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,4f * framerateScalar);
        rb.velocity = new Vector2(facing * 35f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.position += new Vector2(facing * 1f,4.5f);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,4f * framerateScalar);
        rb.velocity = new Vector2(facing * 35f,5f);
    }
}


