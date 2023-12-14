using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoParhelionMasterHit : MasterHit
{

	//Inhereted Roll Methods
	public GrabHitboxCollision Up_W_Grab;

	override public void UpdateFrame() {
		base.UpdateFrame();
	}

	public new void returnToIdle() {
		base.returnToIdle();
		Up_W_Grab.victim = null;
	}

	public void W_Up_Slam() {
		if(Up_W_Grab.victim != null) playState("W_Up_Down");
	}

	//Flips the direction the charactr is facing mid move)
	public void invertDirection() {
		movement.flipFacing();
	}

	public void dust() {

		if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(4f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
	public override MasterhitRollbackFrame SerializeFrame() {
		MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
		baseFrame.CharacterFrame = new ParhelionRollbackFrame() {};

		return baseFrame;
	}

	//Rolls back the entity to a given frame state
	public override void DeserializeFrame(MasterhitRollbackFrame p_frame) {
		DeserializeBaseFrame(p_frame);
	}

}

public class ParhelionRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
	
}

