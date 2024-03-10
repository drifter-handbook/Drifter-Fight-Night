using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SandbagMasterHit : MasterHit
{
	bool dust = false;

	InstantiatedEntityCleanup Sandblast;
	InstantiatedEntityCleanup Sandspear1;
	InstantiatedEntityCleanup Sandspear2;

	int dustCount = 7;

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(status.HasEnemyStunEffect() || movement.ledgeHanging)
			dust = false;
		else if(dust) {
			if(dustCount > 3f) {
				dustCount = 0;
				movement.spawnJuiceParticle(transform.position + new Vector3(movement.Facing * -1.4f,2.7f,0), MovementParticleMode.SmokeTrail);
			}
			dustCount +=1;
		}

		Sandblast?.UpdateFrame();
		Sandspear1?.UpdateFrame();
		Sandspear2?.UpdateFrame();

	}

	//Particle system
	public void Setdust() {
		dust = true;
		
		GameObject ring = GameController.Instance.CreatePrefab("LaunchRing", transform.position,  transform.rotation,drifter.peerID);
		ring.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		dustCount = 3;
	}
	public void disableDust() {
		dust = false;
	}

	public new void returnToIdle() {
		base.returnToIdle();
		dust = false;
	}

	public override void clearMasterhitVars() {
		base.clearMasterhitVars();
		//dust = false;
		clear();
	}

	public void clear() {
		if(Sandspear1 != null ){
			Destroy(Sandspear1.gameObject);
			Destroy(Sandspear2.gameObject);
			Sandspear1 = null;
			Sandspear2 = null;
		}
		dust = false;
	}

	public void Neutral_Special() {
		Sandblast?.animator.Play("Sandblast_Detonate");
		CreateSandblast();
	}

	void CreateSandblast() {
		GameObject proj = GameController.Instance.CreatePrefab("Sandblast", transform.position + new Vector3(1.5f * movement.Facing, 2.5f), transform.rotation,drifter.peerID);
		proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}

		SetObjectColor(proj);
		proj.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 25f,0,0);

		Sandblast = proj.GetComponent<InstantiatedEntityCleanup>();
	}

	public void Ground_Down() {

		CreateSandSpears();

		//sandspear1 = Sandspear1.GetComponent<InstantiatedEntityCleanup>();
		//sandspear2 = Sandspear2.GetComponent<InstantiatedEntityCleanup>();
	}

	void CreateSandSpears() {

		if(Sandspear1 != null || Sandspear2 != null){
			Destroy(Sandspear1.gameObject);
			Destroy(Sandspear2.gameObject);
		}

		GameObject proj1 = GameController.Instance.CreatePrefab("Sandspear", transform.position + new Vector3(1.2f * movement.Facing, 1.3f,1), transform.rotation,drifter.peerID);
		GameObject proj2 = GameController.Instance.CreatePrefab("Sandspear", transform.position + new Vector3(-1.5f * movement.Facing, 1.3f,-1), transform.rotation,drifter.peerID);
		proj1.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		proj2.transform.localScale = new Vector3(-10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in proj1.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}
		foreach (HitboxCollision hitbox in proj2.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}
		SetObjectColor(proj1);
		SetObjectColor(proj2);
		proj1.transform.SetParent(drifter.gameObject.transform);
		proj2.transform.SetParent(drifter.gameObject.transform);

		Sandspear1 = proj1.GetComponent<InstantiatedEntityCleanup>();
		Sandspear2 = proj2.GetComponent<InstantiatedEntityCleanup>();
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		if(Sandblast == null)
			bw.Write(false);
		else{
			bw.Write(true);
			Sandblast.Serialize(bw);
		}

		if(Sandspear1 == null || Sandspear2 == null)
			bw.Write(false);
		else{
			bw.Write(true);
			Sandspear1.Serialize(bw);
			Sandspear2.Serialize(bw);
		}
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {

		if(br.ReadBoolean()){
			if(Sandblast == null) CreateSandblast();
			Sandblast.Deserialize(br);
		}
		else if(Sandblast != null){
			Destroy(Sandblast.gameObject);
			Sandblast = null;
		}

		if(br.ReadBoolean()){
			if(Sandspear1 == null) CreateSandSpears();
			Sandspear1.Deserialize(br);
			Sandspear2.Deserialize(br);
		}
		else if(Sandspear1 != null){
			Destroy(Sandspear1.gameObject);
			Destroy(Sandspear2.gameObject);
			Sandspear1 = null;
			Sandspear2 = null;
		}

	}

}
