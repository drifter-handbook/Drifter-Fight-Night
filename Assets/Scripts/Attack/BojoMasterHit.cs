using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BojoMasterHit : MasterHit
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

    public void freeze(){
        rb.velocity = Vector2.zero;
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }

    public void callTheRecovery(){
        facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x  + facing * 20,45);
    }
    public void tootToot(){
        facing = movement.Facing;
        rb.gravityScale = gravityScale;
        rb.velocity += new Vector2(facing * 10,15);
    }
    // public override void cancelTheRecovery(){
    //     rb.gravityScale = gravityScale;
    // } 

}
