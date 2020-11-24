using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParhelionMasterHit : MasterHit
{
    float terminalVelocity;

    void Start()
    {
        terminalVelocity = movement.terminalVelocity;
    }

    
    //Side W Projectile

    public void lightningStrike()
    {
        // jump upwards and create spear projectile
        facing = movement.Facing;
        Vector3 pos = new Vector3(facing * - 4.3f,2.8f,0);
        if (GameController.Instance.IsHost)
        {
            GameObject bolt = GameController.Instance.host.CreateNetworkObject("ParhelionBolt", transform.position + pos, transform.rotation);
            foreach (HitboxCollision hitbox in bolt.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
        }
    }
    

    //Terminal Veloity Controls for Down W
    
    public void setTerminalVelocity()
    {
        movement.terminalVelocity = 150;
    }

    public void resetTerminal(){
        movement.terminalVelocity = terminalVelocity;
    }


    //Inherited Roll Methods

    public override void roll(){
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -30f,0f);
    }

    public override void rollGetupStart(){
        applyEndLag(1);
        rb.velocity = new Vector3(0,75f,0);
    }

    public override void rollGetupEnd()
    {
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }
}
