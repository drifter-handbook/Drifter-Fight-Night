using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectileRollbackFrame: INetworkData
{
	public string Type { get; set; }

	public Vector2 Velocity;
	public Vector2 Position;
	public float Rotation;
	public int Duration;
	public int AnimatorState;
	public float AnimatorTime;
	public bool AnimatorActive;

	//Hitpause
	public int FreezeDuration;
	public bool UseHitpause;

	//Super Freeze
	public bool PauseBehavior;
	public bool Paused;	
	public Vector2 SavedVelocity;
	public float SavedGravity;
	public bool DataSaved;

	public HitboxRollbackFrame[] Hitboxes;    
}

public class InstantiatedEntityCleanup : MonoBehaviour{

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
			if(duration <=0)
				Destroy(gameObject);
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

	private void applyFreeze(int frames){
		if(pauseBehavior && !dataSaved){
				if(animator !=null) animator.enabled = false;
				if(rb != null){
					savedVelocity = rb.velocity;
					savedGravity = rb.gravityScale;
					rb.velocity = Vector2.zero;
					rb.gravityScale = 0;
					dataSaved = true;
				}
				freezeDuration = frames;
			}
			paused = true;

	}

	public void unfreeze(){
		if(pauseBehavior && dataSaved) {
			if(animator !=null) animator.enabled = true;
			if(rb != null) {
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
	public BasicProjectileRollbackFrame SerializeFrame() {
		HitboxRollbackFrame[] HitboxFrames = new HitboxRollbackFrame[hitboxes.Length];
		//Searialize each hitbox
		for(int i = 0; i < hitboxes.Length; i++) {
			HitboxFrames[i] = hitboxes[i].SerializeFrame();
		}

		return new BasicProjectileRollbackFrame()  {
			Duration = duration,

			Velocity = rb !=null ? rb.velocity: Vector2.zero,
			Position = rb !=null ? rb.position: Vector2.zero,
			Rotation = rb !=null ? rb.rotation: 0,
			AnimatorState = animator !=null ?animator.GetCurrentAnimatorStateInfo(0).shortNameHash : -1,
			AnimatorTime = animator !=null ? animator.GetCurrentAnimatorStateInfo(0).normalizedTime : -1,
			AnimatorActive = animator !=null ? animator.enabled : false,
			Hitboxes = HitboxFrames,

			PauseBehavior = pauseBehavior,
			Paused = paused,
			SavedVelocity = savedVelocity,
			SavedGravity = savedGravity,
			DataSaved = dataSaved,
			FreezeDuration = freezeDuration,
			UseHitpause = useHitpause,
		};
	}

	//Rolls back the entity to a given frame state
	public void DeserializeFrame(BasicProjectileRollbackFrame p_frame) {
		
		duration = p_frame.Duration;
		if(rb != null) {
			rb.velocity = p_frame.Velocity;
			rb.position = p_frame.Position;
			rb.rotation = p_frame.Rotation;
		}
		
		if(animator != null) {
			animator.enabled = p_frame.AnimatorActive;
			animator.Play(p_frame.AnimatorState,0,p_frame.AnimatorTime);
		}

		for(int i = 0; i < p_frame.Hitboxes.Length; i++) {
			hitboxes[i].DeserializeFrame(p_frame.Hitboxes[i]);
		}

		pauseBehavior = p_frame.PauseBehavior;
		paused = p_frame.Paused;
		savedVelocity = p_frame.SavedVelocity;
		savedGravity = p_frame.SavedGravity;
		dataSaved = p_frame.DataSaved;
		useHitpause = p_frame.UseHitpause;
		freezeDuration = p_frame.FreezeDuration;

	}

}
