using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeoParhelionMasterHit : MasterHit {
	
	string staticBurstTarget = "";
	int staticBurstTimer = 0;
	int numBursts = -1;
	int staticCycles = 0;

	InstantiatedEntityCleanup staticField;
	InstantiatedEntityCleanup[] aftershocks = new InstantiatedEntityCleanup[5];

	GameObject dashTrail;

	const int MAX_STATIC_CHARGE_DURATION = 400;

	//Inhereted Roll Methods
	public GrabHitboxCollision Up_W_Grab;

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(drifter.status.HasEnemyStunEffect() || movement.ledgeHanging) {
			deleteStaticField();
			Remove_Dash_Trail();
		}

		if(drifter.status.HasEnemyStunEffect()) {
			staticBurstTimer = 0;
			staticBurstTarget = "";
			staticCycles = 0;
		}

		if(staticBurstTimer > 0 && !status.HasStatusEffect(PlayerStatusEffect.HITPAUSE) && !drifter.usingSuper && staticBurstTarget != "") {
			staticBurstTimer--;
			GameObject staticBurstTargetObject = GameObject.Find(staticBurstTarget);
			if(staticBurstTargetObject != null) {
				if(staticBurstTimer == 0) {
					if(numBursts > 1) {
						numBursts--;
						staticBurstTimer = 4;

						Create_Aftershock(staticBurstTargetObject.transform.position + new Vector3(1.75f *Mathf.Cos(numBursts/5f), 1.75f *Mathf.Sin(numBursts/5f)), numBursts);
					}
					else
						Create_Aftershock(staticBurstTargetObject.transform.position, numBursts);
				}
			}
			else
				staticBurstTarget = "";	
		}

		foreach(InstantiatedEntityCleanup aftershock in aftershocks)
			aftershock?.UpdateFrame();

		staticField?.UpdateFrame();

		ledgeDetector.UpdateFrame();
	}

	public void Create_Static_Field(int launcher) {
		deleteStaticField();
		GameObject projectile;
		projectile = GameController.Instance.CreatePrefab("Parhelion_Static", transform.position + new Vector3(0,2f), transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		SetObjectColor(projectile);
		projectile.transform.SetParent(drifter.gameObject.transform);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		staticCycles++;

		staticField = projectile.GetComponent<InstantiatedEntityCleanup>();
		if(launcher != 0)
			staticField.PlayAnimation("Parhelion_Static_End");
	}

	private void Create_Aftershock(Vector2 pos, int index) {
		GameObject projectile = GameController.Instance.CreatePrefab("Parhelion_Burst", pos, transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		SetObjectColor(projectile);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		aftershocks[index] = projectile.GetComponent<InstantiatedEntityCleanup>();;
	}

	public void Loop_W_Down() {
		if(!status.HasStatusEffect(PlayerStatusEffect.ELECTRIFIED)) {
			status.AddStatusBar(PlayerStatusEffect.ELECTRIFIED, MAX_STATIC_CHARGE_DURATION);
			status.AddStatusDuration(PlayerStatusEffect.ELECTRIFIED, 99, MAX_STATIC_CHARGE_DURATION);
		}
		else status.AddStatusDuration(PlayerStatusEffect.ELECTRIFIED, 100, MAX_STATIC_CHARGE_DURATION);

		if(!drifter.input[0].Special || staticCycles >3) {
			staticCycles = 0;
			playState("W_Down_End");
		}
	}

	public void Create_Dash_Trail() {
		dashTrail = drifter.createParticleEffector("PARHELION_DASH_Particle");
		GameObject projectile = GameController.Instance.CreatePrefab("Parhelion_Dash", transform.position, transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
	}

	public void Remove_Dash_Trail() {
		if(dashTrail == null) return;
		dashTrail.GetComponent<ParticleSystemController>().Cleanup();
		dashTrail = null;
	}

	private void deleteStaticField() {
		if(staticField != null) {
			Destroy(staticField.gameObject);
			staticField = null;
		}
	}

	public override void TriggerOnHit(Drifter target_drifter, bool isProjectle, AttackHitType hitType) {
		
		if(isProjectle || (hitType != AttackHitType.HIT && hitType != AttackHitType.BLOCK) || !status.HasStatusEffect(PlayerStatusEffect.ELECTRIFIED) || staticCycles >0)return;
		//If a burst is already charging, reset timer instead and dont consume moe juice
		if(staticBurstTimer >0)	{
			staticBurstTimer = 8;
			return;
		}
		if(hitType == AttackHitType.BLOCK) {
			numBursts = 1;
			status.AddStatusDuration(PlayerStatusEffect.ELECTRIFIED, -100);
		}
		else {
			numBursts = status.remainingDuration(PlayerStatusEffect.ELECTRIFIED)/100;
			status.ApplyStatusEffect(PlayerStatusEffect.ELECTRIFIED,0);
		}
		staticBurstTimer = 8;
		staticBurstTarget = target_drifter.gameObject.name;
	}

	public new void returnToIdle() {
		base.returnToIdle();
		//Up_W_Grab.victim = null;
		deleteStaticField();
		staticCycles = 0;
	}

	public override void clearMasterhitVars() {
		base.clearMasterhitVars();
		deleteStaticField();
		staticCycles = 0;
		Remove_Dash_Trail();
		//staticBurstTarget = "";
		//numBursts = 0;
	}

	//Flips the direction the charactr is facing mid move)
	public void invertDirection() {
		movement.flipFacing();
	}

	public void dust() {
		if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(4f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		bw.Write(staticBurstTimer);
		bw.Write(numBursts);
		bw.Write(staticCycles);

		bw.Write(staticBurstTarget);

		for(int i = 0; i < aftershocks.Length; i++) {
			if(aftershocks[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				aftershocks[i].Serialize(bw);
			}
		}

		if(staticField == null)
			bw.Write(false);
		else{
			bw.Write(true);
			staticField.Serialize(bw);
		}

	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		base.Deserialize(br);

		staticBurstTimer = br.ReadInt32();
		numBursts = br.ReadInt32();
		staticCycles = br.ReadInt32();

		staticBurstTarget = br.ReadString();

		for(int i = 0; i < aftershocks.Length; i++) {
			if(br.ReadBoolean()) {
				if(aftershocks[i] == null) Create_Aftershock(transform.position, i);
				aftershocks[i].Deserialize(br);
			}
			else if(aftershocks[i] != null) {
				Destroy(aftershocks[i].gameObject);
				aftershocks[i] = null;
			}
		}

		if(br.ReadBoolean()) {
			if(staticField == null) Create_Static_Field(0);
			staticField.Deserialize(br);
		}
		else if(staticField != null) {
			Destroy(staticField.gameObject);
			staticField = null;
		}

	}

}
