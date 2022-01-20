using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroReworkMasterHit : MasterHit
{
	public SyncAnimatorStateHost bubble;
 
    BeanWrangler bean;
    
    GameObject beanObject;
	GameObject platform;

    bool beanIsCharging = false;
    bool beanFollowing = true;   
    bool canHover = true;
    bool listeningForMovement = false;

	float hoverTime = 1.5f;
	static float maxHoverTime = 1.5f;
    
    Vector3 targetPos;

    void Start()
    {
        spawnBean();
        Empowered = false;
    }

    new void FixedUpdate()
    {

    	if(!isHost)return;

        base.FixedUpdate();

        //Refresh hover values when you land
        if(movement.grounded)
        {
            canHover = true;
            hoverTime = maxHoverTime;
        }

        //reset bean when he dies
        if(!bean.alive)
        {
            beanFollowing= true;
            Empowered = false;
        }

        //Deactivate stance if cerain conditions are met
        if(status.HasEnemyStunEffect() || movement.ledgeHanging  ||(drifter.input[0].Special && !drifter.input[1].Special &&(drifter.input[0].MoveX !=0 || drifter.input[0].MoveY !=0)))
        {
            status.ApplyStatusEffect(PlayerStatusEffect.STANCE,0f);
            bubble.SetState("Hide");
        }
        //Otherwise, use a stance move 
        else if(status.HasStatusEffect(PlayerStatusEffect.STANCE) && drifter.input[0].Light && !drifter.input[1].Light )
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
            //status.ApplyStatusEffect(PlayerStatusEffect.STANCE,0f);
        }

        if(listeningForMovement)
        {
        	if(hoverTime <=0)
        	{
        		//movement.updateFacing();
        		playState("W_Down_Ground");
        		listeningForMovement = false;
        		canHover = false;
  			}
  			else
  			{
                hoverTime-= Time.fixedDeltaTime;
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

        //If orro cancels, or is hit out of a move where bean charges, cancel that move
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

    /*
    	Down Special Functions
    */

    //Handles orros float timer if he uses the move while it is depleted
    public void WDownStateSelect()
    {
    	if(!isHost)return;
    	if(movement.grounded || !canHover)drifter.PlayAnimation("W_Down_Ground");
    	else
    		spawnPlatform();
    }

    // Creates a platform to represent orro's float state
    public void spawnPlatform()
    {
    	if(!isHost)return;
        facing = movement.Facing;
    	deletePlatform();
    	platform = host.CreateNetworkObject("orro_w_down_platform", transform.position, transform.rotation);
        platform.transform.localScale = new Vector3(10f * facing, 10f , 1f);
    	platform.transform.SetParent(drifter.gameObject.transform);
    	platform.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());

    }

    //Deletes orro's floatstate platform
    public void deletePlatform()
    {
    	if(!isHost)return;
    	if(platform != null)
    	{
    		platform.GetComponent<SyncAnimatorStateHost>().SetState("W_Down_Platform_Decay");
    		platform = null;
    	}
    	
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

    /*
    	Side Special Functions
    */

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

    //Fires bean or recalls him for neutral W
    public void WSideFire()
    {
        if(!isHost)return;

        clearMasterhitVars();
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

    /*
       	Neutral Special Functions
    */

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
            hitbox.isActive = true;
            hitbox.Facing = facing;
        }

        bean = beanObject.GetComponent<BeanWrangler>();

        foreach (HurtboxCollision hurtbox in beanObject.GetComponentsInChildren<HurtboxCollision>(true))
            hurtbox.owner = drifter.gameObject;
        
        bean.facing = facing;
        bean.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        bean.color = drifter.GetColor();

    }

    /*

		Other Projectiles

    */


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
            hitbox.Facing = facing;
       }

       scratch.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    }

    //Creates a side air projectile
    public void SpawnNeutralAir()
    {
        if(!isHost)return;


        RaycastHit2D ray = Physics2D.Raycast(transform.position+ new Vector3(0,1f),new Vector3(facing * 7f/5f,-5f/5f,0),5f,1);

        facing = movement.Facing;
        Vector3 pos = new Vector3((ray.distance +1) * facing,-1* ray.distance +1f,0);
        if(ray.distance ==0)pos = new Vector3(8* facing,-4,0);
        
        GameObject scratch = host.CreateNetworkObject("Orro_Nair_Proj", transform.position + pos, transform.rotation);
        scratch.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in scratch.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
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
            hitbox.isActive = true;
            hitbox.Facing = bean.facing;
        }
    }

    /*
    	Unique particle spawn Functions
    */

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


    //Overloads orro's return to idle command
    public new void returnToIdle()
    {
        base.returnToIdle();
        deletePlatform();
        specialCharge = 0;
        listeningForMovement = false;
    }

    public new void clearMasterhitVars()
    {
    	base.clearMasterhitVars();
        listeningForMovement = false;
    	if(platform != null)
        {
            Destroy(platform);
            platform = null;
        }
    }

}


