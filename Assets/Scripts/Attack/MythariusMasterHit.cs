using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ProjectileIndex
{
	Bird
}

public class MythariusMasterHit : MasterHit
{
	bool listeningForDirection = false;
	int delaytime = 0;
	Vector2 heldDirection = Vector2.zero;

	GameObject g_Bird;
	GameObject g_Letter;

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(g_Bird != null) g_Bird.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

		if(listeningForDirection) {
			if(!drifter.input[0].Special) delaytime++;
			heldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
			if(heldDirection != Vector2.zero || delaytime > 5) NeutralSpecial();
		}

	}

	public void listenForDirection() {
		delaytime = 0;
		listeningForDirection = true;
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
			case ProjectileIndex.Bird:
				CreateLetter();
				break;
			default:
				break;
		}
	}

	//Move to particle system
	public void ring() {
		GameObject ring = GameController.Instance.CreatePrefab("LaunchRing", transform.position + new Vector3(0,1.4f),  transform.rotation);
	}

	public void CreateBird() {

		if(g_Bird != null) {
			Destroy(g_Bird);
		}
		
		g_Bird = GameController.Instance.CreatePrefab("Mytharius_Bird", transform.position + new Vector3(movement.Facing * 1.4f,3f), transform.rotation);
		g_Bird.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		foreach (HitboxCollision hitbox in g_Bird.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   }

	   SetObjectColor(g_Bird);
	   g_Bird.GetComponent<RemoteProjectileUtil>().hit = this;

	}

	public void CreateLetter() {

		int birdFacing = g_Bird.GetComponentInChildren<HitboxCollision>().Facing;

		g_Letter = GameController.Instance.CreatePrefab("Mytharius_Letter", g_Bird.transform.position, g_Bird.transform.rotation);
		g_Letter.transform.localScale = new Vector3(birdFacing *10,10,1f);
		attacks.SetMultiHitAttackID();
		foreach (HitboxCollision hitbox in g_Letter.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = birdFacing;
		}

		SetObjectColor(g_Letter);
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
		baseFrame.CharacterFrame = new MythariusRollbackFrame()  {
			Bird = (g_Bird != null) ? g_Bird.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			Letter = (g_Letter != null) ? g_Letter.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			ListeningForDirection = listeningForDirection,
			heldDirection = heldDirection,
		};


		return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);

		MythariusRollbackFrame myth_frame = (MythariusRollbackFrame)p_frame.CharacterFrame;

		//Bird reset
		if(myth_frame.Bird != null) {
			if(g_Bird == null)CreateBird();
			g_Bird.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(myth_frame.Bird);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(g_Bird);
			g_Bird = null;
		}

		//Letter reset
		if(myth_frame.Letter != null) {
			if(g_Letter == null)CreateLetter();
			g_Letter.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(myth_frame.Letter);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(g_Letter);
			g_Letter = null;
		}  
		listeningForDirection = myth_frame.ListeningForDirection;
		heldDirection = myth_frame.heldDirection;
	}

}

public class MythariusRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
	
	public BasicProjectileRollbackFrame Bird;
	public BasicProjectileRollbackFrame Letter;
	public bool ListeningForDirection;
	public Vector2 heldDirection;
	
}