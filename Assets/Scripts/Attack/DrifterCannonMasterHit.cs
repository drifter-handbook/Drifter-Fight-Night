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

    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);
    }

    override public void UpdateFrame()
    {
        base.UpdateFrame();

        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            drifter.Sparkle(false);
            jumpGranted = false;
            SetCharge(1);
        }

        if(jumpGranted && movement.grounded)jumpGranted = false;

        if(listeningForWallbounce && movement.IsWallSliding())
        {
            listeningForWallbounce = false;
            drifter.PlayAnimation("W_Side_End_Early");
            rb.velocity = new Vector2(movement.Facing * -15f,30f);
            if(!jumpGranted && movement.currentJumps <= movement.numberOfJumps -1) movement.currentJumps++;
            jumpGranted = true;
            GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Restitution,rb.position + new Vector2(movement.Facing * .5f,0), (movement.Facing > 0)?90:-90,Vector3.one);
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
                drifter.PlayAnimation("W_Up_Loop",0,true);
                boostTime --;
            }
            else
            {
                drifter.PlayAnimation("W_Up_Idle",0,true);
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
        
        Vector3 pos = new Vector3(1.9f * movement.Facing,3.3f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("ExplosionSide", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }


    public void SideWExplosion()
    {
        
        Vector3 pos = new Vector3(-1.5f * movement.Facing,2.7f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("ExplosionSide", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(-10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void listenForWallBounce()
    {
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
        
        Vector3 pos = new Vector3(-.4f* movement.Facing,2f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("UairExplosion", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f* movement.Facing, 10f, 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void DownSpecialBomb()
    {
        
        Vector3 pos = new Vector3(-.5f * movement.Facing,2.7f,0);
        
        GameObject grenade = GameController.Instance.CreatePrefab("DCGenade", transform.position + pos, transform.rotation);
        grenade.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

        grenade.GetComponent<Rigidbody2D>().velocity = new Vector2(20* movement.Facing,25);

        foreach (HitboxCollision hitbox in grenade.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    //W_Neutral

    public void handleRanchStartup()
    {
    	//sets all special inputs to true to "clear" it

    	foreach(PlayerInputData input in drifter.input)
    		input.Special = true;
    	listenForSpecialTapped("W_Neutral_Fire");
        if(Empowered) drifter.PlayAnimation("W_Neutral_Fire");
    	else if(charge > 1) drifter.PlayAnimation("W_Neutral_" + charge);
    }

    public void SetCharge(int charge)
    {
    	this.charge = charge;
    	Empowered = (charge == 3);

        drifter.Sparkle(Empowered);

        drifter.SetAnimationOverride(Empowered?1:0);

    }

    public void FireRanchProjectile()
    {
        
        Vector3 pos = new Vector3(1f * movement.Facing,2.7f,0);
        
        GameObject ranch = GameController.Instance.CreatePrefab("Ranch" + charge, transform.position + pos, transform.rotation);
        ranch.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

        rb.velocity = new Vector2((charge - 1) * -15f* movement.Facing,0);
        
        if(charge < 3)ranch.GetComponent<Rigidbody2D>().velocity = new Vector2((charge == 1?55f:25f)* movement.Facing,0);

        SetCharge(1);

        foreach (HitboxCollision hitbox in ranch.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }
}


