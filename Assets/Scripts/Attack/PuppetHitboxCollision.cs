using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuppetHitboxCollision : HitboxCollision
{

	void OnTriggerStay2D(Collider2D collider)
	{
		if((collider.gameObject.layer != 10 && collider.gameObject.layer != 9) || FlagForDestruction ) return;
		//Debug.Log("name " + name + " " + (gameObject.activeSelf || gameObject.activeInHierarchy));
		HurtboxCollision hurtbox = collider.GetComponent<HurtboxCollision>();
		HitboxCollision hitbox = collider.GetComponent<HitboxCollision>();
	
		int hitResult = -3;
		if (hurtbox != null && isActive) {
			//string player = playerType.NetworkType;
			hitResult = (int)hurtbox.parent.GetComponent<PlayerHurtboxHandler>().RegisterAttackHit(this, hurtbox, AttackID + 64, OverrideData != null?OverrideData:drifter.attacks.GetCurrentAttackData());
	 
			if(hitResult == 1) isActive = false;
			
		}
		else if(hitbox != null && projectilePriority >= 0 && hitbox.projectilePriority >=-1) {
			if((hitbox.projectilePriority == -1 && projectilePriority >= 0) || (hitbox.projectilePriority >= projectilePriority)){
				FlagForDestruction = true;
				GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Deflect,
				collider.ClosestPoint(gameObject.GetComponent<Collider2D>().ClosestPoint(collider.transform.position)), 
				collider.gameObject.transform.rotation.eulerAngles.z, new Vector2(Facing * 1, 1));
			}
		}
	}
}