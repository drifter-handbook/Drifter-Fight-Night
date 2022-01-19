using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterCannonMasterHit : MasterHit
{

    float boostTime = 2.6f;
    bool jumpGranted = false;
    int charge = 1;

    bool listeningForWallbounce = false;

    void Update()
    {
    	if(!isHost)return;
    	if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
    	{
    		Empowered = false;
            drifter.sparkle.SetState("Hide");
            jumpGranted = false;
    		SetCharge(1);
    	}

        if(jumpGranted && movement.grounded)jumpGranted = false;

        if(movement.wallSliding != Vector3.zero && listeningForWallbounce)
        {
        	listeningForWallbounce = false;
            drifter.PlayAnimation("W_Side_End_Early");
            rb.velocity = new Vector2(movement.Facing * -15f,30f);
            if(!jumpGranted && movement.currentJumps <= movement.numberOfJumps -1) movement.currentJumps++;
            jumpGranted = true;
            GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Restitution,rb.position + new Vector2(facing * .5f,0), (facing > 0)?90:-90,Vector3.one);
            unpauseGravity();
        }
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

    public void listenForWallBounce()
    {
        if(!isHost)return;
        listeningForWallbounce = true;
    }

    public new void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        listeningForWallbounce = false;
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
        boostTime = 2.6f;
    }

     public void upWGlide()
    {
        if(!isHost)return;

        movement.updateFacing();
        rb.velocity = new Vector2(Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)? drifter.input[0].MoveX * 20f:(.6f*20f)),rb.velocity.x,.75f),(drifter.input[0].MoveY >0?Mathf.Lerp(35f,rb.velocity.y,.45f):rb.velocity.y));
        if(drifter.input[0].MoveY > 0 && activeCancelFlag)
        {
            drifter.PlayAnimation("W_Up_Loop");
            boostTime -= .1f;
        } 
        else if(activeCancelFlag) drifter.PlayAnimation("W_Up_Idle");
        if(boostTime <=0)drifter.PlayAnimation("W_Up_End");
        
    }

    //W_Neutral

    public void handleRanchStartup()
    {
    	if(!isHost)return;
    	//sets all special inputs to true to "clear" it

    	foreach(PlayerInputData input in drifter.input)
    		input.Special = true;
    	listenForSpecialTapped("W_Neutral_Fire");
        if(Empowered) drifter.PlayAnimation("W_Neutral_Fire");
    	else if(charge > 1) drifter.PlayAnimation("W_Neutral_" + charge);
    }

    public void SetCharge(int charge)
    {
    	if(!isHost)return;
    	this.charge = charge;
    	Empowered = (charge == 3);

        if(Empowered)drifter.sparkle.SetState("ChargeIndicator");
        else drifter.sparkle.SetState("Hide");

    	drifter.WalkStateName = Empowered?"Walk_Ranch":"Walk";
        drifter.GroundIdleStateName = Empowered?"Idle_Ranch":"Idle";
        drifter.JumpStartStateName = Empowered?"Jump_Start_Ranch":"Jump_Start";
        drifter.AirIdleStateName = Empowered?"Hang_Ranch":"Hang";
    }

     public void FireRanchProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(1f * facing,2.7f,0);
        
        GameObject ranch = host.CreateNetworkObject("Ranch" + charge, transform.position + pos, transform.rotation);
        ranch.transform.localScale = new Vector3(10f * facing, 10f , 1f);

        rb.velocity = new Vector2((charge - 1) * -15f* facing,0);
        
        if(charge < 3)ranch.GetComponent<Rigidbody2D>().velocity = new Vector2((charge == 1?55f:25f)* facing,0);

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
}


