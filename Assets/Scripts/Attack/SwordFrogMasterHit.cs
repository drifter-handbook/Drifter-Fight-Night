using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordFrogMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerStatus status;
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
        status = drifter.GetComponent<PlayerStatus>();
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 30f,0f);
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
            drifter.SetAnimatorBool("HasCharge",false);
        }

    }
    public void counter(){
        if(!status.HasInulvernability()){
            status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        }
        if(status.HasHit()){
            drifter.SetAnimatorBool("Empowered",true);
        }

    }
    public void hitCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0.3f);
        StartCoroutine(resetCounter());
        
    }

    IEnumerator resetCounter(){
        yield return new WaitForSeconds(.3f);
         drifter.SetAnimatorBool("Empowered",false);
    }

    public void whiffCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.65f);
    }

    public void grantCharge(){
        chargeProgress++;
        if(chargeProgress >= 3){
            chargeProgress = 0;
            drifter.SetAnimatorBool("HasCharge",true);
            if(drifter.Charge <3){
                drifter.Charge++;
            }
        }
        
        
    }
}
