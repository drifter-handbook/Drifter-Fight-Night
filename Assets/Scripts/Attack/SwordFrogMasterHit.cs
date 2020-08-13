using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFrogMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;

    public int charges;

    public int facing;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
    }


    public override void callTheRecovery()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }

    public void bigLeap(){
        facing = movement.Facing;
        rb.gravityScale = gravityScale;
        rb.velocity= new Vector2(0,60);
    }

    public override void hitTheNeutralW(GameObject target)
    {
        if(charges >0){
            charges--;
        }
    }
}
