using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceJamMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public SpriteRenderer sprite;
    public Drifter self;
    public Animator anim;
    public int charges;
    PlayerStatus status;
    int maxCharge = 30;

    public AudioSource audio;
    public AudioClip[] audioClips;

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
        audio.Pause();
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -45f,0f);
    }

     public void pullup(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0f,45f,0);
    }

    public void pullupDodgeRoll()
    {
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.45f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -45f,5f);
    }

    public void multihit(){
        attacks.SetMultiHitAttackID();
    }

    public void sideW()
    {
        audio.PlayOneShot(audioClips[1], 1f);
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
            sprite.color = Color.white;
            audio.PlayOneShot(audioClips[1], 1f);
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
            amber.GetComponent<OopsiePoopsie>().hurtbox = gameObject.transform.Find("Hurtboxes").gameObject.GetComponent<CapsuleCollider2D>();
            amber.GetComponent<OopsiePoopsie>().status = status;
        entities.AddEntity(amber);
        charges = 0;
        }
    }



    public void callTheRecovery(){
        facing = movement.Facing;
        rb.gravityScale = 0;
        movement.gravityPaused= true;
        rb.velocity= new Vector2(facing * -25,25);
    }
    public void cancelTheRecovery(){
        rb.gravityScale = gravityScale;
        movement.gravityPaused = false;
    } 

    void grantCharges(){
    	if(charges < maxCharge){
            charges++;
            
        }
        if(charges >= maxCharge){
            audio.Stop();
            audio.PlayOneShot(audioClips[2],1f);
            drifter.SetAnimatorBool("Empowered",true);
            sprite.color = new Color(255,165,0);
        }
    }

    public void chargeNeutral()
    {
        if(charges < maxCharge){
    			grantCharges();
    		}
        
        if(self.DamageTaken >= .5f){
            self.DamageTaken -= .5f;
        }
        else{
            self.DamageTaken = 0f;
        }

        

    }
}
