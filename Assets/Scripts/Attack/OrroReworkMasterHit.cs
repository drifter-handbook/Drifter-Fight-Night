using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroReworkMasterHit : MasterHit
{

    float neutralSpecialCharge = 0;

    //Roll Methods

    public void WNeutralCharge()
    {
        if(!isHost)return;
        applyEndLag(1);
        if(neutralSpecialCharge > 8)
        {
            playState("W_Neutral_Fire");
            neutralSpecialCharge = 0;
        }
        switch(chargeAttackSingleUse("W_Neutral_Fire"))
        {
            case 0:
                neutralSpecialCharge += 1;
                break;
            case 1:
                neutralSpecialCharge = 0;
                break;
            default:
            // The attack was fired;
                break;     
        }
    }

    public new void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        neutralSpecialCharge = 0;

    }

    public void WNeutralFire()
    {
        neutralSpecialCharge = 0;
    }


    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,4f * framerateScalar);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.position += new Vector2(facing * 1f,5.9f);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,4f * framerateScalar);
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 35f,5f);
    }
}


