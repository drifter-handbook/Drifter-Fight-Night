﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{
 	GameObject slowfield;
 	float terminalVelocity;

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

        //Allow for Down W if stunned
        if(Empowered && !status.HasStatusEffect(PlayerStatusEffect.END_LAG) && drifter.input.Special && drifter.input.MoveY < 0)
        {
        	fightOrFlight();
        }
    }

	//Down W

    public void counter()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.HIT)){
            drifter.PlayAnimation("W_Down_Success");
            status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.2f);
        }
    }


    public void spawnSlowZone()
    {

    	if(!isHost)return;

        Vector3 pos = new Vector3(0f, 4.5f, 0f);
        //TODO Add delete animation here
        if (slowfield)Destroy(slowfield);
        slowfield = host.CreateNetworkObject("myth_slowfield", transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in slowfield.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID + 150;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

        
        Empowered = true;

        slowfield.GetComponent<MultihitZoneProjectile>().attacks = attacks;
    }

    public void fightOrFlight()
    {
    	if(!isHost || !Empowered)return;
    	Empowered = false;
    	//Heal
    	if(drifter.DamageTaken >= 10f)drifter.DamageTaken -= 10f;
       
        else drifter.DamageTaken = 0f;

        //GP orange
        status.clearAllStatus();
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,3f);
        drifter.PlayAnimation("W_Down_Boost");

        movement.walkSpeed = 29.5f;
    	movement.airSpeed = 29.5f;
    	StartCoroutine(resetSpeed());

    }

    IEnumerator resetSpeed()
    {
    	for(int i = 0; i < 40;i++)
    	{
    		GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Heal, transform.position + new Vector3(UnityEngine.Random.Range(-2f,2f), UnityEngine.Random.Range(1.5f,6f)), 0, new Vector2(1, 1));
    		yield return new WaitForSeconds(.175f);
    	}
    	movement.walkSpeed = 23.5f;
    	movement.airSpeed = 23.5f;
    }

    //Up W

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
            movement.terminalVelocity = 10f;
        }
    }

    public void resetTerminal()
    {
    	if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }

    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,75f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
