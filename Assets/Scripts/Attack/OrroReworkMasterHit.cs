using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroReworkMasterHit : MasterHit
{

    //Roll Methods

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


