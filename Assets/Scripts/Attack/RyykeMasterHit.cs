using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeMasterHit : MasterHit
{
    //Static values
    static int maxBurrowTime = 120;
    static float zombieRadius = 3.5f;

    //Tether References
    //Tether Recovery Range object
    public TetherRange tether;
    
    //Tether armTether;
    
    //Gameobjects to sync separately
    //0 Up stone
    //1 Side Stone
    //2 Down Stone
    Tombstone[] tombstones = new Tombstone[] {null,null,null};
    
    GameObject g_arm;
    GameObject g_hand;
    public GameObject line;


	//Gamestate sync
    bool listeningForDirection = false;
    bool listeningForMovement = false;
    bool burrowing = false;
    int burrowTime = maxBurrowTime;
    Vector3 tetherPoint = Vector3.zero;
    //Index of the next stone to place
    int tombstoneIndex = 0;
    //The current closest active stone
    int nearbyStone = -1;
    //The current stone being targeted for teleportation with down special
    int targetStone = -1;

    override public void UpdateFrame()
    {
        base.UpdateFrame();

        //Remove the arm if it is not needed
        //if(g_arm!= null &&(movement.ledgeHanging || status.HasEnemyStunEffect())) deleteArm();

        //remove all tombstones on death
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            for(int i = 0; i <3; i++)
                if(tombstones[i] != null)
                    Destroy(tombstones[i].gameObject);
        }


        //Listen for a directional input in empowered down special
        if(listeningForDirection)
        {
            targetStone = nearbyStone;
            if(drifter.input[0].MoveY  !=0 || drifter.input[0].MoveX != 0)
            {
                movement.updateFacing();
                if(drifter.input[0].MoveY > 0) targetStone = 0;
                else if(drifter.input[0].MoveY < 0) targetStone = 2;
                else targetStone = 1;
                playState("W_Down_Emerge_Empowered");
            }
            
        }
        else
            targetStone = -1;

        //Listen for movement commands in unempowered down special
        if(listeningForMovement)
        {
            movement.move(14f);
            if((!drifter.input[1].Jump && drifter.input[0].Jump) || burrowTime <=0)
            {
                attacks.SetMultiHitAttackID();
                playState("W_Down_Emerge");
                listeningForMovement = false;
            }
            else if(drifter.input[0].MoveX !=0)
                playState("W_Down_Move");
            else
                playState("W_Down_Idle");

        }

        //Tick down the burrow timer
        if(burrowing && burrowTime >0)
        {
            burrowTime--;
        }

        isNearStone();

         for(int i = 0; i <3; i++)
             if(tombstones[i] != null) tombstones[i].GetComponent<Tombstone>().UpdateFrame();

        if(g_arm != null && g_hand != null)
        {
            g_arm.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
            g_hand.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
            line.GetComponentInChildren<LineRenderer>().SetPosition(0,g_arm.transform.position);
            line.GetComponentInChildren<LineRenderer>().SetPosition(1,g_hand.transform.position);
        }
        else
        {
            line.SetActive(false);
        }

        
    }

    public void playStateByStone(string state)
    {

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

       playState(index + state);
    }


	//Creates a tombstone projectile
    public void SpawnTombstone(int mode = 0)
    {
        
        bool stonesFull = true;
        
        GameObject stone = GameController.Instance.CreatePrefab("Tombstone", transform.position + new Vector3(1 * movement.Facing,.5f,0), transform.rotation);
        stone.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in stone.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
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

       //stone.GetComponent<SyncAnimatorStateHost>().SetState(tombstoneIndex + "_Spin");
       stone.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);
       tombstones[tombstoneIndex] = stone.GetComponent<Tombstone>().setup(tombstoneIndex,movement.Facing,drifter.gameObject,zombieRadius);
       tombstones[tombstoneIndex].throwStone(mode);
    }

    void isNearStone()
    {
    	bool reset = true;
    	float bestDistance = 100f;
    	for(int i = 0; i <3; i++)
    	{
            //If the tombstone with ID I exists
    		if(tombstones[i] != null)
    		{
                //Get the distance to Ryyke
    			float distance = tombstones[i].getDistance(rb.position);
	    		
                //If it is within the active range, and Ryyke is not burrowing
	        	if(distance < zombieRadius && !burrowing && !status.HasEnemyStunEffect() &&  tombstones[i].canAct)
	        	{
                    //Disable the reset flag for empowered state if a stone is nearby
	        		reset = false;
                    //If the tombstone can act and it is the closer than the current closest tombstone
	        		if(distance < bestDistance)
	        		{
                        //Set it as the closest stone and update the distance
	        			nearbyStone = i;
	        			bestDistance = distance;
	        		}
	        		//if(!tombstones[i].active)tombstones[i].PlayConditionalAnimation("Activate",true,true);
	        		

	        		// if(tombstones[i].active)
	        		// {
	        		// 	if(!Empowered)drifter.sparkle.SetState("ChargeIndicator");
	        		// 	Empowered = true;
	        		// }
	        		
	        	}
                //Otherwise, if Ryyke is stunned, burrowing, or out of range
	        	else if((distance >= zombieRadius  || burrowing || status.HasStunEffect()) && !tombstones[i].attacking)
	        	{
	        		//Deactivate tombstones that are not nearby
	        		if(tombstones[i].active)tombstones[i].PlayConditionalAnimation("Deactivate",true,true);
	        		tombstones[i].active = false;
	        		
	        	}
        	}
    	}

        //After the closest stone is found, Activate it
        if(nearbyStone >=0 && !tombstones[nearbyStone].active)
        {
            tombstones[nearbyStone].PlayConditionalAnimation("Activate",true,true);

            //Deactivate all other active stones, so only one zombie is active at a time
            for(int i = 0; i <3; i++)
            {
                if(tombstones[i] != null && i != nearbyStone && tombstones[i].active && !tombstones[i].attacking)
                {
                    tombstones[i].PlayConditionalAnimation("Deactivate",true,true);
                    tombstones[i].active = false;
                }

            }
        }
        
        //If the closest stone is already active and exists, and Ryyke is not empowered
        if(nearbyStone >=0 && tombstones[nearbyStone].active && !Empowered && tombstones[nearbyStone].canAct)
        {
            //Show the empowered sparkle
            drifter.Sparkle(true);
            Empowered = true;
        }

        //If the reset flag is set, de-empower Ryyke and hide the sparkle.
    	if(reset)
    	{
    		if(Empowered)drifter.Sparkle(false);
    		Empowered = false;
    		nearbyStone = -1;
    	}
    	
    }


    //W Up Methods
    public void SpawnTether()
    {

        if(g_arm!= null) Destroy(g_arm);
        if(g_hand!= null) Destroy(g_hand);
         float angle = 55f  * movement.Facing;
        // Vector3 pos = new Vector3(1.5f * movement.Facing,3.7f,0);
        // if(tether.TetherPoint != Vector3.zero)
        // {

        //     tetherPoint = tether.TetherPoint;
        //     //angle = Vector2.Angle(tether.tetherPoint,transform.position + pos);
        //     float deltay = tetherPoint.y- (transform.position + pos).y;
        //     float deltax = tetherPoint.x- (transform.position + pos).x;
        //     angle = Mathf.Atan2(deltay, deltax)*180 / Mathf.PI + (movement.Facing < 0 ?180:0);
        // }


         // if(arm != null)deleteArm();
        // bool targetLedge = false;
        if(tether.TetherPoint != Vector3.zero)
        {
            tetherPoint = tether.TetherPoint;
            //targetLedge = true;
            playState("W_Up_Ledge");

        }
        else
            tetherPoint = Vector3.zero;

        


        g_arm = GameController.Instance.CreatePrefab("Ryyke_Arm", transform.position + new Vector3(1.5f * movement.Facing,3.7f), Quaternion.Euler(0,0,angle));
        g_arm.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        g_arm.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);

        g_arm.transform.SetParent(drifter.gameObject.transform);

        g_hand = GameController.Instance.CreatePrefab("Ryyke_Hand", transform.position + new Vector3(1.5f * movement.Facing,3.7f), Quaternion.Euler(0,0,angle));
        g_hand.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

        g_hand.GetComponent<Rigidbody2D>().velocity = rb.velocity + new Vector2(25f * movement.Facing * Mathf.Cos((55* Mathf.PI)/180),25f *Mathf.Sin((55 * Mathf.PI)/180));

        foreach (HitboxCollision hitbox in g_hand.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }
        g_hand.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);

        line.SetActive(true);

        line.GetComponentInChildren<LineRenderer>().SetPosition(0,g_arm.transform.position);
        line.GetComponentInChildren<LineRenderer>().SetPosition(1,g_hand.transform.position);


        // float angle = 55f  * movement.Facing;
        // float len = 1.28f;
                
        // Vector3 pos = new Vector3(1.5f * movement.Facing,3.7f,0);

        // if(arm != null)deleteArm();
        // bool targetLedge = false;
        // if(tether.TetherPoint != Vector3.zero)
        // {

        //     tetherPoint = tether.TetherPoint;
        //     //angle = Vector2.Angle(tether.tetherPoint,transform.position + pos);
        //     float deltay = tetherPoint.y- (transform.position + pos).y;
        //     float deltax = tetherPoint.x- (transform.position + pos).x;
        //     angle = Mathf.Atan2(deltay, deltax)*180 / Mathf.PI + (movement.Facing < 0 ?180:0);


        //     len = Vector2.Distance(transform.position + pos,tetherPoint) /10f;
        //     targetLedge = true;
        //     playState("W_Up_Ledge");

        // }
        // else
        //     tetherPoint = Vector3.zero;

        // arm = GameController.Instance.CreatePrefab("Ryyke_Arm", transform.position + pos, Quaternion.Euler(0,0,angle));
        // arm.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        // foreach (HitboxCollision hitbox in arm.GetComponentsInChildren<HitboxCollision>(true))
        // {
        //     hitbox.parent = drifter.gameObject;
        //     hitbox.AttackID = attacks.AttackID;
        //     hitbox.Facing = movement.Facing;
        // }
        // arm.transform.SetParent(drifter.gameObject.transform);
        // arm.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);
        // armTether = arm.GetComponentInChildren<Tether>();
        // armTether.setTargetLength(len);
        // //Cut this later?
        // //if(movement.currentJumps < movement.numberOfJumps) movement.currentJumps++;
        // if(targetLedge)disableArmHitbox();
    }

    // public void setArmLen(float len)
    // {
    //     if( arm == null)return;
    //     armTether.setTargetLength(len);
    // }

    // public void freezeTether()
    // {
    //     if( arm == null)return;
    //     armTether.freezeLen();
    // }


    public void pullToLedge()
    {
        if(tetherPoint!=Vector3.zero)
        {
            Vector3 dir = tetherPoint - new Vector3(rb.position.x,rb.position.y);
            Vector3.Normalize(dir);
            rb.velocity = 10f * dir;
            //armTether.setSpeed(.5f);
   
            tetherPoint=Vector3.zero;
        }
    }
    // public void deleteArm()
    // {
    //     if(arm != null)Destroy(arm);
    //     arm = null;
    //     armTether= null;
    // }

    // public void disableArmHitbox()
    // {
    //     armTether.togglehitbox(0);
    // }

    //Flips the direction the character is movement.Facing mid move)
    public void invertDirection()
    {
        movement.flipFacing();
    }


    //Particles
    public void dust()
    {

        if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(3.5f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
    }

    //Zombie Methods

    public void decrementStoneUses()
    {
        UnityEngine.Debug.Log("REMOVE ME");
    }


    public void Command(string state)
    {
    	if(nearbyStone >=0 && tombstones[nearbyStone] != null && tombstones[nearbyStone].active && tombstones[nearbyStone].canAct)
    	{
    		refeshStoneHitboxes(tombstones[nearbyStone]);
    		tombstones[nearbyStone].PlayConditionalAnimation(state,false,true);
            tombstones[nearbyStone].attacking = true;
    		//decrementStoneUses();
    	}
    }

    public void refeshStoneHitboxes(Tombstone stone)
    {

        stone.updateDirection(movement.Facing);

        foreach (HitboxCollision hitbox in stone.gameObject.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = stone.facing;
            hitbox.isActive = true;
        }

    }

    //W_Down Methods

    //Up Special empowered
    public void listenForDirection()
    {
        listeningForDirection = true;
        isNearStone();
    }

    public void burrow()
    {
        burrowing = true;
        burrowTime = maxBurrowTime;
        listenForLedge(true);
        movement.cancelJump();

    }

    public void moveWhileBurrowed(int moveFlag)
    {
        listeningForMovement = (moveFlag != 0);
    }

    public void warpToStone()
    {
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
        //deleteArm(); 
    }

    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();

        TombstoneRollbackFrame[] p_Tombstones = new TombstoneRollbackFrame[] {null,null,null};

        for(int i = 0; i <3; i++)
             p_Tombstones[i] = tombstones[i] != null ? tombstones[i].GetComponent<Tombstone>().SerializeFrame(): null;

        baseFrame.CharacterFrame = new RyykeRollbackFrame()
        {
            Tombstones = p_Tombstones,
            ListeningForDirection = listeningForDirection, 
            ListeningForMovement = listeningForMovement,
            Burrowing = burrowing,
            BurrowTime = burrowTime, 
            TetherPoint = this.tetherPoint,
            TombstoneIndex = tombstoneIndex
        };

         return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);

        RyykeRollbackFrame ryyke_frame = (RyykeRollbackFrame)p_frame.CharacterFrame;


        //Tombstone reset
        for(int i = 0; i <3; i++)
        {
            if(ryyke_frame.Tombstones[i] != null)
            {
                if(tombstones[i] == null) SpawnTombstone(i);
                tombstones[i].GetComponent<Tombstone>().DeserializeFrame(ryyke_frame.Tombstones[i]);
            }
            //Projectile does not exist in rollback frame
            else
            {
                Destroy(tombstones[i].gameObject);
                tombstones[i] = null;
            }  
        }
        

    }

}

public class RyykeRollbackFrame: ICharacterRollbackFrame
{
    public string Type { get; set; }
    public bool ListeningForDirection;
    public bool ListeningForMovement;
    public bool Burrowing;
    public int BurrowTime;
    public Vector3 TetherPoint;
    public int TombstoneIndex;
    public TombstoneRollbackFrame[] Tombstones;
}
