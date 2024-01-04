using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
	public int W_Down_Projectiles = 3;
	public GameObject GrabConnectionPoint;
	Vector2 HeldDirection = Vector2.zero; 

	public TetherRange tether;
	GameObject g_Tether_Tongue;
	GameObject g_Tether_Head;
	bool tongueRetracting = false;
	Vector3 tetherPoint = Vector3.zero;

	GameObject[] kunais = new GameObject[3];

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

		if(projnum >0) {
			fireKunaiGroundLine(projnum); 
			projnum--;
		}
		else if(projnum <0) {
			fireKunaiAirLine(projnum);
			projnum++;
		}

		if(g_Tether_Tongue != null) {
			g_Tether_Tongue.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

			if(tetherPoint != Vector3.zero)
				g_Tether_Tongue.GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(g_Tether_Tongue.GetComponent<Rigidbody2D>().position, tetherPoint,3);

			else if(tongueRetracting)
				g_Tether_Tongue.GetComponent<Rigidbody2D>().position = Vector2.MoveTowards(g_Tether_Tongue.GetComponent<Rigidbody2D>().position, (g_Tether_Head != null) ?g_Tether_Head.transform.position:GrabConnectionPoint.transform.position,1);


			g_Tether_Tongue.GetComponentInChildren<LineRenderer>().SetPosition(0,g_Tether_Tongue.transform.position);
			g_Tether_Tongue.GetComponentInChildren<LineRenderer>().SetPosition(1,(g_Tether_Head != null) ?g_Tether_Head.transform.position:GrabConnectionPoint.transform.position);
		}
		else {
			tongueRetracting = false;
		}


		//Update Child Frames
		foreach(GameObject kunai in kunais)
			if(kunai != null)
				kunai.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
	
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


	void CreateTongue(Vector3 pos) {

		g_Tether_Tongue = GameController.Instance.CreatePrefab("SwordFrog_Tongue", pos, transform.rotation);
		g_Tether_Tongue.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

		g_Tether_Tongue.GetComponent<Rigidbody2D>().velocity = rb.velocity + new Vector2(movement.Facing * 75f, 0f);

		SetObjectColor(g_Tether_Tongue);

		g_Tether_Tongue.GetComponent<RemoteProjectileUtil>().hit = this;
	}

	public void DeleteTongue() {
		if(g_Tether_Tongue!= null) Destroy(g_Tether_Tongue);
		if(g_Tether_Head!= null) Destroy(g_Tether_Head);
		g_Tether_Tongue = null;
		g_Tether_Head = null;

	}

	public void SpawnGrabTongue() {
		DeleteTongue();

		CreateTongue(GrabConnectionPoint.transform.position);

		foreach (HitboxCollision hitbox in g_Tether_Tongue.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}
	
		g_Tether_Tongue.GetComponentInChildren<LineRenderer>().SetPosition(0, g_Tether_Tongue.transform.position);
		g_Tether_Tongue.GetComponentInChildren<LineRenderer>().SetPosition(1, GrabConnectionPoint.transform.position);

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
		projnum = W_Down_Projectiles;
	}

	public void downSpecialProjectileAir() {
		projnum = -1 * W_Down_Projectiles;
	}

	void fireKunaiGroundLine(int index) {
		
		Vector3 size = new Vector3(10f * movement.Facing, 10f, 1f);
		Vector3 pos = new Vector3(.2f * movement.Facing, 2.7f, 1f);


		GameObject kunai = GameController.Instance.CreatePrefab("Kunai", transform.position + new Vector3(1.5f * movement.Facing, 1.5f + W_Down_Projectiles/5f + (W_Down_Projectiles - index) * .6f, 0), transform.rotation);

		kunai.transform.localScale = size;
		kunai.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + 50f * movement.Facing, 0);

		foreach (HitboxCollision hitbox in kunai.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;              
			hitbox.Facing = movement.Facing;
		}

		refreshHitboxID();
		kunais[Mathf.Abs(index) - 1] = kunai;

	}


	void fireKunaiAirLine(int index) {

		Vector3 size = new Vector3(10f, 10f, 1f);
		Vector3 pos = new Vector3(.2f * movement.Facing, 2.7f, 1f);

	
		float degreesA = movement.Facing >0 ? (335f  + index * 4f) : (215f  - index * 4f);
		float radiansA = degreesA * Mathf.PI/180f;
		float posDegrees = (movement.Facing >0 ? 335f  : 215f);
		float posRadians = posDegrees * Mathf.PI/180f;

		GameObject kunai = GameController.Instance.CreatePrefab("Kunai", transform.position + new Vector3(movement.Facing * (-.5f - index/2f), index/-2f -.9f)
																 + pos, 
																 Quaternion.Euler(0,0,posDegrees));


		kunai.transform.localScale = size;
		kunai.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + (Mathf.Cos(posRadians) *50f), Mathf.Sin(posRadians)*50f);

		foreach (HitboxCollision hitbox in kunai.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}

		refreshHitboxID();
		kunais[Mathf.Abs(index) - 1] = kunai;
	}

	GameObject createKunai() {
		GameObject kunai = GameController.Instance.CreatePrefab("Kunai", transform.position,transform.rotation);
		kunai.transform.localScale = new Vector3(10f, 10f, 1f);

		foreach (HitboxCollision hitbox in kunai.GetComponentsInChildren<HitboxCollision>(true)) {
			hitbox.parent = drifter.gameObject;
			hitbox.AttackID = attacks.AttackID;
			hitbox.Facing = movement.Facing;
		}

		refreshHitboxID();
		return kunai;
	}


	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();

		BasicProjectileRollbackFrame[] kunaiList = new BasicProjectileRollbackFrame[3];

		for(int i = 0; i < kunaiList.Length; i++)// (GameObject kunai in kunais)
			kunaiList[i] = (kunais[i] != null ? kunais[i].GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null);

		baseFrame.CharacterFrame = new SwordfrogRollbackFrame()  {
			ListeningForDirection = listeningForDirection,
			Delaytime = delaytime,
			Projnum = projnum,
			Kunais = kunaiList,

		};

		return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);
		SwordfrogRollbackFrame sf_frame = ((SwordfrogRollbackFrame)p_frame.CharacterFrame);

		listeningForDirection = sf_frame.ListeningForDirection;
		delaytime = sf_frame.Delaytime;
		projnum = sf_frame.Projnum;


		for(int i = 0; i <3; i++) {
			if(sf_frame.Kunais[i] != null){
				if(kunais[i] == null)kunais[i] = createKunai();
				kunais[i].GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(sf_frame.Kunais[i]);
			}
			else {
				Destroy(kunais[i]);
				kunais[i] = null;
			}  
		} 
	}

}

public class SwordfrogRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
	public bool ListeningForDirection;
	public int Delaytime;
	public int Projnum;


	public BasicProjectileRollbackFrame[] Kunais;
}
	