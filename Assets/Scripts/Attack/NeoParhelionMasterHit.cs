using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoParhelionMasterHit : MasterHit
{
	int MAX_ORBS = 3;
	GameObject[] orbs = new GameObject[] {null,null,null};
	GameObject burst;
	Drifter orbTarget = null;
	int orbTimer = 0;

	//Inhereted Roll Methods
	public GrabHitboxCollision Up_W_Grab;

	override public void UpdateFrame() {
		base.UpdateFrame();
		if(drifter.status.HasEnemyStunEffect()){
			orbTimer = -1;
			removeAllOrbs();
		}

		if(orbTimer > 0 && !status.HasStatusEffect(PlayerStatusEffect.HITPAUSE)){
			orbTimer--;
			if(orbTimer == 0){
				int orbIndex = -1;
				for(int i = 0; i < MAX_ORBS; i++) {
					if(orbs[i] != null) {
						orbIndex = i;
						i = MAX_ORBS;
					}
				}
				if(orbIndex < 0) return;

				Create_Burst(orbTarget.gameObject.transform.position);

				Destroy(orbs[orbIndex]);
				orbs[orbIndex] = null;
				orbTarget = null;
			}

		}
	}

	void removeAllOrbs(){
		for(int i = 0; i < MAX_ORBS; i++){
			if(orbs[i] != null){
				Destroy(orbs[i]);
				orbs[i] = null;
			}
		}
	}

	public void Create_Orb() {

		const float ORB_RADIUS = 3f;

		int orbIndex = -1;

		for(int i = 0; i <MAX_ORBS; i++) {
			if(orbs[i] == null) {
				orbIndex = i;
				i = MAX_ORBS;
			}
		}
		if(orbIndex < 0)
			return;
		
		float angle = orbIndex * 2f/MAX_ORBS * Mathf.PI;

		GameObject projectile = Create_Orb(transform.position + new Vector3(Mathf.Sin(angle) * movement.Facing * ORB_RADIUS ,Mathf.Cos(angle) *ORB_RADIUS + 3f,0));

	   	orbs[orbIndex] = projectile;
	}


	public GameObject Create_Orb(Vector2 pos){
		GameObject projectile;
		projectile = GameController.Instance.CreatePrefab("Parhelion_Orb", pos, transform.rotation);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
	   	SetObjectColor(projectile);
	   	projectile.transform.SetParent(drifter.gameObject.transform);

	   	return projectile;
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

	   	burst = projectile;
	}

	public override void TriggerOnHit(Drifter target_drifter, bool isProjectle, AttackHitType hitType){
		
		if(isProjectle || (hitType != AttackHitType.HIT && hitType != AttackHitType.BLOCK) )return;
		orbTimer = 8;
		orbTarget = target_drifter;
	}

	public new void returnToIdle() {
		base.returnToIdle();
		Up_W_Grab.victim = null;
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

