using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RykkeMasterHit : MasterHit
{
    public Animator anim;
    public TetherRange playerRange;
    public TetherRange ledgeRange;
    GameObject activeStone;
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    bool tethering = false;
    bool tetheredPlayer = false;
    Vector2 tetherTarget = Vector3.zero;

    int recoveryReset =2;


    void Update()
    {
        if(status.HasEnemyStunEffect() && tethering)tethering = false;

        if(((Vector2.Distance(tetherTarget,rb.position) < 4.5f && tetheredPlayer) || movement.ledgeHanging)){
            UnityEngine.Debug.Log(Vector2.Distance(tetherTarget,rb.position));
            cancelTethering();
        }

        if(drifter.Charge == 0){
                drifter.SetAnimatorBool("Empowered",false);
                drifter.BlockReduction = .25f;
            }
        if(movement.grounded){
            recoveryReset = 2;
        }

        if(tethering){
            UnityEngine.Debug.Log(Vector2.Distance(tetherTarget,rb.position));
            rb.position =  Vector3.Lerp(rb.position,tetherTarget,.15f);
        }
    }


    //Tether Recovery Logic

    public void enableTetherBox()
    {
        playerRange.gameObject.transform.parent.gameObject.SetActive(true);
        
    }

    public void selectTetherTarget()
    {
        facing = movement.Facing;
        GameObject arms = Instantiate(entities.GetEntityPrefab("LongArmOfTheLaw"), transform.position + new Vector3(facing * 2,5,0), transform.rotation);
                foreach (HitboxCollision hitbox in arms.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = drifter.gameObject;
                    hitbox.AttackID = attacks.AttackID;
                    hitbox.AttackType = attacks.AttackType;
                    hitbox.Active = true;
                    hitbox.Facing = facing;
        }
        float length = 15f;

        if(ledgeRange.TetherPoint != Vector3.zero)
        {
            arms.transform.rotation = Quaternion.Euler(0,0, (Mathf.Atan2(arms.transform.position.x -ledgeRange.TetherPoint.x,-arms.transform.position.y+ledgeRange.TetherPoint.y)*180 / Mathf.PI));
            length = Vector2.Distance(ledgeRange.TetherPoint,arms.transform.position);
            tetherTarget = ledgeRange.TetherPoint;
            tetheredPlayer = false;
        }
        else if(playerRange.TetherPoint != Vector3.zero)
        {
            arms.transform.rotation = Quaternion.Euler(0,0, (Mathf.Atan2(arms.transform.position.x -(playerRange.TetherPoint.x + playerRange.enemyVelocity.x *.15f),-arms.transform.position.y+(playerRange.TetherPoint.y+ playerRange.enemyVelocity.y *.15f))*180 / Mathf.PI));
            length = Vector2.Distance(playerRange.TetherPoint +  playerRange.enemyVelocity * .15f,arms.transform.position);
            tetherTarget = playerRange.TetherPoint;// + (playerRange.enemyVelocity *.15f);
            tetheredPlayer = true;

        }
        
        else
        {
            arms.transform.rotation = Quaternion.Euler(0,0,45 * -facing);
            tetherTarget = Vector2.zero;
            tetheredPlayer = false;
        }
        arms.transform.localScale = new Vector3(13,length/1.3f,1);

        entities.AddEntity(arms);
    }

    public void moveToTetherTarget()
    {
        facing = movement.Facing;
        
        if(tetherTarget != Vector2.zero)
        {
            tethering = true;
        }

        else if (recoveryReset > 0){
            attacks.currentRecoveries = 1;
            recoveryReset--;
        }
        playerRange.gameObject.transform.parent.gameObject.SetActive(false);
        rb.gravityScale = gravityScale;
        movement.gravityPaused = false;
        //tetheredPlayer = false;
    }

    public void cancelTethering()
    {
        if(tethering){
            UnityEngine.Debug.Log("CANCEL TETHERING");
            tethering = false;
            tetherTarget = Vector2.zero;
            if(tetheredPlayer){
                drifter.SetAnimatorTrigger("GrabbedPlayer");
                UnityEngine.Debug.Log("ZOOM");
                rb.velocity = new Vector3(facing*35,30,0);
            }
            
        }
        
    }


    //Side Grab "Projectile"

    public void sideGrab()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *8f,8f,1f);
        Vector3 loc = new Vector3(facing *4f,0f,0f);
        GameObject HoldPerson = Instantiate(entities.GetEntityPrefab("HoldPerson"), transform.position + loc, transform.rotation);
        HoldPerson.transform.localScale = flip;
        foreach (HitboxCollision hitbox in HoldPerson.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        HoldPerson.GetComponentInChildren<RyykeGrab>().drifter = drifter;
        entities.AddEntity(HoldPerson);
        
    }

    //Down W
    public void plantGravestone()
    {

        if(activeStone){
            activeStone.GetComponent<RyykeTombstone>().Break();
        }  
        facing = movement.Facing;
        if(!movement.grounded)rb.velocity = new Vector2(0,10);
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
            hitbox.Facing = facing;
        }
        
        tombstone.GetComponent<RyykeTombstone>().facing=facing;
        activeStone = tombstone;
        entities.AddEntity(tombstone);
    }

    //Neutral W
    public void awaken(){
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *8f,8f,1f);
        Vector3 loc = new Vector3(facing *3.5f,0f,0f);
        GameObject tombstone = Instantiate(entities.GetEntityPrefab("RyykeTombstone"), transform.position + loc, transform.rotation);
        tombstone.transform.localScale = flip;
        foreach (HitboxCollision hitbox in tombstone.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        
        tombstone.GetComponent<RyykeTombstone>().facing=facing;
        tombstone.GetComponent<RyykeTombstone>().grounded = true;
        tombstone.GetComponent<RyykeTombstone>().activate = true;
        entities.AddEntity(tombstone);
    }

    public void grantStack()
    {
    	if(drifter.Charge < 3){
            audioSource.PlayOneShot(audioClips[0]);
    		drifter.Charge++;
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



    //Inhereted Roll Methods

    public override void roll()
    {
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }

    public override void rollGetupStart(){
        applyEndLag(1);
        rb.velocity = new Vector3(0,78f,0);
    }

    public override void rollGetupEnd()
    {
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 45f,5f);
    }
}
