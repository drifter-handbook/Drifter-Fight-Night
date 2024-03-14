using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public enum DrifterType
{
	None,
	Random,
	Sandbag,
	Bojo,
	Swordfrog,
	Lady_Parhelion,
	Spacejam,
	Orro,
	Ryyke,
	Megurin,
	Nero,
	Lucille,
	Mytharius,
	Maryam,
	Drifter_Cannon,
	//Klatz,
	//Eldaris,
	//Reed,
	//Bytor,
	//Dyo,
	//Ramstein,
	//Tai,
	//Tasma,
	//Sola,
	//Oono,

}

/** 
 * This is the class that will be put into a prefab and instantiated 
 */
[RequireComponent(typeof(PlayerMovement))]
[RequireComponent(typeof(PlayerStatus))]
[RequireComponent(typeof(PlayerAttacks))]
public class Drifter : MonoBehaviour
{
	//Static values durring gameplay
	public PlayerMovement movement;
	public PlayerStatus status;
	public PlayerAttacks attacks;
	public MasterHit masterhit;
	public PlayerHurtboxHandler hurtbox;
	public InstantiatedEntityCleanup entity;
	
	public Animator animator;
	public AnimatorOverrideController[] animOverrides;
	public Animator sparkle;
	public PlayerInput playerInputController;


	public DrifterType drifterType;

	public GameObject ParticlePoint;

	[NonSerialized]
	public int myColor;
	[NonSerialized]
	public int peerID;
	
	//Input Buffer
	public PlayerInputData[] input;

	// [NonSerialized]
	[NonSerialized]
	public bool guarding = false;
	[NonSerialized]
	public bool canFeint = true;
	[NonSerialized]
	public bool knockedDown = false;
	[NonSerialized]
	public bool canSpecialCancelFlag = false; //True when a move has connected but the player has not yet canceled their move
	[NonSerialized]
	public bool hiddenFlag = false;
	[NonSerialized]
	public int superCharge = 200;
	[NonSerialized]
	public bool sparkleMode = false;
	[NonSerialized]
	public bool usingSuper = false;
	[NonSerialized]
	public bool CanGrabLedge = true;
	[NonSerialized]
	public bool enforceFullDistance = false;
	[NonSerialized]
	public AttackHitType lastHitType = AttackHitType.NONE;

	public int Stocks;
	public float DamageTaken;
	public int inspirationCharges = 3;
	
	private int overrideIndex = 0; 
	private int cancelTimer = 0;
	private bool _canSpecialCancel = false;

	private bool isDummy = true;

	//Used to prevent certain animation events from firing if another animation was set to play in the current frame
	//We have to do thsi becasue of the timing in whcih Unity processes animation events relative to the update loop
	//Becasue of this ordering, we need to set this to 2 if the trigger is on a physics collision, as it will tick down before once before the event is fired
	//For DFN:
	//Physics -> Queue animation events -> Fixed Update -> Fire animation events
	[NonSerialized]
	public int blockEvent = 0;

	//Cancel Normals into Specials Logic
	public bool listenForSpecialCancel
	{
		get{
			return _canSpecialCancel;
		}
		set{
			_canSpecialCancel = value;
			cancelTimer = _canSpecialCancel ? 18:0;
		}
	}

	public void Awake(){
		masterhit = GetComponentInChildren<MasterHit>();
		Stocks = !GameController.Instance.IsTraining ? 4:9999;
		DamageTaken = 0f;

		if(animOverrides != null && animOverrides.Length > 0)animOverrides[0] = new AnimatorOverrideController(animator.runtimeAnimatorController);
	}

	public bool canSpecialCancel() {
		return (canSpecialCancelFlag && listenForSpecialCancel && cancelTimer > 0);
	}

	//Returns the character's outline color as an int
	public int GetColor() { 
		return myColor;
	}

