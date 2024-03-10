using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class DrifterCannonMasterHit : MasterHit {

	int boostTime = 90;
	int charge = 0;
	bool jumpGranted = false;

	protected bool listeningForWallbounce = false;
	protected bool listeningForDirection = false;

	InstantiatedEntityCleanup explosion;
	InstantiatedEntityCleanup[] grenades = new InstantiatedEntityCleanup[6];
	//Stored in groups of 4
	//0,1,2,3,0,1,2,3...
	InstantiatedEntityCleanup[] ranches = new InstantiatedEntityCleanup[16];

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(status.HasStatusEffect(PlayerStatusEffect.DEAD)) {
			Empowered = false;
			drifter.Sparkle(false);
			jumpGranted = false;
			SetCharge(0);
		}

		if(jumpGranted && movement.grounded)jumpGranted = false;

		if(listeningForWallbounce && movement.IsWallSliding()) {
			listeningForWallbounce = false;
			drifter.PlayAnimation("W_Side_End_Early");
			rb.velocity = new Vector2(movement.Facing * -15f,30f);
			if(!jumpGranted && movement.currentJumps <= movement.numberOfJumps -1) movement.currentJumps++;
			jumpGranted = true;
			GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Restitution,rb.position + new Vector2(movement.Facing * .5f,0), (movement.Facing > 0)?90:-90,Vector3.one);
			unpauseGravity();
		}

		if(listeningForDirection) {
			movement.updateFacing();
			movement.move(10f);
			rb.velocity = new Vector2(rb.velocity.x,(drifter.input[0].MoveY >0?Mathf.Lerp(20f,rb.velocity.y,.45f):rb.velocity.y));

			if(attacks.lightPressed()) {
				attacks.useNormal();
				listeningForDirection = false;
			}

			else if(drifter.input[0].MoveY > 0) {
				drifter.PlayAnimation("W_Up_Loop",0,true);
				boostTime --;
			}
			else {
				drifter.PlayAnimation("W_Up_Idle",0,true);
			}

			if(boostTime <=0) {
				listeningForDirection = false;
				drifter.PlayAnimation("W_Up_End");
			}
		}

		foreach(InstantiatedEntityCleanup ranch in ranches)
			ranch?.UpdateFrame();

		foreach(InstantiatedEntityCleanup grenade in grenades)
			grenade?.UpdateFrame();

		explosion?.UpdateFrame();

	}

	public void listenForDirection() {
		listeningForDirection = true;
		boostTime = 90;
		listenForGrounded("Jump_End");
	}

	public void cancelWUp() {
		listeningForDirection = false;
	}

	public void SairExplosion() {
		SpawnExplosion(new Vector3(1.9f * movement.Facing,3.3f,0),1);
	}


	public void SideWExplosion() {
		SpawnExplosion(new Vector3(-1.5f * movement.Facing,2.7f,0), -1);
	}

	public void UairExplosion() {
		SpawnExplosion(new Vector3(-.4f* movement.Facing,5.5f,0),1,90 );
	}

	void SpawnExplosion(Vector3 pos, int flip, int direction = 0) {
		GameObject projectile = GameController.Instance.CreatePrefab("DC_Explosion", transform.position + pos, Quaternion.Euler(0,0,movement.Facing *direction),drifter.peerID);
		projectile.transform.localScale = new Vector3(flip * 10f * movement.Facing, 10f , 1f);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   }
	   explosion = projectile.GetComponent<InstantiatedEntityCleanup>();
	}

	public void listenForWallBounce() {
		listeningForWallbounce = true;
	}

	public override void clearMasterhitVars() {
		base.clearMasterhitVars();
		listeningForWallbounce = false;
		listeningForDirection = false;

		//TODO TEST ME 
		// if(explosion != null){
		// 	Destroy(explosion);
		// 	explosion = null;
		// }
		// //Remove lvl 4 ranch on super
		// if(ranches[ranches.Length -1] != null){
		// 	Destroy(ranches[ranches.Length -1] );
		// 	ranches[ranches.Length -1]  = null;
		// }
	}

	public void SpawnGrenade() { 
		for(int i = 0; i < grenades.Length; i++){
			if(grenades[i] == null) {
				SpawnGrenade(i);
				return;
			}
		}
	}

	void SpawnGrenade(int index) {

		Vector3 pos = new Vector3(.5f * movement.Facing,3.7f,0);
		
		GameObject projectile = GameController.Instance.CreatePrefab("DC_Genade", transform.position + pos, transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(20* movement.Facing,25);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   	}

	   	SetObjectColor(projectile);

 		grenades[index] = projectile.GetComponent<InstantiatedEntityCleanup>();

	}

	//W_Neutral

	public void handleRanchStartup() {
		//Fix this
		foreach(PlayerInputData input in drifter.input)
			input.Special = true;
		listenForSpecialTapped("W_Neutral_Fire");
		if(Empowered) drifter.PlayAnimation("W_Neutral_Fire");
		else if(charge >0)drifter.PlayAnimation("W_Neutral_" + charge);
	}

	public void SetCharge(int charge) {
		this.charge = charge;
		Empowered = (charge == 3);

		drifter.Sparkle(Empowered);

		drifter.SetAnimationOverride(Empowered?1:0);

	}

	public void SpawnRanch(){
		for(int i = (charge % 4); i < ranches.Length ; i += 4){
			if(ranches[i] == null) SpawnRanch(charge, i);
		}
	}

	void SpawnRanch(int ranchLevel, int index) {

		Vector3 pos = new Vector3(1f * movement.Facing,2.7f,0);
		
		GameObject projectile = GameController.Instance.CreatePrefab("Ranch" + ranchLevel, transform.position + pos, transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		rb.velocity = new Vector2(ranchLevel * -10f * movement.Facing,0);
		
		if(ranchLevel < 3) projectile.GetComponent<Rigidbody2D>().velocity = new Vector2((3 - ranchLevel) * 15f * movement.Facing,0);

		SetCharge(0);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
	   }

	   ranches[index] = projectile.GetComponent<InstantiatedEntityCleanup>();
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void  Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		bw.Write(jumpGranted);
		bw.Write(listeningForWallbounce);
		bw.Write(listeningForDirection);

		bw.Write(boostTime);
		bw.Write(charge);

		for(int i = 0; i < grenades.Length; i++){
			if(grenades[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				grenades[i].Serialize(bw);
			}
		}

		for(int i = 0; i < ranches.Length; i++){
			if(ranches[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				ranches[i].Serialize(bw);
			}
		}

		if(explosion == null)
			bw.Write(false);
		else{
			bw.Write(true);
			explosion.Serialize(bw);
		}

	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		base.Deserialize(br);

		jumpGranted = br.ReadBoolean();
		listeningForWallbounce = br.ReadBoolean();
		listeningForDirection = br.ReadBoolean();

		boostTime = br.ReadInt32();
		charge = br.ReadInt32();


		for(int i = 0; i < grenades.Length; i++){
			if(br.ReadBoolean()){
				if(grenades[i] == null)SpawnGrenade(i);
				grenades[i].Deserialize(br);
			}
			else if(grenades[i] != null){
				Destroy(grenades[i].gameObject);
				grenades[i] = null;
			}
		}

		for(int i = 0; i < ranches.Length; i++){
			if(br.ReadBoolean()){
				if(ranches[i] == null)SpawnRanch((i%4),i);
				ranches[i].Deserialize(br);
			}
			else if(ranches[i] != null){
				Destroy(ranches[i].gameObject);
				ranches[i] = null;
			}
		}

		if(br.ReadBoolean()){
			if(explosion == null)SpawnExplosion(transform.position,1,0);
			explosion.Deserialize(br);
		}
		else if(explosion != null){
			Destroy(explosion.gameObject);
			explosion = null;
		}
	}

}