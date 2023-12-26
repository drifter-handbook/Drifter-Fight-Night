using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum DrifterAttackType {
	Null,
	Ground_Q_Side, Ground_Q_Down, Ground_Q_Up, Ground_Q_Neutral,
	Aerial_Q_Side, Aerial_Q_Down, Aerial_Q_Up, Aerial_Q_Neutral,
	W_Side, W_Down, W_Up, W_Neutral,
	E_Side, E_Air, Roll, Super_Cancel
}

[Serializable]
public class SingleAttack {
	public DrifterAttackType attack;
	public SingleAttackData attackData;
	public bool hasAirVariant;
}

public class PlayerAttacks : MonoBehaviour {

	public static Dictionary<DrifterAttackType, string> AnimatorStates = new Dictionary<DrifterAttackType, string>() {
		{ DrifterAttackType.Ground_Q_Neutral, "Ground_Neutral" },
		{ DrifterAttackType.Aerial_Q_Neutral, "Aerial_Neutral" },
		{ DrifterAttackType.W_Up, "W_Up" },
		{ DrifterAttackType.E_Side, "Grab_Ground"},
		{ DrifterAttackType.E_Air, "Grab_Air" },
		{ DrifterAttackType.W_Neutral, "W_Neutral" },
		{ DrifterAttackType.W_Side, "W_Side" },
		{ DrifterAttackType.W_Down, "W_Down" },
		{ DrifterAttackType.Roll, "Roll" },
		{ DrifterAttackType.Aerial_Q_Up, "Aerial_Up" },
		{ DrifterAttackType.Aerial_Q_Down, "Aerial_Down" },
		{ DrifterAttackType.Aerial_Q_Side, "Aerial_Side" },
		{ DrifterAttackType.Ground_Q_Up, "Ground_Up" },
		{ DrifterAttackType.Ground_Q_Down, "Ground_Down" },
		{ DrifterAttackType.Ground_Q_Side, "Ground_Side" },
		{ DrifterAttackType.Super_Cancel, "Super_Cancel" },
	};

	// get a new attack ID
	HitboxCollision[] hitboxes;

	// current attack ID and Type, used for outgoing attacks
	public int AttackID { get;  set; }
	public DrifterAttackType AttackType { get;  set; }
	public List<SingleAttack> AttackMap = new List<SingleAttack>();

	public int AttackFrameDelay = 0;

	public Dictionary<DrifterAttackType,SingleAttackData> Attacks = new Dictionary<DrifterAttackType,SingleAttackData>();
	public Dictionary<DrifterAttackType,bool> AttackVariants = new Dictionary<DrifterAttackType,bool>();
   
	//[Help("Declares if any specials other than Up-W consume and require a recovery charge", UnityEditor.MessageType.Info)]
	public bool W_Neutral_Is_Recovery = false;
	public bool W_Down_Is_Recovery = false;
	public bool W_Side_Is_Recovery = false;

	public int maxRecoveries = 1;

	public bool shareRecoveries = false;

	[NonSerialized]
	public static int nextID = 0;

	public int NextID { get { nextID++;if(nextID>63)nextID=0; return nextID; } set{ nextID = value;}}
	[NonSerialized]
	public int currentUpRecoveries;
	[NonSerialized]
	public int currentDownRecoveries;
	[NonSerialized]
	public int currentSideRecoveries;
	[NonSerialized]
	public int currentNeutralRecoveries;

	Drifter drifter;

	void Awake() {
		foreach (SingleAttack attack in AttackMap) {
			Attacks[attack.attack] = attack.attackData;
			AttackVariants[attack.attack] = attack.hasAirVariant;

		}
	}

	// Start is called before the first frame update
	void Start() {
		drifter = GetComponent<Drifter>();
		currentUpRecoveries = maxRecoveries;
		currentDownRecoveries = maxRecoveries;
		currentSideRecoveries = maxRecoveries;
		currentNeutralRecoveries = maxRecoveries;

		hitboxes = GetComponentsInChildren<HitboxCollision>();
	}

	// Update is called once per frame
	public void UpdateFrame() {

		if (GameController.Instance.IsPaused)
			return;

		if(drifter.input[0].Pause && ! drifter.input[1].Pause) NetworkPlayers.Instance.rollemback();

		if(AttackFrameDelay > 0 ) {
			AttackFrameDelay--;
			if(AttackFrameDelay == 0) {
				foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
					hitbox.isActive = true;
			}
		}

		bool canAct = !drifter.status.HasStunEffect() && !drifter.guarding && !drifter.movement.ledgeHanging;
		bool canSpecial = !drifter.status.HasStunEffect() && !drifter.movement.ledgeHanging;

		if((drifter.movement.grounded && !drifter.status.HasStatusEffect(PlayerStatusEffect.END_LAG)) || drifter.status.HasEnemyStunEffect()) resetRecovery();
		
		if(superPressed())  drifter.movement.superCancel();
		
		else if (grabPressed() && canAct) useGrab();

		else if(specialPressed() && canSpecial) useSpecial();

		else if (lightPressed() && canAct) useNormal();
	}

