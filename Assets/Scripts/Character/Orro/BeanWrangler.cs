using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class BeanState {
		public BeanState(Vector2 pos, int facing) {
			Pos = pos;
			Facing = facing;
		}

		public Vector2 Pos { get; set;}
		public int Facing { get; set;}
}

public class BeanWrangler : NonplayerHurtboxHandler
{
	// Constnat Values
	private int _color = 0;
	public int color {
		get{ return _color;}
		set {
			_color = value;
			healthBar?.setColor((PlayerColor)value);
		}	
	}

	float RETURN_SPEED = 25f;

	//Utility References
	Animator animator;
	PlayerAttacks attacks;

	//Synced Values
	public bool following = true;
	public int beanMovementDelay = 100;
	public bool canAct = false;
	public bool alive = true;
	public int prevHitstunDuration;

	BeanState targetState;
	BeanState state;

	void Awake() {
		entity = GetComponent<InstantiatedEntityCleanup>();
		//base.Start();
		animator = GetComponent<Animator>();
		//Movement Stuff
		targetState = new BeanState(rb.position, facing);
		attacks = gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponent<PlayerAttacks>();
		hurtState = "Hitstun";
		takesKnockback = true;

	}

	public override void UpdateFrame() {

		base.UpdateFrame();
			
		if(HitstunDuration > 0) {
			prevHitstunDuration = HitstunDuration;
			return;
		}
		else if(prevHitstunDuration != HitstunDuration && alive){
			prevHitstunDuration = 0;
			returnToNeutral();
		}
		else {
			//Get the next state for bean to move towards
			targetState = state;
			//If bean is currently following orro,
			if(following && canAct && targetState != null) {
				facing = targetState.Facing;
				if(!alive) {
					//Heal bean if he is dead
					if(percentage > 0) percentage -= .06f;
					if(percentage <= 0)	{
						percentage = 0;
						alive = true;
						PlayAnimation("Bean_Spawn");
					}
				}
				//Return to orro
				//If bean is too far away (more than 3 stage lengths, he will immediately teleport to orro.
				if(Vector2.Distance(rb.position,targetState.Pos) > 100f) {
					rb.position = targetState.Pos;
					transform.localScale = new Vector2(targetState.Facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y);
				}
				//If bean is returning to orro, he will move at a slower speed and not heal
				if(Vector2.Distance(rb.position,targetState.Pos) > 3.8f) {
					rb.position =  Vector2.MoveTowards(rb.position,targetState.Pos,RETURN_SPEED/60f);
					transform.localScale = new Vector2((targetState.Pos.x > rb.position.x ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
						transform.localScale.y); 
						beanMovementDelay = 50;
				}
				//Follow orro while attatched
				//Bean follows more closely while attatched to not get left behind
				else {
					//Tick down beans damage when he is attatched to orro
					if(percentage > 0) percentage -=.02f;
					//Follow Logic
					rb.position =  Vector2.Lerp(rb.position,targetState.Pos, .25f * beanMovementDelay / 100f);
					transform.localScale = new Vector2(targetState.Facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y); 
					if(beanMovementDelay < 100) beanMovementDelay++;
				}
			}
		}
	}

	public void PlayAnimation(string p_state, float p_normalizedTime = -1) {
		animator.Play(Animator.StringToHash(p_state),0,p_normalizedTime < 0 ? 0: p_normalizedTime);
	}


	//Enqueus a state for bean to mimic after a short delay
	public void addBeanState(Vector2 pos,int facingDir) {
		state = new BeanState(pos,facingDir);
	}

	//Enqueus a state for bean to mimic after a short delay
	public void setBeanDirection(int facingDir) {
		facing = facingDir;
		transform.localScale = new Vector2(facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y); 
	}

	//Tells bean to start returning to orro. 
	public void recallBean(Vector2 pos,int facingDir) {

		if(!following) {
			state = null;
			targetState = new BeanState(pos, facingDir);
			following = true; 
		}
		else {
			state = null;
			addBeanState(rb.position,facing);
			following = false;
		}
		
	}

	//BEAN IS GONE :Crab:
	public void die() {
		if(!alive)return;
		canAct = false;
		alive = false;
		rb.velocity = Vector2.zero;
		PlayAnimation("Bean_True_Death");
	}

	//Sends bean out at a set speed.
	public void setBean(float speed) {
		if(HitstunDuration > 0f || !alive)return;
		state = null;
		following = false;
		transform.localScale = new Vector2(facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y); 
		if(speed > 0 && Vector2.Distance(rb.position,targetState.Pos) < 3.8f)
			rb.velocity = new Vector2(facing * speed,0);
	}

	//Returns bean to his neutral state, clearing all previous states and variables.
	public void returnToNeutral() {
		state = null;
		canAct = true;
		PlayAnimation("Bean_Idle");
	}


	//Use this at the end of beans death animation
	public void setCanAct() {
		state = null;
		rb.position = targetState.Pos;
		canAct = true;
		following = true;
		//alive = true;
	}

	//Plays a follow up state, ignoring if bean "can act" or not. 
	//Still will not play if he is in hitstun
	public void playFollowState(String stateName) {
		if(!alive || HitstunDuration >0) return;
		canAct = false;
		transform.localScale = new Vector2(targetState.Facing * Mathf.Abs(transform.localScale.x),
					transform.localScale.y); 
		PlayAnimation(stateName);
	}


	//Plays an animation for bean, if he can act and is alive
	public void playState(String stateName) {
		if(!canAct || !alive)return;

		canAct = false;

		transform.localScale = new Vector2(targetState.Facing * Mathf.Abs(transform.localScale.x),
					transform.localScale.y); 
		PlayAnimation(stateName);
	}


	//Plays an animation for bean, if he can act and is alive
	public void playChargeState(String stateName) {
		if(HitstunDuration >0  || !canAct || !alive)return;

		transform.localScale = new Vector2(targetState.Facing * Mathf.Abs(transform.localScale.x),
					transform.localScale.y); 
		PlayAnimation(stateName);
	}
 
	//Refreshes beans hitboxes so he can multihit
	public void multihit() {
		attacks.SetMultiHitAttackID();
	}

	//Registers a hit on bean, and handles his counter.
	//If bean has taken over 40%, he becomes inactive untill he can heal
	public override AttackHitType RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, SingleAttackData attackData) {

		AttackHitType returnCode = AttackHitType.NONE;

		if(hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && CanHit(attackID)) {
			if(following && Vector2.Distance(rb.position,targetState.Pos) <= 3.8f) return AttackHitType.NONE;

				returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackData);
				oldAttacks[attackID] = MAX_ATTACK_DURATION;

			if(percentage > maxPercentage) {
					alive = false;
					canAct = false;
					PlayAnimation("Bean_Death");
					HitstunDuration = 0;
					//Delay before bean begins recharging
					rb.velocity = Vector2.zero;
					delayedVelocity = Vector2.zero;
			}
		}

		return returnCode;

	}
	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		
		base.Serialize(bw);
		
		bw.Write(following);
		bw.Write(canAct);
		bw.Write(alive);

		bw.Write(beanMovementDelay);
		bw.Write(prevHitstunDuration);


		bw.Write(targetState.Facing);
		bw.Write(state.Facing);

		bw.Write(targetState.Pos.x);
		bw.Write(targetState.Pos.y);

		bw.Write(state.Pos.x);
		bw.Write(state.Pos.y);
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		
		base.Deserialize(br);

		following = br.ReadBoolean();
		canAct = br.ReadBoolean();
		alive = br.ReadBoolean();

		beanMovementDelay = br.ReadInt32();
		prevHitstunDuration = br.ReadInt32();

		targetState.Facing = br.ReadInt32();
		state.Facing = br.ReadInt32();

		Vector2 TargetState = Vector2.zero;
		TargetState.x = br.ReadSingle();
		TargetState.y = br.ReadSingle();
		targetState.Pos = TargetState;

		Vector2 State = Vector2.zero;
		State.x = br.ReadSingle();
		State.y = br.ReadSingle();
		state.Pos = State;
	}

}