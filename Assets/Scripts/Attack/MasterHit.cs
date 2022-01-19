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

    protected bool dacusCancelFlag = false;

    protected bool knockdownFlag = false;


    //Every frame, listen for a given event if the flag is active
    protected void FixedUpdate()
    {
        if(!isHost)return;
        attackWasCanceled = true;
        //Clear all flags if the character is dead or stunned by an opponent

        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN))
        {
            drifter.knockedDown = true;
            knockdownFlag = true;
            movement.terminalVelocity = terminalVelocity;
            if(listeningForGroundedFlag && movement.grounded)
            {
                status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,0f);
                status.ApplyStatusEffect(PlayerStatusEffect.FLATTEN,3f);
                BounceParticle();
                playQueuedState();
                clearMasterhitVars();
            }
        }
        else if(knockdownFlag && !status.HasStatusEffect(PlayerStatusEffect.KNOCKDOWN))
        {
            knockdownFlag = false;
            movement.terminalVelocity = terminalVelocity;
            if(!status.HasEnemyStunEffect())
            {
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,3);
                playState("Jump_End");
            }
        }

        else if(status.HasEnemyStunEffect() || movement.ledgeHanging)
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

        else if(movementCancelFlag && movement.currentDashes >0 && drifter.doubleTappedX())
        {
            if(movement.dash())
            {
                movement.techParticle();
                clearMasterhitVars();
            }            
        }
        else if(dacusCancelFlag && attacks.grabPressed())
         {
            status.ApplyStatusEffect(PlayerStatusEffect.INVULN,0);
            unpauseGravity();
            setXVelocity(movement.dashSpeed);
            attacks.useGrab();
            clearMasterhitVars();

        }
        else if(dacusCancelFlag && attacks.specialPressed())
         {
            status.ApplyStatusEffect(PlayerStatusEffect.INVULN,0);
            unpauseGravity();
            setXVelocity(movement.dashSpeed);
            attacks.useSpecial();
            clearMasterhitVars();

        }
        else if(dacusCancelFlag && attacks.lightPressed())
        {
            status.ApplyStatusEffect(PlayerStatusEffect.INVULN,0);
            unpauseGravity();
            setXVelocity(movement.dashSpeed);
            attacks.useNormal();
            clearMasterhitVars();

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
        else if((activeCancelFlag || jumpFlag)&& ((drifter.input[0].Jump && !drifter.input[1].Jump && movement.currentJumps>0) || (drifter.doubleTappedX() && movement.currentDashes >0) ))
        {
            if(drifter.input[0].Jump)
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
            else
            {
                if(movement.dash())
                {
                    movement.techParticle();
                    clearMasterhitVars();
                }
            }

        }
        else if(lightTappedFlag && checkForLightTap())
        {
            queuedStateTrigger = true;
            lightTappedFlag = false;
        }
        else if(drifter.canSpecialCancel() && !attacks.grabPressed() && attacks.specialPressed() && !status.HasEnemyStunEffect())
        {
            int dir = (int)checkForDirection(8);
            attacks.useSpecial();
            status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE, 2f *.0833333333f);
            movement.setFacingDelayed(dir);
            clearMasterhitVars();
            drifter.canFeint = true;
            movement.techParticle();
            drifter.canSpecialCancelFlag = false;
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

    //Allow for a special cancel on a move that would normally not be canceleable
    public void listenForSpecial()
    {
        if(!isHost)return;
        drifter.canSpecialCancelFlag = true;
    }

    public void listenForDacus()
    {
        if(!isHost)return;
        setXVelocity(movement.dashSpeed);
        if(movement.dashLock <=0)status.ApplyStatusEffect(PlayerStatusEffect.INVULN,3f*framerateScalar);
        dacusCancelFlag = true;
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
        resetTerminalVelocity();
        ledgeDetector.setPreventWalkoff(false);
        dacusCancelFlag= false;
        //drifter.knockedDown = false;
        knockdownFlag = false;
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
        rb.velocity = new Vector2(rb.velocity.x,y * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f));
        status.saveYVelocity(y);
    }

    public void setYVelocityIfGrounded(float y)
    {
        if(!isHost || !movement.grounded)return;
        setYVelocity(y);
    }


    public void setXVelocity(float x)
    {
        if(!isHost)return;

        if(movement.grounded && x >0) movement.spawnKickoffDust();
        rb.velocity = new Vector2(movement.Facing * x * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
        status.saveXVelocity(movement.Facing * x);
    }

    public void setXVelocityMin(float x)
    {
        if(!isHost)return;

        if(movement.grounded && x >0) movement.spawnKickoffDust();
        rb.velocity = new Vector2(movement.Facing * Mathf.Max(x,Mathf.Abs(rb.velocity.x)) * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
        status.saveXVelocity(rb.velocity.x);
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

    public void setLandingCancel()
    {
        if(!isHost)return;
        movement.canLandingCancel = true;
    }

    public void pauseGravity()
    {
        if(!isHost)return;
        savingVelocity = false;
        movement.pauseGravity();
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

    public void setTerminalVelocity(float speed)
    {
        movement.terminalVelocity = speed;
    }
    
    public void resetTerminalVelocity()
    {
        movement.terminalVelocity = terminalVelocity;
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
        status.clearVelocity();
        movement.terminalVelocity = terminalVelocity * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f);
        clearMasterhitVars();
    	drifter.returnToIdle();
        movement.updateFacing();
        if(checkForJumpTap())movement.jump();
        attacks.UpdateInput();
        //drifter.knockedDown = false;
    }

    public void knockdownRecover()
    {
        if(status.HasStatusEffect(PlayerStatusEffect.FLATTEN)) status.ApplyStatusEffect(PlayerStatusEffect.FLATTEN,0f);
        if(status.HasEnemyStunEffect())status.clearStunStatus();
        movement.techParticle();
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

    public bool checkForJumpTap()
    {
        if(!isHost)return false;
        int state = 0;
        for(int i = 0; i < 15; i++)
        {
            if(state >0 && !drifter.input[i].Jump)return true;
            else if(state == 0 && drifter.input[i].Jump) state++;
        }
        return false;
    }

    //Returns the most common direction held over the last X inputs
    public float checkForDirection(int frames = 8)
    {
        float dir = 0;
        facing = movement.Facing;
        if(!isHost)return facing;
        for(int i = 0; i < frames; i++)
        {
            dir += drifter.input[i].MoveX;
        }
        return dir !=0 ?dir:facing;
    }

    public bool checkForSpecialTap(int frames = 8)
    {
        if(!isHost)return false;
        int state = 0;
        for(int i = 0; i < frames; i++)
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

    public void BounceParticle()
    {
        if(!isHost || !movement.grounded)return;
        movement.spawnJuiceParticle(transform.position, MovementParticleMode.Restitution);
    }
}
