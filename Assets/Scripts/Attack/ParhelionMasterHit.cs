using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParhelionMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    PlayerStatus status;
    float terminalVelocity;
    public int facing;

    void Start()
    {

        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
        terminalVelocity = movement.terminalVelocity;
    }

    public void nairMultihit(){
        attacks.SetMultiHitAttackID();
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -30f,0f);
    }

    public void SpikeBolt()
    {
        // jump upwards and create spear projectile
        facing = movement.Facing;
        Vector3 pos = new Vector3(facing * - 4.3f,2.8f,0);
        GameObject bolt = Instantiate(entities.GetEntityPrefab("ParhelionBolt"), transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in bolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(bolt);
    }


    public void RecoveryPauseMidair()
    {
        Debug.Log("Recovery start!");
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        movement.gravityPaused= true;
    }
    public override void callTheRecovery()
    {
        facing = movement.Facing;
        // pause in air
        rb.velocity = new Vector2(facing *-50, 20);
    }

    public void neutralSmash()
    {
    	facing = movement.Facing;
    	status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.4f);
    	rb.velocity = new Vector2(facing *-35f,5f);
    }

    public void downSmash()
    {
    	status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.4f);
        movement.terminalVelocity = 150;
    	
    }

    public void GroundedSlide(){
        facing = movement.Facing;
        if(drifter.input.MoveX * facing <0){
            rb.velocity = new Vector2(facing * -1f * movement.walkSpeed,0f);
        }
        
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
        rb.velocity = new Vector2(facing * -35f,0f);
    }



    public void downJump(){
        rb.velocity += new Vector2(0,35);
    }

    public void downSlam(){
        rb.velocity += new Vector2(0,-60);
    }
    public void resetTerminal(){
        movement.terminalVelocity = terminalVelocity;
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
}
