using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public enum ProjectileIndex
{
	BIRD
}

public class MythariusMasterHit : MasterHit
{
	bool listeningForDirection = false;
	int neutralSpecialReleaseDelay = 0;
	Vector2 heldDirection = Vector2.zero;

	InstantiatedEntityCleanup bird;
	InstantiatedEntityCleanup letter;

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(movement.ledgeHanging || status.HasEnemyStunEffect())
			clearMasterhitVars();

		bird?.UpdateFrame();
		letter?.UpdateFrame();

		if(listeningForDirection) {
			if(!drifter.input[0].Special) neutralSpecialReleaseDelay++;
			heldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
			if(heldDirection != Vector2.zero || neutralSpecialReleaseDelay > 5) NeutralSpecial();
		}

	}

	public void listenForDirection() {
		neutralSpecialReleaseDelay = 0;
		listeningForDirection = true;
	}

	public override void clearMasterhitVars() {
		base.clearMasterhitVars();
		listeningForDirection = false;
	}

	public void NeutralSpecial() {

		listeningForDirection = false;
		movement.updateFacing();

		foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
			hitbox.Facing = drifter.movement.Facing;
		
		if(heldDirection.y <0 && movement.grounded) playState("W_Neutral_GD");
		else if(heldDirection.y <0) playState("W_Neutral_D");
		else if(heldDirection.y >0) playState("W_Neutral_U");
		else playState("W_Neutral_S");

		heldDirection = Vector2.zero;

	}

	public void warpStart() {
		movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Myth_Warp_Start,false);
	}

	public void warpEnd() {

		movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Myth_Warp_End,false);
	}

	public override void TriggerRemoteSpawn(int index) {
		switch((ProjectileIndex)index){
			case ProjectileIndex.BIRD:
				CreateLetter();
				break;
			default:
				break;
		}
	}

	//Move to particle system
	public void ring() {
		GameObject ring = GameController.Instance.CreatePrefab("LaunchRing", transform.position + new Vector3(0,1.4f),  transform.rotation,drifter.peerID);
	}

	public void CreateBird() {

		if(bird != null) 
			Destroy(bird.gameObject);
		
		GameObject proj = GameController.Instance.CreatePrefab("Mytharius_Bird", transform.position + new Vector3(movement.Facing * 1.4f,3f), transform.rotation,drifter.peerID);
		proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
	   }

	   SetObjectColor(proj);
	   proj.GetComponent<RemoteProjectileUtil>().hit = this;

	   bird = proj.GetComponent<InstantiatedEntityCleanup>();

	}

	public void CreateLetter() {

		int birdFacing = bird.GetComponentInChildren<HitboxCollision>().Facing;

		GameObject proj = GameController.Instance.CreatePrefab("Mytharius_Letter", bird.transform.position, bird.transform.rotation,drifter.peerID);
		proj.transform.localScale = new Vector3(birdFacing *10,10,1f);
		foreach (HitboxCollision hitbox in proj.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = birdFacing;
		}

		SetObjectColor(proj);

		letter = proj.GetComponent<InstantiatedEntityCleanup>();
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		bw.Write(listeningForDirection);
		
		bw.Write(neutralSpecialReleaseDelay);

		bw.Write(heldDirection.x);
		bw.Write(heldDirection.y);

		if(bird == null)
			bw.Write(false);
		else{
			bw.Write(true);
			bird.Serialize(bw);
		}

		if(letter == null)
			bw.Write(false);
		else{
			bw.Write(true);
			letter.Serialize(bw);
		}
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {

		base.Deserialize(br);

		listeningForDirection = br.ReadBoolean();
		
		neutralSpecialReleaseDelay = br.ReadInt32();

		heldDirection.x = br.ReadSingle();
		heldDirection.y = br.ReadSingle();

		if(br.ReadBoolean()){
			if(bird == null) CreateBird();
			bird.Deserialize(br);
		}
		else if(bird != null){
			Destroy(bird.gameObject);
			bird = null;
		}

		if(br.ReadBoolean()){
			if(letter == null) CreateLetter();
			letter.Deserialize(br);
		}
		else if(letter != null){
			Destroy(letter.gameObject);
			letter = null;
		}
	}

}