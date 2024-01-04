using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
	public int W_Down_Projectiles = 3;
	Vector2 HeldDirection = Vector2.zero; 


	GameObject g_tongue;
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
		}

		if(movement.ledgeHanging || status.HasEnemyStunEffect()) {
			listeningForDirection = false;
			projnum = 0;
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
	