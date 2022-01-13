using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoParhelionMasterHit : MasterHit
{

    //Inhereted Roll Methods

	Vector2 HeldDirection;
	public GrabHitboxCollision Up_W_Grab;


	public void saveDirection()
    {
        if(!isHost)return;
        Vector2 TestDirection = new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
        HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
    }

    public void UpWThrow()
    {
    	if(!isHost)return;
    	saveDirection();
    	if(Up_W_Grab.victim != null)attacks.resetRecovery();
    	else return;
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
    }

    //Causes a non-aerial move to cancle on htiing the ground
    public void cancelSideQ()
    {
        if(!isHost)return;
        movement.canLandingCancel = true;
    }

    //Flips the direction the charactr is facing mid move)
    public void invertDirection()
    {
        if(!isHost)return;
        movement.flipFacing();
    }

    public void dust()
    {
        if(!isHost)return;

        if(movement.grounded)movement.spawnJuiceParticle(transform.position + new Vector3(4f * movement.Facing,0,0),MovementParticleMode.Dash_Cloud, true);
    }
}
