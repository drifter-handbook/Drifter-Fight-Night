using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum CancelType
{
	Feint_Cancel,
	Offensive_Cancel,
	Defensive_Cancel,
	Hyper_Guard_Burst,
	Time_Cancel,
	Inspiration_Burst,
}

public class PlayerMovement : MonoBehaviour
{
	//Character Properties
	public int numberOfJumps = 2;
	public int numberOfDashes = 1;
	public float dashSpeed = 35f;
	// public float delayedJumpDuration = 0.05f;
	
	public int groundAccelerationTime = 36;
	public int crippledAirAccelerationTime = 48;
	public int airAccelerationTime = 24;
	public float airSpeed = 15f;
	public float jumpHeight = 20f;
	public float jumpTime = 1f;
	//public int Weight = 90;
	public float ledgeOffset = 1f;
	public float ledgeClimbOffset = 0f;
	public Vector3 particleOffset =  Vector3.zero;
	public float fullhopFrames = 10f;
	public float walkSpeed = 15f;
	[NonSerialized]
	public float currentWalkSpeed;

	//Calculated character properties
	protected float jumpSpeed;
	protected float baseGravity;
	[NonSerialized]
	public float baseTerminalVelocity;


	//Animator State Fields
	public int Facing { get; set; } = 1;
	public float terminalVelocity = 25f;

	[NonSerialized]
	public int currentJumps;
	[NonSerialized]
	public int currentDashes;  
	[NonSerialized]
	public bool grounded = true;
	[NonSerialized]
	public bool hitstun = false;
	[NonSerialized]
	public bool canLandingCancel = false;
	[NonSerialized]
	public bool canFastFall = true;
	[NonSerialized]
	public bool jumping = false;
	[NonSerialized]
	public bool dashing = false;
	[NonSerialized]
	public bool gravityPaused = false;
	//[NonSerialized]
	public bool ledgeHanging = false;

	[NonSerialized]
	public bool strongLedgeGrab = true;
	[NonSerialized]
	public bool passThrough = false;
	[NonSerialized]
	public float jumpTimer = 30f;

	InstantiatedEntityCleanup SuperCancel;
	CancelType cancelType = CancelType.Feint_Cancel;

	//Situational Iteration variables
	int ledgeGrabLockout = 0;
	int dropThroughTime = 18;
	int ringTime = 6;
	int dustCloudTimer = 0;
	Vector2 prevVelocity;

	float currentSpeed;

	bool delayedFacingFlip = false;

	//Access to main camera for screen darkening
	ScreenShake mainCamera;

	PolygonCollider2D frictionCollider;
	public Pushbox Pushbox; 
	PolygonCollider2D pushBoxCollider;


	//Component Fields
	[NonSerialized]
	public Rigidbody2D rb;
	Drifter drifter;
	GameObjectShake shake;

	//public GameObject PushBox;
 	//GameObject Pusher;
 	GameObject smoketrail;


	void Awake() {
		//Aggregate componenents
		rb = GetComponent<Rigidbody2D>();
		drifter = GetComponent<Drifter>();
		shake = gameObject.GetComponentInChildren<GameObjectShake>();

		//Do this better
		mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();

		// status = drifter.status;
		// animator = drifter.animator;

		//Pushbox = GetComponentInChildren<PolygonCollider2D>();
		frictionCollider = GetComponent<PolygonCollider2D>();
		pushBoxCollider = Pushbox.gameObject.GetComponent<PolygonCollider2D>();

		baseTerminalVelocity = terminalVelocity;

		baseGravity = rb.gravityScale;
		jumpSpeed = (jumpHeight / jumpTime + .5f*(rb.gravityScale * jumpTime));
		currentWalkSpeed = walkSpeed;		
	}

