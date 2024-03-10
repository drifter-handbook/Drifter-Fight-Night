using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class InstantiatedEntityCleanup : MonoBehaviour{

	public string destoryState = "";

	public int duration = -1;
	public Rigidbody2D rb;
	public Animator animator;

	public bool pauseBehavior = true;
	public bool paused = false;
	public bool useHitpause = false;

	private Vector2 savedVelocity;
	private float savedGravity;
	private bool dataSaved = false;
	private int freezeDuration = 0;

	//Padded to support consistency with binary serialization
	//As of now, characters cannot have more than this number of hitbox collision objects on their prefab
	HitboxCollision[] hitboxes;

	//Destroy if the projectile leaves play
	//Turn off for things that respawn, ie Players and Bean
	public bool DestroyOnExit = true;

	public void UpdateFrame() {
		if(freezeDuration > 0){
			freezeDuration--;
			if(useHitpause && freezeDuration <= 0)
				unfreeze();
		}
		
		if(duration >0 && !paused) {
			duration--;
			if(duration <=0) {
				if(destoryState != "" ) animator.Play(destoryState);
				else Destroy(gameObject);
			}	
		}
		foreach(HitboxCollision hb in hitboxes){
			if(hb.FlagForDestruction){
				if(destoryState != "" ) animator.Play(destoryState);
				else Destroy(gameObject);
			}
		}
	}

	void Awake() {
		hitboxes = GetComponentsInChildren<HitboxCollision>();
	}

	void OnTriggerExit2D(Collider2D other) {
		if(other.gameObject.tag == "Killzone" && DestroyOnExit)
			Destroy(gameObject);
		else if(other.gameObject.tag == "SuperFreeze")
			unfreeze();
			
	}

	void OnTriggerStay2D(Collider2D other) {
		if(other.gameObject.tag == "SuperFreeze")
			applyFreeze(0);
		
	}

	public void ApplyFreeze(int frames){
		if(useHitpause)
			applyFreeze(frames);
	}

	public void PlayAnimation(string state){
		animator.Play(state);
	}

	public void setDelayedVelocity(Vector2 p_savedVelocity) {
		rb.gravityScale = 0;
		rb.velocity = Vector2.zero;
		if(p_savedVelocity != Vector2.zero){
			savedVelocity = p_savedVelocity;
			dataSaved = true;
		}
	}

	public void stopMovement() {
		setDelayedVelocity(Vector2.zero);
	}

	private void applyFreeze(int frames){
		if(pauseBehavior){
			if(animator !=null) animator.enabled = false;
			if(rb != null && !dataSaved){
				UnityEngine.Debug.Log(rb.velocity + " : " + gameObject);
				savedVelocity = rb.velocity;
				savedGravity = rb.gravityScale;
				rb.velocity = Vector2.zero;
				rb.gravityScale = 0;
				dataSaved = true;
			}
			freezeDuration = frames;
			paused = true;
		}
		
	}

	public void unfreeze(){
		if(pauseBehavior && dataSaved) {
			if(animator !=null) animator.enabled = true;
			if(rb != null) {
				UnityEngine.Debug.Log(savedVelocity + " : " + gameObject);
				rb.velocity = savedVelocity;
				rb.gravityScale = savedGravity;
				savedVelocity = Vector2.zero;
				savedGravity = 0;
			}
			dataSaved = false;
		}
		paused = false;
	}

	public void Cleanup() {
		Destroy(gameObject);
	}

	//Takes a snapshot of the current frame to rollback to
	public void Serialize(BinaryWriter bw) {

		bw.Write(duration);

		bw.Write(transform.localScale.x);
		bw.Write(transform.position.z);

		if(rb != null) {
			bw.Write(rb.velocity.x);
			bw.Write(rb.velocity.y);
			bw.Write(rb.position.x);
			bw.Write(rb.position.y);
			bw.Write(rb.rotation);
		}

		bw.Write(savedVelocity.x);
		bw.Write(savedVelocity.y);
		bw.Write(savedGravity);

		if(animator !=null){
			bw.Write(animator.GetCurrentAnimatorStateInfo(0).shortNameHash);
			bw.Write(animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
			bw.Write(animator.enabled);
		}

		bw.Write(freezeDuration);

		bw.Write(pauseBehavior);
		bw.Write(paused);
		bw.Write(dataSaved);
		bw.Write(useHitpause);

		for(int i = 0; i < hitboxes.Length; i++)
			hitboxes[i].Serialize(bw);
	}

	//Rolls back the entity to a given frame state
	public void Deserialize(BinaryReader br) {
		
		duration = br.ReadInt32();

		transform.localScale 	= new Vector2(br.ReadSingle(),transform.localScale.y);
		transform.position 		= new Vector3(transform.position.x,transform.position.y,br.ReadSingle());

		if(rb != null) {
			Vector2 Velocity = Vector2.zero;
			Vector3 Position = Vector3.zero;

			Velocity.x 		= br.ReadSingle();
			Velocity.y 		= br.ReadSingle();
			rb.velocity 	= Velocity;
			Position.x 		= br.ReadSingle();
			Position.y 		= br.ReadSingle();
			rb.position		= Position;
			rb.rotation 	= br.ReadSingle();
		}

		Vector2 SavedVelocity = Vector2.zero;
		SavedVelocity.x = br.ReadSingle();
		SavedVelocity.y = br.ReadSingle();
		savedVelocity = SavedVelocity;
		savedGravity = br.ReadSingle();
		
		if(animator != null) {
			int AnimatorHash = br.ReadInt32();
			float AnimatorTime = br.ReadSingle();
			animator.Play(AnimatorHash,0,AnimatorTime);
			animator.enabled = br.ReadBoolean();
		}

		freezeDuration = br.ReadInt32();

		pauseBehavior = br.ReadBoolean();
		paused = br.ReadBoolean();
		dataSaved = br.ReadBoolean();
		useHitpause = br.ReadBoolean();

		for(int i = 0; i < hitboxes.Length; i++)
			hitboxes[i].Deserialize(br);
	}
}
