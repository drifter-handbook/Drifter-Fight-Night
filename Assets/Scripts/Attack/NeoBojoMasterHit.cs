using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoBojoMasterHit : MasterHit
{

	GameObject g_centaur;
	GameObject g_note;
	GameObject g_soundwave;
	int power = 0;

	override public void UpdateFrame() {
		base.UpdateFrame();
		if(g_centaur != null) g_centaur.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
		if(g_soundwave != null) g_soundwave.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
		if(g_note != null) g_note.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
	}

	public void SpawnSoundwave() {
		g_soundwave = GameController.Instance.CreatePrefab("BojoDTiltWave", transform.position , transform.rotation);
		g_soundwave.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		g_soundwave.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 33,-22);
		foreach (HitboxCollision hitbox in g_soundwave.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   }

	   SetObjectColor(g_soundwave);
	}

	public void whirl() {
		movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Bojo_Whirl, false);
	}

	public void SpawnNote() {
		if(g_note != null)  {
				//Detonate here;
			return;
		}

		g_note = GameController.Instance.CreatePrefab("Bojo_Note", transform.position + new Vector3(1.5f * movement.Facing, 4f), transform.rotation);
		g_note.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in g_note.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}
		SetObjectColor(g_note);
		g_note.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 18f,0,0);
	}


	public void SpawnCentaur() {
		if(g_centaur == null) {
			g_centaur = GameController.Instance.CreatePrefab("Centaur", transform.position , transform.rotation);
			g_centaur.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
			g_centaur.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 15,0);
			foreach (HitboxCollision hitbox in g_centaur.GetComponentsInChildren<HitboxCollision>(true)) {
					hitbox.parent = drifter.gameObject;
					hitbox.AttackID = attacks.NextID;
					hitbox.Facing = movement.Facing;
			}

			foreach (HurtboxCollision hurtbox in g_centaur.GetComponentsInChildren<HurtboxCollision>(true))
				hurtbox.owner = drifter.gameObject;
	   

			SetObjectColor(g_centaur);
			UnityEngine.Debug.Log("PLACING CENTAUR");
			g_centaur.GetComponent<Animator>().Play("Centaur_" + power);
			UnityEngine.Debug.Log("Centaur_" + power);
		}
	}

	public void fireCentaur() {
		if(g_centaur != null) {
			foreach (HitboxCollision hitbox in g_centaur.GetComponentsInChildren<HitboxCollision>(true)) {
				hitbox.parent = drifter.gameObject;
				hitbox.AttackID = attacks.NextID;
				hitbox.Facing = movement.Facing;
			}
			g_centaur.GetComponent<Animator>().Play("Centaur_Fire_" + power);
			UnityEngine.Debug.Log("Centaur_Fire_" + power);
			
		}
		power = 0;
	}

	public void fireCentaurState() {
		if(g_centaur != null)
			playState("W_Down_Fire");
	}

	public void setCentaurPower(int pow) {
		power = pow; 
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
		baseFrame.CharacterFrame= new BojoRollbackFrame()  {
			Centaur = g_centaur != null ? g_centaur.GetComponent<NonplayerHurtboxHandler>().SerializeFrame(): null,
			Note = g_note != null ? g_note.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			Soundwave = g_soundwave != null ? g_soundwave.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			Power = power
		};

		return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);

		BojoRollbackFrame bj_frame = (BojoRollbackFrame)p_frame.CharacterFrame;

		power = bj_frame.Power;

		//Sandblast reset
		if(bj_frame.Centaur != null) {
			if(g_centaur == null)SpawnCentaur();
			g_centaur.GetComponent<NonplayerHurtboxHandler>().DeserializeFrame(bj_frame.Centaur);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(g_centaur);
			g_centaur = null;
		}  

		if(bj_frame.Soundwave != null) {
			if(g_soundwave == null)SpawnSoundwave();
			g_soundwave.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(bj_frame.Soundwave);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(g_soundwave);
			g_soundwave = null;
		} 

		if(bj_frame.Note != null) {
			if(g_note == null)SpawnNote();
			g_note.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(bj_frame.Note);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(g_note);
			g_note = null;
		} 
	}

}

public class BojoRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }

	public NPCHurtboxRollbackFrame Centaur;
	public BasicProjectileRollbackFrame Soundwave;
	public BasicProjectileRollbackFrame Note;
	public int Power;
	
}
