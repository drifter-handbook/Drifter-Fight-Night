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
    public Animator anim;
    float timeSinceGun = 0f;
    public int facing;
    float boofTime;
    bool checkBoof;

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

        if(checkBoof && Time.time - boofTime > 1f){
            attacks.currentRecoveries = 0;
            checkBoof = false;
        }
    	if(timeSinceGun < .7f){
    		timeSinceGun += Time.deltaTime;
    	}
    	else{
    		drifter.SetAnimatorBool("HasCharge",false);
    	}


    }

    public void multihit(){
        attacks.SetMultiHitAttackID();
    }

    public void freeze(){
        rb.velocity = Vector2.zero;
    }

    public void GUN(){
    	facing = movement.Facing;
        Vector3 flip = new Vector3(facing *6f,6f,0f);
        Vector3 pos = new Vector3(facing *3f,4f,1f);
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
        bubble.GetComponent<BojoBubble>().mode = Random.Range(0,8);
        entities.AddEntity(bubble);
    }

    public void callTheSideW(){
        facing = movement.Facing;
        drifter.SetAnimatorBool("Empowered",true);
        rb.velocity = new Vector2(55 *facing, 0);

    }
    public void continueCharge(){
        rb.velocity = new Vector2(55 *facing, rb.velocity.y);
    }

    public void misfire(){
        facing = movement.Facing;
        rb.velocity = new Vector2(50f * facing,15f);
    }

    public void dismount(){
         rb.velocity = new Vector2(rb.velocity.x - facing * 10, 45f);
         status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.4f);

        Vector3 flip = new Vector3(facing *9f,9f,0f);
        Vector3 pos = new Vector3(facing *0f,0f,1f);
        GameObject Centaur = Instantiate(entities.GetEntityPrefab("Kamikaze"), transform.position + pos, transform.rotation);
        Centaur.transform.localScale = flip;
        Centaur.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 65, 0);
        
        foreach (HitboxCollision hitbox in Centaur.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(Centaur);
        drifter.SetAnimatorBool("Empowered",false);
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        drifter.SetAnimatorBool("Empowered",false);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }
    public void loseEmpowered(){
        drifter.SetAnimatorBool("Empowered",false);
    }

    public void boof(){
        facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x  + facing * 10,45);
        boofTime = Time.time;
        checkBoof = true;
    }

}
