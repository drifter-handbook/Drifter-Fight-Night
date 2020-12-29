using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParhelionMasterHit : MasterHit
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

    
    //Side W Projectile

    public void lightningStrike()
    {
        // jump upwards and create spear projectile
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(facing * - 4.3f,2.8f,0);

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
    

    //Terminal Veloity Controls for Down W
    
    public void setTerminalVelocity()
    {
        if(!isHost)return;
        listeningForGround = true;
        movement.terminalVelocity = 150;
    }

    public void resetTerminal()
    {
        if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }

    public void wallRide(float speed)
    {
		if(!isHost)return;

		if(movement.wallSliding != Vector3.zero) rb.velocity = new Vector2(movement.Facing * Mathf.Abs(movement.wallSliding.y),Mathf.Abs(movement.wallSliding.x)) * speed;    	
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
