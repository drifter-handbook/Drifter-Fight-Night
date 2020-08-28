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
    public Animator anim;
    float timeSinceGun = 0f;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }

    void Update()
    {

    	if(timeSinceGun < .7f){
    		timeSinceGun += Time.deltaTime;
    	}
    	else{
    		drifter.SetAnimatorBool("HasCharge",false);
    	}
    	
    }

    public void freeze(){
        rb.velocity = Vector2.zero;
    }

    public void GUN(){
    	facing = movement.Facing;
        Vector3 flip = new Vector3(facing *2f,2f,0f);
        Vector3 pos = new Vector3(facing *3f,5.5f,1f);
        GameObject bubble = Instantiate(entities.GetEntityPrefab("Mockery"), transform.position + pos, transform.rotation);
        bubble.transform.localScale = flip;
        bubble.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 55, 0);
       	drifter.SetAnimatorBool("HasCharge",true);
       	timeSinceGun = 0f;
        
        foreach (HitboxCollision hitbox in bubble.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(bubble);
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }

    public override void callTheRecovery(){
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
