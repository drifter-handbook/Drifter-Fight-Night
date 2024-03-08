using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class OrroReworkMasterHit : MasterHit {
	const int MAX_BOOKS = 3;
	const int BOOK_BOLT_DELAY = 10;

	//For orros porjectile normals
	GameObject[] explosions = new GameObject[17];

	//W_Down data
	GameObject[] books = new GameObject[] {null,null,null};
	GameObject[] bookBolts = new GameObject[] {null,null,null};
	int bookBoltTimer = 0;
	Drifter boltTarget = null;

	//Bean data
	BeanWrangler bean;
	GameObject beanObject;
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
			beanObject = null;
			Empowered = false;
		}
		//Make a new bean projectile when orro respawns
		else if(beanObject == null) {
			spawnBean();
		}
		//Send bean orros position and direction so he can follow on a delay
		else {
			targetPos = rb.position - new Vector2(-1f * movement.Facing,3f);
			bean.addBeanState(targetPos,movement.Facing);

			Empowered = !beanFollowing || Vector3.Distance(targetPos,bean.rb.position) > 3.8f;
		}

		bean.UpdateFrame();

		foreach(GameObject exp in explosions)
			if(exp != null) exp.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

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

		beanObject = GameController.Instance.CreatePrefab("Bean", transform.position - new Vector3(-1f * movement.Facing, 1f), transform.rotation);
		foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}

		bean = beanObject.GetComponent<BeanWrangler>();

		foreach (HurtboxCollision hurtbox in beanObject.GetComponentsInChildren<HurtboxCollision>(true))
			hurtbox.owner = drifter.gameObject;
		
		bean.facing = movement.Facing;
		SetObjectColor(beanObject);
		bean.color = drifter.GetColor();

	}

	//Refreshes each of beans hitbox ids so he can keep doing damage
	private void refreshBeanHitboxes() {
		bean.facing = movement.Facing;

		foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true)) {
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
		projectile = GameController.Instance.CreatePrefab("Orro_Book", pos, transform.rotation);
		projectile.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
	   	SetObjectColor(projectile);
	   	projectile.transform.SetParent(drifter.gameObject.transform);

	   	return projectile;
	}

	private GameObject _Create_Book_Bolt(Vector2 pos){
		GameObject projectile = GameController.Instance.CreatePrefab("Orro_Book_Bolt", pos, transform.rotation);
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

		GameObject projectile = _Create_Book(transform.position + new Vector3(Mathf.Sin(angle) * movement.Facing * BOOK_RADIUS ,Mathf.Cos(angle) *BOOK_RADIUS + 3f,0));

	   	books[bookIndex] = projectile;
	}

	void removeAllBooks() {
		for(int i = 0; i < MAX_BOOKS; i++)
			if(books[i] != null){
				Destroy(books[i]);
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

		FireAtTarget(projectile,boltTarget.gameObject.transform.position,books[bookIndex].transform.position,40f);

		bookBolts[bookIndex] = projectile;
		bookBoltTimer = BOOK_BOLT_DELAY;

		Destroy(books[bookIndex]);
		books[bookIndex] = null;
		
	}

	public override void TriggerOnHit(Drifter target_drifter, bool isProjectle, AttackHitType hitType){
		if(hitType != AttackHitType.HIT) return;
		bookBoltTimer = BOOK_BOLT_DELAY;
		boltTarget = target_drifter;
	}


	public void Create_Explosion() {
		Create_Explosion(drifter.attacks.AttackType);
	}

	//Creates a normal projectile
	private void Create_Explosion(DrifterAttackType p_attack) {

		GameObject projectile;

		switch(p_attack) {
			case DrifterAttackType.Ground_Q_Neutral:
				projectile = GameController.Instance.CreatePrefab("Orro_Jab_Explosion", transform.position + new Vector3(3f * movement.Facing,3f,0), transform.rotation);
				break;

			case DrifterAttackType.Ground_Q_Up:
				projectile = GameController.Instance.CreatePrefab("Orro_Up_Ground_Explosion", transform.position + new Vector3(2f * movement.Facing,6.7f,0), transform.rotation);
				break;

			case DrifterAttackType.Ground_Q_Side:
				projectile = GameController.Instance.CreatePrefab("Orro_Side_Ground_Explosion", transform.position + new Vector3(.6f *movement.Facing,3f,0), transform.rotation);
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
	   explosions[(int)p_attack] = projectile;

	}

	// //Creates a side air projectile
	// public void SpawnSideAir() {

	// 	Vector3 pos = new Vector3(7f * movement.Facing,2.7f,0);
		
	// 	GameObject scratch = GameController.Instance.CreatePrefab("Orro_Sair_Proj", transform.position + pos, transform.rotation);
	// 	scratch.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
	// 	foreach (HitboxCollision hitbox in scratch.GetComponentsInChildren<HitboxCollision>(true)) {
	// 		hitbox.parent = drifter.gameObject;
	// 		hitbox.AttackID = attacks.AttackID;
	// 		hitbox.Facing = movement.Facing;
	//    }
	// }

	// //Creates a side air projectile
	// public void SpawnNeutralAir() {

	// 	RaycastHit2D ray = Physics2D.Raycast(transform.position+ new Vector3(0,1f),new Vector3(movement.Facing * 7f/5f,-5f/5f,0),5f,1);
		
	// 	Vector3 pos = new Vector3((ray.distance +1) * movement.Facing,-1* ray.distance +1f,0);
	// 	if(ray.distance ==0)pos = new Vector3(8* movement.Facing,-4,0);
		
	// 	GameObject scratch = GameController.Instance.CreatePrefab("Orro_Nair_Proj", transform.position + pos, transform.rotation);
	// 	scratch.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
	// 	foreach (HitboxCollision hitbox in scratch.GetComponentsInChildren<HitboxCollision>(true)) {
	// 		hitbox.parent = drifter.gameObject;
	// 		hitbox.AttackID = attacks.AttackID;
	// 		hitbox.Facing = movement.Facing;
	//    }

	// }

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
		//removes all of oors projectile normals when he supers
		for(int i =0; i < explosions.Length; i++){
			Destroy(explosions[i]);
			explosions[i] = null;
		}
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();

		BasicProjectileRollbackFrame[] p_Explosions = new BasicProjectileRollbackFrame[17];
		BasicProjectileRollbackFrame[] p_Books = new BasicProjectileRollbackFrame[MAX_BOOKS];
		BasicProjectileRollbackFrame[] p_BookBolts = new BasicProjectileRollbackFrame[MAX_BOOKS];

		for(int i = 0; i < 17; i++)
			p_Explosions[i] = (explosions[i] != null) ? explosions[i].GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null;

		for(int i = 0; i < MAX_BOOKS; i++) {
			p_Books[i] = (books[i] != null) ? books[i].GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null;
			p_BookBolts[i] = (bookBolts[i] != null) ? bookBolts[i].GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null;
		}

		baseFrame.CharacterFrame = new OrroRollbackFrame()  {
			Bean = (bean != null) ? bean.SerializeFrame(): null,
			ListeningForDirection = listeningForDirection,
			HeldDirection = heldDirection,
			BeanIsCharging = beanIsCharging,
			BeanFollowing = beanFollowing,
			TargetPos = targetPos,
			NeutralSpecialReleaseDelay = neutralSpecialReleaseDelay,
			Explosions = p_Explosions,
			BoltTarget = boltTarget,
		};


		return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);

		OrroRollbackFrame orro_frame = (OrroRollbackFrame)p_frame.CharacterFrame;

		
		listeningForDirection = orro_frame.ListeningForDirection;
		heldDirection = orro_frame.HeldDirection;
		beanIsCharging = orro_frame.BeanIsCharging;
		beanFollowing = orro_frame.BeanFollowing;
		targetPos = orro_frame.TargetPos;
		neutralSpecialReleaseDelay = orro_frame.NeutralSpecialReleaseDelay;
		boltTarget = orro_frame.BoltTarget;

		//handle each normal projectile
		for(int i = 0; i < 17; i++) {

			if(orro_frame.Explosions[i] != null) {
				if(explosions[i] == null)Create_Explosion((DrifterAttackType)i);
				explosions[i].GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(orro_frame.Explosions[i]);
				}
				//Projectile does not exist in rollback frame
			else {
				Destroy(explosions[i]);
				explosions[i] = null;
			}
		}

		//Handle each W_Down projectile

		for(int i = 0; i < MAX_BOOKS; i++) {

			if(orro_frame.Books[i] != null) {
				if(books[i] == null)_Create_Book(transform.position);
				books[i].GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(orro_frame.Books[i]);
			}
			else {
				Destroy(books[i]);
				books[i] = null;
			}

			if(orro_frame.BookBolts[i] != null) {
				if(bookBolts[i] == null)_Create_Book_Bolt(transform.position);
				bookBolts[i].GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(orro_frame.BookBolts[i]);
			}
			else {
				Destroy(bookBolts[i]);
				bookBolts[i] = null;
			}
		}


		//Hnadle Bean
		if(orro_frame.Bean != null) {
			if(beanObject == null)spawnBean();
			bean.DeserializeFrame(orro_frame.Bean);
		}
		//Projectile does not exist in rollback frame
		else {
			Destroy(beanObject);
			bean = null;
			beanObject = null;
		}

	}

}

public class OrroRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
	
	public BeanRollbackFrame Bean;
	public bool ListeningForDirection;
	public Vector2 HeldDirection;
	public bool BeanIsCharging;
	public bool BeanFollowing;   
	public Vector3 TargetPos;
	public int NeutralSpecialReleaseDelay;
	public BasicProjectileRollbackFrame[] Explosions;
	public BasicProjectileRollbackFrame[] Books;
	public BasicProjectileRollbackFrame[] BookBolts;
	public Drifter BoltTarget;
	
}