	//Restitution
	void OnCollisionEnter2D(Collision2D col) {
		 if(!drifter.status.HasGroundFriction() && ((prevVelocity.y < 0 || col.gameObject.tag !=  "Platform" ))) {
			Vector3 normal = col.contacts[0].normal;

			if(normal.y == 1f && drifter.status.canbeKnockedDown() && !drifter.knockedDown) {
				//Save velocity the frame before hitting the ground to be used for the KD bounce
				//kdbounceVelocity = Vector2.Reflect(prevVelocity,normal) *.65f;
			}

			//Restitute at higher speeds
			else if(prevVelocity.magnitude > 35f && !drifter.status.canbeKnockedDown()) {
				UnityEngine.Debug.Log("Restitution");
				rb.velocity = Vector2.Reflect(prevVelocity,normal) *.8f;
				spawnJuiceParticle(col.contacts[0].point, MovementParticleMode.Restitution, Quaternion.Euler(0f,0f, ( (rb.velocity.x < 0)?1:-1 ) * Vector3.Angle(Vector3.up,normal)),false);
			}
			// //Soft knockdown on low% ground spike
			// else if (prevVelocity.magnitude <= 45f && !drifter.status.canbeKnockedDown()){
			// 	drifter.status.hkd = false;
			// 	drifter.status.ApplyStatusEffect(PlayerStatusEffect.TUMBLE,60);
			// }
		}
	}

	public void UpdateFrame() {

		if(SuperCancel != null) SuperCancel.GetComponentInChildren<InstantiatedEntityCleanup>().UpdateFrame();

		bool jumpPressed = !drifter.input[1].Jump && drifter.input[0].Jump;
		bool canAct = !drifter.status.HasStunEffect() && !drifter.guarding;// && !drifter.input[0].Guard;
		bool canGuard = !drifter.status.HasStunEffect() && !jumping && !ledgeHanging;
		bool moving = drifter.input[0].MoveX != 0;
		bool hasCollision = !drifter.status.HasEnemyStunEffect() && !ledgeHanging && !passThrough;
		//Only collide with other players when not using a move or hanging on a ledge
		Pushbox.gameObject.layer = (hasCollision ? 14:17);
		Pushbox.hasCollision = !moving;

		if(ledgeGrabLockout > 0){
			ledgeGrabLockout --;
			if(ledgeGrabLockout ==0)
				drifter.CanGrabLedge = true;
		}

		//Unpause gravity when hit
		if(!drifter.status.HasGroundFriction()){
			gravityPaused= false;
			dashing= false;
			jumping = false;
		}

		//pause attacker during hitpause, and apply hurt animation to defender
		if(drifter.status.HasStatusEffect(PlayerStatusEffect.HITPAUSE)) {
			
			if(drifter.status.HasStatusEffect(PlayerStatusEffect.FLATTEN)) {
				//do nothing
			}
			else if(drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)){
				drifter.PlayAnimation("Knockdown_Bounce");
				drifter.ToggleAnimator(false);
			}
			else if(drifter.status.HasEnemyStunEffect() && !drifter.guarding) {
				drifter.PlayAnimation("HitStun");
				shake.Shake(12,.7f);
			}

			else if(drifter.status.HasEnemyStunEffect()) {
				drifter.PlayAnimation("BlockStun");
				shake.Shake(6,.7f);
			}
			else{
				drifter.ToggleAnimator(false);
			}
			
		}
		//Reactivate attacker when hitpause removed
		else {
			drifter.ToggleAnimator(true);
			if(delayedFacingFlip) {
				delayedFacingFlip = false;
				drifter.SetIndicatorDirection(Facing);
				transform.localScale = new Vector2(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y);
			}
		}

