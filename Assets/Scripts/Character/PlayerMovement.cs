using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	
	public float groundAccelerationTime = 36f;
	public float airAccelerationTime = 48f;
	public float airSpeed = 15f;
	public float jumpHeight = 20f;
	public float jumpTime = 1f;
	public int Weight = 90;
	public float ledgeOffset = 1f;
	public float ledgeClimbOffset = 0f;
	public Vector3 particleOffset =  Vector3.zero;
	public float fullhopFrames = 10f;
	public float walkSpeed = 15f;
	

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
	public int accelerationFrames = 6;
	[NonSerialized]
	public float dashLock = 0;
	[NonSerialized]
	public float jumpTimer = 30f;

	GameObject SuperCancel;
	CancelType canceltype = CancelType.Feint_Cancel;

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
	BoxCollider2D BodyCollider; 


	//Component Fields
	[NonSerialized]
	public Rigidbody2D rb;
	Drifter drifter;
	GameObjectShake shake;

	public GameObject PushBox;
 	GameObject Pusher;
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

		BodyCollider = GetComponent<BoxCollider2D>();
		frictionCollider = GetComponent<PolygonCollider2D>();

		baseTerminalVelocity = terminalVelocity;

		baseGravity = rb.gravityScale;
		jumpSpeed = (jumpHeight / jumpTime + .5f*(rb.gravityScale * jumpTime));
		if (!GameController.Instance.IsHost) {
			rb.isKinematic = true;
		}
		
	}

	//Restitution
	void OnCollisionEnter2D(Collision2D col) {
		 if(!drifter.status.HasGroundFriction() && ((prevVelocity.y < 0 || col.gameObject.tag !=  "Platform" ))) {
			Vector3 normal = col.contacts[0].normal;

			if(normal.y == 1f && drifter.status.canbeKnockedDown() && !drifter.knockedDown) {
				//Save velocity the frame before hitting the ground to be used for the KD bounce
				//kdbounceVelocity = Vector2.Reflect(prevVelocity,normal) *.65f;
			}

			else if(prevVelocity.magnitude > 35f && !drifter.status.canbeKnockedDown()) {
				UnityEngine.Debug.Log("Restitution");
				rb.velocity = Vector2.Reflect(prevVelocity,normal) *.8f;
				spawnJuiceParticle(col.contacts[0].point, MovementParticleMode.Restitution, Quaternion.Euler(0f,0f, ( (rb.velocity.x < 0)?1:-1 ) * Vector3.Angle(Vector3.up,normal)),false);
			}
		}
	}

	void OnTriggerStay2D(Collider2D col) {
		if(col.gameObject.tag == "Pushbox") {
			Pusher = col.gameObject;
		}
	}

	void OnTriggerExit2D(Collider2D col) {
		if(col.gameObject.tag == "Pushbox") {
			Pusher = null;
		}
	}


	public void UpdateFrame() {

		if(SuperCancel != null) SuperCancel.GetComponentInChildren<InstantiatedEntityCleanup>().UpdateFrame();

		if(dashLock >0) dashLock--;

		bool jumpPressed = !drifter.input[1].Jump && drifter.input[0].Jump;
		bool canAct = !drifter.status.HasStunEffect() && !drifter.guarding;// && !drifter.input[0].Guard;
		bool canGuard = !drifter.status.HasStunEffect() && !jumping && !ledgeHanging;
		bool moving = drifter.input[0].MoveX != 0;
		bool hasCollision = !drifter.status.HasStunEffect() && !ledgeHanging;
		//Only collide with other players when not using a move or hanging on a ledge
		PushBox.SetActive(hasCollision);

		if(ledgeGrabLockout > 0){
			ledgeGrabLockout --;
			if(ledgeGrabLockout ==0)
				drifter.CanGrabLedge = true;
		}

		//Unpause gravity when hit
		if(!drifter.status.HasGroundFriction())gravityPaused=false;

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
				transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y, transform.localScale.z);
			}
		}

		//Cancel aerials on landing + landing animation
		if(!grounded && IsGrounded() && !drifter.status.HasEnemyStunEffect() && !jumping && !drifter.guarding && (!drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG) || canLandingCancel)) {
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
			if(!drifter.enforceFullDistance && jumpTimer >= 0 && grounded && prevJumpTimer <0 && (!drifter.input[0].Jump || drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG))) {
				jumpTimer = fullhopFrames;
				rb.velocity = new Vector2(rb.velocity.x, jumpSpeed * (drifter.status.hasSloMoEffect() ? .4f : 1f));
				if(drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG)) UnityEngine.Debug.Log("JUMP QUEUED A MOVE");
			}
			//fullhop
			else if(jumpTimer >= 0) {
				rb.velocity = new Vector2(rb.velocity.x, jumpSpeed * (drifter.status.hasSloMoEffect() ? .4f : 1f));
				if(drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG)) jumpTimer = fullhopFrames;
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

				GameObject launchRing = GameController.Instance.host.CreateNetworkObject("LaunchRing", transform.position + particleOffset,  Quaternion.Euler(0,0,((rb.velocity.y>0)?1:-1) * Vector3.Angle(rb.velocity, new Vector3(1f,0,0))));

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
		 || drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD) 
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

		if(!moving)accelerationFrames = 6;
		drifter.toggleHidden(drifter.status.HasStatusEffect(PlayerStatusEffect.HIDDEN));

		//Normal walking logic
		if (moving && canAct && !ledgeHanging) {

			updateFacing();


			//If just started moving or switched directions
			if((accelerationFrames == 6 || rb.velocity.x * drifter.input[0].MoveX < 0) && IsGrounded())
				if(groundFrictionPosition) spawnJuiceParticle(new Vector2(-Facing * 1.5f,0) + contacts[0].point, MovementParticleMode.KickOff);
			
			
			if(IsGrounded()) {

				if(!jumping) {
					if(drifter.input[0].MoveX !=0 && drifter.input[1].MoveX == 0)
						drifter.PlayAnimation("Walk");

					//Spawn dust clouds as characters walk, every 20 frames
					if(groundFrictionPosition) {
						if(dustCloudTimer > 15) {
							spawnJuiceParticle(new Vector2(-Facing * 1.5f,0) + contacts[0].point, MovementParticleMode.WalkDust);
							dustCloudTimer = 0;
						}
						else dustCloudTimer ++;
						
					}

				}
				if(accelerationFrames < groundAccelerationTime) accelerationFrames ++;
				else accelerationFrames = (int)groundAccelerationTime;

				currentSpeed = walkSpeed * ((drifter.status.hasSloMoEffect() || Pusher!=null) ? .4f: 1f) * (drifter.status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f) * (drifter.input[0].MoveX > 0 ? 1 : -1);

				rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x,currentSpeed,accelerationFrames/groundAccelerationTime), rb.velocity.y);

			}
			else {
				if(!jumping)
					drifter.PlayAnimation("Hang");

				if(accelerationFrames < airAccelerationTime) accelerationFrames ++;
				else accelerationFrames = (int)airAccelerationTime;

				currentSpeed = airSpeed * ((drifter.status.hasSloMoEffect() || Pusher!=null) ? .4f: 1f) * (drifter.status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f) * (drifter.input[0].MoveX > 0 ? 1 : -1);

				rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x,currentSpeed,accelerationFrames/airAccelerationTime), rb.velocity.y);
			}
		}

		//Character """collision"""
		if(Pusher!=null && hasCollision) 
			rb.AddForce(new Vector2(
				-1 * Mathf.Sign(Pusher.transform.position.x-PushBox.transform.position.x) * Mathf.Clamp(1/Mathf.Abs(Pusher.transform.position.x-PushBox.transform.position.x),2f,5f), 0), ForceMode2D.Impulse);

		//Guard
		if(drifter.input[0].Guard && canGuard) {
			//shift is guard
			drifter.guard();
			updateFacing();
		}
	  
		//Disable Guarding
		else if(!drifter.input[0].Guard && !drifter.status.HasStunEffect() && drifter.guarding) {
			//drifter.status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,18);
			//drifter.canSpecialCancelFlag = true;
			//drifter.listenForSpecialCancel = true;
			drifter.returnToIdle();
			//drifter.parrying = true;
			//drifter.PlayAnimation("Guard_Drop");
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
			if(!drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG))canLandingCancel = true;
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


		else if(canAct && (drifter.doubleTappedX() || drifter.input[0].Dash)) {
			dash();
		}

		//Pause movement for relevent effects.
		
	}

	//Moves the character left or right, based on the speed provided
	public void move(float speed, bool flipDirection = true) {

		if(accelerationFrames < airAccelerationTime) accelerationFrames ++;
		else accelerationFrames = (int)airAccelerationTime;

		if(flipDirection)updateFacing();

		if(drifter.input[0].MoveX != 0) {
			currentSpeed = speed * (drifter.status.hasSloMoEffect() ? .4f: 1f) * (drifter.status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f) * (drifter.input[0].MoveX > 0 ? 1 : -1);
			rb.velocity = new Vector2(Mathf.Lerp(rb.velocity.x,currentSpeed,accelerationFrames/airAccelerationTime), rb.velocity.y);
		}
		
	}
	

	//Made it public for streamlining channeled attack cancels
	public void techParticle() {
		spawnJuiceParticle(BodyCollider.bounds.center, MovementParticleMode.Tech, Quaternion.Euler(0f,0f,0f),false);
	}

    public void actionCancelParticle() {
    	//UnityEngine.Debug.Log("CANCEL PARTICLE");
        spawnJuiceParticle(BodyCollider.bounds.center, MovementParticleMode.Cancel, Quaternion.Euler(0f,0f,0f),false);
    }

	//Updates the direction the player is facing
	public void updateFacing() {

		if(Facing != drifter.input[0].MoveX)accelerationFrames = 6;

		if(drifter.input[0].MoveX > 0) Facing = 1;
		else if(drifter.input[0].MoveX < 0) Facing = -1;

		drifter.SetIndicatorDirection(Facing);
		transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),
		transform.localScale.y, transform.localScale.z);
	}

	//Used to forcibly invert the players direction
	public void flipFacing(){
		Facing *= -1;
		drifter.SetIndicatorDirection(Facing);
		transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y, transform.localScale.z);
	}

	public void setFacing(int dir){
		Facing = Math.Sign(dir);
		drifter.SetIndicatorDirection(Facing);
		transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y, transform.localScale.z);
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
		int count = Physics2D.RaycastNonAlloc(BodyCollider.bounds.center + new Vector3( BodyCollider.bounds.extents.x * ((Facing > 0)?1:-1),BodyCollider.bounds.extents.y,0), ((Facing > 0)?Vector3.right:Vector3.left),wallHits, 0.35f);

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
			
			//jump needs a little delay so character animations can spend
			//a frame or two preparing to jump
			//jumpCoroutine = StartCoroutine(DelayedJump());

			jumpTimer = -5f;
			return true;
		}
		return false;

	}

	public bool dash(bool enforceFullDistance = false) {
		if(currentDashes > 0 && !dashing) {
			updateFacing();
			accelerationFrames = 120;
			dashLock = 60;
			dashing = true;
			spawnJuiceParticle(BodyCollider.bounds.center + new Vector3(Facing * 1.5f,0), MovementParticleMode.Dash_Ring, Quaternion.Euler(0f,0f,0f), false);
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
		UnityEngine.Debug.Log("USED SUPER: " + inspiration);

		if(drifter.status.HasStatusEffect(PlayerStatusEffect.DEAD) || !drifter.CanUseSuper()) return;

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
			else if(drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG) && drifter.superCharge >= 100) {
				if(drifter.superCharge >= 200 && !drifter.canFeint) {
					drifter.attacks.useSuper();
					spawnSuperParticle(CancelType.Offensive_Cancel,200,20);		
				}
				else if(drifter.canFeint) {
					drifter.attacks.useSuper();
					spawnSuperParticle(CancelType.Feint_Cancel,100,8);
				}
			}
			//Burst/Defensive Cancel
			else if(!drifter.guarding && drifter.superCharge >= 200 && drifter.status.HasEnemyStunEffect() && !drifter.status.HasStatusEffect(PlayerStatusEffect.GRABBED) && !drifter.status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)) {
				drifter.ToggleAnimator(true);
				hitstun = false;
				drifter.status.clearStunStatus();
				//drifter.status.ApplyStatusEffect(PlayerStatusEffect.INVULN,8);
				drifter.attacks.useSuper();
				spawnSuperParticle(CancelType.Defensive_Cancel,200,8);
				if(currentJumps+1 < numberOfJumps) currentJumps++;
				
			}
			else if (!drifter.guarding && drifter.superCharge >= 100 && !drifter.status.HasStunEffect()) {
				drifter.attacks.useSuper();
				spawnSuperParticle(CancelType.Time_Cancel,100,8);
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
		if(SuperCancel!= null)
			Destroy(SuperCancel);

		canceltype = mode;
		canLandingCancel = false;
		mainCamera.Darken(darkentime);
		drifter.attacks.SetMultiHitAttackID();
		Vector3 flip = new Vector3(Facing * 10f, 10f, 0f);
		
		drifter.superCharge -= cost;

		SuperCancel = GameController.Instance.CreatePrefab("SuperEffect", transform.position , transform.rotation);
		foreach (HitboxCollision hitbox in SuperCancel.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = drifter.attacks.NextID;
			hitbox.isActive = true;
			hitbox.Facing = Facing;
		}
		SuperCancel.GetComponent<InstantiatedEntityCleanup>().animator.Play(canceltype.ToString());
		
	}

	//Rollback
	//====================================
	
	//Takes a snapshot of the current frame to rollback to
	public MovementRollbackFrame SerializeFrame() {
		return new MovementRollbackFrame() {
			//Rigid body
			// Velocity = rb.velocity,
			// Gravity = rb.gravityScale,
			// Position = rb.position,

			//Flags
			Facing = this.Facing,
			LedgeGrabLockout = ledgeGrabLockout,
			TerminalVelocity = terminalVelocity,
			CurrentJumps = currentJumps,
			CurrentDashes = currentDashes,
			Grounded = grounded,
			Hitstun = hitstun,
			CanLandingCancel = canLandingCancel,
			CanFastFall = canFastFall,
			Jumping = jumping,
			Dashing = dashing,
			GravityPaused = gravityPaused,
			LedgeHanging = ledgeHanging,
			StrongLedgeGrab = strongLedgeGrab,
			AccelerationFrames = accelerationFrames,
			DashLock = dashLock,
			JumpTimer = jumpTimer,
			DropThroughTime = dropThroughTime,
			PrevVelocity = prevVelocity,
			CurrentSpeed = currentSpeed,
			DelayedFacingFlip = delayedFacingFlip,
			SuperCancel = this.SuperCancel != null ? SuperCancel.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			CancelType = canceltype

		};
	}

	//Rolls back the entity to a given frame state
	public void DeserializeFrame(MovementRollbackFrame p_frame) {
		//Rigid body
		// rb.velocity = p_frame.Velocity;
		// rb.gravityScale = p_frame.Gravity;
		// rb.position = p_frame.Position;

		//Flags
		Facing = p_frame.Facing;
		ledgeGrabLockout = p_frame.LedgeGrabLockout;
		terminalVelocity = p_frame.TerminalVelocity;
		currentJumps = p_frame.CurrentJumps;
		currentDashes = p_frame.CurrentDashes;
		grounded = p_frame.Grounded;
		hitstun = p_frame.Hitstun;
		canLandingCancel = p_frame.CanLandingCancel;
		canFastFall = p_frame.CanFastFall;
		jumping = p_frame.Jumping;
		dashing = p_frame.Dashing;
		gravityPaused = p_frame.GravityPaused;
		ledgeHanging = p_frame.LedgeHanging;
		strongLedgeGrab = p_frame.StrongLedgeGrab;
		accelerationFrames = p_frame.AccelerationFrames;
		dashLock = p_frame.DashLock;
		jumpTimer = p_frame.JumpTimer;
		dropThroughTime = p_frame.DropThroughTime;
		prevVelocity = p_frame.PrevVelocity;
		currentSpeed = p_frame.CurrentSpeed;
		delayedFacingFlip = p_frame.DelayedFacingFlip;
		canceltype = p_frame.CancelType;

		//Super Particle reset
		if(p_frame.SuperCancel != null) {
			if(SuperCancel == null)spawnSuperParticle(canceltype,0,8);
			SuperCancel.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(p_frame.SuperCancel);
		}
		//Projectile does not exist in rollback frame
		else if(p_frame.SuperCancel == null) {
			Destroy(SuperCancel);
			SuperCancel = null;
		}

	}
}

public class MovementRollbackFrame: INetworkData
{
	public string Type { get; set; }

	//public Vector2 Velocity;
	//public float Gravity;
	//public Vector2 Position;

	public int Facing;
	public float TerminalVelocity;
	public int CurrentJumps;
	public int CurrentDashes;
	public int LedgeGrabLockout;
	public bool Grounded;
	public bool Hitstun;
	public bool CanLandingCancel;
	public bool CanFastFall;
	public bool Jumping;
	public bool Dashing;
	public bool GravityPaused;
	public bool LedgeHanging;
	public bool StrongLedgeGrab;
	public int AccelerationFrames;
	public float DashLock;
	public float JumpTimer;
	public int DropThroughTime;
	public float WalkTime;
	public Vector2 PrevVelocity;
	public float CurrentSpeed;
	public bool DelayedFacingFlip;
	public BasicProjectileRollbackFrame SuperCancel;
	public CancelType CancelType;

}
