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
        Vector2 TestDirection = new Vector2(drifter.input.MoveX,drifter.input.MoveY);
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

    		UnityEngine.Debug.Log("FORWARD");
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

    public void slamRestitute()
    {
    	if(!isHost || !movement.grounded)return;
    	movement.spawnJuiceParticle(transform.position, MovementParticleMode.Restitution);
    }

    public void setTerminalVelocity(float vel)
    {
        if(!isHost)return;
        movement.canLandingCancel = false;  
        movement.terminalVelocity = vel;
    }

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,75f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
