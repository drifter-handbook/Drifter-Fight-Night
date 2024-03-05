﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BeanState {
		public BeanState(Vector3 pos, int facing) {
			Pos = pos;
			Facing = facing;
		}

		public Vector3 Pos { get; set;}
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
	int BEAN_RESPAWN_DELAY = 180;

	//Utility References
	Animator animator;
	PlayerAttacks attacks;

	//Synced Values
	public bool following = true;
	public int beanMovementDelay = 100;
	public bool canAct = false;
	public bool alive = true;
	public float prevHitstunDuration;

	BeanState targetPos;
	BeanState state;

	new void Start() {
		entity = GetComponent<InstantiatedEntityCleanup>();
		base.Start();
		animator = GetComponent<Animator>();
		//Movement Stuff
		targetPos = new BeanState(rb.position, facing);
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
			targetPos = state;
			//If bean is currently following orro,
			if(following && canAct) {
				facing = targetPos.Facing;
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
				if(Vector3.Distance(rb.position,targetPos.Pos) > 100f) {
					rb.position = targetPos.Pos;
					transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y, transform.localScale.z);
				}
				//If bean is returning to orro, he will move at a slower speed and not heal
				if(Vector3.Distance(rb.position,targetPos.Pos) > 3.8f) {
					rb.position =  Vector3.MoveTowards(rb.position,targetPos.Pos,RETURN_SPEED/60f);
					transform.localScale = new Vector3((targetPos.Pos.x > rb.position.x ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
						transform.localScale.y, transform.localScale.z); 
						beanMovementDelay = 50;
				}
				//Follow orro while attatched
				//Bean follows more closely while attatched to not get left behind
				else {
					//Tick down beans damage when he is attatched to orro
					if(percentage > 0) percentage -=.02f;
					//Follow Logic
					rb.position =  Vector3.Lerp(rb.position,targetPos.Pos, .25f * beanMovementDelay / 100f);
					transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y, transform.localScale.z); 
					if(beanMovementDelay < 100) beanMovementDelay++;
				}
			}
		}
	}

	public void PlayAnimation(string p_state, float p_normalizedTime = -1) {
		animator.Play(Animator.StringToHash(p_state),0,p_normalizedTime < 0 ? 0: p_normalizedTime);
	}


	//Enqueus a state for bean to mimic after a short delay
	public void addBeanState(Vector3 pos,int facingDir) {
		state = new BeanState(pos,facingDir);
	}

	//Enqueus a state for bean to mimic after a short delay
	public void setBeanDirection(int facingDir) {
		facing = facingDir;
		transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y, transform.localScale.z); 
	}

	//Tells bean to start returning to orro. 
	public void recallBean(Vector3 pos,int facingDir) {

		if(!following) {
			state = null;
			targetPos = new BeanState(pos, facingDir);
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
		if(!GameController.Instance.IsHost || !alive)return;
		canAct = false;
		alive = false;
		rb.velocity = Vector3.zero;
		PlayAnimation("Bean_True_Death");
	}

	//Sends bean out at a set speed.
	public void setBean(float speed) {
		if(!GameController.Instance.IsHost || HitstunDuration > 0f || !alive)return;
		state = null;
		following = false;
		transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
						transform.localScale.y, transform.localScale.z); 
		if(speed > 0 && Vector3.Distance(rb.position,targetPos.Pos) < 3.8f)
			rb.velocity = new Vector3(facing * speed,0,0);
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
		rb.position = targetPos.Pos;
		canAct = true;
		following = true;
		//alive = true;
	}

	//Plays a follow up state, ignoring if bean "can act" or not. 
	//Still will not play if he is in hitstun
	public void playFollowState(String stateName) {
		if(!GameController.Instance.IsHost || !alive || HitstunDuration >0) return;
		canAct = false;
		transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
					transform.localScale.y, transform.localScale.z); 
		PlayAnimation(stateName);
	}


	//Plays an animation for bean, if he can act and is alive
	public void playState(String stateName) {
		if(!GameController.Instance.IsHost || !canAct || !alive)return;

		canAct = false;

		transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
					transform.localScale.y, transform.localScale.z); 
		PlayAnimation(stateName);
	}


	//Plays an animation for bean, if he can act and is alive
	public void playChargeState(String stateName) {
		if(!GameController.Instance.IsHost || HitstunDuration >0  || !canAct || !alive)return;

		transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
					transform.localScale.y, transform.localScale.z); 
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

		if(GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && CanHit(attackID)) {
			if(following && Vector3.Distance(rb.position,targetPos.Pos) <= 3.8f) return AttackHitType.NONE;

				returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackData);
				oldAttacks[attackID] = MAX_ATTACK_DURATION;

			if(percentage > maxPercentage) {
					alive = false;
					canAct = false;
					PlayAnimation("Bean_Death");
					HitstunDuration = 0;
					//Delay before bean begins recharging
					rb.velocity = Vector3.zero;
					delayedVelocity = Vector3.zero;
			}
		}

		return returnCode;

	}
    //Rollback
    //=========================================

    //Takes a snapshot of the current frame to rollback to
    public BeanRollbackFrame SerializeFrame()
    {
        return new BeanRollbackFrame()
        {
            NPCFrame = base.SerializeFrame(),
            Following = following,
			BeanMovementDelay = beanMovementDelay,
			CanAct = canAct,
			Alive = alive,
			PrevHitstunDuration = prevHitstunDuration,
			TargetPos = targetPos,
			State = state,
        };
    }

    //Rolls back the entity to a given frame state
    public void DeserializeFrame(BeanRollbackFrame p_frame)
    {

        DeserializeFrame(p_frame.NPCFrame);
        following = p_frame.Following;
		beanMovementDelay = p_frame.BeanMovementDelay;
		canAct = p_frame.CanAct;
		alive = p_frame.Alive;
		prevHitstunDuration = p_frame.PrevHitstunDuration;
		targetPos = p_frame.TargetPos;
		state = p_frame.State;
        
    }

}

public class BeanRollbackFrame: INetworkData
{

    public NPCHurtboxRollbackFrame NPCFrame;

    public bool Following;
	public int BeanMovementDelay;
	public bool CanAct;
	public bool Alive;
	public float PrevHitstunDuration;
	public BeanState TargetPos;
	public BeanState State;

    public string Type { get; set; }
    
}