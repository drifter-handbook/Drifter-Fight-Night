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
    PlayerStatus status;
    GameObject bolt;

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
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }

    public void multihit(){
        attacks.SetMultiHitAttackID();
    }

    public void sideW()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *-3f,0f,1f);
        GameObject GuidingBolt = Instantiate(entities.GetEntityPrefab("GuidingBolt"), transform.position + pos, transform.rotation);
        GuidingBolt.transform.localScale = flip;
        GuidingBolt.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * -30, 0);
        foreach (HitboxCollision hitbox in GuidingBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(GuidingBolt);       
            
    }

    public void oopsiePoopsie()
    {
        facing = movement.Facing;
        rb.velocity += new Vector2(-20*facing,0);
        if(anim.GetBool("Empowered")){
            drifter.SetAnimatorBool("Empowered",false);
            
            Vector3 flip = new Vector3(facing *12f,12f,0f);
            Vector3 pos = new Vector3(facing *-3f,0f,1f);
            GameObject amber = Instantiate(entities.GetEntityPrefab("Amber"), transform.position + pos, transform.rotation);
            amber.transform.localScale = flip;
            amber.GetComponent<Rigidbody2D>().velocity = rb.velocity;
            foreach (HitboxCollision hitbox in amber.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
            }
        entities.AddEntity(amber);

        }
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

    public void chargeNeutral()
    {
        if(charges < 40){
            charges++;
        }
        if(charges == 40){
            drifter.SetAnimatorBool("Empowered",true);
        }
        if(self.DamageTaken >= .5f){
            self.DamageTaken -= .5f;
        }
        else{
            self.DamageTaken = 0f;
        }

        

    }
}