	//Sets the character's super charge to a given value
	public void SetCharge(int newCharge) { 
		if(superCharge != newCharge) superCharge=newCharge;
		//gameObject.GetComponent<SyncChargeHost>().setCharge(superCharge);
	}

	//Grants the character additonal charge for their super meter, up to the cap of 5 bars
	public void gainSuperMeter(int charge) {
		superCharge += charge;
		if(superCharge >500) superCharge = 500;
	}


	//Stes Peerid for networking
	public void SetPeerId(int id){
		peerID = id;
	}

	//Sets the character's outline color
	public void SetColor(int colorID) {
		myColor = (colorID>=0?colorID:0);
		transform.GetChild(0).GetComponent<SpriteRenderer>().color = CharacterMenu.ColorFromEnum[(PlayerColor)myColor];
		transform.GetChild(2).GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)myColor]);
		transform.GetChild(0).GetComponent<Animator>().Play( (colorID < 8)?"P" + (colorID + 1):"P9");
	}

	//Makes the player sprite invisble
	public void toggleHidden(bool hidden) {
		if(hiddenFlag == hidden) return;
		hiddenFlag = hidden;
		SpriteRenderer render = transform.GetChild(2).GetComponent<SpriteRenderer>();
		Color newColor = render.color;
		newColor.a = hidden?0:1;
		render.color = newColor;
	}

	//Flips text-based objects attacked to characters to keep them readable as the character turns
	public void SetIndicatorDirection(float facing) {
		transform.GetChild(0).localScale = new Vector2(Mathf.Abs(transform.GetChild(0).localScale.x) * facing, transform.GetChild(0).transform.localScale.y);
		//transform.GetChild(3).localScale = new Vector2(Mathf.Abs(transform.GetChild(3).localScale.x) * facing,transform.GetChild(3).localScale.y);
	}


	public void Sparkle(bool p_mode) {
		if(sparkleMode ==p_mode) return;

		sparkleMode = p_mode;
		sparkle.Play(p_mode?"ChargeIndicator":"Hide");
	}

	public void die(){
		if(status.isDead()) return;
		Stocks--;
        DamageTaken = 0f;
        superCharge = 200;
        status.ApplyStatusEffect(PlayerStatusEffect.DEAD, 120);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN, 420);
        transform.position = new Vector2(0f, 150f);
	}

	//Replaces the animator state transition function
	public void PlayAnimation(string p_state, float p_normalizedTime = -1, bool p_gate = false, int eventBlockTime = 2) {

		if(!p_gate || Animator.StringToHash(p_state) != animator.GetCurrentAnimatorStateInfo(0).shortNameHash) {
			animator.Play(Animator.StringToHash(p_state),0,p_normalizedTime < 0 ? 0: p_normalizedTime);
			blockEvent = eventBlockTime;
		}
	}


	public void SetAnimationOverride(int p_index) {
		if(animOverrides == null || animOverrides.Length < p_index+1) {
			return;
		}
		animator.runtimeAnimatorController = animOverrides[p_index];
		overrideIndex = p_index;
	}

	public void SetAnimationSpeed(float p_speed) {
		animator.speed = p_speed;
	}

	public void ToggleAnimator(bool p_mode) {
		animator.enabled = p_mode;
	}

	public void TriggerOnHit(Drifter drifterHit, bool isProjectile, AttackHitType hitType){
		lastHitType = hitType;
		//UnityEngine.Debug.Log(drifterType + " Hit a target");
		masterhit.TriggerOnHit(drifterHit,isProjectile, hitType);
	}

	//Return to idle is called anytime the player regains control
	public void returnToIdle() {
		//if(isTrainingDummy()) UnityEngine.Debug.Log("DRIFTER: RETURNING TO IDLE");
		movement.canLandingCancel = false;
		movement.jumping = false;
		movement.dashing = false;
		movement.canFastFall = true;
		SetUsingSuper(false);
		canFeint = true;
		clearGuardFlags();
		if(movement.grounded && input[0].MoveX !=0)PlayAnimation("Walk");
		else if(movement.grounded)PlayAnimation("Idle");
		else if(movement.ledgeHanging)PlayAnimation("Ledge_Grab");
		else PlayAnimation("Hang");
		if(status.HasStatusEffect(PlayerStatusEffect.END_LAG)) status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0);
		if(status.HasStatusEffect(PlayerStatusEffect.FLATTEN)) status.ApplyStatusEffect(PlayerStatusEffect.FLATTEN,0);
		if(status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN))  status.ApplyStatusEffect(PlayerStatusEffect.KNOCKDOWN,0);
		if(status.HasStatusEffect(PlayerStatusEffect.TUMBLE))  status.ApplyStatusEffect(PlayerStatusEffect.TUMBLE,0);
		if(status.HasStatusEffect(PlayerStatusEffect.SUPERBLOCKED))  status.ApplyStatusEffect(PlayerStatusEffect.SUPERBLOCKED,0);
		movement.resetTerminalVelocity();
		canSpecialCancelFlag = false;
		listenForSpecialCancel = false;     
		knockedDown = false;
		CanGrabLedge = true;
		enforceFullDistance = false;
		lastHitType = AttackHitType.NONE;
		if(transform.position.z != -1) transform.position = new Vector3(transform.position.x,transform.position.y,-1);
		masterhit.clearMasterhitVars();

		if(input[0].Guard && !movement.ledgeHanging) {
			guarding = true;
			masterhit.listenForActiveCancel();
			PlayAnimation(movement.hitstun?"Guard":"Guard_Start");
		}
		movement.hitstun = false;
	}

	public void guard(){
		masterhit.listenForActiveCancel();
		if(!guarding)
			PlayAnimation("Guard_Start");
		guarding = true;
	}

	public void SetUsingSuper(bool SuperState) {
		usingSuper = SuperState;
		entity.pauseBehavior = !SuperState;
		//Unpause the animatior if using a super
		ToggleAnimator(SuperState);
		if(SuperState){
			movement.DropLedge(false,0);
			CanGrabLedge = false;
			masterhit.pauseGravity();
			masterhit.clearMasterhitVars();
		}
	}

	public bool CanUseSuper(){
		return (!entity.paused && !usingSuper && !status.HasSuperBlockingEffect());
	} 

	//Clears all flags associated with guard state
	public void clearGuardFlags() {
		guarding = false;
		// parrying = false;
		// perfectGuarding = false;
	}

	public GameObject createParticleEffector(string name){
		GameObject effector = GameController.Instance.CreatePrefab(name, ParticlePoint.transform.position, transform.rotation,peerID);
		effector.transform.SetParent(transform);

		return effector;
	}

	//Training mode functions
	public bool isTrainingDummy(){
		return isDummy;
	}
	public void setTrainingDummy(bool dummy){
		isDummy = dummy;
	}

	public bool hasActiveHitbox(){
		Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
		foreach( Collider2D col in colliders){
			if(col.enabled) return true;
		}
		return false;
	}

	//Command Input Detection

	//Detects if the character double tapped the X directional key
	public bool doubleTappedX() {

		if(input[0].MoveX ==0)return false;

		int state = 0;

		for(int i = 1; i < input.Length-6; i++) {
			if(state ==0 && input[i].MoveX == 0)
				state++;
			else if(state == 1 && input[i].MoveX == -1 * input[0].MoveX)
				return false;
			else if(state == 1 && input[i].MoveX == input[0].MoveX)
				return true;
		}

		return false;
	}

	//Detects if the character double tapped the Y directional key
	public bool doubleTappedY() {
		if(input[0].MoveY ==0)return false;

		int state = 0;

		for(int i = 1; i < input.Length-6; i++) {

			if(state ==0 && input[i].MoveY == 0)
				state++;
			else if(state == 1 && input[i].MoveY == -1 * input[0].MoveY) return false;

			else if(state == 1 && input[i].MoveY == input[0].MoveY)
				return true;
		}

		return false;
	}

	//Detects if the character executed a Quater Circle motion
	public bool qcf() {
		
		if(input[0].MoveX ==0 || input[0].MoveY !=0 )return false;

		int state = 0;
		for(int i = 1; i < input.Length; i++) {
			if(state ==0 && input[i].MoveY == -1 && input[i].MoveX == input[0].MoveX)
				state++;
			else if(state == 1 && input[i].MoveY == 0 && input[i].MoveX == input[0].MoveX) 
				state--;

			else if(state == 1 && input[i].MoveY == -1 && input[i].MoveX ==0)
				return true;


		}
		return false;
	}

	public static DrifterType DrifterTypeFromString(String drfiterString){
		return (DrifterType)Enum.Parse(typeof(DrifterType), drfiterString.Replace(" ", "_"));
	}


	//Rollback
	//====================================

	//Takes a snapshot of the current frame to rollback to
	public void Serialize(BinaryWriter bw) {

		//Input buffer
		foreach(PlayerInputData inputFrame in input)
			inputFrame.Serialize(bw);
		

		//Bools
		bw.Write(canFeint);
		bw.Write(_canSpecialCancel);
		bw.Write(guarding);
		bw.Write(hiddenFlag);
		bw.Write(knockedDown);
		bw.Write(listenForSpecialCancel);
		bw.Write(sparkleMode);
		bw.Write(usingSuper);

		//Ints
		bw.Write(overrideIndex);
		bw.Write(blockEvent);
		bw.Write(cancelTimer);
		bw.Write(inspirationCharges);
		bw.Write(Stocks);
		bw.Write(superCharge);

		//Floats
		bw.Write(animator.speed);
		bw.Write(DamageTaken);

		//Children
		entity.Serialize(bw);
		status.Serialize(bw);
		movement.Serialize(bw);
		attacks.Serialize(bw);
		hurtbox.Serialize(bw);
		masterhit.Serialize(bw);
	}

	//Rolls back the entity to a given frame state
	public void Deserialize(BinaryReader br) {

		//input buffer
		foreach(PlayerInputData inputFrame in input)
			inputFrame.Deserialize(br);
		
		//Bools
		canFeint = br.ReadBoolean();
		_canSpecialCancel = br.ReadBoolean();
		guarding = br.ReadBoolean();
		hiddenFlag = br.ReadBoolean();
		knockedDown = br.ReadBoolean();
		listenForSpecialCancel = br.ReadBoolean();
		sparkleMode = br.ReadBoolean();
		usingSuper = br.ReadBoolean();

		//Ints
		overrideIndex = br.ReadInt32();
		blockEvent = br.ReadInt32();
		cancelTimer = br.ReadInt32();
		inspirationCharges = br.ReadInt32();
		Stocks = br.ReadInt32();
		superCharge = br.ReadInt32();

		//Floats
		animator.speed = br.ReadSingle();
		DamageTaken = br.ReadSingle();

		//Children
		entity.Deserialize(br);
		status.Deserialize(br);
		movement.Deserialize(br);
		attacks.Deserialize(br);
		hurtbox.Deserialize(br);
		masterhit.Deserialize(br);
		
		
	}

	public void UpdateFrame() {

		if(GameController.Instance.IsPaused)
            return;

		if(cancelTimer >0) {
			cancelTimer--;
			if(cancelTimer <=0) {
				cancelTimer = 0;
				listenForSpecialCancel = false;
				//canSpecialCancelFlag = false;
			}
		}

		if(blockEvent > 0) blockEvent--;
		
		entity.UpdateFrame();
		//Do not update components if in super freeze
		if(!entity.paused) {
			status.UpdateFrame();
			movement.UpdateFrame();
			attacks.UpdateFrame();
			hurtbox.UpdateFrame();
			masterhit.UpdateFrame();
		}
		
		
	}
}
