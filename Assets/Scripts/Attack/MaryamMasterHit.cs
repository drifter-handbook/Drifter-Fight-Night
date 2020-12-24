using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaryamMasterHit : MasterHit
{

    bool hasSGRecovery = true;
    bool hasUmbrellaRecovery = true;

    public void StanceChange()
    {
        if(!isHost)return;
        SetStance(!Empowered);
    }


    public void SetStance(bool stance)
    {
        if(!isHost)return;
        Empowered = stance;
        attacks.currentRecoveries = (Empowered && hasSGRecovery) || (!Empowered && hasUmbrellaRecovery)? 1:0;

        drifter.WalkStateName = Empowered?"Walk_SG":"Walk";
        drifter.GroundIdleStateName = Empowered?"Idle_SG":"Idle";
        drifter.JumpStartStateName = Empowered?"Jump_Start_SG":"Jump_Start";
        drifter.AirIdleStateName = Empowered?"Hang_SG":"Hang";
        drifter.JumpEndStateName = Empowered?"Jump_End_SG":"Jump_End";
        drifter.WeakLedgeGrabStateName = Empowered?"Ledge_Grab_Weak_SG":"Ledge_Grab_Weak";
        drifter.StrongLedgeGrabStateName = Empowered?"Ledge_Grab_Strong_SG":"Ledge_Grab_Strong";

        drifter.LedgeRollStateName = Empowered?"Ledge_Roll_SG":"Ledge_Roll";
        drifter.LedgeClimbStateName = Empowered?"Ledge_Climb_SG":"Ledge_Climb";

    }


    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0f,35f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 30f,5f);
    }
}


