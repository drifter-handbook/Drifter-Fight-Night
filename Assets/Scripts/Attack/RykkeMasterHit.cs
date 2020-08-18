using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RykkeMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public Animator anim;
    public int facing;
    public TetherRange playerRange;
    public TetherRange ledgeRange;
    GameObject activeStone;
    PlayerStatus status;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();

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
        playerRange.gameObject.transform.parent.gameObject.SetActive(true);
        
    }

    public void daisyChain()
    {

        facing = movement.Facing;
        rb. gravityScale = gravityScale;
        if(playerRange.TetherPoint != Vector3.zero)
        {
            rb.velocity = new Vector2((-rb.position.x + playerRange.TetherPoint.x) *4f + 10 * facing, Mathf.Min((-rb.position.y + playerRange.TetherPoint.y) *4f,55) + 15);
            attacks.resetRecovery();

        }
        else if(ledgeRange.TetherPoint != Vector3.zero)
        {
            rb.velocity = new Vector2((-rb.position.x + ledgeRange.TetherPoint.x) *4f, Mathf.Min((-rb.position.y + ledgeRange.TetherPoint.y) *4f + 15f,55f) + 20);
            movement.currentJumps++;
        }
        else
        {
            UnityEngine.Debug.Log("Uhoh");
            //draw tether whiff
        }
        
        playerRange.gameObject.transform.parent.gameObject.SetActive(false);
        

    }

    public void notify()
    {
      Debug.Log("hit something!");
    }
    public void updatePosition(Vector3 position){
        //movement.updatePosition(position);
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

    public void sideGrab()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *8f,8f,1f);
        Vector3 loc = new Vector3(facing *6.5f,0f,0f);
        GameObject HoldPerson = Instantiate(entities.GetEntityPrefab("HoldPerson"), transform.position + loc, transform.rotation);
        HoldPerson.transform.localScale = flip;
        foreach (HitboxCollision hitbox in HoldPerson.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        HoldPerson.GetComponentInChildren<RyykeGrab>().animator = anim;
        entities.AddEntity(HoldPerson);
    }

    public void grabWhiff()
    {
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.8f);
    }

    public void dodgeRoll()
    {
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }

    public void grabEmpowered(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.9f);
    }

    //Down W

    public override void callTheDownW()
    {
        Debug.Log("DOWN W START");
    }

    public void dropStone()
    {
      facing = movement.Facing;
      rb.velocity = new Vector2(0,10);
      Vector3 flip = new Vector3(facing *8f,8f,1f);
      Vector3 loc = new Vector3(facing *1f,.8f,0f);
      GameObject tombstone = Instantiate(entities.GetEntityPrefab("RyykeTombstone"), transform.position + loc, transform.rotation);
      tombstone.transform.localScale = flip;
        foreach (HitboxCollision hitbox in tombstone.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        if(activeStone){
            activeStone.GetComponent<RyykeTombstone>().Break();
        }
        tombstone.GetComponent<RyykeTombstone>().facing=facing;
        tombstone.GetComponent<RyykeTombstone>().chadController=this;
        activeStone = tombstone;
        entities.AddEntity(tombstone);
    }
    public void grantStack()
    {
    	if(drifter.Charge < 3){
    		drifter.Charge++;
    		anim.SetBool("Empowered",true);
    	}

    }

    public void conmsumeStack()
    {
    	if(drifter.Charge > 0){
    		drifter.Charge--;
    		if(drifter.Charge == 0){
    			anim.SetBool("Empowered",false);
    		}
    	}
    }

     public void SpawnChad(int direction){
        Vector3 flip = new Vector3(direction *8f,8f,1f);
        GameObject zombie = Instantiate(entities.GetEntityPrefab("Chadwick"), activeStone.transform.position, activeStone.transform.transform.rotation);
        zombie.transform.localScale = flip;
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
