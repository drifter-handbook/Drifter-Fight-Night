using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceJamMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public Drifter self;
    public Animator anim;
    public int charges;

    public int facing;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
    }


    // public override void callTheRecovery()
    // {
    //     rb.gravityScale = 0;
    //     rb.velocity = Vector2.zero;
    // }

    public void callTheRecovery(){
        facing = movement.Facing;
        rb.gravityScale = 0;
        rb.velocity= new Vector2(facing * -25,25);
    }
    public override void cancelTheRecovery(){
        rb.gravityScale = gravityScale;
    } 

    public override void hitTheNeutralW(GameObject target)
    {
        if(charges < 30){
            charges++;
        }
        if(charges == 30){
            anim.SetBool("Empowered",true);
        }
        if(self.DamageTaken >= .5f){
            self.DamageTaken -= .5f;
        }
        else{
            self.DamageTaken = 0f;
        }

        

    }
}
