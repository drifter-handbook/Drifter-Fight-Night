
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEditor;

public class HitboxCollision : MonoBehaviour {
	public int Facing { get; set; } = 1;
	public bool isActive { get; set; } = true;
	public int AttackID { get; set; }

	public SingleAttackData OverrideData;
	public GameObject parent;

	// #if UNITY_EDITOR
	// [Help("If mutilple hitboxes with the same attack id hit on the same frame, the higher priority hit will apply", UnityEditor.MessageType.Info)]
	// #endif
	public int priority = 0;
	// #if UNITY_EDITOR
	// [Help("Defines how a projectile interacts with other projectiles. \n-2 no interaction\n -1 eat all projectiles, and faze throuhg other projectile eaters \n 0 eat all projectiles, but be be destroyed if hitting a 0 or a -1 \n 1+ priority ", UnityEditor.MessageType.Info)]
	// #endif
	public int projectileStrength = -2;
	// #if UNITY_EDITOR
	// [Help("Can the hitbox's attack be special canceled on hit?", UnityEditor.MessageType.Info)]
	// #endif
	public bool cancelable = true;
	// #if UNITY_EDITOR
	// [Help("On successful hit, play this animation state. Unused if left blank", UnityEditor.MessageType.Info)]
	// #endif
	public string OnHitAnimationState = "";

	// #if UNITY_EDITOR
	// [Help("Enables addition behavior for remote objects like projectiles or summons", UnityEditor.MessageType.Info)]
	// #endif
	[HideInInspector] public bool isPuppet = false;
	[HideInInspector] public string PuppetOnHitAnimationState = "";
	[HideInInspector] public bool playOnInvuln = false;
	[HideInInspector] public bool playOnBlock = false;
	[HideInInspector] public InstantiatedEntityCleanup entity = null;

	[HideInInspector] public bool FlagForDestruction = false;
	protected Drifter drifter;

	// Start is called before the first frame update
	void Start() {
		drifter = parent.GetComponent<Drifter>();
	}

	//Ground Collision Sparks
	void OnTriggerEnter2D(Collider2D collider) {

		if(collider.gameObject.tag == "Ground") {

			 GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.CollisionSpark,

			  collider.ClosestPoint(gameObject.GetComponent<Collider2D>().ClosestPoint(collider.transform.position)), 

			  collider.gameObject.transform.rotation.eulerAngles.z, new Vector2(Facing * 1, 1));

		}
	}

	void OnTriggerStay2D(Collider2D collider) {
		if((collider.gameObject.layer != 10 && collider.gameObject.layer != 9) || FlagForDestruction) return;
		//Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
		HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
		HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();
	
		if (hurtbox != null && isActive) 
			hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID + (isPuppet? 64:0), (OverrideData != null ) ? OverrideData :  drifter.attacks.GetCurrentAttackData());
		
		else if(hitbox != null && projectileStrength >= 0 && hitbox.projectileStrength >=-1) {
			if((hitbox.projectileStrength == -1 && projectileStrength >= 0) || (hitbox.projectileStrength >= projectileStrength)){
				FlagForDestruction = true;
				GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Deflect,
				collider.ClosestPoint(gameObject.GetComponent<Collider2D>().ClosestPoint(collider.transform.position)), 
				collider.gameObject.transform.rotation.eulerAngles.z, new Vector2(Facing * 1, 1));
			}
		}
	}

	//Rollback
	//====================================
	
	//Takes a snapshot of the current frame to rollback to
	public void Serialize(BinaryWriter bw) {
		bw.Write(isActive);
		bw.Write(FlagForDestruction);
		bw.Write(Facing);
		bw.Write(AttackID);

	}

	//Rolls back the entity to a given frame state
	public void Deserialize(BinaryReader br) {
		isActive = br.ReadBoolean();
		FlagForDestruction = br.ReadBoolean();
		Facing = br.ReadInt32();
		AttackID = br.ReadInt32();
	}

	#if UNITY_EDITOR
		[CustomEditor(typeof(HitboxCollision))]
		public class HitboxCollisionEditor : Editor {
			public override void OnInspectorGUI() {
				base.OnInspectorGUI();
				HitboxCollision hitCol = (HitboxCollision)target;
				hitCol.isPuppet = EditorGUILayout.Toggle("Projectile is a puppet", hitCol.isPuppet);
				if (hitCol.isPuppet) {
					hitCol.PuppetOnHitAnimationState = EditorGUILayout.TextField ("Projectile on-hit State name:", hitCol.PuppetOnHitAnimationState);
					if(hitCol.PuppetOnHitAnimationState != ""){
						hitCol.playOnInvuln = EditorGUILayout.Toggle ("Play projectile state on invulnerable targets?", hitCol.playOnInvuln);
						hitCol.playOnBlock = EditorGUILayout.Toggle ("Play projectile state on blocking targets?", hitCol.playOnBlock);
					}
					
				}

			}
		}
	#endif
}