		//Cancel aerials on landing + landing animation
		if(!grounded && IsGrounded() && !drifter.status.HasEnemyStunEffect() && !jumping && !drifter.guarding && (!drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG) || canLandingCancel)) {
			drifter.clearMasterhitVars();
			drifter.PlayAnimation("Jump_End");
			//Remove armour on landing 
			//TODO determine if there are more things that need to be removed on actionable landing
			if(drifter.status.HasStatusEffect(PlayerStatusEffect.ARMOUR))drifter.status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0);
		}

		//Handle Jump
		if(jumpTimer < fullhopFrames && !drifter.status.HasStatusEffect(PlayerStatusEffect.HITPAUSE)) {
			float prevJumpTimer = jumpTimer;
			jumpTimer += (drifter.status.hasSloMoEffect() ? .4f : 1f);

			//Shorthop

			if(jumpTimer >= 0){

				//Allow player to qucikly change direction with a jump
				float currentSpeed;
				if(drifter.input[0].MoveX ==0 || jumpTimer > 0) currentSpeed = rb.velocity.x;
				else currentSpeed= calculateSpeedModifiers(grounded?WalkSpeed:airSpeed) * Facing;

				rb.velocity = new Vector2(currentSpeed,	jumpSpeed * (drifter.status.hasSloMoEffect() ? .4f : 1f));

				if(!drifter.enforceFullDistance && jumpTimer >= 0 && grounded && prevJumpTimer <0 && (!drifter.input[0].Jump || drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG))) {
					jumpTimer = fullhopFrames;
					if(drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG)) UnityEngine.Debug.Log("JUMP QUEUED A MOVE");
				}
				//fullhop
				else if(drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG)) jumpTimer = fullhopFrames;
			}
		}

		//Handles jumps
		if(grounded && !jumping) {
			//Resets jumps if player is on the ground
			currentJumps = numberOfJumps;
			currentDashes = numberOfDashes;
			strongLedgeGrab = true;
		
			//If the player walked off a ledge, remove their grounded jump
			if(!IsGrounded()) {
				currentJumps--;
			}            
		}
		else if(IsGrounded() && !drifter.status.HasStunEffect() && !jumping) {
			//drifter.PlayAnimation("Jump_End");
			spawnJuiceParticle(transform.position + particleOffset + new Vector3(0,-1,0), MovementParticleMode.Land);
		}

		if(grounded && !IsGrounded() && !moving && canAct && !jumping){
			UnityEngine.Debug.Log("SLID OFF LEDGE");
			drifter.PlayAnimation("Hang");
		}


		grounded = IsGrounded();
	   
		//Sets hitstun state when applicable

		if(drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)) {
			hitstun = true;
			//DropLedge(false);
		}

		else if(drifter.status.HasEnemyStunEffect() && !drifter.guarding) {
			hitstun = true;
			drifter.PlayAnimation("HitStun");
			DropLedge(false);
		}

		else if(drifter.status.HasEnemyStunEffect() && drifter.guarding) {
			drifter.PlayAnimation("BlockStun");
			hitstun = true;
		}  
		
		//come out of hitstun logic
		if(hitstun && !drifter.status.HasEnemyStunEffect()) {
			drifter.returnToIdle();
			drifter.AirCrippled = true;
			drifter.knockedDown = false;
			ringTime = 6;
		}

		//Smoke Trail
		if(drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && rb.velocity.magnitude > 45f ){
			if(smoketrail == null) smoketrail = drifter.createParticleEffector("LAUNCHED_PARTICLE");
		}
		else if (smoketrail != null) {
			smoketrail.GetComponent<ParticleSystemController>().Cleanup();
			smoketrail = null;
		}

		//Sonic Boom Trail
		if(drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && rb.velocity.magnitude > 75f){
			
			if(ringTime>= 6){
				particleOffset = new Vector3(particleOffset.x * Facing ,particleOffset.y,0);

				GameObject launchRing = GameController.Instance.CreatePrefab("LaunchRing", transform.position + particleOffset,  Quaternion.Euler(0,0,((rb.velocity.y>0)?1:-1) * Vector3.Angle(rb.velocity, new Vector3(1f,0,0))));

				launchRing.transform.localScale = new Vector3(  7.5f* Facing ,7.5f,1);

				ringTime = 0;

			}
			else{
				ringTime++;
			}

		}

		//Inverts controls if revered
		// if(drifter.status.HasStatusEffect(PlayerStatusEffect.REVERSED)){
		// 	drifter.input[0].MoveX *= -1;
		// }

		//Pauses you in place if you have a corresponding status effect.
		if(drifter.status.HasStatusEffect(PlayerStatusEffect.STUNNED)
		 || drifter.status.HasStatusEffect(PlayerStatusEffect.PLANTED)
		 || drifter.status.isDead()
		 || drifter.status.HasStatusEffect(PlayerStatusEffect.HITPAUSE) 
		 || drifter.status.HasStatusEffect(PlayerStatusEffect.GRABBED)
		 || drifter.status.HasStatusEffect(PlayerStatusEffect.CRINGE)
		) {
			//cancelJump();
			rb.velocity = Vector2.zero;
			rb.gravityScale = 0;                       
		}
		else if(drifter.status.hasSloMoEffect() && !gravityPaused) {
			rb.gravityScale = baseGravity*.4f;
			terminalVelocity =  baseTerminalVelocity *.4f;
		}
		

		//makes sure gavity is always reset after using a move
		//TODO make sure this is still necessary
		else if((!drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG) || !gravityPaused) && !ledgeHanging && !drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)){
			resetGravity();
			if(!drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG))resetTerminalVelocity();
		}

		//Saves previpus vleocity for resitution. REMOVE IF NOT NEEDED
		if(rb.velocity != Vector2.zero)prevVelocity = rb.velocity;


		//Update input


	   //Platform dropthrough
		if(dropThroughTime < 18) dropThroughTime++;
		if(gameObject.layer != 8 && dropThroughTime >= 18)
			gameObject.layer = 8;
		

		ContactPoint2D[] contacts = new ContactPoint2D[1];
		bool groundFrictionPosition = frictionCollider.GetContacts(contacts) >0;

		//if(!moving)accelerationFrames = 6;
		drifter.toggleHidden(drifter.status.HasStatusEffect(PlayerStatusEffect.HIDDEN));

		//Normal walking logic
		if (moving && canAct && !ledgeHanging) {

			updateFacing();
			//If just started moving or switched directions
			// if((accelerationFrames == 6 || rb.velocity.x * drifter.input[0].MoveX < 0) && IsGrounded())
			// 	if(groundFrictionPosition) spawnJuiceParticle(new Vector2(-Facing * 1.5f,0) + contacts[0].point, MovementParticleMode.KickOff);
			
			if(!jumping) {
				if(grounded){
					drifter.PlayAnimation("Walk", -1, true);
					if(groundFrictionPosition) {
						if(dustCloudTimer > 15) {
							spawnJuiceParticle(new Vector2(-Facing * 1.5f,0) + contacts[0].point, MovementParticleMode.WalkDust);
							dustCloudTimer = 0;
						}
						else dustCloudTimer ++;
					}
				}
				else drifter.PlayAnimation("Hang");
			}

			int accelrationQuotient;
			if(grounded) accelrationQuotient = groundAccelerationTime;
			else if(drifter.AirCrippled) accelrationQuotient = crippledAirAccelerationTime;
			else accelrationQuotient = airAccelerationTime;

			currentSpeed = calculateSpeedModifiers(grounded?currentWalkSpeed:airSpeed);
			rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x,(drifter.input[0].MoveX > 0 ? 1 : -1) * currentSpeed,currentSpeed/accelrationQuotient), rb.velocity.y);
		}

		//Guard
		if(drifter.input[0].Guard && canGuard) {
			//shift is guard
			drifter.guard();
			updateFacing();
		}
	  
		//Disable Guarding
		else if(!drifter.input[0].Guard && !drifter.status.HasStunEffect() && drifter.guarding) {
			drifter.returnToIdle();
		}

		//Ledgegrabs Stuff
		else if(canAct && ledgeHanging) {
			//rb.velocity = Vector2.zero;

			//Jump away from ledge
			if((drifter.input[0].MoveX  * Facing < 0)){
				JumpFromLedge();
				//UnityEngine.Debug.Log("Ledge Jump");
			}
	
			//Neutral Getup
			else if((drifter.input[0].MoveX  * Facing > 0)  || drifter.input[0].MoveY > 0 || drifter.input[0].Guard ){
				DropLedge();
				drifter.status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,18);
				drifter.PlayAnimation("Ledge_Climb");

				//UnityEngine.Debug.Log("Ledge Climb");

				rb.position = new Vector3(rb.position.x + (rb.position.x > 0 ? -1 :1) *2f, rb.position.y + 5f - ledgeClimbOffset);
			}

			//Drop down from ledge
			else if(drifter.input[0].MoveY < 0 && drifter.input[1].MoveY < 0 && ledgeHanging){
				DropLedge();
				drifter.returnToIdle();
				drifter.CanGrabLedge = false;
				//UnityEngine.Debug.Log("Drop");
			}

		}
		//Inspiration
		if(drifter.inspirationCharges > 0  
			&& drifter.status.HasEnemyStunEffect()  
			&& !drifter.status.HasStatusEffect(PlayerStatusEffect.HITPAUSE) 
			&& !drifter.guarding 
			&& !drifter.status.HasStatusEffect(PlayerStatusEffect.INSPIRATION) 
			&& drifter.input[0].Guard 
			&& drifter.input[0].Light
			&& !drifter.entity.paused 
			&& !drifter.usingSuper
			&& !drifter.status.HasSuperBlockingEffect()){

			drifter.inspirationCharges--;
			drifter.status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,20 + drifter.status.remainingDuration(PlayerStatusEffect.KNOCKDOWN));
			drifter.status.ApplyStatusEffect(PlayerStatusEffect.INSPIRATION,20);
		}

		//Player is not trying to move, and is not in hitstun
		else if (!moving && canAct) {
			if(drifter.input[1].MoveX !=0 && drifter.input[0].MoveX == 0 && canAct && !jumping && !drifter.guarding)
				drifter.returnToIdle();
			//standing ground friction (When button is not held)
			if(!grounded)rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 20f * Time.fixedDeltaTime), rb.velocity.y);
			else rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 80f * Time.fixedDeltaTime), rb.velocity.y);
		}


		//Slowdown on the ground
		else if(IsGrounded()) {
			//Moving Ground Friction
			rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 40f * Time.fixedDeltaTime), rb.velocity.y);
		}

		//Drop through platforms && fastfall
		if(drifter.doubleTappedY() && drifter.input[0].MoveY < 0 && !gravityPaused && canFastFall && !ledgeHanging && !jumping && !drifter.status.HasEnemyStunEffect()) {
			//If you are not in an attack, play the landing animation when you hit the ground
			if(!drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG) && !drifter.guarding){
				drifter.PlayAnimation("Hang",-1,true);
				canLandingCancel = true;
			}
			gameObject.layer = 13;
			rb.velocity = new Vector2(rb.velocity.x,Mathf.Min(-terminalVelocity /2f,rb.velocity.y));
			dropThroughTime = 0;
		}

		//Terminal velocity
		if(rb.velocity.y < -terminalVelocity && (!drifter.status.HasEnemyStunEffect() || drifter.guarding || drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN))){
			rb.velocity = new Vector2(rb.velocity.x,-terminalVelocity);
		}

		//Jump
		if (jumpPressed && canAct) {
			jump();
		}


		else if(canAct && (drifter.doubleTappedX() || (!drifter.input[2].Dash && drifter.input[0].Dash))) {
			dash();
		}

		//Pause movement for relevent effects.
		
	}

	//Moves the character left or right, based on the speed provided
	public void move(float speed, bool flipDirection = true) {

		if(flipDirection)updateFacing();

		if(drifter.input[0].MoveX != 0) {
			currentSpeed = calculateSpeedModifiers(speed);
			rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x,currentSpeed * (drifter.input[0].MoveX > 0 ? 1 : -1),currentSpeed/groundAccelerationTime), rb.velocity.y);
		}
		
	}

	public float calculateSpeedModifiers(float speed){
		return  speed * ((drifter.status.hasSloMoEffect()) ? .4f: 1f) * (drifter.status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f);
	}
	

	//Made it public for streamlining channeled attack cancels
	public void techParticle() {
		spawnJuiceParticle(pushBoxCollider.bounds.center, MovementParticleMode.Tech, Quaternion.Euler(0f,0f,0f),false);
	}

    public void actionCancelParticle() {
    	//UnityEngine.Debug.Log("CANCEL PARTICLE");
        spawnJuiceParticle(pushBoxCollider.bounds.center, MovementParticleMode.Cancel, Quaternion.Euler(0f,0f,0f),false);
    }

	//Updates the direction the player is facing
	public void updateFacing() {

		//if(Facing != drifter.input[0].MoveX)accelerationFrames = 6;

		if(drifter.input[0].MoveX > 0) Facing = 1;
		else if(drifter.input[0].MoveX < 0) Facing = -1;

		drifter.SetIndicatorDirection(Facing);
		transform.localScale = new Vector2(Facing * Mathf.Abs(transform.localScale.x), transform.localScale.y);
	}

	//Used to forcibly invert the players direction
	public void flipFacing(){
		Facing *= -1;
		drifter.SetIndicatorDirection(Facing);
		transform.localScale = new Vector2(Facing * Mathf.Abs(transform.localScale.x), transform.localScale.y);
	}

	public void setFacing(int dir){
		Facing = Math.Sign(dir);
		drifter.SetIndicatorDirection(Facing);
		transform.localScale = new Vector2(Facing * Mathf.Abs(transform.localScale.x), transform.localScale.y);
	}

	public void setFacingDelayed(int dir){
		delayedFacingFlip = true;
		Facing = Math.Sign(dir);
	}


	//Kills jump coroutines if they exist, for paused gravity attacks
	public void cancelJump() {
		jumpTimer = fullhopFrames;
	}

	public void updatePosition (Vector3 position){
	  transform.position = position;
	}

	RaycastHit2D[] hits = new RaycastHit2D[10];
	private bool IsGrounded() {
		int count = Physics2D.RaycastNonAlloc(frictionCollider.bounds.center + frictionCollider.bounds.extents.y * Vector3.down, Vector3.down, hits, 0.2f);

		for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform")) return rb.velocity.y <=.1f;

		return false;
	}

	
	public bool IsWallSliding() {
		RaycastHit2D[] wallHits = new RaycastHit2D[10];
		int count = Physics2D.RaycastNonAlloc(pushBoxCollider.bounds.center + new Vector3( pushBoxCollider.bounds.extents.x * ((Facing > 0)?1:-1),pushBoxCollider.bounds.extents.y,0), ((Facing > 0)?Vector3.right:Vector3.left),wallHits, 0.35f);

		for (int i = 0; i < count; i++)if (wallHits[i].collider.gameObject.tag == "Ground" && drifter.status.HasGroundFriction())return true;

		return false;
	}

	public void pauseGravity() {
		cancelJump();
		gravityPaused = true;
		rb.gravityScale = 0f;
		rb.velocity = Vector2.zero;
		drifter.status.clearVelocity();
	}

	//Sets many movement flags to specific vlaues to allow for ledge hanging
	public void GrabLedge(Vector3 pos) {
		if(!canGrabLedge())	return; 

		UnityEngine.Debug.Log("Grabbed Ledge");
		drifter.status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,5);
		pauseGravity();
		jumping = false;
		dashing = false;
		passThrough = false;
		drifter.clearGuardFlags();
		ledgeHanging = true;
		if(strongLedgeGrab)drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN,150);
		drifter.PlayAnimation("Ledge_Grab");

		setFacing(rb.position.x > 0 ? -1 :1);

		rb.position = new Vector3(pos.x - (rb.position.x > 0 ? -1 :1) *1.5f, pos.y - 1.75f - ledgeOffset,pos.z);
 
		drifter.attacks.resetRecovery();      
		
		currentJumps = numberOfJumps;
		currentDashes = numberOfDashes;
	}

	//Manages all the things that need to happen when a ledge is released
	public void DropLedge(bool grantInvuln = true, int lockoutTime = 30){
		//Apply ledge invuln if the play is currently invuln
		if(drifter.status.HasStatusEffect(PlayerStatusEffect.INVULN) && grantInvuln)drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN,20);
		ledgeHanging = false;
		resetGravity();
		strongLedgeGrab = false;
		ledgeGrabLockout = lockoutTime;
		drifter.CanGrabLedge = false;
	}

	public bool canGrabLedge(){
		return drifter.CanGrabLedge && ledgeGrabLockout ==0;
	}	

	public void JumpFromLedge(){
		DropLedge();
		drifter.returnToIdle();
		drifter.CanGrabLedge = false;
		rb.velocity = new Vector3(Facing  * -25f,25f);
	}

	//Wrapper for spawning particles at the character's feet
	public void spawnKickoffDust() {
		ContactPoint2D[] contacts = new ContactPoint2D[1];
		bool groundFrictionPosition = frictionCollider.GetContacts(contacts) >0;
		
		if(groundFrictionPosition) spawnJuiceParticle(new Vector2(-Facing * 1.5f,0) + contacts[0].point, MovementParticleMode.KickOff);
	}


	//Public jump method allows for forced jumps from attacks
	public bool jump(bool enforceFullDistance = false) {
		if (currentJumps > 0) {
			rb.velocity = new Vector3(rb.velocity.x,Mathf.Max(0,rb.velocity.y));
			jumping = true;
			dashing = false;
			drifter.guarding = false;
			if(ledgeHanging)DropLedge();
			//jump
			gravityPaused = false;
			currentJumps--;
			if(!grounded)drifter.PlayAnimation("Air_Jump_Start");
			else drifter.PlayAnimation("Jump_Start");
			//Particles
			if(IsGrounded()){
				if(enforceFullDistance){
					UnityEngine.Debug.Log("FULLHOP ENFORCED");
					drifter.enforceFullDistance = true;
				}
				spawnJuiceParticle(transform.position + particleOffset + new Vector3(0,-1,0), MovementParticleMode.Jump);
			}
			
			else
				spawnJuiceParticle(transform.position + particleOffset +new Vector3(0,-1,0), MovementParticleMode.DoubleJump);
			
			jumpTimer = -5f;
			return true;
		}
		return false;

	}

	public bool dash(bool enforceFullDistance = false) {
		if(currentDashes > 0 && !dashing) {
			updateFacing();
			//accelerationFrames = 120;
			dashing = true;
			passThrough = true;
			spawnJuiceParticle(pushBoxCollider.bounds.center + new Vector3(Facing * 1.5f,0), MovementParticleMode.Dash_Ring, Quaternion.Euler(0f,0f,0f), false);
			drifter.status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,480);
			drifter.PlayAnimation("Dash");
			drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN,10);
			jumping = false;
			if(IsGrounded() && enforceFullDistance) {
				UnityEngine.Debug.Log("FULL DASH DISTANCE ENFORCED");
				drifter.enforceFullDistance = true;
			}
			currentDashes--;
			GraphicalEffectManager.Instance.CreateMovementCancel(drifter.gameObject);
			return true;
		}
		return false;
	}

	//Public wrapper for movement particle spawning
	public void spawnJuiceParticle(Vector3 pos, MovementParticleMode mode) {
		spawnJuiceParticle(pos, mode, transform.rotation, false);
	}

	public void spawnJuiceParticle(Vector3 pos, MovementParticleMode mode, bool flip) {
		 spawnJuiceParticle(pos, mode, transform.rotation, flip);
	}

	//Creates a movement particle at the designated location
	private void spawnJuiceParticle(Vector3 pos, MovementParticleMode mode, Quaternion angle, bool flip){

		particleOffset = new Vector3(particleOffset.x * Facing ,particleOffset.y,0);
		GraphicalEffectManager.Instance.CreateMovementParticle(mode, pos, angle.eulerAngles.z, new Vector2(Facing * (flip ? -1 : 1), 1));
	}


	public void superCancel(bool inspiration = false) {
		if( !drifter.CanUseSuper()) return;

		else if(drifter.status.HasStatusEffect(PlayerStatusEffect.INSPIRATION) && inspiration) {
			drifter.ToggleAnimator(true);
			hitstun = false;
			drifter.status.clearStunStatus();
			drifter.attacks.useSuper();
			spawnSuperParticle(CancelType.Inspiration_Burst,0,12);
		}
		else if(drifter.superCharge >= 100 && drifter.CanUseSuper() && !drifter.status.HasStatusEffect(PlayerStatusEffect.INSPIRATION) ) {
			//Hyperguard
			if(drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && drifter.guarding  && drifter.superCharge >= 100) {
				drifter.ToggleAnimator(true);
				hitstun = false;
				drifter.status.clearStunStatus();
				drifter.attacks.useSuper();
				spawnSuperParticle(CancelType.Hyper_Guard_Burst,100,8);	
			}
			//Offensive Cancel
			else if(drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG) && drifter.superCharge >= 200) {
				if(drifter.superCharge >= 300 && !drifter.canFeint) {
					drifter.attacks.useSuper();
					spawnSuperParticle(CancelType.Offensive_Cancel,300,20);		
				}
				else if(drifter.canFeint) {
					drifter.attacks.useSuper();
					spawnSuperParticle(CancelType.Feint_Cancel,200,8);
				}
			}
			//Burst/Defensive Cancel
			else if(!drifter.guarding && drifter.superCharge >= 300 && drifter.status.HasEnemyStunEffect() && !drifter.status.HasStatusEffect(PlayerStatusEffect.GRABBED) && !drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)) {
				drifter.ToggleAnimator(true);
				hitstun = false;
				drifter.status.clearStunStatus();
				//drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN,8);
				drifter.attacks.useSuper();
				spawnSuperParticle(CancelType.Defensive_Cancel,300,8);
				if(currentJumps+1 < numberOfJumps) currentJumps++;
				
			}
			else if (!drifter.guarding && drifter.superCharge >= 200 && !drifter.status.HasStunEffect()) {
				drifter.attacks.useSuper();
				spawnSuperParticle(CancelType.Time_Cancel,200,8);
			}
		}

	}

	public void useInpiration(){
		spawnSuperParticle(CancelType.Inspiration_Burst,0,12);
	}

	public void resetTerminalVelocity() {
		terminalVelocity = baseTerminalVelocity;
	}

	public void resetGravity() {
		rb.gravityScale = baseGravity;
	}

	private void spawnSuperParticle(CancelType mode,int cost,int darkentime) {
		if(SuperCancel != null)
			Destroy(SuperCancel.gameObject);

		cancelType = mode;
		canLandingCancel = false;
		mainCamera.Darken(darkentime);
		drifter.attacks.SetMultiHitAttackID();
		Vector3 flip = new Vector3(Facing * 10f, 10f, 0f);
		
		drifter.superCharge -= cost;

		GameObject proj = GameController.Instance.CreatePrefab("SuperEffect", transform.position , transform.rotation,drifter.peerID);
		foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = drifter.attacks.NextID;
			hitbox.isActive = true;
			hitbox.Facing = Facing;
		}

		SuperCancel = proj.GetComponent<InstantiatedEntityCleanup>();
		SuperCancel.animator.Play(cancelType.ToString());
		
	}

	//Rollback
	//====================================
	
	//Takes a snapshot of the current frame to rollback to
	public void Serialize(BinaryWriter bw) {
		//Bool
		bw.Write(grounded);
		bw.Write(hitstun);
		bw.Write(canLandingCancel);
		bw.Write(canFastFall);
		bw.Write(jumping);
		bw.Write(dashing);
		bw.Write(gravityPaused);
		bw.Write(ledgeHanging);
		bw.Write(strongLedgeGrab);
		bw.Write(delayedFacingFlip);
		bw.Write(passThrough);

		//Int
		bw.Write(dropThroughTime);
		bw.Write(Facing);
		bw.Write(currentJumps);
		bw.Write(currentDashes);
		bw.Write(ledgeGrabLockout);
		bw.Write((int)cancelType);
		bw.Write(currentWalkSpeed);

		//Float
		bw.Write(terminalVelocity);
		bw.Write(jumpTimer);
		bw.Write(currentSpeed);

		bw.Write(prevVelocity.x);
		bw.Write(prevVelocity.y);

		//Child
		if(SuperCancel != null){
			bw.Write(true);
			SuperCancel.Serialize(bw);
		}
		else
			bw.Write(false);
	}

	//Rolls back the entity to a given frame state
	public void Deserialize(BinaryReader br) {
		
		//Bool
		grounded = br.ReadBoolean();
		hitstun = br.ReadBoolean();
		canLandingCancel = br.ReadBoolean();
		canFastFall = br.ReadBoolean();
		jumping = br.ReadBoolean();
		dashing = br.ReadBoolean();
		gravityPaused = br.ReadBoolean();
		ledgeHanging = br.ReadBoolean();
		strongLedgeGrab = br.ReadBoolean();
		delayedFacingFlip = br.ReadBoolean();
		passThrough = br.ReadBoolean();

		//Int
		dropThroughTime = br.ReadInt32();
		Facing = br.ReadInt32();
		currentJumps = br.ReadInt32();
		currentDashes = br.ReadInt32();
		ledgeGrabLockout = br.ReadInt32();
		cancelType = (CancelType)br.ReadInt32();
		currentWalkSpeed = br.ReadInt32();

		//Float
		terminalVelocity = br.ReadSingle();
		jumpTimer = br.ReadSingle();
		currentSpeed = br.ReadSingle();

		prevVelocity.x = br.ReadSingle();
		prevVelocity.y = br.ReadSingle();

		if(br.ReadBoolean()){
			if(SuperCancel == null)spawnSuperParticle(cancelType,0,8);
			SuperCancel.Deserialize(br);
		}
		else if(SuperCancel != null){
			Destroy(SuperCancel.gameObject);
			SuperCancel = null;
		}
	}
}
