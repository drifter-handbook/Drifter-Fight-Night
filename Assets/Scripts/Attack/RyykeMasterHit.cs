using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeMasterHit : MasterHit
{

    static float maxBurrowTime = 2f;
	bool listeningForDirection = false;
	bool listeningForMovement = false;
	bool burrowing = false;
	float burrowTime = maxBurrowTime;

	float zombieRadius = 3.75f;

    //Tether Recovery Range object
    public TetherRange tether;
    //Point to tether to
    Vector3 TetherPoint = Vector3.zero;

    GameObject arm;

	//0 Up stone
	//1 Side Stone
	//2 Down Stone
	Tombstone[] tombstones = new Tombstone[] {null,null,null};

    Tether armTether;


	//Index of the next stone to place
	int tombstoneIndex = 0;

	//The current closest active stone
	int nearbyStone = -1;

	//The current stone being targeted for teleportation with down special
	int targetStone = -1;

	void Update()
	{
		if(burrowing && burrowTime >0)
        {
        	burrowTime-= Time.deltaTime;
        }

        if(arm!= null &&(movement.ledgeHanging || status.HasEnemyStunEffect())) deleteArm();
	}


	new void FixedUpdate()
    {
        if(!isHost)return;

        base.FixedUpdate();
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
       		for(int i = 0; i <3; i++)
       			if(tombstones[i] != null)
       				Destroy(tombstones[i].gameObject);
       		
        }

        

        if(listeningForDirection)
        {
        	targetStone = nearbyStone;
        	if(drifter.input[0].MoveY  !=0 || drifter.input[0].MoveX != 0)
      		{
        		if(drifter.input[0].MoveY > 0) targetStone = 0;
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
        	if((!drifter.input[1].Jump && drifter.input[0].Jump) || burrowTime <=0)
        	{
        		attacks.SetupAttackID(DrifterAttackType.W_Down);
        		playState("W_Down_Emerge");
        		listeningForMovement = false;
        	}
        	else if(drifter.input[0].MoveX !=0)
        		playState("W_Down_Move");
        	else
        		playState("W_Down_Idle");

        }

        isNearStone();
    }

    public void playStateByStone(string state)
    {
    	if(!isHost)return;

    	int index = tombstoneIndex;
    	bool stonesFull = true;
    	for(int i = 0; i <3; i++)
       {
       		if(tombstones[i] == null)
       		{
       			index = i;
       			i = 3;
       			stonesFull = false;
       		}
       }

       if(stonesFull)
       {
       		index++;
       		if(index >2)index = 0;
       }

       UnityEngine.Debug.Log(index);

       playState(index + state);
    }


	//Creates a tombstone projectile
    public void SpawnTombstone(int mode = 0)
    {
        if(!isHost)return;
        facing = movement.Facing;

        bool stonesFull = true;
        
        GameObject stone = host.CreateNetworkObject("Tombstone", transform.position + new Vector3(1 * facing,.5f,0), transform.rotation);
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
       tombstones[tombstoneIndex] = stone.GetComponent<Tombstone>().setup(tombstoneIndex,facing,attacks,drifter.gameObject,zombieRadius);
       tombstones[tombstoneIndex].throwStone(mode);
    }

    void isNearStone()
    {
    	bool reset = true;
    	float bestDistance = 100f;
    	for(int i = 0; i <3; i++)
    	{
    		if(tombstones[i] != null)
    		{
    			float distance = tombstones[i].getDistance(rb.position);
	    		
	        	if(distance < zombieRadius && !burrowing)
	        	{
	        		
	        		if(tombstones[i].canAct && distance < bestDistance)
	        		{
	        			nearbyStone = i;
	        			bestDistance = distance;
	        		}
	        		if(!tombstones[i].active)tombstones[i].playAnimation("Activate",true,true);
	        		reset = false;

	        		if(tombstones[i].active)
	        		{
	        			if(!Empowered)drifter.sparkle.SetState("ChargeIndicator");
	        			Empowered = true;
	        		}
	        		
	        	}
	        	else if(distance >= zombieRadius || status.HasStunEffect() || burrowing)
	        	{
	        		//Deactivate tombstones that are not nearby
	        		if(tombstones[i].active)tombstones[i].playAnimation("Deactivate",true,true);
	        		tombstones[i].active = false;
	        		
	        	}
        	}
    	}
    	if(reset)
    	{
    		if(Empowered)drifter.sparkle.SetState("Hide");
    		Empowered = false;
    		nearbyStone = -1;
    	}
    	
    }


    //W Up Methods
    public void SpawnTether()
    {
        if(!isHost)return;
        facing = movement.Facing;

        float angle = 55f  * facing;
        float len = 1.28f;
                
        Vector3 pos = new Vector3(1.5f * facing,3.7f,0);

        if(arm != null)deleteArm();
        bool targetLedge = false;
        if(tether.TetherPoint != Vector3.zero)
        {

            TetherPoint = tether.TetherPoint;
            //angle = Vector2.Angle(tether.TetherPoint,transform.position + pos);
            float deltay = TetherPoint.y- (transform.position + pos).y;
            float deltax = TetherPoint.x- (transform.position + pos).x;
            angle = Mathf.Atan2(deltay, deltax)*180 / Mathf.PI + (facing < 0 ?180:0);


            len = Vector2.Distance(transform.position + pos,TetherPoint) /10f;
            targetLedge = true;
            playState("W_Up_Ledge");

        }

        arm = host.CreateNetworkObject("Ryyke_Arm", transform.position + pos, Quaternion.Euler(0,0,angle));
        arm.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in arm.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        arm.transform.SetParent(drifter.gameObject.transform);
        arm.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());

        armTether = arm.GetComponentInChildren<Tether>();
        armTether.setTargetLength(len);
        //Cut this later?
        //if(movement.currentJumps < movement.numberOfJumps) movement.currentJumps++;
        if(targetLedge)disableArmHitbox();
    }

    public void setArmLen(float len)
    {
        if(!isHost || arm == null)return;
        armTether.setTargetLength(len);
    }

    public void freezeTether(float len)
    {
        if(!isHost || arm == null)return;
        armTether.freezeLen();
    }


    public void pullToLedge()
    {
        if(!isHost)return;
        if(TetherPoint!=Vector3.zero)
        {
            Vector3 dir = TetherPoint - new Vector3(rb.position.x,rb.position.y);
            Vector3.Normalize(dir);
            rb.velocity = 10f * dir;
            armTether.setSpeed(.5f);
   
            TetherPoint=Vector3.zero;
        }
    }
    public void deleteArm()
    {
        if(arm != null)Destroy(arm);
        arm = null;
        armTether= null;

    }

    public void disableArmHitbox()
    {
        armTether.togglehitbox(0);
    }

    //Flips the direction the character is facing mid move)
    public void invertDirection()
    {
        if(!isHost)return;
        movement.flipFacing();
    }


    //Particles
    public void dust()
    {
        if(!isHost)return;

        if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(3.5f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
    }

    //Zombie Methods

    public void decrementStoneUses()
    {
    	if(tombstones[nearbyStone] !=  null && Empowered && nearbyStone >=0)tombstones[nearbyStone].Uses--;
    }


    public void Command(string state)
    {
    	if(!isHost)return;
    	if(nearbyStone >=0 && tombstones[nearbyStone] != null && tombstones[nearbyStone].active && tombstones[nearbyStone].canAct)
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
        burrowTime = maxBurrowTime;
        listenForLedge(true);

    }

    public void moveWhileBurrowed(int moveFlag)
    {
        if(!isHost)return;
        listeningForMovement = (moveFlag != 0);
    }

    public void warpToStone()
    {
    	if(!isHost)return;
    	if(targetStone != -1 && tombstones[targetStone] != null && tombstones[targetStone].canAct)
    			rb.position = tombstones[targetStone].gameObject.transform.position + new Vector3(0,2f);

    	listeningForDirection = false;

    	burrowing = false;
    	targetStone = -1;
    	
    }


    public new void returnToIdle()
    {
        base.returnToIdle();
        clear();
    }

    public void clear()
    {
        listeningForDirection = false;
        listeningForMovement = false;
        burrowTime = maxBurrowTime;
        burrowing = false;
        deleteArm(); 
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
