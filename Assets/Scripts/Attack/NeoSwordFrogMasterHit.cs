using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NeoSwordFrogMasterHit : MasterHit
{
	public const int W_DOWN_PROJECTILES = 3;
	public TetherRange tether;
	public GameObject GrabConnectionPoint;

	Vector2 HeldDirection = Vector2.zero; 

	InstantiatedEntityCleanup tether_Tongue;
	InstantiatedEntityCleanup tether_Head;
	LineRenderer tether_Tongue_Line;
	bool tongueRetracting = false;
	Vector3 tetherPoint = Vector3.zero;

	InstantiatedEntityCleanup[] kunais = new InstantiatedEntityCleanup[3];

	bool listeningForDirection = false;
	int delaytime = 0;
	int projnum;
	   

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(status.HasStatusEffect(PlayerStatusEffect.DEAD)) {
			Empowered = false;
			drifter.Sparkle(false);
			projnum = 0;
			DeleteTongue();
		}

		if(movement.ledgeHanging || status.HasEnemyStunEffect()) {
			listeningForDirection = false;
			projnum = 0;
			DeleteTongue();
		}

		//Handle neutral special attacks
		if(listeningForDirection) {
			if(!drifter.input[0].Special) delaytime++;
			HeldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
			if(HeldDirection != Vector2.zero || delaytime > 5) NeutralSpecialSlash();
		}

		if(projnum != 0) {
			fireKunai(projnum);
			projnum += (projnum > 0) ? -1 : 1;
		}


		if(tether_Tongue != null) {
			tether_Tongue.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

			if(tetherPoint != Vector3.zero)
				tether_Tongue.GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(tether_Tongue.GetComponent<Rigidbody2D>().position, tetherPoint,3);

			else if(tongueRetracting)
				tether_Tongue.GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(tether_Tongue.GetComponent<Rigidbody2D>().position, (tether_Head != null) ?tether_Head.transform.position:GrabConnectionPoint.transform.position,1);

			tether_Tongue_Line.SetPosition(0,tether_Tongue.transform.position);
			tether_Tongue_Line.SetPosition(1,(tether_Head != null) ?tether_Head.transform.position:GrabConnectionPoint.transform.position);
		}
		else {
			tongueRetracting = false;
		}


		//Update Child Frames
		foreach(InstantiatedEntityCleanup kunai in kunais)
			kunai?.UpdateFrame();
	
	}

	public void listenForDirection() {
		listeningForDirection = true;
		delaytime = 0;
	}

	public override void clearMasterhitVars() {
		base.clearMasterhitVars();
		listeningForDirection = false;
		projnum = 0;
	}

	public new void returnToIdle() {
		base.returnToIdle();
		DeleteTongue();
		projnum = 0;
	}

	public void NeutralSpecialSlash() {

		listeningForDirection = false;
		movement.updateFacing();

		foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
			hitbox.Facing = drifter.movement.Facing;
		
		if(HeldDirection.y <0 && movement.grounded) playState("W_Neutral_GD");
		else if(HeldDirection.y <0) playState("W_Neutral_D");
		else if(HeldDirection.y >0) playState("W_Neutral_U");
		else playState("W_Neutral_S");

		HeldDirection = Vector2.zero;

	}


	void CreateTongue(Vector3 pos, float angle) {

		GameObject proj = GameController.Instance.CreatePrefab("SwordFrog_Tongue", pos, Quaternion.Euler(0,0,angle),drifter.peerID);
		proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		proj.GetComponent<Rigidbody2D>().velocity = rb.velocity + new Vector2(movement.Facing * 75f, 0f);

		foreach (PuppetGrabHitboxCollision hitbox in proj.GetComponentsInChildren<PuppetGrabHitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
			hitbox.OverrideData = attacks.Attacks[attacks.AttackType];
		}

		SetObjectColor(proj);

		proj.GetComponent<RemoteProjectileUtil>().hit = this;

		tether_Tongue = proj.GetComponent<InstantiatedEntityCleanup>();
		tether_Tongue_Line = tether_Tongue.GetComponentInChildren<LineRenderer>();
	}

	void CreateHead(Vector3 pos, float angle) {

		GameObject proj = GameController.Instance.CreatePrefab("SwordFrog_Head", pos, Quaternion.Euler(0,0,angle),drifter.peerID);
		proj.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		SetObjectColor(proj);

		proj.transform.SetParent(drifter.gameObject.transform);

		tether_Head = proj.GetComponent<InstantiatedEntityCleanup>();
	}

	public void DeleteTongue() {
		if(tether_Tongue!= null) Destroy(tether_Tongue.gameObject);
		if(tether_Head!= null) Destroy(tether_Head.gameObject);
		tether_Tongue = null;
		tether_Head = null;

	}

	public void SpawnGrabTongue() {
		DeleteTongue();

		CreateTongue(GrabConnectionPoint.transform.position, 0);
	
		tether_Tongue_Line.SetPosition(0, tether_Tongue.transform.position);
		tether_Tongue_Line.SetPosition(1, GrabConnectionPoint.transform.position);

	}

	public void SpawnWUpTongue(){

		DeleteTongue();

		float angle = 65f *movement.Facing;
		Vector3 pos = new Vector3(1.5f * movement.Facing,3.6f,0);

		//Calculate Ledge Position
		if(tether.TetherPoint != Vector3.zero) {
			tetherPoint = tether.TetherPoint;
			float deltay = tetherPoint.y- (transform.position + pos).y;
			float deltax = tetherPoint.x- (transform.position + pos).x;
			angle = Mathf.Atan2(deltay, deltax)*180 / Mathf.PI + (movement.Facing < 0 ?180:0);
			playState("W_Up_Ledge");
		}
		else
			tetherPoint = Vector3.zero;


		CreateTongue(transform.position,angle);
		CreateHead(transform.position,angle);

		if(tetherPoint == Vector3.zero) tether_Tongue.GetComponent<Rigidbody2D>().velocity = rb.velocity + new Vector2(75f * movement.Facing * Mathf.Cos((angle*movement.Facing* Mathf.PI)/180),75f *Mathf.Sin((angle*movement.Facing * Mathf.PI)/180));
		else {
			foreach (HitboxCollision hitbox in tether_Tongue.GetComponentsInChildren<HitboxCollision>(true))
				hitbox.isActive = false;
			tether.g_obj.GetComponent<HopUp>().ledgeLock = LedgeLockState.Tethered;
		}

		tether_Tongue.GetComponent<RemoteProjectileUtil>().ProjectileIndex = -1;
	}

	public void pullToLedge() {
		if(tetherPoint!=Vector3.zero) {
			Vector3 dir = tetherPoint - new Vector3(rb.position.x,rb.position.y);
			Vector3.Normalize(dir);
			rb.velocity = 10f * dir;
   
			tetherPoint = Vector3.zero;
		}
	}

	public void RetractTongue() {
		tongueRetracting = true;
	}

	public override void TriggerRemoteSpawn(int index) {
		switch(index){
			case(0):
				playState("Grab_Ground_Success");
				break;
			default:
				break;
		}
	}


	 //Flips the direction the charactr is movement.Facing mid move)
	public void invertDirection() {
		movement.flipFacing();
	}

	public void downSpecialProjectile() {
		projnum = W_DOWN_PROJECTILES;
	}

	public void downSpecialProjectileAir() {
		projnum = -1 * W_DOWN_PROJECTILES;
	}

	void fireKunai(int index) {
		if(index == 0) return;

		Vector2 size;
		Vector3 pos = new Vector3(.2f * movement.Facing, 2.7f,1f);

		GameObject kunai;

		if(index>0){
			size = new Vector2(10f * movement.Facing, 10f);
		 	kunai =  GameController.Instance.CreatePrefab("Kunai", transform.position + new Vector3(1.5f * movement.Facing, 1.5f + W_DOWN_PROJECTILES/5f + (W_DOWN_PROJECTILES - index) * .6f, 0), transform.rotation,drifter.peerID);
		 	kunai.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + 50f * movement.Facing, 0);
		 	kunai.transform.localScale = size;
		}
		else{
		 	size = new Vector2(10f, 10f);
		 	float degreesA = movement.Facing >0 ? (335f  + index * 4f) : (215f  - index * 4f);
			float radiansA = degreesA * Mathf.PI/180f;
			float posDegrees = (movement.Facing >0 ? 335f  : 215f);
			float posRadians = posDegrees * Mathf.PI/180f;
			kunai = GameController.Instance.CreatePrefab("Kunai", transform.position + new Vector3(movement.Facing * (-.5f - index/2f), index/-2f -.9f)
																 + pos, 
																 Quaternion.Euler(0,0,posDegrees),drifter.peerID);

			kunai.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + (Mathf.Cos(posRadians) *50f), Mathf.Sin(posRadians)*50f);
			kunai.transform.localScale = size;
		}

		foreach (HitboxCollision hitbox in kunai.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		kunais[Mathf.Abs(index) - 1] = kunai.GetComponent<InstantiatedEntityCleanup>();
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		bw.Write(tongueRetracting);
		bw.Write(listeningForDirection);

		bw.Write(delaytime);
		bw.Write(projnum);

		bw.Write(HeldDirection.x);
		bw.Write(HeldDirection.y);  
		bw.Write(tetherPoint.x);
		bw.Write(tetherPoint.y);

		for(int i = 0; i < kunais.Length; i++){
			if(kunais[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				kunais[i].Serialize(bw);
			}
		}

		if(tether_Head == null)
			bw.Write(false);
		else{
			bw.Write(true);
			tether_Head.Serialize(bw);
		}

		if(tether_Tongue == null)
			bw.Write(false);
		else{
			bw.Write(true);
			tether_Tongue.Serialize(bw);
		}
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {
		base.Deserialize(br);


		tongueRetracting =  br.ReadBoolean();
		listeningForDirection =  br.ReadBoolean();

		delaytime = br.ReadInt32();
		projnum = br.ReadInt32();

		HeldDirection.x = br.ReadSingle();
		HeldDirection.y = br.ReadSingle();  
		tetherPoint.x = br.ReadSingle();
		tetherPoint.y = br.ReadSingle();



		for(int i = 0; i < kunais.Length; i++){
			if(br.ReadBoolean()){
				if(kunais[i] == null)fireKunai(i);
				kunais[i].Deserialize(br);
			}
			else if(kunais[i] != null){
				Destroy(kunais[i].gameObject);
				kunais[i] = null;
			}
		}

		if(br.ReadBoolean()){
			if(tether_Head == null)CreateHead(transform.position, 65f * movement.Facing);
			tether_Head.Deserialize(br);
		}
		else if(tether_Head != null){
			Destroy(tether_Head.gameObject);
			tether_Head = null;
		}

		if(br.ReadBoolean()){
			if(tether_Tongue == null)CreateTongue(transform.position, 65f * movement.Facing);
			tether_Tongue.Deserialize(br);
		}
		else if(tether_Tongue != null){
			Destroy(tether_Tongue.gameObject);
			tether_Tongue = null;
		}
			

		
	}

}
	