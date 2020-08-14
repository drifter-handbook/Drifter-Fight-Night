using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParhelionMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public int facing;

    void Start()
    {

        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
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
    
    public override void hitTheRecovery(GameObject target)
    {
        Debug.Log("Recovery hit!");
    }
    public override void cancelTheRecovery()
    {
        Debug.Log("Recovery end!");
        rb.gravityScale = gravityScale;
    }
}
