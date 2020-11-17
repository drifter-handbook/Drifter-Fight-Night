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
    float chargeTime = 0;

    public int facing;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }
    void Update(){
        if(drifter.Charge < 4)
        {
            chargeTime += Time.deltaTime;
            if(chargeTime >= 3f)
            {
                drifter.SetAnimatorBool("HasCharge",true);
                drifter.Charge++;
                chargeTime = 0;
            }
        }
        
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.45f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }


     public void pullup(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(facing * -5f,40f,0);
    }

    public void pullupDodgeRoll()
    {
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.2f);
        rb.velocity = new Vector2(facing * 30f,5f);
    }


    public override void callTheRecovery()
    {
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
        movement.gravityPaused= true;
    }

    public void bigLeap(){
        facing = movement.Facing;
        rb.gravityScale = gravityScale;
        movement.gravityPaused= false;
        rb.velocity= new Vector2(0,60);
    }

    public void removeCharge()
    {
        facing = movement.Facing;
        if(drifter.Charge >0){
            drifter.Charge--;
            
            GameObject arrow = Instantiate(entities.GetEntityPrefab("Arrow"), transform.position + new Vector3(0,3.8f,0), transform.rotation);
            arrow.transform.localScale = new Vector3(7.5f * facing,7.5f,1f);
            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x  + facing * 60f,5f);
            foreach (HitboxCollision hitbox in arrow.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            entities.AddEntity(arrow);

        }
        else{

            GameObject poof = Instantiate(entities.GetEntityPrefab("MovementParticle"), transform.position + new Vector3(facing *4f,3.8f,0),  transform.rotation);
            poof.GetComponent<JuiceParticle>().mode = 1;
            entities.AddEntity(poof);

        }
        if(drifter.Charge ==0){
            drifter.SetAnimatorBool("HasCharge",false);
        }

    }

    public void counterFrame1(){
        rb.velocity = new Vector3(rb.velocity.x,0);

        counter();
    }  
    public void counter(){
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.SetAnimatorBool("Empowered",true);
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        }
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.65f);

    }
    public void hitCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.3f);
        StartCoroutine(resetCounter());
        
    }

    IEnumerator resetCounter(){
        yield return new WaitForSeconds(.3f);
        drifter.SetAnimatorBool("Empowered",false);
    }

    public void whiffCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.95f);
    }
}
