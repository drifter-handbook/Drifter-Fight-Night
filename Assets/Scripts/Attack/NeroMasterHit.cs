using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public int facing;
    PlayerStatus status;
    public Animator anim;
    float dashDistance = 30;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }

    public override void callTheRecovery()
    {
        Debug.Log("Recovery start!");
    }
    public void RecoveryPauseMidair()
    {
        // pause in air
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void setMultiHit(){
        attacks.SetMultiHitAttackID();
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.4f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }

    public void grabDash(){
        facing = movement.Facing;
        rb.velocity = new Vector2(facing * 35f,0f);
    }

    public void RecoveryThrowSpear()
    {
        // jump upwards and create spear projectile
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * 35f);
        rb.gravityScale = gravityScale;
        GameObject neroSpear = Instantiate(entities.GetEntityPrefab("NeroSpear"), transform.position, transform.rotation);
        foreach (HitboxCollision hitbox in neroSpear.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(neroSpear);
    }
    public override void hitTheRecovery(GameObject target)
    {
        Debug.Log("Recovery hit!");
    }
    public override void cancelTheRecovery()
    {
        Debug.Log("Recovery end!");
        rb.gravityScale = gravityScale;
    }

    public void resetCharge(){
        dashDistance = 30f;
    }

    public void neutralWCharge(){
    	        
        rb.gravityScale = .1f;
        dashDistance += 10;
        if(dashDistance>=80)drifter.SetAnimatorBool("HasCharge",true);
     }

     public void neutralWDash(){
        facing = movement.Facing;

        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.4f);
        rb.velocity = new Vector3( facing * dashDistance, 0);
        rb.gravityScale = 0;
        dashDistance = 30f;
        drifter.SetAnimatorBool("HasCharge",false);
    }
    
    public override void callTheNeutralW(){
        facing = movement.Facing;
    }     

    public void counter(){
        if(!status.HasInulvernability()){
            status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.1f);
        }
        if(status.HasHit()){
            drifter.SetAnimatorBool("Empowered",true);
        }

    }
    public void hitCounter(){
        drifter.SetAnimatorBool("Empowered",false);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.45f);
        
    }
    public void whiffCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,1f);
    }
    public override void cancelTheNeutralW()
    {
        rb.gravityScale = gravityScale;
        dashDistance = 30f;
    }
}
