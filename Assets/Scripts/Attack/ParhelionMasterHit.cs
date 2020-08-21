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
    public int facing;

    void Start()
    {

        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }

    public void RecoveryPauseMidair()
    {
        Debug.Log("Recovery start!");
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
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
    	rb.velocity = new Vector2(facing *-25f,0);
    }

    public void downSmash()
    {
    	status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.4f);
    	
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
    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }
}
