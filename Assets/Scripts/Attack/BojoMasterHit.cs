﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BojoMasterHit : MasterHit
{
    float boofTime;
    bool checkBoof;

    GameObject Centaur = null;

    void Update()
    {
        if(!isHost)return;
        if(checkBoof && Time.time - boofTime > 1f){
            attacks.currentRecoveries = 0;
            checkBoof = false;
        }
    	else{
    		drifter.SetAnimatorBool("HasCharge",false);
    	}
    }

    public void GUN(){

        if(!isHost)return;

        applyEndLag(drifter.input.Special?0:1);
        
    	facing = movement.Facing;
        Vector3 flip = new Vector3(facing *6f,6f,0f);
        Vector3 pos = new Vector3(facing *3f,4f,1f);
        GameObject bubble = host.CreateNetworkObject("Mockery", transform.position + pos, transform.rotation);
        bubble.transform.localScale = flip;
        bubble.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 55, 0);
       	drifter.SetAnimatorBool("HasCharge",true);
        
        foreach (HitboxCollision hitbox in bubble.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
    }

    public void dismount()
    {
        if(!isHost)return;
        facing = movement.Facing;
        if(TransitionFromChanneledAttack())
        {

            if(Centaur != null)Destroy(Centaur);
            Vector3 flip = new Vector3(facing *9f,9f,0f);
            Vector3 pos = new Vector3(facing *0f,0f,1f);
            Centaur = host.CreateNetworkObject("Kamikaze", transform.position + pos, transform.rotation);
            Centaur.transform.localScale = flip;
            Centaur.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 50, 0);
        
            foreach (HitboxCollision hitbox in Centaur.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
        }

    }

    public void boof()
    {
        if(!isHost)return;
        facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x  + facing * 10,45);
        boofTime = Time.time;
        checkBoof = true;
    }



    //Inhereted Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,70f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
