using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class OrroReworkMasterHit : MasterHit {
	const int MAX_BOOKS = 3;
	const int BOOK_BOLT_DELAY = 10;

	//For orros porjectile normals
	DrifterAttackType[] explosionTypes = new DrifterAttackType[10];
	InstantiatedEntityCleanup[] explosions = new InstantiatedEntityCleanup[10];

	//W_Down data
	InstantiatedEntityCleanup[] books = new InstantiatedEntityCleanup[] {null,null,null};
	InstantiatedEntityCleanup[] bookBolts = new InstantiatedEntityCleanup[] {null,null,null};
	int bookBoltTimer = 0;
	string boltTarget = "";

	//Bean data
	BeanWrangler bean;
	bool beanIsCharging = false;
	bool beanFollowing = true;   
	Vector3 targetPos;

	//For Bean Command
	bool listeningForDirection = false;
	int neutralSpecialReleaseDelay = 0;
	Vector2 heldDirection = Vector2.zero;

	void Start() {
		spawnBean();
		Empowered = false;
	}

	override public void UpdateFrame() {
		base.UpdateFrame();

		if(drifter.status.HasEnemyStunEffect())
			removeAllBooks();

		//Creates book bolts on a delay
		if(bookBoltTimer > 0){
			bookBoltTimer--;
			if(bookBoltTimer == 0)
				Create_Book_Bolt();
		}

		//reset bean when he dies
		if(!bean.alive) {
			beanFollowing= true;
			Empowered = false;
		}

		drifter.Sparkle(bean.alive && bean.canAct);

		if(status.HasEnemyStunEffect() || movement.ledgeHanging) {
			listeningForDirection = false;  
		}

		//Otherwise, use a stance move 
		if(listeningForDirection) {

			if(!drifter.input[0].Special) neutralSpecialReleaseDelay++;
			heldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
			if(heldDirection != Vector2.zero || neutralSpecialReleaseDelay > 5) beanCommand();
		}

		//If orro cancels, or is hit out of a move where bean charges, cancel that move
		//Note, bean continues doing the move if orro Byzantine Cancels the move
		if(beanIsCharging && (status.HasEnemyStunEffect() || movement.ledgeHanging || attackWasCanceled)) {
			beanIsCharging = false;
			bean.returnToNeutral();
		}

		//If orro dies, kill bean
		if(status.HasStatusEffect(PlayerStatusEffect.DEAD)) {
			bean.die();
			bean = null;
			Empowered = false;
		}
		else if(bean == null) {
			spawnBean();
		}

		//Send bean orros position and direction so he can follow on a delay
		else {
			targetPos = rb.position - new Vector2(-1f * movement.Facing,3f);
			bean.addBeanState(targetPos,movement.Facing);

			Empowered = !beanFollowing || Vector3.Distance(targetPos,bean.rb.position) > 3.8f;
		}

		bean?.UpdateFrame();

		foreach(InstantiatedEntityCleanup exp in explosions)
			exp?.UpdateFrame();

	}

	public void listenForDirection() {
		listeningForDirection = true;
		neutralSpecialReleaseDelay = 0;
		heldDirection = Vector2.zero;
	}

	public void beanCommand() {
		movement.updateFacing();
		applyEndLag(480);
		playState("W_Neutral_Command");

		refreshBeanHitboxes();

		if(drifter.input[0].MoveY >0)
			bean.playState("Bean_Up");
		else if(drifter.input[0].MoveY <0)
			bean.playState("Bean_Down");
		else if(drifter.input[0].MoveX != 0)
			bean.playState("Bean_Side");
		else
			bean.playState("Bean_Neutral");

		bean.setBeanDirection(movement.Facing);

		listeningForDirection = false;
	}

	/*
		Side Special Functions
	*/

	//Enables all relevant flags for orro's neutral special
	public void BeginWSide() {
		specialReleasedFlag = true;
		dashCancelFlag = true;
		activeCancelFlag = true;
		queuedState = "W_Side_Fire";
		specialCharge = 0;
		specialLimit = 8;
		beanIsCharging = true;
	}

	//Fires bean or recalls him for neutral W
	public void WSideFire() {

		clearMasterhitVars();
		if(Vector3.Distance(targetPos,bean.rb.position) <= 3.8f && beanFollowing) {
			bean.setBean(specialCharge * 4.5f  + 8f);
			refreshBeanHitboxes();
			bean.playFollowState("Bean_Side_Special_Fire");
			movement.spawnJuiceParticle(targetPos,MovementParticleMode.Bean_Launch, false);
			beanFollowing = false;
		}
		else {
			beanFollowing = true;
			bean.recallBean(rb.position - new Vector2(-2f * movement.Facing,4f),movement.Facing);
		}
		specialCharge = 0;
	}       

	//Tells the current bean object to preform certain actions

	public void BeanSideSpecial() {
		refreshBeanHitboxes();
		bean.playChargeState("Bean_Side_Special");
	}

	public void BeanReset() {
		bean.playFollowState("Bean_Idle");
	}

	//Creates a bean follower
	public void spawnBean() {
		
		Empowered = false;

		GameObject beanObject = GameController.Instance.CreatePrefab("Bean", transform.position - new Vector3(-1f * movement.Facing, 1f), transform.rotation,drifter.peerID);
		foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}

		foreach (HurtboxCollision hurtbox in beanObject.GetComponentsInChildren<HurtboxCollision>(true))
			hurtbox.owner = drifter.gameObject;
		
		SetObjectColor(beanObject);

		bean = beanObject.GetComponent<BeanWrangler>();
		bean.facing = movement.Facing;
		bean.color = drifter.GetColor();
	}

	//Refreshes each of beans hitbox ids so he can keep doing damage
	private void refreshBeanHitboxes() {
		bean.facing = movement.Facing;

		foreach (HitboxCollision hitbox in bean.gameObject.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = bean.facing;
		}
	}

	/*

		Other Projectiles

	*/

	private GameObject _Create_Book(Vector2 pos){
		GameObject projectile;
		projectile = GameController.Instance.CreatePrefab("Orro_Book", pos, transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
	   	SetObjectColor(projectile);
	   	projectile.transform.SetParent(drifter.gameObject.transform);

	   	return projectile;
	}

	private GameObject _Create_Book_Bolt(Vector2 pos){
		GameObject projectile = GameController.Instance.CreatePrefab("Orro_Book_Bolt", pos, transform.rotation,drifter.peerID);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		SetObjectColor(projectile);

		return projectile;
	}

	public void Create_Book() {

		const float BOOK_RADIUS = 3f;

		int bookIndex = -1;

		for(int i = 0; i <MAX_BOOKS; i++) {
			if(books[i] == null) {
				bookIndex = i;
				i = MAX_BOOKS;
			}
		}
		if(bookIndex < 0)
			return;
		
		float angle = bookIndex * 2f/MAX_BOOKS * Mathf.PI;

	   	books[bookIndex] = _Create_Book(transform.position + new Vector3(Mathf.Sin(angle) * movement.Facing * BOOK_RADIUS ,Mathf.Cos(angle) *BOOK_RADIUS + 3f,0)).GetComponent<InstantiatedEntityCleanup>();
	}

	void removeAllBooks() {
		for(int i = 0; i < MAX_BOOKS; i++)
			if(books[i] != null){
				Destroy(books[i].gameObject);
				books[i] = null;
			}
	}

	public void Create_Book_Bolt() {

		int bookIndex = -1;

		for(int i = 0; i < MAX_BOOKS; i++) {
			if(books[i] != null) {
				bookIndex = i;
				i = MAX_BOOKS;
			}
		}
		if(bookIndex < 0) return;

		GameObject projectile = _Create_Book_Bolt(books[bookIndex].transform.position);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
		}

		FireAtTarget(projectile,GameObject.Find(boltTarget).transform.position,books[bookIndex].transform.position,40f);

		bookBolts[bookIndex] = projectile.GetComponent<InstantiatedEntityCleanup>();
		bookBoltTimer = BOOK_BOLT_DELAY;

		Destroy(books[bookIndex].gameObject);
		books[bookIndex] = null;
		
	}

	public override void TriggerOnHit(Drifter target_drifter, bool isProjectle, AttackHitType hitType){
		if(hitType != AttackHitType.HIT) return;
		bookBoltTimer = BOOK_BOLT_DELAY;
		boltTarget = target_drifter.gameObject.name;
	}


	public void Create_Explosion() {
		Create_Explosion(drifter.attacks.AttackType);
	}

	//Creates a normal projectile
	private void Create_Explosion(DrifterAttackType p_attack) {

		//Let Orro have only one of any given attack projectile on screen at a time
		GameObject projectile;

		switch(p_attack) {
			case DrifterAttackType.Ground_Q_Neutral:
				projectile = GameController.Instance.CreatePrefab("Orro_Jab_Explosion", transform.position + new Vector3(3f * movement.Facing,3f,0), transform.rotation,drifter.peerID);
				break;

			case DrifterAttackType.Ground_Q_Up:
				projectile = GameController.Instance.CreatePrefab("Orro_Up_Ground_Explosion", transform.position + new Vector3(2f * movement.Facing,6.7f,0), transform.rotation,drifter.peerID);
				break;

			case DrifterAttackType.Ground_Q_Side:
				projectile = GameController.Instance.CreatePrefab("Orro_Side_Ground_Explosion", transform.position + new Vector3(.6f *movement.Facing,3f,0), transform.rotation,drifter.peerID);
				break;

			default:
				return;
		}

		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		foreach (HitboxCollision hitbox in projectile.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.NextID;
			hitbox.Facing = movement.Facing;
	   }
	   SetObjectColor(projectile);
	   for(int i = 0; i < explosions.Length; i++){
	   		if(explosions[i] == null){
	   			explosions[i] = projectile.GetComponent<InstantiatedEntityCleanup>();
	   			explosionTypes[i] = p_attack;
	   		}
	   }
	  

	}
	/*
		Unique particle spawn Functions
	*/

	//Spawns a page particle behind orro
	public void page() {
		movement.spawnJuiceParticle(transform.position + new Vector3(0,1,0),MovementParticleMode.Orro_Page, false);
	}

	//Spawns a page particle in front of orro
	public void pageFlip() {
		movement.spawnJuiceParticle(transform.position + new Vector3(movement.Facing * 1.5f,1,0),MovementParticleMode.Orro_Page, true);
	}

	//Spawns a boost ring particle for orros up special
	public void boost() {
		movement.spawnJuiceParticle(transform.position + new Vector3(0,2,0),MovementParticleMode.Orro_Boost, false);
	}


	//Overloads orro's return to idle command
	public new void returnToIdle() {
		base.returnToIdle();
		specialCharge = 0;
		beanIsCharging = false;
		listeningForDirection = false;
		
	}

	public override void clearMasterhitVars() {
		base.clearMasterhitVars();
		listeningForDirection = false;
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override void Serialize(BinaryWriter bw) {
		base.Serialize(bw);

		bw.Write(listeningForDirection);
		bw.Write(beanIsCharging);
		bw.Write(beanFollowing);
		
		bw.Write(neutralSpecialReleaseDelay);
		
		bw.Write(heldDirection.x);
		bw.Write(heldDirection.y);
		bw.Write(targetPos.x);
		bw.Write(targetPos.y);

		bw.Write(boltTarget);


		if(bean == null)
			bw.Write(false);
		else{
			bw.Write(true);
			bean.Serialize(bw);
		}

		for(int i = 0; i <explosions.Length; i++){
			if(explosions[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				bw.Write((int)explosionTypes[i]);
				explosions[i].Serialize(bw);
			}
		}

		for(int i = 0; i < MAX_BOOKS; i++){
			if(books[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				books[i].Serialize(bw);
			}
		}

		for(int i = 0; i < MAX_BOOKS; i++){
			if(bookBolts[i] == null)
				bw.Write(false);
			else{
				bw.Write(true);
				bookBolts[i].Serialize(bw);
			}
		}
	}

	//Rolls back the entity to a given frame state
	public override void Deserialize(BinaryReader br) {

		base.Deserialize(br);

		listeningForDirection = br.ReadBoolean();
		beanIsCharging = br.ReadBoolean();
		beanFollowing = br.ReadBoolean();
		
		neutralSpecialReleaseDelay = br.ReadInt32();
		
		heldDirection.x = br.ReadSingle();
		heldDirection.y = br.ReadSingle();
		targetPos.x = br.ReadSingle();
		targetPos.y = br.ReadSingle();

		boltTarget = br.ReadString();

		if(br.ReadBoolean()){
				if(bean == null)spawnBean();
				bean.Deserialize(br);
			}
			else if(bean != null){
				Destroy(bean.gameObject);
				bean = null;
			}


		for(int i = 0; i < explosions.Length; i++){
			if(br.ReadBoolean()){
				explosionTypes[i] = (DrifterAttackType) br.ReadInt32();
				if(explosions[i] == null)Create_Explosion(explosionTypes[i]);
				explosions[i].Deserialize(br);
			}
			else if(explosions[i] != null){
				Destroy(explosions[i].gameObject);
				explosions[i] = null;
			}
		}

		for(int i = 0; i <MAX_BOOKS; i++){
			if(br.ReadBoolean()){
				if(books[i] == null) books[i] = _Create_Book(transform.position).GetComponent<InstantiatedEntityCleanup>();
				books[i].Deserialize(br);
			}
			else if(books[i] != null){
				Destroy(books[i].gameObject);
				books[i] = null;
			}
		}

		for(int i = 0; i <MAX_BOOKS; i++){
			if(br.ReadBoolean()){
				if(bookBolts[i] == null) bookBolts[i] = _Create_Book_Bolt(transform.position).GetComponent<InstantiatedEntityCleanup>();
				bookBolts[i].Deserialize(br);
			}
			else if(bookBolts[i] != null){
				Destroy(bookBolts[i].gameObject);
				bookBolts[i] = null;
			}
		}
	}
}