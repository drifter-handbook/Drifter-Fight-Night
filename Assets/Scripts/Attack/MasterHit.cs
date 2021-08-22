using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MasterHit : MonoBehaviour, IMasterHit
{

    public string assetPathName;

    protected Drifter drifter;
    protected NetworkHost host;
    protected Rigidbody2D rb;
    protected PlayerMovement movement;
    protected PlayerStatus status;
    protected float gravityScale;
    protected PlayerAttacks attacks;
    protected Animator anim;
    protected WalkOff ledgeDetector;

    protected float framerateScalar =.0833333333f;


    public int facing;

    protected bool isHost = false;

    protected bool Empowered = false;

    protected Vector3 savedVelocity;

    protected bool savingVelocity = false;

    protected float terminalVelocity;


    //Listener Bools

    protected bool specialTappedFlag = false;

    protected bool specialReleasedFlag = false;

    protected bool lightTappedFlag = false;

    protected bool verticalCancelFlag = false;

    protected bool movementCancelFlag = false;

    protected bool activeCancelFlag = false;

    protected bool listeningForGroundedFlag = false;

    protected bool queuedStateTrigger = false;

    protected bool jumpFlag = false;

    protected int specialCharge = 0;

    protected int specialLimit = -1;

    protected string queuedState = "";

    protected bool attackWasCanceled = false;


    //Every frame, listen for a given event if the flag is active
    protected void FixedUpdate()
    {
        if(!isHost)return;
        attackWasCanceled = true;
        //Clear all flags if the character is dead or stunned by an opponent
        if(status.HasEnemyStunEffect() || movement.ledgeHanging)
        {
            clearMasterhitVars();
            movement.terminalVelocity = terminalVelocity;
        }
        else if(
            (listeningForGroundedFlag && movement.grounded)||
            (specialReleasedFlag && !drifter.input[0].Special)||
            (specialTappedFlag && checkForSpecialTap())||
            (specialLimit > 0 && specialCharge >= specialLimit)
            )

        {
            attackWasCanceled = false;
            playQueuedState();
            clearMasterhitVars();
        }

        else if(movementCancelFlag && movement.currentJumps >0 && drifter.doubleTappedX() )
        {
            if(movement.dash())
            {
                movement.techParticle();
                clearMasterhitVars();
            }            
        }
        else if(verticalCancelFlag && drifter.doubleTappedY() && drifter.input[0].MoveY <0)
        {
            playQueuedState();
            movement.techParticle();
            returnToIdle();
        }
        else if(activeCancelFlag && drifter.input[0].Guard && !drifter.input[1].Guard)
        {
            movement.techParticle();
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0f);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            clearMasterhitVars();
            movement.terminalVelocity = terminalVelocity;
            playState("Guard");
            drifter.guarding = true;
            movement.jumping = false;
            unpauseGravity();
            

        }
        else if((activeCancelFlag || jumpFlag)&& drifter.input[0].Jump && !drifter.input[1].Jump && movement.currentJumps>0)
        {
            movement.jumping = false;
            movement.techParticle();
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,0f);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0f);
            clearMasterhitVars();
            movement.terminalVelocity = terminalVelocity;
            movement.jump();
            unpauseGravity();
            

        }
        else if(lightTappedFlag && checkForLightTap())
        {
            queuedStateTrigger = true;
            lightTappedFlag = false;
        }
        else
            attackWasCanceled = false;
    }

    //Flag the character to begin listen for a given event
    public void listenForGrounded(string stateName)
    {
        if(!isHost)return;
        queueState(stateName);
        listeningForGroundedFlag = true;
    }

    public void listenForSpecialTapped(string stateName)
    {
        if(!isHost)return;
        queueState(stateName);
        specialTappedFlag = true;
    }

     public void listenForLightTapped(string stateName)
    {
        if(!isHost)return;
        queueState(stateName);
        lightTappedFlag = true;
    }

    public void listenForSpecialReleased(string stateName)
    {
        if(!isHost)return;
        queueState(stateName);
        specialReleasedFlag = true;
    }

    public void listenForMovementCancel()
    {
        if(!isHost)return;
        movementCancelFlag = true;
    }
    public void listenForVerticalCancel(string stateName)
    {
        if(!isHost)return;
        queueState(stateName);
        verticalCancelFlag = true;
    }

    public void listenForActiveCancel()
    {
        if(!isHost)return;
        activeCancelFlag = true;
    }

    public void listenForJumpCancel()
    {
        if(!isHost)return;
        jumpFlag = true;
    }


    public void addCharge(int charge =1)
    {
        if(!isHost)return;
        specialCharge += charge;
    }

    public void listenForLedge()
    {
        if(!isHost)return;

        ledgeDetector.togglePreventWalkoff();
    }

    public void listenForLedge(bool toggle)
    {
        if(!isHost)return;

        ledgeDetector.setPreventWalkoff(toggle);
    }


    //Clear all flags
    public void clearMasterhitVars()
    {
        if(!isHost)return;
        specialTappedFlag = false;
        specialReleasedFlag = false;
        lightTappedFlag = false;
        verticalCancelFlag = false;
        movementCancelFlag = false;
        activeCancelFlag = false;
        listeningForGroundedFlag = false;
        queuedStateTrigger = false;
        jumpFlag = false;
        specialLimit = -1;
        queuedState = "";
        ledgeDetector.setPreventWalkoff(false);
    }


    // Start is called before the first frame update
    void Awake()
    {
        //Is Host
        isHost = GameController.Instance.IsHost;

        Resources.LoadAll("/Characters/" + assetPathName);
        Resources.LoadAll("/Projectiles/" + assetPathName);

        if(!isHost)return;
        host = GameController.Instance.host;
        //Parent Components
        drifter = transform.parent.gameObject.GetComponent<Drifter>();
        rb = drifter.GetComponent<Rigidbody2D>();
        movement = drifter.GetComponent<PlayerMovement>();
        attacks = drifter.GetComponent<PlayerAttacks>();
        status = drifter.GetComponent<PlayerStatus>();
        anim = drifter.GetComponent<Animator>();
        ledgeDetector = GetComponentInChildren<WalkOff>();
        ledgeDetector.rb = rb;

        terminalVelocity = movement.terminalVelocity;
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
        savedVelocity = new Vector3(Mathf.Clamp(rb.velocity.x,-45f,45f), Mathf.Clamp(rb.velocity.y,-45f,45f));
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
    
    //Dynamically adjust walk speed to match walk cycle animations
    public void walkCycleSpeedSync(float speed)
    {
        if(!isHost)return;
        movement.walkSpeed = speed;
    }

    public void returnToIdle()
	{
        if(!isHost)return;
		movement.jumping = false;
		unpauseGravity();
        movement.terminalVelocity = terminalVelocity;
        clearMasterhitVars();
    	drifter.returnToIdle();
        movement.updateFacing();
        attacks.UpdateInput();
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

    public bool checkForLightTap()
    {
        if(!isHost)return false;
        int state = 0;
        for(int i = 0; i < 8; i++)
        {
            if(state >0 && !drifter.input[i].Light)return true;
            else if(state == 0 && drifter.input[0].Light) state++;
        }
        return false;
    }

    public bool checkForSpecialTap()
    {
        if(!isHost)return false;
        int state = 0;
        for(int i = 0; i < 8; i++)
        {
            if(state >0 && !drifter.input[i].Special)return true;
            else if(state == 0 && drifter.input[0].Special) state++;
        }
        return false;

    }

    public void queueState(string stateName)
    {
        if(!isHost)return;
        queuedState = stateName;
    }

    public void playQueuedState()
    {
        if(!isHost || queuedState.Equals(""))return;
        playState(queuedState);
    }

    public void triggerQueuedState()
    {
        if(!isHost || queuedState.Equals("") || !queuedStateTrigger)return;
        applyEndLag(8);
        playState(queuedState);
        clearMasterhitVars();
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
