using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaryamMasterHit : MasterHit
{

    bool hasSGRecovery = true;
    bool hasUmbrellaRecovery = true;
    float terminalVelocity;

    public void StanceChange()
    {
        if(!isHost)return;
        SetStance(Empowered?1:0);
    }

    void Start()
    {
        if(!isHost)return;
        terminalVelocity = movement.terminalVelocity;
    }

    void Update()
    {
        if(!isHost)return;

        if(movement.terminalVelocity != terminalVelocity  && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminal();
        }

        if((!hasUmbrellaRecovery || !hasSGRecovery) &&( movement.grounded || status.HasEnemyStunEffect() || movement.ledgeHanging)){
            hasSGRecovery = true;
            hasUmbrellaRecovery = true;
        }
    }

    //Flips the direction the charactr is facing mid move)
    public void invertDirection()
    {
        if(!isHost)return;
        movement.flipFacing();
    }

    //Umbrella Dair
    public void setTerminalVelocity()
    {
        if(!isHost)return;
        movement.canLandingCancel = false;  
        movement.terminalVelocity = 75;
    }


    // Swaps between two movesents by changing the animation layer being used
    public void SetStance(int stance)
    {
        //if(!isHost)return;
        Empowered = (stance==0);
        
        if(isHost) attacks.currentRecoveries = (Empowered && hasSGRecovery) || (!Empowered && hasUmbrellaRecovery)? 1:0;

        drifter.SetAnimationLayer(Empowered?1:0);
    }

    //Causes a non-aerial move to cancle on htiing the ground
    public void cancelSideQ()
    {
        if(!isHost)return;
        movement.canLandingCancel = true;
    }

    //Consumes the recovery in umbrella stance
    public void UmbrellaRecovery()
    {
        hasUmbrellaRecovery = false;
    }

    //Consumes recovery in shotugn stance
    public void SGRecovery()
    {
        hasSGRecovery = false;
    }

    //Slows fall speed after umbrella up special
    public void upWGlide()
    {
        if(!isHost)return;

        if(cancelAttack())
        {
            resetTerminal();
        }

        else if(movement.grounded)
        {
            resetTerminal();
            returnToIdle();
        }
        else
        {

            movement.updateFacing();
            rb.velocity = new Vector2(Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)? drifter.input.MoveX * 23f:(.6f*23f)),rb.velocity.x,.75f),rb.velocity.y);
            movement.updateFacing();
            movement.terminalVelocity = 8f;
        }
    }

    //Resests terminal velocity to normal value after is has been altered by a move
    public void resetTerminal()
    {
        if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }

    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 35f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.position += new Vector2(facing * 1f,6f);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 30f,0f);
    }



    //Shotgun explosion projectiles
    public void SGJabExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(1.8f * facing,2.5f,0);
        
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

    public void SGUTiltExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(0f * facing,3.3f,0);
        
        GameObject explosion = host.CreateNetworkObject("Explosion_Diagonal", transform.position + pos, transform.rotation);
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

    public void SGUAirFirstExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(0f * facing,3f,0);
        
        GameObject explosion = host.CreateNetworkObject("Explosion_Diagonal_Uair", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.AttackData = attacks.Attacks[DrifterAttackType.W_Down];
            hitbox.Active = true;
            hitbox.Facing = facing;
       }
    }

    public void SGUAirVerticalExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(-.5f * facing,3.6f,0);
        
        GameObject explosion = host.CreateNetworkObject("UairExplosion_Maryam", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(-10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.AttackData = attacks.Attacks[DrifterAttackType.Roll];
            hitbox.Active = true;
            hitbox.Facing = facing;
       }
    }


    public void SGUAirLauncherExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(-1f * facing,3f,0);
        
        GameObject explosion = host.CreateNetworkObject("Explosion_Diagonal_Uair", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(-10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.AttackData = attacks.Attacks[DrifterAttackType.Aerial_Q_Neutral];
            hitbox.Active = true;
            hitbox.Facing = facing;
       }
    }

    public void SGJSairExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(2.5f * facing,4f,0);
        
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

    public void SGUWExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(.5f * facing,1f,0);
        
        GameObject explosion = host.CreateNetworkObject("ExplosionDiagonalDown", transform.position + pos, transform.rotation);
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
}


