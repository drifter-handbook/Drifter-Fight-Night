﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeMasterHit : MasterHit
{

	public SyncAnimatorStateHost sparkle;


	bool listeningForDirection = false;
	bool listeningForMovement = false;
	bool burrowing = false;

	//0 Up stone
	//1 Side Stone
	//2 Down Stone
	Tombstone[] tombstones = new Tombstone[] {null,null,null};



	//Constant vector to offset stone detection range
	Vector2 stoneOffset = new Vector2(0,2);

	//Index of the next stone to place
	int tombstoneIndex = 0;

	//The current closest active stone
	int nearbyStone = -1;

	//The current stone being targeted for teleportation with down special
	int targetStone = -1;



	new void Update()
    {
        if(!isHost)return;

        base.Update();
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
       		for(int i = 0; i <3; i++)
       			Destroy(tombstones[i].gameObject);
        }

        if(listeningForDirection)
        {
        	targetStone = nearbyStone;
        	if(drifter.input[0].MoveY  !=0 || drifter.input[0].MoveX != 0)
      		{
        		if(drifter.input[0].MoveY > 0 && drifter.input[1].MoveY  <= 0) targetStone = 0;
        		else if(drifter.input[0].MoveY < 0) targetStone = 2;
        		else targetStone = 1;
        		playState("W_Down_Emerge_Empowered");
        	}
        	
        }
        else
        	targetStone = -1;

        if(listeningForMovement)
        {
        	movement.move(14f);
        	if(!drifter.input[1].Jump && drifter.input[0].Jump)
        		playState("W_Down_Emerge");
        }

        isNearStone();
    }


	//Creates a tombstone projectile
    public void SpawnTombstone()
    {
        if(!isHost)return;
        facing = movement.Facing;

        bool stonesFull = true;
        
        GameObject stone = host.CreateNetworkObject("Tombstone", transform.position, transform.rotation);
        stone.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in stone.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
       }

       foreach (HurtboxCollision hurtbox in stone.GetComponentsInChildren<HurtboxCollision>(true))
            hurtbox.owner = drifter.gameObject;


       for(int i = 0; i <3; i++)
       {
       		if(tombstones[i] == null)
       		{
       			tombstoneIndex = i;
       			i = 3;
       			stonesFull = false;
       		}
       }

       if(stonesFull)
       {
       		tombstoneIndex++;
       		if(tombstoneIndex >2)tombstoneIndex = 0;
       		Destroy(tombstones[tombstoneIndex].gameObject);
       }

       stone.GetComponent<SyncAnimatorStateHost>().SetState(tombstoneIndex + "_Idle");
       stone.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
       tombstones[tombstoneIndex] = stone.GetComponent<Tombstone>();
       tombstones[tombstoneIndex].tombstoneType = tombstoneIndex;
       tombstones[tombstoneIndex].facing = facing;
       tombstones[tombstoneIndex].attacks = attacks;       
    }

    void isNearStone()
    {
    	bool reset = true;
    	for(int i = 0; i <3; i++)
    	{
        	if(tombstones[i] != null && Vector3.Distance(rb.position,(tombstones[i].rb.position + stoneOffset) ) < 4.5f && !burrowing)
        	{
        		if(!Empowered)sparkle.SetState("ChargeIndicator");
        		if(tombstones[i].canAct)nearbyStone = i;
        		if(!tombstones[i].active)tombstones[i].playAnimation("Activate",true,true);

        		tombstones[i].active = true;
        		Empowered= true;
        		reset = false;
        	}
        	else if(tombstones[i] != null && (Vector3.Distance(rb.position,(tombstones[i].rb.position + stoneOffset) ) >= 4.5f || status.HasStunEffect() || burrowing))
        	{
        		//Deactivate tombstones that are not nearby
        		if(tombstones[i].active)tombstones[i].playAnimation("Deactivate",false,true);
        		tombstones[i].active = false;
        		
        	}
    	}
    	if(reset)
    	{
    		if(Empowered)sparkle.SetState("Hide");
    		Empowered = false;
    		nearbyStone = -1;
    	}
    	
    }

    //Zombie Methods

    public void decrementStoneUses()
    {
    	if(tombstones[nearbyStone] !=  null && Empowered && nearbyStone >=0)tombstones[nearbyStone].Uses--;
    }


    public void Command(string state)
    {
    	if(!isHost)return;
    	if(tombstones[nearbyStone] !=  null && tombstones[nearbyStone].active)
    	{
    		refeshStoneHitboxes(tombstones[nearbyStone]);
    		tombstones[nearbyStone].playAnimation(state,false,true);
    		//decrementStoneUses();
    	}
    }

    public void refeshStoneHitboxes(Tombstone stone)
    {
    	if(!isHost)return;

        stone.updateDirection(movement.Facing);

        foreach (HitboxCollision hitbox in stone.gameObject.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = stone.facing;
        }

    }

    //W_Down Methods

    //Up Special empowered
    public void listenForDirection()
    {
        if(!isHost)return;
        listeningForDirection = true;
        isNearStone();
    }

    public void burrow()
    {
        if(!isHost)return;
        burrowing = true;
    }

    public void moveWhileBurrowed(int moveFlag)
    {
        if(!isHost)return;
        listeningForMovement = (moveFlag != 0);
    }

    public void warpToStone()
    {
    	if(!isHost)return;
    	if(targetStone != -1 && tombstones[targetStone] != null)
    			rb.position = tombstones[targetStone].gameObject.transform.position;

    	listeningForDirection = false;

    	burrowing = false;
    	targetStone = -1;
    	
    }

    //Adjusts Ryyke's terminal velocity for his down air
    public void setTerminalVelocity(float vel)
    {
        if(!isHost)return;
        movement.canLandingCancel = false;  
        movement.terminalVelocity = vel;
    }

    //Returns Ryyke's TV to normal at the end of the move
    public void resetTerminalVelocity()
    {
        if(!isHost)return; 
        movement.terminalVelocity = terminalVelocity;
    }


    public new void returnToIdle()
    {
        base.returnToIdle();
        listeningForDirection = false;
        listeningForMovement = false;
        burrowing = false;
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
        rb.velocity = new Vector3(0,75f,0);
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
