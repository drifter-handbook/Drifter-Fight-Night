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
    public AudioSource audio;
    public AudioClip[] audioClips;

    

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
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,1.1f);
        Debug.Log("Recovery start!");
    }
    public void RecoveryPauseMidair()
    {
        // pause in air
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        playerRange.gameObject.transform.parent.gameObject.SetActive(true);
        
    }

    public void throwHands(){
        facing = movement.Facing;
        GameObject arms = Instantiate(entities.GetEntityPrefab("LongArmOfTheLaw"), transform.position + new Vector3(facing * 2,5,0), transform.rotation);
                foreach (HitboxCollision hitbox in arms.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = drifter.gameObject;
                    hitbox.AttackID = attacks.AttackID;
                    hitbox.AttackType = attacks.AttackType;
                    hitbox.Active = true;
        }
        float length = 15f;

        if(ledgeRange.TetherPoint != Vector3.zero)
        {
            arms.transform.rotation = Quaternion.Euler(0,0, (Mathf.Atan2(arms.transform.position.x -ledgeRange.TetherPoint.x,-arms.transform.position.y+ledgeRange.TetherPoint.y)*180 / Mathf.PI));
            length = Vector2.Distance(ledgeRange.TetherPoint,arms.transform.position);
        }
        else if(playerRange.TetherPoint != Vector3.zero)
        {
            arms.transform.rotation = Quaternion.Euler(0,0, (Mathf.Atan2(arms.transform.position.x -playerRange.TetherPoint.x,-arms.transform.position.y+playerRange.TetherPoint.y)*180 / Mathf.PI));
            length = Vector2.Distance(playerRange.TetherPoint,arms.transform.position);

        }
        
        else
        {
            arms.transform.rotation = Quaternion.Euler(0,0,45 * -facing);
        }
        arms.transform.localScale = new Vector3(13,length/1.3f,1);

        entities.AddEntity(arms);
    }

    public void daisyChain()
    {
        facing = movement.Facing;
        
        if(ledgeRange.TetherPoint != Vector3.zero)
        {
            rb.velocity = new Vector2((-rb.position.x + ledgeRange.TetherPoint.x) *3f, Mathf.Min((-rb.position.y + ledgeRange.TetherPoint.y) *3f,50f) + 30);
            if(movement.currentJumps < movement.numberOfJumps){
                movement.currentJumps++;
            }
        }

        else if(playerRange.TetherPoint != Vector3.zero)
        {
            // rb.velocity = new Vector2((-rb.position.x + playerRange.TetherPoint.x) *3f + 10 * facing, Mathf.Max(Mathf.Min((Mathf.Min(-rb.position.y,0) + playerRange.TetherPoint.x) *4f,20),0) +40);
            // attacks.resetRecovery();
            rb.position = new Vector3(playerRange.TetherPoint.x -.5f *facing,playerRange.TetherPoint.y +.5f,0);
            rb.velocity = new Vector3(facing*35, 45,0);
            if(movement.currentJumps < movement.numberOfJumps){
                movement.currentJumps++;
            }

        }
        
        else
        {
            UnityEngine.Debug.Log("Uhoh");
            //draw tether whiff
        }
        //rb. gravityScale = gravityScale;
        playerRange.gameObject.transform.parent.gameObject.SetActive(false);
        rb.gravityScale = gravityScale;
    }


    public void AmrourUp(){
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.5f);
    }

    public void resetGravity(){
        rb.gravityScale = gravityScale;
    }

    public void pauseGravity(){
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }


    public void sideWslide()
    {
        facing = movement.Facing;
        if(!anim.GetBool("Empowered")){
            rb.velocity = new Vector3(facing * 25,0);
        }
        else{
            rb.velocity = new Vector3(facing * 35,0);
        }
        
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
        resetGravity();
    }

    public void sideGrab()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *8f,8f,1f);
        Vector3 loc = new Vector3(facing *5f,0f,0f);
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
        pauseGravity();
    }

    //Down W
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
            audio.PlayOneShot(audioClips[0]);
    		drifter.Charge++;
    		//anim.SetBool("Empowered",true);
            drifter.SetAnimatorBool("Empowered",true);
            drifter.BlockReduction = .75f;
    	}

    }

    public void conmsumeStack()
    {
    	if(drifter.Charge > 0){
    		drifter.Charge--;
    		if(drifter.Charge == 0){
    			//anim.SetBool("Empowered",false);
                drifter.SetAnimatorBool("Empowered",false);
                drifter.BlockReduction = .25f;
    		}
    	}
    }

     public void SpawnChad(int direction){
        Vector3 flip = new Vector3(direction *8f,8f,1f);
        GameObject zombie = Instantiate(entities.GetEntityPrefab("Chadwick"), activeStone.transform.position, activeStone.transform.transform.rotation);
        zombie.transform.localScale = flip;
        attacks.SetupAttackID(DrifterAttackType.W_Down);
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
