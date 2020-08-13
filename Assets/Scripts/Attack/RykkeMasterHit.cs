using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RykkeMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public int awakenStacks = 0;
    public Animator anim;
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
        Debug.Log("Recovery start!");
    }
    public void RecoveryPauseMidair()
    {
        // pause in air
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }
    public void RecoveryPreEmpty(){
      Debug.Log("Recovery preempted!");
      facing = movement.Facing;
      // jump upwards and create spear projectile
      //rb.velocity = new Vector2(rb.velocity.x, 1.5f * movement.jumpSpeed);
      //rb.gravityScale = gravityScale;
      Vector3 pos = new Vector3(facing * 4.5f, 7f, 0f);
      GameObject RykkeBox = Instantiate(entities.GetEntityPrefab("RykkeBox"), transform.position + pos, transform.rotation * (Quaternion.Euler(new Vector3(0, 0, facing * 45))));
      RykkeBox.transform.localScale = new Vector3(5, 5, 5);
      foreach (HitboxCollision hitbox in RykkeBox.GetComponentsInChildren<HitboxCollision>(true))
      {
          hitbox.parent = drifter.gameObject;
          hitbox.AttackID = attacks.AttackID;
          hitbox.AttackType = attacks.AttackType;
          hitbox.Active = true;
      }
      entities.AddEntity(RykkeBox);
    }
    public void RecoveryThrowString()
    {
        facing = movement.Facing;
        // jump upwards and create spear projectile
        //rb.velocity = new Vector2(rb.velocity.x, 1.5f * movement.jumpSpeed);
        //rb.gravityScale = gravityScale;
        Vector3 pos = new Vector3(facing * 4.5f, 7f, 0f);
        GameObject RykkeTether = Instantiate(entities.GetEntityPrefab("RykkeTether"), transform.position + pos, Quaternion.Euler(new Vector3(0, 0, facing * -45)));
        RykkeTether.transform.localScale = new Vector3(5, 5, 5);
        foreach (HitboxCollision hitbox in RykkeTether.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(RykkeTether);
    }
    public void updatePosition(Vector3 position){
        movement.updatePosition(position);
    }
    public override void hitTheRecovery(GameObject target)
    {
        Debug.Log("Recovery hit!");
    }
    public override void cancelTheRecovery()
    {
        Debug.Log("Recovery end!");
        rb.gravityScale = gravityScale;
    }

    //Down W

    public override void callTheDownW()
    {
        Debug.Log("DOWN W START");
    }

    public void dropStone(){
      facing = movement.Facing;
      rb.velocity = new Vector2(0,10);
      Vector3 flip = new Vector3(facing *8f,1f,1f);
      Vector3 loc = new Vector3(facing *1f,.5f,0f);
      GameObject tombstone = Instantiate(entities.GetEntityPrefab("RyykeTombstone"), transform.position + loc, transform.rotation);
      tombstone.transform.localScale += flip;
        foreach (HitboxCollision hitbox in tombstone.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(tombstone);
    }
    public void grantStack(){
    	if(awakenStacks < 3){
    		awakenStacks++;
    		anim.SetBool("Empowered",true);
    	} 

    }

    public void conmsumeStack(){
    	if(awakenStacks > 0){
    		awakenStacks--;
    		if(awakenStacks == 0){
    			anim.SetBool("Empowered",false);
    		}
    	} 
    }

     public void SpawnChad(){

        GameObject zombie = Instantiate(entities.GetEntityPrefab("Chadwick"), transform.position, transform.rotation);
        zombie.transform.localScale = this.gameObject.transform.localScale;
        foreach (HitboxCollision hitbox in zombie.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        grantStack();
        entities.AddEntity(zombie);
    }
}
