using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectileRollbackFrame: INetworkData
{
	public string Type { get; set; }

	public Vector2 SavedVelocity;
	public Vector2 Velocity;
	public Vector2 Position;
	public float Rotation;
	public int Duration;
	public int AnimatorState;
	public float AnimatorTime;
	public bool AnimatorActive;
	public HitboxRollbackFrame[] Hitboxes;    
}

public class InstantiatedEntityCleanup : MonoBehaviour
{
	public int duration = -1;
	// Start is called before the first frame update
	public Rigidbody2D rb;
	public Animator animator;

	public bool paused = false;
	public bool pauseBehavior = true;

	private Vector2 savedVelocity;
	private float savedGravity;
	private bool dataSaved = false;

	HitboxCollision[] hitboxes;

	//Remove Me
	public bool DestroyOnExit = true;

	public void UpdateFrame() {
		if(duration >0) {
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
		else if(other.gameObject.tag == "SuperFreeze") {
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
	}

	void OnTriggerStay2D(Collider2D other) {
		if(other.gameObject.tag == "SuperFreeze") {
			if(pauseBehavior && !dataSaved){
				if(animator !=null) animator.enabled = false;
				if(rb != null){
					savedVelocity = rb.velocity;
					savedGravity = rb.gravityScale;
					rb.velocity = Vector2.zero;
					rb.gravityScale = 0;
					dataSaved = true;
				}
			}
			paused = true;
		}
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
			Hitboxes = HitboxFrames
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

	}

}
