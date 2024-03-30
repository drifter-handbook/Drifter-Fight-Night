using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeoNeroMasterHit : MasterHit{

	InstantiatedEntityCleanup pillar;

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		base.Deserialize(br);
	}


	public void Create_Pillar() {

		if(pillar!= null)
			Destroy(pillar.gameObject);

		GameObject projectile = GameController.Instance.CreatePrefab("Nero_Pillar", transform.position + new Vector3(9f * movement.Facing,0,0), transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		SetObjectColor(projectile);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		foreach (HurtboxCollision hurtbox in projectile.GetComponentsInChildren<HurtboxCollision>(true))
			hurtbox.owner = drifter.gameObject;

		pillar = projectile.GetComponent<InstantiatedEntityCleanup>();
	}

}
