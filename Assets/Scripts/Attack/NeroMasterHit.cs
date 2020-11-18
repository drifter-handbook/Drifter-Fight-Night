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
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void setMultiHit(){
        attacks.SetMultiHitAttackID();
    }


    public void pullup(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.4f);
        rb.velocity = new Vector3(0,75f,0);
    }

    public void pullupDodgeRoll()
    {
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.4f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 35f,0f);
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
        movement.gravityPaused= false;
        rb.gravityScale = gravityScale;
        GameObject neroSpear = Instantiate(entities.GetEntityPrefab("NeroSpear"), transform.position, transform.rotation);
        foreach (HitboxCollision hitbox in neroSpear.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        entities.AddEntity(neroSpear);
    }
    public override void hitTheRecovery(GameObject target)
    {
        Debug.Log("Recovery hit!");
    }
    public void cancelTheRecovery()
    {
        Debug.Log("Recovery end!");
        movement.gravityPaused= false;
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
        movement.gravityPaused= true;
        rb.gravityScale = 0;
        dashDistance = 30f;

        attacks.SetupAttackID(DrifterAttackType.W_Neutral);

        drifter.SetAnimatorBool("HasCharge",false);
    }
    
    public override void callTheNeutralW(){
        facing = movement.Facing;
    }


    public void counterFrame1(){
        rb.velocity = rb.velocity = new Vector3(rb.velocity.x,0);

        counter();
    }     

    public void counter(){
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.SetAnimatorBool("Empowered",true);
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        }
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.65f);

    }
    public void hitCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        StartCoroutine(resetCounter());
        
    }

    IEnumerator resetCounter(){
        yield return new WaitForSeconds(.3f);
        drifter.SetAnimatorBool("Empowered",false);
    }

    public void whiffCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.95f);
    }

    public void cancelTheNeutralW()
    {
        movement.gravityPaused= false;
        rb.gravityScale = gravityScale;
        dashDistance = 30f;
    }
}
