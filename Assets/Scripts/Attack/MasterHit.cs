using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICharacterRollbackFrame : INetworkData
{

}

public class MasterhitRollbackFrame: INetworkData
{
	public string Type { get; set; }

	public bool Empowered;
	public Vector3 SavedVelocity;
	public bool SavingVelocity ;
	public bool SpecialTappedFlag;
	public bool SpecialReleasedFlag;
	public bool LightTappedFlag;
	public bool VerticalCancelFlag;
	public bool MovementCancelFlag;
	public bool ActiveCancelFlag;
	public bool ListeningForGroundedFlag;
	public bool QueuedStateTrigger;
	public bool JumpFlag;
	public int SpecialCharge;
	public int SpecialLimit;
	public string QueuedState;
	public bool AttackWasCanceled;
	public bool DacusCancelFlag;
	public bool KnockdownFlag;

	public ICharacterRollbackFrame CharacterFrame = null;
	
}

public abstract class MasterHit : MonoBehaviour, IMasterHit
{

	public string assetPathName;

	protected Drifter drifter;
	protected Rigidbody2D rb;
	protected PlayerMovement movement;
	protected PlayerStatus status;
	protected PlayerAttacks attacks;
	protected Animator anim;
	protected WalkOff ledgeDetector;


	//Listener Bools

	protected bool Empowered = false;

	protected Vector3 savedVelocity;

	protected bool savingVelocity = false;

	protected bool specialTappedFlag = false;

	protected bool specialReleasedFlag = false;

	protected bool lightTappedFlag = false;

	protected bool verticalCancelFlag = false;

	protected bool movementCancelFlag = false;

	protected bool activeCancelFlag = false;

	protected bool listeningForGroundedFlag = false;

	protected bool queuedStateTrigger = false;

	protected bool jumpFlag = false;

	protected int specialCharge = 0;

	protected int specialLimit = -1;

	protected string queuedState = "";

	protected bool attackWasCanceled = false;

	protected bool dacusCancelFlag = false;

	protected bool knockdownFlag = false;


	public virtual void UpdateFrame() {
		attackWasCanceled = true;
		//Clear all flags if the character is dead or stunned by an opponent

		if(drifter.status.canbeKnockedDown() && !drifter.knockedDown && movement.grounded) {
			if(!drifter.status.HasStatusEffect(PlayerStatusEffect.HITPAUSE)){
				//Determine knockdown duration
				movement.gravityPaused = true;
				setTerminalVelocity(20f);
				rb.gravityScale = 10f;
				drifter.PlayAnimation("Knockdown_Bounce");
				drifter.status.ApplyStatusEffect(PlayerStatusEffect.KNOCKDOWN,40);
				//movement.mainCamera.Shake(6,.33f);
				//If the victim is in hitpause, set their delayed velocity instead
				rb.velocity = new Vector2(movement.Facing *-12f,20);
				listeningForGroundedFlag = true;
			}
		}

		//Handles knockdown after bounce
		else if(status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN)){
			if(!status.HasStatusEffect(PlayerStatusEffect.HITPAUSE)) {
				drifter.knockedDown = true;
				knockdownFlag = true;
				if(listeningForGroundedFlag && movement.grounded) {
					resetTerminalVelocity();
					movement.resetGravity();
					status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,0);
					status.ApplyStatusEffect(PlayerStatusEffect.KNOCKDOWN,91);
					playState("Knockdown");
					status.ApplyStatusEffect(PlayerStatusEffect.FLATTEN,90);
					rb.velocity = new Vector2(movement.Facing * -10f * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
					BounceParticle();
					playQueuedState();
					clearMasterhitVars();
				}
			}
		}

		else if(status.HasEnemyStunEffect() || movement.ledgeHanging ) {
			clearMasterhitVars();
		}
		else if(
			(listeningForGroundedFlag && movement.grounded)||
			(specialReleasedFlag && !drifter.input[0].Special)||
			(specialTappedFlag && checkForSpecialTap())||
			(specialLimit > 0 && specialCharge >= specialLimit)
			) {
			attackWasCanceled = false;
			playQueuedState();
			clearMasterhitVars();
		}

