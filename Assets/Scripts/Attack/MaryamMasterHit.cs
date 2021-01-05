using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaryamMasterHit : MasterHit
{

    bool hasSGRecovery = true;
    bool hasUmbrellaRecovery = true;
    bool WSideFinisher = false;
    float terminalVelocity;

    public void StanceChange()
    {
        if(!isHost)return;
        SetStance(!Empowered);
    }

    void Start()
    {
        if(!isHost)return;
        terminalVelocity = movement.terminalVelocity;
    }

    void Update()
    {
        if(!isHost)return;

        if(movement.terminalVelocity != terminalVelocity  && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminal();
        }

        if((!hasUmbrellaRecovery || !hasSGRecovery) &&( movement.grounded || status.HasEnemyStunEffect() || movement.ledgeHanging)){
            hasSGRecovery = true;
            hasUmbrellaRecovery = true;
        }
    }


    public void SetStance(bool stance)
    {
        //if(!isHost)return;
        Empowered = stance;
        attacks.currentRecoveries = (Empowered && hasSGRecovery) || (!Empowered && hasUmbrellaRecovery)? 1:0;

        drifter.SetAnimationLayer(Empowered?1:0);


        // drifter.WalkStateName = Empowered?"Walk_SG":"Walk";
        // drifter.GroundIdleStateName = Empowered?"Idle_SG":"Idle";
        // drifter.JumpStartStateName = Empowered?"Jump_Start_SG":"Jump_Start";
        // drifter.AirIdleStateName = Empowered?"Hang_SG":"Hang";
        // drifter.JumpEndStateName = Empowered?"Jump_End_SG":"Jump_End";
        // drifter.WeakLedgeGrabStateName = Empowered?"Ledge_Grab_Weak_SG":"Ledge_Grab_Weak";
        // drifter.StrongLedgeGrabStateName = Empowered?"Ledge_Grab_Strong_SG":"Ledge_Grab_Strong";

        // drifter.LedgeRollStateName = Empowered?"Ledge_Roll_SG":"Ledge_Roll";
        // drifter.LedgeClimbStateName = Empowered?"Ledge_Climb_SG":"Ledge_Climb";

    }

    public void cancelSideQ()
    {
        movement.canLandingCancel = true;
    }

    public void UmbrellaRecovery()
    {
        hasUmbrellaRecovery = false;
    }

    public void SGRecovery()
    {
        hasSGRecovery = false;
    }


    public void upWGlide()
    {
        if(!isHost)return;

        if(TransitionFromChanneledAttack())
        {
            resetTerminal();
        }

        else if(drifter.input.MoveY <0 || movement.grounded)
        {
            resetTerminal();
            returnToIdle();
        }
        else
        {

            movement.updateFacing();
            rb.velocity = new Vector2(Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)? drifter.input.MoveX * 23f:(.6f*23f)),rb.velocity.x,.75f),rb.velocity.y);
            movement.updateFacing();
            movement.terminalVelocity = 8f;
        }
    }

    public void resetTerminal()
    {
        if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }


    //Side W state logic

    public void checkForContinueWSide()
    {
        if(!isHost)return;
        if(drifter.input.Special)continueJabFlag = true;
    }

    public void checkForWSideFinisher()
    {
        if(!isHost)return;
        if(drifter.input.MoveY !=0 ) WSideFinisher = false;
        else if(drifter.input.MoveX != 0) WSideFinisher = true;
        else WSideFinisher = Empowered;
    }

    public void SideWFinisher()
    {
        if(!isHost)return;
        
        refreshHitboxID();
        continueJabFlag = false;
        playState(WSideFinisher?"W_Side_Finisher_Umbrella":"W_Side_Finisher_SG");

    }


    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 35f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.position += new Vector2(facing * 1f,6f);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 30f,0f);
    }
}