	public void useSpecial(bool isCancel = false) {

		if(isCancel) drifter.movement.setFacingDelayed((int)drifter.masterhit.checkForDirection(8));

		if(drifter.input[0].MoveY > 0 && currentUpRecoveries > 0) {
				StartAttack(DrifterAttackType.W_Up);
				if(shareRecoveries)
					decrementAllRecoveries();
				else
					currentUpRecoveries--;
		}
		else if((!W_Down_Is_Recovery || currentDownRecoveries > 0) && drifter.input[0].MoveY < 0) {
			StartAttack(DrifterAttackType.W_Down);
			if(W_Down_Is_Recovery)
				if(shareRecoveries)
					decrementAllRecoveries();
				else
					currentDownRecoveries--;
		}
		else if((!W_Side_Is_Recovery || currentSideRecoveries > 0) &&drifter.input[0].MoveX!=0) {
			StartAttack(DrifterAttackType.W_Side);
			if(W_Side_Is_Recovery)
				if(shareRecoveries)
					decrementAllRecoveries();
				else
					currentSideRecoveries--;
		}
		else if((!W_Neutral_Is_Recovery || currentNeutralRecoveries > 0) && drifter.input[0].MoveY==0 && drifter.input[0].MoveX==0) {
			StartAttack(DrifterAttackType.W_Neutral);
			if(W_Neutral_Is_Recovery)
				if(shareRecoveries)
					decrementAllRecoveries();
				else
					currentNeutralRecoveries--;
		}
		else
			return;

		drifter.movement.canLandingCancel = false;

		if(isCancel) {
			AttackFrameDelay = 4;
			drifter.status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE, 2);
			drifter.masterhit.clearMasterhitVars();
			drifter.canFeint = true;
			drifter.movement.techParticle();
			drifter.canSpecialCancelFlag = false;
		}
	}

	public void useNormal() {
		drifter.canSpecialCancelFlag = true;

		if (drifter.movement.grounded) {
			if(drifter.input[0].MoveY > 0)StartAttack(DrifterAttackType.Ground_Q_Up);
			else if(drifter.input[0].MoveY < 0)StartAttack(DrifterAttackType.Ground_Q_Down);
			else if(drifter.input[0].MoveX!=0)StartAttack(DrifterAttackType.Ground_Q_Side);
			else StartAttack(DrifterAttackType.Ground_Q_Neutral);
		}
		else
		{   
			drifter.movement.canLandingCancel = true;    
			if(drifter.input[0].MoveY > 0)StartAttack(DrifterAttackType.Aerial_Q_Up);
			else if(drifter.input[0].MoveY < 0)StartAttack(DrifterAttackType.Aerial_Q_Down);
			else if(drifter.input[0].MoveX!=0)StartAttack(DrifterAttackType.Aerial_Q_Side);
			else StartAttack(DrifterAttackType.Aerial_Q_Neutral);
		}
	}

	public void useSuper() {
		drifter.SetUsingSuper(true);
		StartAttack(DrifterAttackType.Super_Cancel);
		drifter.movement.pauseGravity();
	}

	public void useGrab() {
		if (drifter.movement.grounded)StartAttack(DrifterAttackType.E_Side);
		else
		{
			drifter.movement.canLandingCancel = true;
			StartAttack(DrifterAttackType.E_Air);  
		} 
	}

	public bool lightPressed() {
		bool _lightPressed = false;
		for (int i = 0; i < drifter.input.Length - 3; i++)
			if(!_lightPressed) _lightPressed = !drifter.input[i+2].Light && drifter.input[i+1].Light && drifter.input[i].Light;
			else return _lightPressed;
		return _lightPressed;
	}
	public bool specialPressed() {
		bool _specialPressed = false;
		for (int i = 0; i < drifter.input.Length - 3; i++)
		   if(!_specialPressed) _specialPressed = !drifter.input[i + 2].Special &&  drifter.input[i + 1].Special && drifter.input[i].Special;
		   else return _specialPressed;

		return _specialPressed;
	}
	public bool grabPressed() {
		bool _grabPressed = false;
		for (int i = 0; i < drifter.input.Length - 3; i++) {
			if(!_grabPressed) _grabPressed = ((drifter.input[i].Light || drifter.input[i + 1].Light) && !drifter.input[i + 2].Light &&
					(drifter.input[i].Special || drifter.input[i + 1].Special) && !drifter.input[i+ 2].Special)
					|| (!drifter.input[i + 2].Grab && drifter.input[i + 1].Grab && drifter.input[i].Grab);
			else return _grabPressed;
		}
		return _grabPressed;
	}

	public bool superPressed() {
		bool _superPressed = false;
		for (int i = 0; i < drifter.input.Length - 3; i++)
			if(!_superPressed) _superPressed = !drifter.input[i+2].Super && drifter.input[i+1].Super && drifter.input[i].Super;
			else return _superPressed;

		return _superPressed;
	}

	void decrementAllRecoveries() {
		currentUpRecoveries--;
		currentDownRecoveries--;
		currentSideRecoveries--;
		currentNeutralRecoveries--;
	}

	public void resetRecovery(){
		currentUpRecoveries = maxRecoveries;
		currentDownRecoveries = maxRecoveries;
		currentSideRecoveries = maxRecoveries;
		currentNeutralRecoveries = maxRecoveries;
	}

	public void StartAttack(DrifterAttackType attackType, int frameDelay = 2) {
		drifter.status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,0);
		drifter.gainSuperMeter(5);
		drifter.movement.jumping = false;
		drifter.status?.ApplyStatusEffect(PlayerStatusEffect.END_LAG,480);
		if(!AttackVariants[attackType])
			drifter.PlayAnimation(AnimatorStates[attackType]);
		else if(drifter.movement.grounded)
			drifter.PlayAnimation(AnimatorStates[attackType] + "_Ground");
		else
			drifter.PlayAnimation(AnimatorStates[attackType] + "_Air");
		//Delay setting Attack key for 3 frames for special cancels
		AttackFrameDelay = frameDelay;
		AttackType = attackType;
		SetupAttackID(attackType);
		//UnityEngine.Debug.Log("STARTING ATTACK: " + attackType.ToString() + "  With attackID: " + AttackID);

	}

	public SingleAttackData GetCurrentAttackData() {
		return Attacks[AttackType];
	}

	public void SetupAttackID(DrifterAttackType attackType) {
		//AttackType = attackType;
		AttackID = NextID;
		foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.AttackID = AttackID;
			hitbox.isActive = false;
			hitbox.Facing = drifter.movement.Facing;
		}
	}
	// called by hitboxes during attack animation
	// reset attack ID, allowing for another hit in the same attack animation
	public void SetMultiHitAttackID() {
		AttackID = NextID;
		foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.AttackID = AttackID;
			hitbox.isActive = true;
		}
	}

	public AttackRollbackFrame SerializeFrame() {
		HitboxRollbackFrame[] HitboxFrames = new HitboxRollbackFrame[hitboxes.Length];
		//Searialize each hitbox
		for(int i = 0; i < hitboxes.Length; i++) {
			HitboxFrames[i] = hitboxes[i].SerializeFrame();
		}

		return new AttackRollbackFrame() {
			AttackID = this.AttackID,
			nextID = PlayerAttacks.nextID,
			AttackType = this.AttackType,
			CurrentUpRecoveries = currentUpRecoveries,
			CurrentDownRecoveries = currentDownRecoveries,
			CurrentSideRecoveries = currentSideRecoveries,
			CurrentNeutralRecoveries = currentNeutralRecoveries,
			//Hitboxes = HitboxFrames
		};
	}

	//Rolls back the entity to a given frame state
	public  void DeserializeFrame(AttackRollbackFrame p_frame) {
			AttackID = p_frame.AttackID;
			nextID = p_frame.nextID;
			AttackType = p_frame.AttackType;
			currentUpRecoveries = p_frame.CurrentUpRecoveries;
			currentDownRecoveries = p_frame.CurrentDownRecoveries;
			currentSideRecoveries = p_frame.CurrentSideRecoveries;
			currentNeutralRecoveries = p_frame.CurrentNeutralRecoveries;

			// for(int i = 0; i < p_frame.Hitboxes.Length; i++) {
			// 	hitboxes[i].DeserializeFrame(p_frame.Hitboxes[i]);
			// }

	}
}

public class AttackRollbackFrame: INetworkData
{
	public string Type { get; set; }
	public int AttackID;
	public int nextID;
	public DrifterAttackType AttackType;
	public int CurrentUpRecoveries;
	public int CurrentDownRecoveries;
	public int CurrentSideRecoveries;
	public int CurrentNeutralRecoveries;
	//public HitboxRollbackFrame[] Hitboxes;
}