		else if(movementCancelFlag && movement.currentDashes >0 && drifter.doubleTappedX()) {
			if(movement.dash()) {
				movement.techParticle();
				clearMasterhitVars();
				drifter.clearGuardFlags();
			}            
		}
		else if(dacusCancelFlag && attacks.grabPressed())
		 {
			status.ApplyStatusEffect(PlayerStatusEffect.INVULN,0);
			unpauseGravity();
			setXVelocity(movement.dashSpeed);
			movement.updateFacing();
			attacks.useGrab();
			clearMasterhitVars();

		}
		else if(dacusCancelFlag && attacks.specialPressed())
		 {
			status.ApplyStatusEffect(PlayerStatusEffect.INVULN,0);
			unpauseGravity();
			setXVelocity(movement.dashSpeed);
			movement.updateFacing();
			attacks.useSpecial();
			clearMasterhitVars();

		}
		else if(dacusCancelFlag && attacks.lightPressed()) {
			status.ApplyStatusEffect(PlayerStatusEffect.INVULN,0);
			unpauseGravity();
			setXVelocity(movement.dashSpeed);
			movement.updateFacing();
			attacks.useNormal();
			clearMasterhitVars();

		}
		else if(verticalCancelFlag && drifter.doubleTappedY() && drifter.input[0].MoveY <0) {
			playQueuedState();
			movement.techParticle();
			returnToIdle();
		}
		else if(activeCancelFlag && drifter.input[0].Guard && !drifter.input[1].Guard) {
			movement.techParticle();
			status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0);
			status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0);
			clearMasterhitVars();
			resetTerminalVelocity();
			playState("Guard");
			drifter.guarding = true;
			movement.jumping = false;
			unpauseGravity();
			

		}
		else if((activeCancelFlag || jumpFlag)&& ((drifter.input[0].Jump && !drifter.input[1].Jump && movement.currentJumps>0) || (drifter.doubleTappedX() && movement.currentDashes >0) )) {
			if(drifter.input[0].Jump) {
				movement.jumping = false;
				movement.techParticle();
				status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0);
				status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0);
				clearMasterhitVars();
				resetTerminalVelocity();
				movement.jump();
				drifter.clearGuardFlags();
				unpauseGravity();
			}
			else {
				if(movement.dash())
				{
					movement.techParticle();
					clearMasterhitVars();
				}
			}

		}
		else if(lightTappedFlag && checkForLightTap()) {
			queuedStateTrigger = true;
			lightTappedFlag = false;
		}
		else if(drifter.canSpecialCancel() && !attacks.grabPressed() && attacks.specialPressed() && !status.HasEnemyStunEffect()) {
			attacks.useSpecial(true);
		}
		else
			attackWasCanceled = false;

		//ledgeDetector.UpdateFrame();

	}

	//Flag the character to begin listen for a given event
	public void listenForGrounded(string stateName) {
		queueState(stateName);
		listeningForGroundedFlag = true;
	}

	public void listenForSpecialTapped(string stateName) {
		queueState(stateName);
		specialTappedFlag = true;
	}

	 public void listenForLightTapped(string stateName) {
		queueState(stateName);
		lightTappedFlag = true;
	}

	public void listenForSpecialReleased(string stateName) {
		queueState(stateName);
		specialReleasedFlag = true;
	}

	public void listenForMovementCancel() {
		movementCancelFlag = true;
	}
	public void listenForVerticalCancel(string stateName) {
		queueState(stateName);
		verticalCancelFlag = true;
	}

	public void listenForActiveCancel() {
		activeCancelFlag = true;
	}

	public void listenForJumpCancel() {
		jumpFlag = true;
	}


	public void addCharge(int charge =1) {
		specialCharge += charge;
	}

	public void listenForLedge() {

		ledgeDetector.togglePreventWalkoff();
	}

	public void listenForLedge(bool toggle) {

		ledgeDetector.setPreventWalkoff(toggle);
	}

	//Allow for a special cancel on a move that would normally not be canceleable
	public void listenForSpecial() {
		drifter.canSpecialCancelFlag = true;
	}

	public void listenForDacus() {
		setXVelocity(movement.dashSpeed);
		if(movement.dashLock <=0)status.ApplyStatusEffect(PlayerStatusEffect.INVULN,3);
		dacusCancelFlag = true;
	}


	//Clear all flags
	public virtual void clearMasterhitVars() {
		specialTappedFlag = false;
		specialReleasedFlag = false;
		lightTappedFlag = false;
		verticalCancelFlag = false;
		movementCancelFlag = false;
		activeCancelFlag = false;
		listeningForGroundedFlag = false;
		queuedStateTrigger = false;
		jumpFlag = false;
		specialLimit = -1;
		queuedState = "";
		resetTerminalVelocity();
		ledgeDetector.setPreventWalkoff(false);
		dacusCancelFlag= false;
		knockdownFlag = false;
	}


	// Start is called before the first frame update
	void Awake() {
		Resources.LoadAll("/Characters/" + assetPathName);
		Resources.LoadAll("/Projectiles/" + assetPathName);

		//Parent Components

		drifter = transform.parent.gameObject.GetComponent<Drifter>();
		
		movement = drifter.movement;
		attacks = drifter.attacks;
		status = drifter.status;
		anim = drifter.animator;
		rb = movement.rb;
		ledgeDetector = GetComponentInChildren<WalkOff>();
	}

	//Populates hitbox array
	void Start() {
		HitboxCollision[] Hitboxes;
		//grabBoxes = drifter.GetComponentsInChildren<HitboxCollision>(true);
	}

	public void setYVelocity(float y) {
		rb.velocity = new Vector2(rb.velocity.x,y * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f));
		status.saveYVelocity(y);
	}

	public void setYVelocityIfGrounded(float y) {
		if(!movement.grounded)return;
		setYVelocity(y);
	}


	public void setXVelocity(float x) {

		if(movement.grounded && x >0) movement.spawnKickoffDust();
		rb.velocity = new Vector2(movement.Facing * x * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
		status.saveXVelocity(movement.Facing * x);
	}

	public void setXVelocityMin(float x) {

		if(movement.grounded && x >0) movement.spawnKickoffDust();
		rb.velocity = new Vector2(movement.Facing * Mathf.Max(x,Mathf.Abs(rb.velocity.x)) * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
		status.saveXVelocity(rb.velocity.x);
	}

	public void applyEndLag(int statusDuration) {
		status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,statusDuration);
	}

	public void applyArmour(int statusDuration) {
		status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,statusDuration);
	}

	public void setLandingCancel() {
		movement.canLandingCancel = true;
	}

	public void pauseGravity() {
		savingVelocity = false;
		movement.pauseGravity();
	}

	public void freezeGravity() {
		savedVelocity = new Vector3(Mathf.Clamp(rb.velocity.x,-45f,45f), Mathf.Clamp(rb.velocity.y,-45f,45f));
		savingVelocity = true;
		movement.cancelJump();
		movement.gravityPaused= true;
		rb.gravityScale = 0f;
		rb.velocity = Vector2.zero;
	}

	public void unpauseGravity() {
		if(savingVelocity)rb.velocity = savedVelocity;
		savingVelocity = false;
		movement.resetGravity();
		movement.gravityPaused= false;
	}

	public void refreshHitboxID() {
		attacks.SetMultiHitAttackID();
	}

	public void setTerminalVelocity(float speed) {
		movement.terminalVelocity = speed;
	}
	
	public void resetTerminalVelocity() {
		movement.resetTerminalVelocity();
	}

	//Dynamically adjust walk speed to match walk cycle animations
	public void walkCycleSpeedSync(float speed) {
		movement.walkSpeed = speed;
	}

	public void returnToIdle() {
		movement.jumping = false;
		unpauseGravity();
		status.clearVelocity();
		movement.terminalVelocity = movement.baseTerminalVelocity * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f);
		clearMasterhitVars();
		drifter.returnToIdle();
		movement.updateFacing();
		if(checkForJumpTap())movement.jump();
		//Still needed?
		attacks.UpdateFrame();
	}

	public void playState(string state) {
		drifter.PlayAnimation(state);
	}

	public void playStateIfEmpowered(string state) {
		if(Empowered)drifter.PlayAnimation(state);
	}

	public void playStateIfGrounded(string state) {
		if(movement.grounded)drifter.PlayAnimation(state);
	}

	public void playStateIfEmpoweredOrRetunToIdle(string state) {
		if(Empowered)drifter.PlayAnimation(state);
		else returnToIdle();
	}

	public bool checkForLightTap() {
		int state = 0;
		for(int i = 0; i < 8; i++) {
			if(state >0 && !drifter.input[i].Light)return true;
			else if(state == 0 && drifter.input[0].Light) state++;
		}
		return false;
	}

	public bool checkForJumpTap() {
		int state = 0;
		for(int i = 0; i < 15; i++) {
			if(state >0 && !drifter.input[i].Jump)return true;
			else if(state == 0 && drifter.input[i].Jump) state++;
		}
		return false;
	}

	//Returns the most common direction held over the last X inputs
	public float checkForDirection(int frames = 8) {
		float dir = 0;
		for(int i = 0; i < frames; i++) {
			dir += drifter.input[i].MoveX;
		}
		return dir !=0 ?dir:movement.Facing;
	}

	public bool checkForSpecialTap(int frames = 8) {
		int state = 0;
		for(int i = 0; i < frames; i++) {
			if(state >0 && !drifter.input[i].Special)return true;
			else if(state == 0 && drifter.input[0].Special) state++;
		}
		return false;

	}

	public void queueState(string stateName) {
		queuedState = stateName;
	}

	public void playQueuedState() {
		if(queuedState.Equals(""))return;
		playState(queuedState);
	}

	public void triggerQueuedState() {
		if(queuedState.Equals("") || !queuedStateTrigger)return;
		applyEndLag(480);
		playState(queuedState);
		clearMasterhitVars();
	}

	public void beginGuard() {
		applyEndLag(10);
		drifter.perfectGuarding = true;
	}

	public void endPerfectGuard() {
		drifter.perfectGuarding = false;
		if(drifter.guarding)drifter.PlayAnimation("Guard");
		listenForJumpCancel();
		listenForMovementCancel();
	}

	public void endParry() {
		drifter.parrying = false;
	}

	public void BounceParticle(float offset = 0) {
		if(!movement.grounded)return;
		movement.spawnJuiceParticle(transform.position + new Vector3(offset * movement.Facing,0,0), MovementParticleMode.Restitution);
	}

	public void blockFastFalling() {
		movement.canFastFall = false;
	}

	public void SetObjectColor(GameObject Obj) {
		foreach (Renderer renderer in Obj.GetComponentsInChildren<Renderer>(true))
			renderer.material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);
	}

	protected void FireAtTarget(GameObject projectile, Vector2 target, Vector2 source, float speed){

		float deltay = target.y- source.y;
		float deltax = target.x- source.x;
		float angle = Mathf.Atan2(deltay, deltax)*180 / Mathf.PI + (movement.Facing < 0 ?180:0);

		projectile.transform.rotation = Quaternion.Euler(0,0,angle);

		projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(speed * movement.Facing * Mathf.Cos((angle*movement.Facing* Mathf.PI)/180),speed *Mathf.Sin((angle*movement.Facing * Mathf.PI)/180));

	}

	public virtual void TriggerRemoteSpawn(int index) {

	}

	public virtual void TriggerOnHit(Drifter drifter, bool isProjectile, AttackHitType hitType) {

	}


	//Rollback
	//============================

	public abstract MasterhitRollbackFrame SerializeFrame();
	public abstract void DeserializeFrame(MasterhitRollbackFrame p_frame);

	//Takes a snapshot of the current frame to rollback to
	protected MasterhitRollbackFrame SerializeBaseFrame() {

		return new MasterhitRollbackFrame()  {
			Empowered = this.Empowered,
			SavedVelocity = savedVelocity,
			SavingVelocity = savingVelocity, 
			SpecialTappedFlag = specialTappedFlag,
			SpecialReleasedFlag = specialReleasedFlag,
			LightTappedFlag = lightTappedFlag,
			VerticalCancelFlag = verticalCancelFlag,
			MovementCancelFlag = movementCancelFlag,
			ActiveCancelFlag = activeCancelFlag,
			ListeningForGroundedFlag = listeningForGroundedFlag,
			QueuedStateTrigger = queuedStateTrigger,
			JumpFlag = jumpFlag,
			SpecialCharge = specialCharge,
			SpecialLimit = specialLimit,
			QueuedState = queuedState,
			AttackWasCanceled = attackWasCanceled,
			DacusCancelFlag = dacusCancelFlag,
			KnockdownFlag = knockdownFlag,
			
		};
	}

	//Rolls back the entity to a given frame state
	protected void DeserializeBaseFrame(MasterhitRollbackFrame p_frame) {

		Empowered = p_frame.Empowered; 
		savedVelocity = p_frame.SavedVelocity; 
		savingVelocity = p_frame.SavingVelocity; 
		specialTappedFlag = p_frame.SpecialTappedFlag; 
		specialReleasedFlag = p_frame.SpecialReleasedFlag; 
		lightTappedFlag = p_frame.LightTappedFlag; 
		verticalCancelFlag = p_frame.VerticalCancelFlag; 
		movementCancelFlag = p_frame.MovementCancelFlag; 
		activeCancelFlag = p_frame.ActiveCancelFlag; 
		listeningForGroundedFlag = p_frame.ListeningForGroundedFlag; 
		queuedStateTrigger = p_frame.QueuedStateTrigger; 
		jumpFlag = p_frame.JumpFlag; 
		specialCharge = p_frame.SpecialCharge; 
		specialLimit = p_frame.SpecialLimit; 
		queuedState = p_frame.QueuedState;
		attackWasCanceled = p_frame.AttackWasCanceled; 
		dacusCancelFlag = p_frame.DacusCancelFlag; 
		knockdownFlag = p_frame.KnockdownFlag; 

	}
}
