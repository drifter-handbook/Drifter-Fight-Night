using System.Collections;
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

    protected float framerateScalar =.0833333333f;


    public int facing;

    protected bool isHost = false;

    protected bool Empowered = false;

    protected bool continueJabFlag = false;

    protected Vector3 savedVelocity;

    protected bool savingVelocity = false;

    protected PolygonCollider2D frictionCollider;

    protected bool specialReleased = false;

    protected bool horizontalReleased = false;

    protected bool verticalReleased = false;

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

        frictionCollider = drifter.GetComponent<PolygonCollider2D>();

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

        if(movement.grounded && x >0) movement.spawnKickoffDust();
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
        status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,statusDuration * framerateScalar);
    }

    public void pauseGravity()
    {
        if(!isHost)return;
        savingVelocity = false;
        movement.cancelJump();
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void freezeGravity()
    {
        if(!isHost)return;
        savedVelocity = rb.velocity;
        savingVelocity = true;
        movement.cancelJump();
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void freezeGravity_Downward()
    {
        if(!isHost)return;
        savedVelocity = new Vector2(rb.velocity.x,Mathf.Max(rb.velocity.y,0));
        savingVelocity = true;
        movement.cancelJump();
        movement.gravityPaused= true;
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }

    public void unpauseGravity()
    {
        if(!isHost)return;
        if(savingVelocity)rb.velocity = savedVelocity;
        savingVelocity = false;
        if(rb.gravityScale != gravityScale) rb.gravityScale=gravityScale;
        movement.gravityPaused= false;
    }

    public void refreshHitboxID()
    {
        if(!isHost)return;
        attacks.SetMultiHitAttackID();
    }

    public void beginChanneledAttack()
    {
        specialReleased = false;
    }

    public void clearMasterhitVars()
    {
        specialReleased = false;   
    }

    //For charged attacks that store their current charge when canceled.
    //Press the button once to start charging. if the button is released and then pressed again, the state will play
    //0: Nothing happened (Executed as client, or no state-changing action occured)
    //1: The attack was canceled by a jump or shield input
    //2: The provided state was executed  
    public int chargeAttackPesistent(string stateName)
    {
        if(!isHost)return 0;

        if(cancelAttack())return 1;

        else if(movementCancel())return 1;
     
        else if(!drifter.input.Special && !specialReleased)specialReleased = true;

        else if(drifter.input.Special && specialReleased)
        {
            specialReleased = false;
            playState(stateName);
            return 2;
        }

        return 0;
    }

    //For charged moves that cannot store charge. While the button is held, the charge will persist.
    //When the button is released, the specified state will play.
    //0: Nothing happened (Executed as client, or no state-changing action occured)
    //1: The attack was canceled by a jump or shield input
    //2: The provided state was executed
    public int chargeAttackSingleUse(string stateName)
    {
        if(!isHost)return 0;

        else if(cancelAttack()) return 1;

        else if(movementCancel())return 1;

        if(!drifter.input.Special)
        {
            playState(stateName);
            return 2;
        }
        return 0;    

    }

    public bool movementCancel()
    {

        if(drifter.input.MoveX ==0 && !horizontalReleased)horizontalReleased = true;

        else if(drifter.input.MoveX != 0 && horizontalReleased && movement.grounded)
        {
            horizontalReleased = false;
            movement.roll();
            movement.techParticle();
            return true;
        }

        return false;

    }

    //Allows for jump and shield canceling of moves. Returns true if it's condition was met
    public bool cancelAttack()
    {
        if(!isHost)return false;

        if(drifter.input.Guard)
        {
            movement.techParticle();
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0f);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            playState("Guard");
            drifter.guarding = true;
            movement.jumping = false;
            unpauseGravity();
            return true;
        }
        else if(drifter.input.Jump && movement.currentJumps>0)
        {
            movement.jumping = false;
            movement.techParticle();
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0f);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            movement.jump();
            unpauseGravity();
            return true;
        }

        if(drifter.input.MoveY ==0 && !verticalReleased)verticalReleased = true;

        else if(drifter.input.MoveY < 0 && verticalReleased)
        {
            verticalReleased = false;
            movement.techParticle();
            returnToIdle();
            return true;
        }

        return false;
    }


    //Dynamically adjust walk speed to match walk cycle animations
    public void walkCycleSpeedSync(float speed)
    {
        if(!isHost)return;
        movement.walkSpeed = speed;
    }

    public void rechargeFoxtrot()
    {
        if(!isHost)return;
        movement.canDash = true;
    }

    public void returnToIdle()
	{
        if(!isHost)return;
		movement.jumping = false;
        specialReleased = false;
        if(drifter.input.MoveX ==0)movement.canDash = true;
		unpauseGravity();
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

    public void playStateIfGrounded(string state)
    {
        if(!isHost)return;
        if(movement.grounded)drifter.PlayAnimation(state);
    }

    public void playStateIfEmpoweredOrRetunToIdle(string state)
    {
        if(!isHost)return;
        if(Empowered)drifter.PlayAnimation(state);
        else returnToIdle();
    }

    public void checkForContinueJab()
    {
        if(!isHost)return;
        if(drifter.input.Light)continueJabFlag = true;
    }

    public void continueJab(string state)
    {
        if(!isHost)return;
        if(continueJabFlag)
        {
            applyEndLag(8);
            refreshHitboxID();
            continueJabFlag = false;
            playState(state);
        }
    }

    public void beginGuard()
    {
        if(!isHost)return;
        applyEndLag(2f * .0833333333f);
        drifter.perfectGuarding = true;
    }

    public void endPerfectGuard()
    {
        if(!isHost)return;
        drifter.perfectGuarding = false;
        if(drifter.guarding)drifter.PlayAnimation("Guard");
    }

    public void endParry()
    {
        if(!isHost)return;
        drifter.parrying = false;
    }


    public abstract void roll();

    public abstract void rollGetupStart();
    

    public abstract void rollGetupEnd();
   
}
