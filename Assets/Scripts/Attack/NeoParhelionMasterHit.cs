using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoParhelionMasterHit : MasterHit
{
	GameObject g_staticField;
	GameObject g_burst;
	Drifter staticBurstTarget = null;
	int staticBurstTimer = 0;
	int numBursts = -1;
	int staticCycles = 0;
	bool onBlock = false;

	GameObject zap;

	GameObject dashTrail;

	const int MAX_STATIC_CHARGE_DURATION = 400;

	//Inhereted Roll Methods
	public GrabHitboxCollision Up_W_Grab;

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(drifter.status.HasEnemyStunEffect() || movement.ledgeHanging){
			staticBurstTarget = null;
			deleteStaticField();
			staticCycles = 0;
			Remove_Dash_Trail();
		}

		if(drifter.status.HasEnemyStunEffect()){
			staticBurstTimer = 0;
		}

		if(staticBurstTimer > 0 && !status.HasStatusEffect(PlayerStatusEffect.HITPAUSE) && !drifter.usingSuper && staticBurstTarget != null){
			staticBurstTimer--;
			if(staticBurstTimer == 0){
				if(numBursts > 1){
					numBursts--;
					staticBurstTimer = 4;
					Create_Burst(staticBurstTarget.gameObject.transform.position + new Vector3(Random.Range(-1.75f, 1.75f), Random.Range(-1.75f, 1.75f)));
				}
				else
					Create_Burst(staticBurstTarget.gameObject.transform.position);
			}
		}

		if(g_burst != null) g_burst.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
		if(g_staticField!= null) g_staticField.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
	}

	public void Create_Static_Field(int launcher){
		deleteStaticField();
		GameObject projectile;
		projectile = GameController.Instance.CreatePrefab("Parhelion_Static", transform.position + new Vector3(0,2f), transform.rotation);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		SetObjectColor(projectile);
		projectile.transform.SetParent(drifter.gameObject.transform);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		staticCycles++;

		if(launcher != 0)
			projectile.GetComponent<InstantiatedEntityCleanup>().PlayAnimation("Parhelion_Static_End");

		g_staticField = projectile;
	}

	private void Create_Burst(Vector2 pos){
		GameObject projectile;
		projectile = GameController.Instance.CreatePrefab("Parhelion_Burst", pos, transform.rotation);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		SetObjectColor(projectile);



		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		g_burst = projectile;
	}

	public void Loop_W_Down(){

		if(!status.HasStatusEffect(PlayerStatusEffect.ELECTRIFIED)){
			status.AddStatusBar(PlayerStatusEffect.ELECTRIFIED, MAX_STATIC_CHARGE_DURATION);
			status.AddStatusDuration(PlayerStatusEffect.ELECTRIFIED, 99, MAX_STATIC_CHARGE_DURATION);

		}
		else status.AddStatusDuration(PlayerStatusEffect.ELECTRIFIED, 100, MAX_STATIC_CHARGE_DURATION);

		if(!drifter.input[0].Special || staticCycles >3){
			staticCycles = 0;

			playState("W_Down_End");
		}
	}

	public void Create_Dash_Trail() {
		dashTrail = drifter.createParticleEffector("PARHELION_DASH_Particle");
		GameObject projectile = GameController.Instance.CreatePrefab("Parhelion_Dash", transform.position, transform.rotation);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

	}

	public void Remove_Dash_Trail() {
		if(dashTrail == null) return;
		dashTrail.GetComponent<ParticleSystemController>().Cleanup();
		dashTrail = null;
	}



	private void deleteStaticField() {
		Destroy(g_staticField);
		g_staticField = null;
	}

	public override void TriggerOnHit(Drifter target_drifter, bool isProjectle, AttackHitType hitType){
		
		if(isProjectle || (hitType != AttackHitType.HIT && hitType != AttackHitType.BLOCK) || !status.HasStatusEffect(PlayerStatusEffect.ELECTRIFIED) || staticCycles >0)return;
		//If a burst is already charging, reset timer instead and dont consume moe juice
		if(staticBurstTimer >0)	{
			staticBurstTimer = 8;
			return;
		}

		if(hitType == AttackHitType.BLOCK){
			numBursts = 1;
			status.AddStatusDuration(PlayerStatusEffect.ELECTRIFIED, -100);
		}
		else{
			numBursts = status.remainingDuration(PlayerStatusEffect.ELECTRIFIED)/100;
			status.ApplyStatusEffect(PlayerStatusEffect.ELECTRIFIED,0);
		}
		staticBurstTimer = 8;
		staticBurstTarget = target_drifter;
	}

	public new void returnToIdle() {
		base.returnToIdle();
		Up_W_Grab.victim = null;
		deleteStaticField();
		staticCycles = 0;
	}

	public override void clearMasterhitVars(){
		base.clearMasterhitVars();
		deleteStaticField();
		staticCycles = 0;
		Remove_Dash_Trail();
		//staticBurstTarget = null;
		//numBursts = 0;
	}

	public void W_Up_Slam() {
		if(Up_W_Grab.victim != null) playState("W_Up_Down");
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
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
		baseFrame.CharacterFrame = new ParhelionRollbackFrame() {};

		return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);
	}

}

public class ParhelionRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
	
}

