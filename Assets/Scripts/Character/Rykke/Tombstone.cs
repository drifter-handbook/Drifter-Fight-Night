using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tombstone : NonplayerHurtboxHandler
{

	//Synced Flags
	public bool canAct = false;
	public bool active = false;
	public bool projectile = true;
	public bool attacking = false;
	public bool listeningForGrounded = true;
	public bool breaking = false;

	//Utility References
	GameObject drifter;
	Collider2D physicsCollider; 
	Animator animator;
	WalkOff ledgeDetector;

	//Calculated values
	int tombstoneType = 0;
	float distanceFromParent = 0;

	//Constants
	Vector2 OFFSET = new Vector2(0,2);
	float ZOMBIE_RADIUS = 4.5f;


	// Start is called before the first frame update
	void Awake() {
		rb = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
		physicsCollider = GetComponentInChildren<PolygonCollider2D>();
		ledgeDetector = GetComponentInChildren<WalkOff>();
		entity = GetComponent<InstantiatedEntityCleanup>(); 
	}

	// // Update is called once per frame
	public override void UpdateFrame() {
		base.UpdateFrame();
		//entity.UpdateFrame();

		if(listeningForGrounded && IsGrounded()) {
			listeningForGrounded = false;
			if(projectile)
				PlayAnimation(tombstoneType + "_Land"); 
			else {
				PlayAnimation("Land");
				canAct = false;
			}
		}
		ledgeDetector.UpdateFrame();
	}

	new void Start() {
		//base.Start();
		PlayAnimationEvent(tombstoneType + "_Spin");
	}

	//sets necessary fields to make spawning cleaner
	public Tombstone setup(int p_tombstoneIndex, int p_facing,GameObject p_drifter,float p_radius,PlayerColor color) {
	   tombstoneType = p_tombstoneIndex;
	   facing = p_facing;
	   drifter = p_drifter;
	   ZOMBIE_RADIUS = p_radius;
	   healthBar.setColor(color);
	   return this;
	}

	//Registers a hit on the stone, and handles his counter.
	//
	public override AttackHitType RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, SingleAttackData attackData) {

		AttackHitType returnCode = AttackHitType.NONE;

		if(takesKnockback)takesKnockback = false;

		if(hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && CanHit(attackID)) {
			returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackData);

			oldAttacks[attackID] = MAX_ATTACK_DURATION;

			if(percentage >= maxPercentage)breakStone();
		}

		return returnCode;

	}

	void OnTriggerEnter2D(Collider2D collider) {
		if(collider.gameObject.tag == "BounceObject" && collider.GetComponent<Tombstone>().drifter == drifter)
			
			//Dont interaxt with tombstones on their way out
			if(breaking || collider.GetComponent<Tombstone>().breaking){
				return;
			}
			else if(projectile){
				rb.velocity = new Vector3(facing * -10f,20f);
				foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
					hitbox.Facing = -facing;
			}
			else if(!attacking && !collider.GetComponent<Tombstone>().projectile){
				updateDirection(collider.GetComponent<Tombstone>().facing);
				projectilize();
				//Add particle here

			}
			//else if(!active && ! canAct)rb.velocity = new Vector3(collider.gameObject.transform.position.x > transform.position.x ? -5f:5f,0);
	}

	public void breakStone(){
		breaking = true;
		canAct = false;
		active = false;
		entity.unfreeze();
		PlayAnimation(tombstoneType + "_Break");
	}	

	private void PlayAnimation(string p_state, float p_normalizedTime = -1) {
		animator.Play(Animator.StringToHash(p_state),0,p_normalizedTime < 0 ? 0: p_normalizedTime);
	}
			

	public void PlayConditionalAnimation(string p_state, bool p_actionable = false, bool p_gated = false,float p_normalizedTime = -1) {
		if(!canAct && p_gated)return;

		canAct = p_actionable;
		PlayAnimation(p_state,p_normalizedTime);
	}

	public void PlayAnimationEvent(string p_state) {
		canAct = false;
		PlayAnimation(p_state);
	}

	//Sets the "Active" flag
	public void activate() {
		active = true;
	}


	//Launches stone with set velocity from an external object
	public void throwStone(int mode) {
		//canAct = false;
		switch(mode) {
			case 0:
				rb.velocity = new Vector3(facing *5f,-40f);
				break;
			case 1:
				rb.velocity = new Vector3(facing * 30f,20f);
				break;
			case 2:
				rb.velocity = new Vector3(0,45f);
				break;
			case 3:
				rb.velocity = new Vector3(facing *-5f,45f);
				break;
			default:
				break;
		}
		

	}

	//Switches the direction the object is facing when recieving a command;
	public void updateDirection(int p_facing) {
		facing = p_facing;
		transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
		transform.localScale.y, transform.localScale.z);

	}

	//Returns to the idle state and resets most flags
	public void returnToIdle() {
		if(projectile)
			PlayAnimation(tombstoneType + "_Idle");
		else if(active && distanceFromParent < ZOMBIE_RADIUS)
			PlayAnimation(IsGrounded() ? "Active_Idle" : "Hang");
		else if(!canAct && !active)
			PlayAnimation(IsGrounded() ? "Deactivate" : "Hang");
		else
			PlayAnimation(tombstoneType + "_Idle");

		//else
			
		listeningForGrounded = false;
		ledgeDetector.setPreventWalkoff(false);
		canAct = true;
		attacking = false;
		projectile = false;
		//entity.useHitpause = true;

	}

	public void setCanAct() {
		canAct = true;
	}

	void projectilize() {
		canAct = false;
		active = false;
		projectile = true;
		listeningForGrounded = true;
		PlayAnimationEvent(tombstoneType + "_Spin");
		throwStone(3);
		//entity.useHitpause = false;
	}


	public void listenForGrounded() {
		listeningForGrounded = true;
	}

	public void listenForLedge() {

		ledgeDetector.togglePreventWalkoff();
	}
	
	public float getDistance(Vector3 parent) {
		distanceFromParent = Vector3.Distance(parent,rb.position + OFFSET);

		return distanceFromParent;
	}


	private bool IsGrounded() {
		RaycastHit2D[] hits = new RaycastHit2D[10];
		int count = Physics2D.RaycastNonAlloc(physicsCollider.bounds.center + physicsCollider.bounds.extents.y * Vector3.down, Vector3.down, hits, 0.2f);

		for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform")) 
			return rb.velocity.y <=.1f;

		return false;
	}

	//Spawns a flame burst effect
	public void burst(int mode = 0) {
		GameObject burst = GameController.Instance.CreatePrefab("Zombie_Burst", transform.position + new Vector3(2f * facing,0), transform.rotation);
		burst.transform.localScale = new Vector3(facing *10,10,1f);

		burst.GetComponent<Animator>().Play(mode ==0? "Vertical":"Horizontal");

	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public TombstoneRollbackFrame SerializeFrame() {
		return new TombstoneRollbackFrame() {
			NPCFrame = base.SerializeFrame(),
			CanAct = canAct,
			Active = active,
			Projectile = projectile,
			Attacking = attacking,
			ListeningForGrounded = listeningForGrounded,
			Breaking = breaking,
		};
	}

	//Rolls back the entity to a given frame state
	public void DeserializeFrame(TombstoneRollbackFrame p_frame) {

		DeserializeFrame(p_frame.NPCFrame);
		canAct = p_frame.CanAct;
		active = p_frame.Active;
		projectile = p_frame.Projectile;
		attacking = p_frame.Attacking;
		listeningForGrounded = p_frame.ListeningForGrounded;
		breaking = p_frame.Breaking;
	}

}

public class TombstoneRollbackFrame: INetworkData
{

	public NPCHurtboxRollbackFrame NPCFrame;

	public bool CanAct;
	public bool Active;
	public bool Projectile;
	public bool Attacking;
	public bool ListeningForGrounded;
	public bool Breaking;

	public string Type { get; set; }
	
}