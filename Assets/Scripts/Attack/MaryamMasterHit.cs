using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaryamMasterHit : MasterHit
{

    bool hasSGRecovery = true;
    bool hasUmbrellaRecovery = true;

    public void StanceChange()
    {
        SetStance(Empowered?1:0);
    }

    void Update()
    {

        // if(movement.terminalVelocity != terminalVelocity  && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        // {
        //     resetTerminal();
        // }

        if((!hasUmbrellaRecovery || !hasSGRecovery) &&( movement.grounded || status.HasEnemyStunEffect() || movement.ledgeHanging)){
            hasSGRecovery = true;
            hasUmbrellaRecovery = true;
        }
    }

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

    //Flips the direction the charactr is movement.Facing mid move)
    public void invertDirection()
    {
        movement.flipFacing();
    }

    // Swaps between two movesents by changing the animation layer being used
    public void SetStance(int stance)
    {
        Empowered = (stance==0);
        
        //if(isHost) attacks.currentRecoveries = (Empowered && hasSGRecovery) || (!Empowered && hasUmbrellaRecovery)? 1:0;

        //drifter.SetAnimationLayer(Empowered?1:0);
    }

    public void shinestall()
    {
        if(rb.velocity.y <=0)setYVelocity(0);
    }

    //Causes a non-aerial move to cancle on htiing the ground
    public void cancelSideQ()
    {
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

        movement.updateFacing();
        rb.velocity = new Vector2(Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)? drifter.input[0].MoveX * 23f:(.6f*23f)),rb.velocity.x,.75f),rb.velocity.y);
        movement.updateFacing();
        movement.terminalVelocity = 8f;
        
    }

    //Shotgun explosion projectiles
    public void SGJabExplosion()
    {
        
        Vector3 pos = new Vector3(1.8f * movement.Facing,2.5f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("ExplosionSide", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void SGUTiltExplosion()
    {
        
        Vector3 pos = new Vector3(0f * movement.Facing,3.3f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("Explosion_Diagonal", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void SGUAirFirstExplosion()
    {
        
        Vector3 pos = new Vector3(0f * movement.Facing,3f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("Explosion_Diagonal_Uair", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void SGUAirVerticalExplosion()
    {
        
        Vector3 pos = new Vector3(-.5f * movement.Facing,3.6f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("UairExplosion_Maryam", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(-10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }


    public void SGUAirLauncherExplosion()
    {
        
        Vector3 pos = new Vector3(-1f * movement.Facing,3f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("Explosion_Diagonal_Uair", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(-10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void SGJSairExplosion()
    {
        
        Vector3 pos = new Vector3(2.5f * movement.Facing,4f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("ExplosionSide", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void SGUWExplosion()
    {
        
        Vector3 pos = new Vector3(.5f * movement.Facing,1f,0);
        
        GameObject explosion = GameController.Instance.CreatePrefab("ExplosionDiagonalDown", transform.position + pos, transform.rotation);
        explosion.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }
}


