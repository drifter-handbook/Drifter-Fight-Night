using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterCannonMasterHit : MasterHit
{

    int boostTime = 70;
    bool jumpGranted = false;
    int charge = 1;
    protected bool listeningForWallbounce = false;
    protected bool listeningForDirection = false;

    override protected void UpdateMasterHit()
    {
        base.UpdateMasterHit();

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

        if(listeningForDirection)
        {
            movement.updateFacing();
            movement.move(10f);
            rb.velocity = new Vector2(rb.velocity.x,(drifter.input[0].MoveY >0?Mathf.Lerp(20f,rb.velocity.y,.45f):rb.velocity.y));

            if(attacks.lightPressed())
            {
                attacks.useNormal();
                listeningForDirection = false;
            }

            else if(drifter.input[0].MoveY > 0)
            {
                drifter.PlayAnimation("W_Up_Loop");
                boostTime --;
            }
            else
            {
                drifter.PlayAnimation("W_Up_Idle");
            }
            if(boostTime <=0)
            {
                listeningForDirection = false;
                drifter.PlayAnimation("W_Up_End");
            }
        }

    }

    public void listenForDirection()
    {
        listeningForDirection = true;
        boostTime = 90;
        listenForGrounded("Jump_End");
    }

    public void cancelWUp()
    {
        listeningForDirection = false;
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
            hitbox.Facing = facing;
       }
    }

    public void listenForWallBounce()
    {
        if(!isHost)return;
        listeningForWallbounce = true;
    }

    public override void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        listeningForWallbounce = false;
        listeningForDirection = false;
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
            hitbox.Facing = facing;
       }
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

        drifter.SetAnimationOverride(Empowered?1:0);

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
            hitbox.Facing = facing;
       }
    }
}


