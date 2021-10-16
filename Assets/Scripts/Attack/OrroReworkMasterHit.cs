using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroReworkMasterHit : MasterHit
{

	public SyncAnimatorStateHost bubble;
    BeanWrangler bean;
    GameObject beanObject;
    bool beanIsCharging = false;

    Vector3 targetPos;

    bool beanFollowing = true;

    float hoverTime = 1.5f;
	float maxHoverTime = 1.5f;

    bool canHover = true;
    bool listeningForMovement = false;

    void Start()
    {
        spawnBean();
        Empowered = false;
    }

    new void FixedUpdate()
    {

    	if(!isHost)return;

        base.FixedUpdate();

        if(listeningForMovement)
        {
        	if(hoverTime <=0)
        	{
        		//movement.updateFacing();
        		playState("W_Down_End");
        		listeningForMovement = false;
        		canHover = false;
  			}
  			else
  			{
  				rb.velocity = new Vector2(Mathf.Lerp((drifter.input[0].MoveX * 12f),rb.velocity.x,movement.accelerationPercent),rb.velocity.y);
  			}

        	if(drifter.input[0].Light)
        	{
        		movement.updateFacing();
        		clearMasterhitVars();
        		if(drifter.input[0].MoveY >0)
        			attacks.StartAttack(DrifterAttackType.Ground_Q_Up);
        		else if(drifter.input[0].MoveY <0)
        			attacks.StartAttack(DrifterAttackType.Ground_Q_Down);
        		else if(drifter.input[0].MoveX !=0)
        			attacks.StartAttack(DrifterAttackType.Ground_Q_Side);
        		else
        			attacks.StartAttack(DrifterAttackType.Ground_Q_Neutral);

        		listeningForMovement = false;
        		canHover = false;
        	}
        }
    }

    void Update()
    {
        if(!isHost)return;

        if(movement.grounded)
        {
        	canHover = true;
        	hoverTime = maxHoverTime;
        }

        if(status.HasEnemyStunEffect() || movement.ledgeHanging  ||(drifter.input[0].Special && !drifter.input[1].Special &&(drifter.input[0].MoveX !=0 || drifter.input[0].MoveY !=0)))
        {
        	status.ApplyStatusEffect(PlayerStatusEffect.STANCE,0f);
        	bubble.SetState("Hide");
        }

        else if(status.HasStatusEffect(PlayerStatusEffect.STANCE) && drifter.input[0].Light )
        {
        	movement.updateFacing();
        	applyEndLag(8f);
        	playState("W_Neutral_Command");

        	if(drifter.input[0].MoveY >0)
        		BeanUp();
        	else if(drifter.input[0].MoveY <0)
        		BeanDown();
        	else if(drifter.input[0].MoveX !=0)
        		BeanSide();
        	else
        		BeanNeutral();

        	bean.setBeanDirection(movement.Facing);
        	status.ApplyStatusEffect(PlayerStatusEffect.STANCE,0f);
        }


        if(listeningForMovement && hoverTime >0)
        {
        	hoverTime-= Time.deltaTime;
        	UnityEngine.Debug.Log(hoverTime);
        }

        //If orro cancles, or is hit out of a move where bean charges, cancel that move
        //Note, bean continues doing the move if orro Byzantine Cancels the move
        if(beanIsCharging && (status.HasEnemyStunEffect() || movement.ledgeHanging || attackWasCanceled))
        {
            beanIsCharging = false;
            bean.returnToNeutral();
        }

        //If orro dies, kill bean
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            bean.die();
            beanObject = null;
            Empowered = false;
        }
        //Make a new bean projectile when orro respawns
        else if(beanObject == null)
        {
            spawnBean();
        }
        //Send bean orros position and direction so he can follow on a delay
        else
        {
            targetPos = rb.position - new Vector2(-1f * movement.Facing,3f);
            bean.addBeanState(targetPos,movement.Facing);

            Empowered = !beanFollowing || Vector3.Distance(targetPos,bean.rb.position) > 3.8f;
        }
    }


    public void WDownStateSelect()
    {
    	if(!isHost)return;
    	if(movement.grounded || !canHover)drifter.PlayAnimation("W_Down_End");
    }

    public void hover()
    {
        if(!isHost)return;
        if(!listeningForMovement)
        {
        	movement.gravityPaused = true;
        	listenForSpecialTapped("W_Down_End");
        	rb.gravityScale = .5f;
        	movement.canLandingCancel = true;
        	listenForJumpCancel();
        	setYVelocity(0);
        	listeningForMovement = true;
        }
        
    }

    public void endCommand()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.STANCE,0f);
    }

    public void awaitCommand()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.STANCE))
        {
        	status.ApplyStatusEffect(PlayerStatusEffect.STANCE,0f);
        	bubble.SetState("Hide");
        }
        else
        {
        	status.ApplyStatusEffect(PlayerStatusEffect.STANCE,1f);
        	bubble.SetState("Wait");
        }
        
    }

    //Enables all relevant flags for orro's neutral special
    public void BeginWSide()
    {
        if(!isHost)return;
        specialReleasedFlag = true;
        movementCancelFlag = true;
        activeCancelFlag = true;
        queuedState = "W_Side_Fire";
        specialCharge = 0;
        specialLimit = 8;
        beanIsCharging = true;
    }

    //Spawns a page particle behind orro
    public void page()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position + new Vector3(0,1,0),MovementParticleMode.Orro_Page, false);
    }

    //Spawns a page particle in front of orro
    public void pageFlip()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.spawnJuiceParticle(transform.position + new Vector3(facing * 1.5f,1,0),MovementParticleMode.Orro_Page, true);
    }

    //Spawns a boost ring particle for orros up special
    public void boost()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position + new Vector3(0,2,0),MovementParticleMode.Orro_Boost, false);
    }
        
    //Bean!
    //Tells the current bean object to preform certain actions
    public void BeanSide()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bubble.SetState("Side");
        bean.playState("Bean_Side");
    }
    public void BeanDown()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bubble.SetState("Down");
        bean.playState("Bean_Down");
    }
    public void BeanUp()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bubble.SetState("Up");
        bean.playState("Bean_Up");
    }
    public void BeanNeutral()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bubble.SetState("Neutral");
        bean.playState("Bean_Neutral");
    }

    public void BeanSideSpecial()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playChargeState("Bean_Side_Special");
    }

    public void BeanReset()
    {
        if(!isHost)return;
        bean.playFollowState("Bean_Idle");
    }

    //Creates a bean follower
    public void spawnBean()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Empowered = false;

        beanObject = host.CreateNetworkObject("Bean", transform.position - new Vector3(-1f * movement.Facing, 1f), transform.rotation);
        foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

        bean = beanObject.GetComponent<BeanWrangler>();

        foreach (HurtboxCollision hurtbox in beanObject.GetComponentsInChildren<HurtboxCollision>(true))
            hurtbox.owner = drifter.gameObject;
        
        bean.facing = facing;
        bean.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        bean.color = drifter.GetColor();

    }


    //Creates a side air projectile
    public void SpawnSideAir()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(7f * facing,2.7f,0);
        
        GameObject scratch = host.CreateNetworkObject("Orro_Sair_Proj", transform.position + pos, transform.rotation);
        scratch.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in scratch.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
       }

       scratch.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    }

    //Creates a side air projectile
    public void SpawnNeutralAir()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(5f * facing,-2f,0);
        
        GameObject scratch = host.CreateNetworkObject("Orro_Nair_Proj", transform.position + pos, transform.rotation);
        scratch.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in scratch.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
       }

       scratch.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    }


    //Refreshes each of beans hitbox ids so he can keep doing damage
    private void refreshBeanHitboxes(){
        if(!isHost)return;

        facing = movement.Facing;
        bean.facing = facing;

        foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = DrifterAttackType.W_Neutral;
            hitbox.Active = true;
            hitbox.Facing = bean.facing;
        }
    }


    //Overloads orro's return to idel command; Doesnt do anything anymore
    //Remove later probably
    public new void returnToIdle()
    {
        base.returnToIdle();
        specialCharge = 0;
        listeningForMovement = false;
    }


    //Fires bean or recalls him for neutral W
    public void WSideFire()
    {
        if(!isHost)return;

        base.clearMasterhitVars();
        if(Vector3.Distance(targetPos,bean.rb.position) <= 3.8f && beanFollowing)
        {
            bean.setBean(specialCharge * 4.5f  + 8f);
            refreshBeanHitboxes();
            bean.playFollowState("Bean_Side_Special_Fire");
            movement.spawnJuiceParticle(targetPos,MovementParticleMode.Bean_Launch, false);
            beanFollowing = false;
        }
        else
        {
            beanFollowing = true;
            bean.recallBean(rb.position - new Vector2(-2f * movement.Facing,4f),movement.Facing);
        }
        specialCharge = 0;
    }


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


