using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroMasterHit : MasterHit
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


    public void callTheRecovery(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
    }
    public void inTheHole(){
        facing = movement.Facing;
        rb.gravityScale = gravityScale;
        rb.position += new Vector2(0,20);
    }

    public void resetGravity(){
        rb.gravityScale = gravityScale;
    } 

}
