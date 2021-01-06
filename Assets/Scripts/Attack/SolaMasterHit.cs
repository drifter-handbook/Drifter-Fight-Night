using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolaMasterHit : MasterHit
{
    float terminalVelocity;
    bool listeningForGround;

    void Start()
    {
        if(!isHost)return;
        terminalVelocity = movement.terminalVelocity;
    }

    void Update()
    {
        if(!isHost)return;
        if(listeningForGround && movement.grounded)
        {
            drifter.PlayAnimation("W_Down_Land");
            listeningForGround = false;
        }
        if(listeningForGround && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            listeningForGround = false;
            resetTerminal();
        }
    }

    //Inherited Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -30f,0f);
    }

    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0,70f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }
}
