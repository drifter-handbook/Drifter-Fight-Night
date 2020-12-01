﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterHit : MonoBehaviour, IMasterHit
{
    protected Drifter drifter;
    protected NetworkHost host;
    protected Rigidbody2D rb;
    protected PlayerMovement movement;
    protected PlayerStatus status;
    protected float gravityScale;
    protected PlayerAttacks attacks;
    protected Animator anim;

    public int facing;

    protected bool isHost = false;

    protected bool listeningForInput = false;

    protected bool Empowered = false;


    // Start is called before the first frame update
    void Awake()
    {
        //Is Host
        isHost = GameController.Instance.IsHost;

        if(!isHost)return;
        host = GameController.Instance.host;
        //Paretn Components
        drifter = transform.parent.gameObject.GetComponent<Drifter>();
        rb = drifter.GetComponent<Rigidbody2D>();
        movement = drifter.GetComponent<PlayerMovement>();
        attacks = drifter.GetComponent<PlayerAttacks>();
        status = drifter.GetComponent<PlayerStatus>();
        anim = drifter.GetComponent<Animator>();

        gravityScale = rb.gravityScale;
    }

    public void setYVelocity(float y)
    {
        if(!isHost)return;
        rb.velocity = new Vector2(rb.velocity.x,y);
    }

    public void setXVelocity(float x)
    {
        if(!isHost)return;
        rb.velocity = new Vector2(movement.Facing * x,rb.velocity.y);
    }

    public void applyEndLag(float statusDuration)
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,statusDuration);
    }

    public void applyArmour(float statusDuration)
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,statusDuration);
    }

    public void pauseGravity()
    {
        if(!isHost)return;
        movement.cancelJump();
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void unpauseGravity()
    {
        if(!isHost)return;
        if(rb.gravityScale != gravityScale) rb.gravityScale=gravityScale;
        movement.gravityPaused= false;
    }

    public void refreshHitboxID()
    {
        if(!isHost)return;
        attacks.SetMultiHitAttackID();
    }

    //Allows for jump and shield canceling of moves. Returns true if it's condition was met
    public bool TransitionFromChanneledAttack()
    {
        if(!isHost)return false;

        if(drifter.input.Guard)
        {
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            playState(drifter.GuardStateName);
            drifter.guarding = true;
            unpauseGravity();
            return true;
        }
        else if(drifter.input.Jump && movement.currentJumps>0){
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            movement.jump();
            unpauseGravity();
            return true;
        }

        return false;
    }


    public void returnToIdle()
	{
        if(!isHost)return;
		movement.jumping = false;
		unpauseGravity();
		listeningForInput = false;
    	drifter.returnToIdle();
    }

    public void playState(string state)
    {
        if(!isHost)return;
    	drifter.PlayAnimation(state);
    }

    public void playStateIfEmpowered(string state)
    {
        if(!isHost)return;
        if(Empowered)drifter.PlayAnimation(state);
    }

    public void playStateIfEmpoweredOrRetunToIdle(string state)
    {
        if(!isHost)return;
        if(Empowered)drifter.PlayAnimation(state);
        else returnToIdle();
    }

    public abstract void roll();

    public abstract void rollGetupStart();
    

    public abstract void rollGetupEnd();
   
}
