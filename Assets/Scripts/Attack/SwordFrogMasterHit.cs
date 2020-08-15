using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFrogMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public Animator anim;
    int chargeProgress = 0;

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

    public void removeCharge()
    {
        if(drifter.Charge >0){
            drifter.Charge--;
        }
        if(drifter.Charge ==0){
            anim.SetBool("HasCharge",false);
        }

    }

    public void grantCharge(){
        chargeProgress++;
        if(chargeProgress >= 3){
            chargeProgress = 0;anim.SetBool("HasCharge",true);
            if(drifter.Charge <3){
                drifter.Charge++;
            }
        }
        
        
    }
}
