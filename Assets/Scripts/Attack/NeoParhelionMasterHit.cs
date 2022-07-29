using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoParhelionMasterHit : MasterHit
{

    //Inhereted Roll Methods

	Vector2 HeldDirection;
	public GrabHitboxCollision Up_W_Grab;
    bool listeningForDirection = false;


    override public void UpdateFrame()
    {
        base.UpdateFrame();

        if(listeningForDirection)
        {
            Vector2 TestDirection = new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
            HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
            
        }

    }

    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);
    }

    public void listenForDirection()
    {
        listeningForDirection = true;
    }

    public new void returnToIdle()
    {
        base.returnToIdle();
        Up_W_Grab.victim = null;
        listeningForDirection = false;
    }

    public void UpWThrow()
    {
    	
    	if(Up_W_Grab.victim == null)
    		return;

    	if(HeldDirection.y < 0)drifter.PlayAnimation("W_Up_Down");
    	else if(HeldDirection.x != 0)
    	{
    		drifter.PlayAnimation("W_Up_Forward");
    		if(HeldDirection.x * movement.Facing < 0)
    		{
    			movement.flipFacing();
    			foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))hitbox.Facing = movement.Facing;
    		}
    	}
    	else drifter.PlayAnimation("W_Up_Up");
    	HeldDirection = Vector2.zero;
    	Up_W_Grab.victim = null;
        listeningForDirection = false;
    }

    //Flips the direction the charactr is facing mid move)
    public void invertDirection()
    {
        movement.flipFacing();
    }

    public void dust()
    {

        if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(4f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
    }
}
