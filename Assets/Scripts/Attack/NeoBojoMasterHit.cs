using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeoBojoMasterHit : MasterHit {

	InstantiatedEntityCleanup centaur;
	InstantiatedEntityCleanup note;
	InstantiatedEntityCleanup soundwave;
	int power = 0;

	override public void UpdateFrame() {
		base.UpdateFrame();
		centaur?.UpdateFrame();
		soundwave?.UpdateFrame();
		note?.UpdateFrame();
	}

	public void SpawnSoundwave() {
		GameObject proj = GameController.Instance.CreatePrefab("BojoDTiltWave", transform.position , transform.rotation,drifter.peerID);
		proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		proj.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 33,-22);
		foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   }

	   SetObjectColor(proj);

	   soundwave =  proj.GetComponent<InstantiatedEntityCleanup>();
	}

	public void whirl() {
		movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Bojo_Whirl, false);
	}

	public void SpawnNote() {
		if(note != null)  {
				//Detonate here;
			return;
		}

		GameObject proj = GameController.Instance.CreatePrefab("Bojo_Note", transform.position + new Vector3(1.5f * movement.Facing, 4f), transform.rotation,drifter.peerID);
		proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}
		SetObjectColor(proj);
		proj.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 18f,0,0);

		note = proj.GetComponent<InstantiatedEntityCleanup>();
	}


	public void SpawnCentaur() {
		if(centaur == null) {
			GameObject proj = GameController.Instance.CreatePrefab("Centaur", transform.position , transform.rotation,drifter.peerID);
			proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
			proj.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 15,0);
			int id = attacks.NextID;
			foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
					hitbox.parent = drifter.gameObject;
					hitbox.AttackID = id;
					hitbox.Facing = movement.Facing;
			}

			foreach (HurtboxCollision hurtbox in proj.GetComponentsInChildren<HurtboxCollision>(true))
				hurtbox.owner = drifter.gameObject;
	   

			SetObjectColor(proj);

			centaur = proj.GetComponent<InstantiatedEntityCleanup>();
			UnityEngine.Debug.Log("PLACING CENTAUR");
			centaur.PlayAnimation("Centaur_" + power);
			UnityEngine.Debug.Log("Centaur_" + power);
		}
	}

	public void fireCentaur() {
		if(centaur != null) {
			int id = attacks.NextID;
			foreach (HitboxCollision hitbox in centaur.GetComponentsInChildren<HitboxCollision>(true)) {
				hitbox.parent = drifter.gameObject;
				hitbox.AttackID = id;
				hitbox.Facing = movement.Facing;
			}
			centaur.PlayAnimation("Centaur_Fire_" + power);
			UnityEngine.Debug.Log("Centaur_Fire_" + power);
			
		}
		power = 0;
	}

	public void fireCentaurState() {
		if(centaur != null)
			playState("W_Down_Fire");
	}

	public void setCentaurPower(int pow) {
		power = pow; 
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		bw.Write(power);

		if(centaur == null)
			bw.Write(false);
		else{
			bw.Write(true);
			centaur.Serialize(bw);
		}

		if(note == null)
			bw.Write(false);
		else{
			bw.Write(true);
			note.Serialize(bw);
		}

		if(soundwave == null)
			bw.Write(false);
		else{
			bw.Write(true);
			soundwave.Serialize(bw);
		}

	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		base.Deserialize(br);

		power = br.ReadInt32();

		if(br.ReadBoolean()){
			if(centaur == null) SpawnCentaur();
			centaur.Deserialize(br);
		}
		else if(centaur != null){
			Destroy(centaur.gameObject);
			centaur = null;
		}

		if(br.ReadBoolean()){
			if(note == null) SpawnNote();
			note.Deserialize(br);
		}
		else if(note != null){
			Destroy(note.gameObject);
			note = null;
		}

		if(br.ReadBoolean()){
			if(soundwave == null) SpawnSoundwave();
			soundwave.Deserialize(br);
		}
		else if(soundwave != null){
			Destroy(soundwave.gameObject);
			soundwave = null;
		}
	}
}