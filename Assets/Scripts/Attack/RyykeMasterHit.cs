using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeMasterHit : MasterHit
{
	//Static values
	static int maxBurrowTime = 120;
	static float zombieRadius = 3.5f;
	
	//Gameobjects to sync separately
	//0 Up stone
	//1 Side Stone
	//2 Down Stone
	Tombstone[] tombstones = new Tombstone[] {null,null,null};	

	//Tether References
	//Tether Recovery Range object
	public TetherRange tether;
	GameObject g_Tether_Arm;
	GameObject g_Tether_Hand;
	bool armRetracting = false;

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

	override public void UpdateFrame() {
		base.UpdateFrame();

		//Remove the arm if it is not needed
		if(g_Tether_Arm!= null &&(movement.ledgeHanging || status.HasEnemyStunEffect())) DeleteArm();

		//remove all tombstones on death
		if(status.HasStatusEffect(PlayerStatusEffect.DEAD)) {
			DeleteAllStones();
			DeleteArm();
		}

		//Listen for a directional input in empowered down special
		if(listeningForDirection) {
			targetStone = nearbyStone;
			if(drifter.input[0].MoveY  !=0 || drifter.input[0].MoveX != 0) {
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
		if(listeningForMovement) {
			movement.move(14f);
			ledgeDetector.UpdateFrame();
			if((!drifter.input[1].Jump && drifter.input[0].Jump) || burrowTime <=0) {
				attacks.SetMultiHitAttackID();
				playState("W_Down_Emerge");
				listeningForMovement = false;
			}
			else if(drifter.input[0].MoveX !=0)
				drifter.PlayAnimation("W_Down_Move",0,true);
			else
				drifter.PlayAnimation("W_Down_Idle",0,true);
		}

		//Tick down the burrow timer
		if(burrowing && burrowTime >0) {
			burrowTime--;
		}

		isNearStone();

		if(g_Tether_Arm != null && g_Tether_Hand != null) {
			if(tetherPoint != Vector3.zero)
				g_Tether_Hand.GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(g_Tether_Hand.GetComponent<Rigidbody2D>().position, tetherPoint,3);

			else if(armRetracting)
				g_Tether_Hand.GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(g_Tether_Hand.GetComponent<Rigidbody2D>().position, g_Tether_Arm.transform.position,3);
			
			g_Tether_Arm.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
			g_Tether_Hand.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

			g_Tether_Arm.GetComponentInChildren<LineRenderer>().SetPosition(0,g_Tether_Arm.transform.position);
			g_Tether_Arm.GetComponentInChildren<LineRenderer>().SetPosition(1,g_Tether_Hand.transform.position);
		}
		else
		{
			armRetracting = false;
		}

		for(int i = 0; i <3; i++)
			 if(tombstones[i] != null)
			 	tombstones[i].GetComponent<Tombstone>().UpdateFrame();
	  
	}

	void DeleteAllStones() {
		for(int i = 0; i <3; i++)
			if(tombstones[i] != null){
				tombstones[i].breakStone();
				tombstones[i] = null;
			}
	}

	//Play a specified Tombstone spawn animation based on which one is being created
	public void playStateByStone(string state) {

		int index = tombstoneIndex;
		bool stonesFull = true;

		for(int i = 0; i <3; i++) {
			if(tombstones[i] == null) {
				index = i;
				i = 3;
				stonesFull = false;
			}
		}

		if(stonesFull) {
			index++;
			if(index >2)index = 0;
		}

		playState(index + state);
	}

	//Creates a tombstone projectile
	public void SpawnTombstone(int mode = 0) {
		
		bool stonesFull = true;
		
		GameObject stone = GameController.Instance.CreatePrefab("Tombstone", transform.position + new Vector3(1 * movement.Facing,.5f,0), transform.rotation);
		stone.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in stone.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   }

	   foreach (HurtboxCollision hurtbox in stone.GetComponentsInChildren<HurtboxCollision>(true))
			hurtbox.owner = drifter.gameObject;

		for(int i = 0; i <3; i++) {
			if(tombstones[i] == null) {
				tombstoneIndex = i;
				i = 3;
				stonesFull = false;
			}
		}

	   if(stonesFull) {
			tombstoneIndex++;
			if(tombstoneIndex >2)tombstoneIndex = 0;
			tombstones[tombstoneIndex].breakStone();
	   }

	   SetObjectColor(stone);
	   tombstones[tombstoneIndex] = stone.GetComponent<Tombstone>().setup(tombstoneIndex,movement.Facing,drifter.gameObject,zombieRadius);
	   tombstones[tombstoneIndex].throwStone(mode);
	}

	void isNearStone() {
		bool reset = true;
		float bestDistance = 100f;
		for(int i = 0; i <3; i++) {
			//If the tombstone with ID I exists
			if(tombstones[i] != null) {
				//Get the distance to Ryyke
				float distance = tombstones[i].getDistance(rb.position);
				
				//If it is within the active range, and Ryyke is not burrowing
				if(distance < zombieRadius && !burrowing && !status.HasEnemyStunEffect() && tombstones[i].canAct) {
					//Disable the reset flag for empowered state if a stone is nearby
					reset = false;
					//If the tombstone can act and it is the closer than the current closest tombstone
					if(distance < bestDistance)	{
						//Set it as the closest stone and update the distance
						nearbyStone = i;
						bestDistance = distance;
					}
				}
				// //Otherwise, if Ryyke is stunned, burrowing, or out of range
				// else if((burrowing || status.HasStunEffect()) && !tombstones[i].attacking) {
				// 	//Deactivate tombstones that are not nearby
				// 	if(tombstones[i].active)tombstones[i].PlayConditionalAnimation("Deactivate",true,true);
				// 	tombstones[i].active = false;
				// }
			}
		}

		//After the closest stone is found, Activate it
		if(nearbyStone >=0 && !tombstones[nearbyStone].active) 
			tombstones[nearbyStone].PlayConditionalAnimation(nearbyStone + "_Activate",true,true);

		//Deactivate all other active stones, so only one zombie is active at a time
		for(int i = 0; i <3; i++) {
			if(tombstones[i] != null && i != nearbyStone && tombstones[i].active && !tombstones[i].attacking) {
				tombstones[i].PlayConditionalAnimation("Deactivate",true,true);
				tombstones[i].active = false;
			}
			
		}
		
		//If the closest stone is already active and exists, and Ryyke is not empowered
		if(nearbyStone >=0 && tombstones[nearbyStone].active && !Empowered && tombstones[nearbyStone].canAct) {
			//Show the empowered sparkle
			drifter.Sparkle(true);
			Empowered = true;
		}

		//If the reset flag is set, de-empower Ryyke and hide the sparkle.
		if(reset) {
			if(Empowered)drifter.Sparkle(false);
			Empowered = false;
			nearbyStone = -1;
		}
	}

	//W Up Methods
	void CreateArm(Vector3 pos, float angle) {

		g_Tether_Arm = GameController.Instance.CreatePrefab("Ryyke_Arm", pos, Quaternion.Euler(0,0,angle));
		g_Tether_Arm.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		SetObjectColor(g_Tether_Arm);

		g_Tether_Arm.transform.SetParent(drifter.gameObject.transform);
	}

	void CreateHand(Vector3 pos, float angle) {

		g_Tether_Hand = GameController.Instance.CreatePrefab("Ryyke_Hand", pos, Quaternion.Euler(0,0,angle));
		g_Tether_Hand.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		SetObjectColor(g_Tether_Hand);

		g_Tether_Hand.GetComponent<RemoteProjectileUtil>().hit = this;
	}

	public void SpawnTether() {
		DeleteArm();

		float angle = 55f *movement.Facing;
		Vector3 pos = new Vector3(1.5f * movement.Facing,3.6f,0);

		//Calculate Ledge Position
		if(tether.TetherPoint != Vector3.zero) {
			tetherPoint = tether.TetherPoint;
			float deltay = tetherPoint.y- (transform.position + pos).y;
			float deltax = tetherPoint.x- (transform.position + pos).x;
			angle = Mathf.Atan2(deltay, deltax)*180 / Mathf.PI + (movement.Facing < 0 ?180:0);
			playState("W_Up_Ledge");
		}
		else
			tetherPoint = Vector3.zero;

		CreateArm(transform.position + pos, angle);
		CreateHand(transform.position + pos, angle);

		//If the arm is not targeting a ledge, fire it as a projectile
		if(tetherPoint == Vector3.zero) g_Tether_Hand.GetComponent<Rigidbody2D>().velocity = rb.velocity + new Vector2(80f * movement.Facing * Mathf.Cos((angle*movement.Facing* Mathf.PI)/180),80f *Mathf.Sin((angle*movement.Facing * Mathf.PI)/180));
		else
			foreach (HitboxCollision hitbox in g_Tether_Hand.GetComponentsInChildren<HitboxCollision>(true))
				hitbox.isActive = false;

		foreach (HitboxCollision hitbox in g_Tether_Hand.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}
	
		g_Tether_Arm.GetComponentInChildren<LineRenderer>().SetPosition(0, g_Tether_Arm.transform.position);
		g_Tether_Arm.GetComponentInChildren<LineRenderer>().SetPosition(1, g_Tether_Hand.transform.position);

	}

	public void RetractArm() {
		armRetracting = true;
	}

	public void pullToLedge() {
		if(tetherPoint!=Vector3.zero) {
			Vector3 dir = tetherPoint - new Vector3(rb.position.x,rb.position.y);
			Vector3.Normalize(dir);
			rb.velocity = 10f * dir;
   
			tetherPoint = Vector3.zero;
		}
	}

	public void DeleteArm() {
		if(g_Tether_Arm!= null) Destroy(g_Tether_Arm);
		if(g_Tether_Hand!= null) Destroy(g_Tether_Hand);
		g_Tether_Arm = null;
		g_Tether_Hand = null;

	}

	public override void TriggerRemoteSpawn(int index) {
		playState("W_Up_Drifter");
	}

	//Flips the direction the character is movement.Facing mid move)
	public void invertDirection() {
		movement.flipFacing();
	}


	//Particles
	public void dust() {
		if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(3.5f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
	}

	//Zombie Methods

	public void decrementStoneUses() {
		UnityEngine.Debug.Log("REMOVE ME");
	}


	public void Command(string state) {
		if(nearbyStone >=0 && tombstones[nearbyStone] != null && tombstones[nearbyStone].active && tombstones[nearbyStone].canAct) {
			refeshStoneHitboxes(tombstones[nearbyStone]);
			tombstones[nearbyStone].PlayConditionalAnimation(state,false,true);
			tombstones[nearbyStone].attacking = true;
			//decrementStoneUses();
		}
	}

	public void refeshStoneHitboxes(Tombstone stone) {
		stone.updateDirection(movement.Facing);

		foreach (HitboxCollision hitbox in stone.gameObject.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = stone.facing;
			hitbox.isActive = true;
		}
	}

	//W_Down Methods

	//Up Special empowered
	public void listenForDirection() {
		listeningForDirection = true;
		isNearStone();
	}

	public void W_Down_Dash() {
		//burrowing = true;
		//burrowTime = maxBurrowTime;
		listenForLedge(true);
		if(ledgeDetector.IsTouchingGround())setXVelocity(35f);
		movement.cancelJump();
	}

	public void burrow() {
		burrowing = true;
		burrowTime = maxBurrowTime;
		listenForLedge(true);
		movement.cancelJump();
	}

	public void moveWhileBurrowed(int moveFlag) {
		listeningForMovement = (moveFlag != 0);
	}

	public void warpToStone() {
		if(targetStone != -1 && tombstones[targetStone] != null && tombstones[targetStone].canAct)
				rb.position = tombstones[targetStone].gameObject.transform.position + new Vector3(0,2f);

		listeningForDirection = false;

		burrowing = false;
		targetStone = -1;
	}


	public new void returnToIdle() {
		base.returnToIdle();
		clear();
	}

	public void clear() {
		listeningForDirection = false;
		listeningForMovement = false;
		burrowTime = maxBurrowTime;
		burrowing = false;
		DeleteArm(); 
	}

	 //Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();

		TombstoneRollbackFrame[] p_Tombstones = new TombstoneRollbackFrame[] {null,null,null};

		for(int i = 0; i <3; i++)
			 p_Tombstones[i] = tombstones[i] != null ? tombstones[i].GetComponent<Tombstone>().SerializeFrame(): null;

		baseFrame.CharacterFrame = new RyykeRollbackFrame() {
			Tombstones = p_Tombstones,
			ListeningForDirection = listeningForDirection, 
			ListeningForMovement = listeningForMovement,
			Burrowing = burrowing,
			BurrowTime = burrowTime, 
			TetherPoint = this.tetherPoint,
			TombstoneIndex = tombstoneIndex,
			Hand = (g_Tether_Hand != null) ? g_Tether_Hand.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			Arm = (g_Tether_Arm != null) ? g_Tether_Arm.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
		};

		 return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);

		RyykeRollbackFrame ryyke_frame = (RyykeRollbackFrame)p_frame.CharacterFrame;


		//Tombstone reset
		for(int i = 0; i <3; i++) {
			if(ryyke_frame.Tombstones[i] != null) {
				if(tombstones[i] == null) SpawnTombstone(i);
				tombstones[i].GetComponent<Tombstone>().DeserializeFrame(ryyke_frame.Tombstones[i]);
			}
			//Projectile does not exist in rollback frame
			else {
				Destroy(tombstones[i].gameObject);
				tombstones[i] = null;
			}  
		}

		//Hand reset
		if(ryyke_frame.Hand != null) {
			if(g_Tether_Hand == null)CreateHand(transform.position, 55f * movement.Facing);
			g_Tether_Hand.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(ryyke_frame.Hand);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(g_Tether_Hand);
			g_Tether_Hand = null;
		}

		//Arm reset
		if(ryyke_frame.Arm != null) {
			if(g_Tether_Arm == null)CreateArm(transform.position, 55f * movement.Facing);
			g_Tether_Arm.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(ryyke_frame.Arm);
		}
		else {
			Destroy(g_Tether_Arm);
			g_Tether_Arm = null;
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
	public BasicProjectileRollbackFrame Arm;
	public BasicProjectileRollbackFrame Hand;
}
