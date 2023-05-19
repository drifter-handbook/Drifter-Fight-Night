using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrifterCannonMasterHit : MasterHit
{

    int boostTime = 70;
    int charge = 1;
    bool jumpGranted = false;

    protected bool listeningForWallbounce = false;
    protected bool listeningForDirection = false;

    GameObject g_explosion;
    GameObject g_grenade;
    GameObject g_ranch;


    override public void UpdateFrame()
    {
        base.UpdateFrame();

        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            drifter.Sparkle(false);
            jumpGranted = false;
            SetCharge(1);
        }

        if(jumpGranted && movement.grounded)jumpGranted = false;

        if(listeningForWallbounce && movement.IsWallSliding())
        {
            listeningForWallbounce = false;
            drifter.PlayAnimation("W_Side_End_Early");
            rb.velocity = new Vector2(movement.Facing * -15f,30f);
            if(!jumpGranted && movement.currentJumps <= movement.numberOfJumps -1) movement.currentJumps++;
            jumpGranted = true;
            GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Restitution,rb.position + new Vector2(movement.Facing * .5f,0), (movement.Facing > 0)?90:-90,Vector3.one);
            unpauseGravity();
        }

        if(listeningForDirection)
        {
            movement.updateFacing();
            movement.move(10f);
            rb.velocity = new Vector2(rb.velocity.x,(drifter.input[0].MoveY >0?Mathf.Lerp(20f,rb.velocity.y,.45f):rb.velocity.y));

            if(attacks.lightPressed())
            {
                attacks.useNormal();
                listeningForDirection = false;
            }

            else if(drifter.input[0].MoveY > 0)
            {
                drifter.PlayAnimation("W_Up_Loop",0,true);
                boostTime --;
            }
            else
            {
                drifter.PlayAnimation("W_Up_Idle",0,true);
            }
            if(boostTime <=0)
            {
                listeningForDirection = false;
                drifter.PlayAnimation("W_Up_End");
            }
        }
        if(g_ranch != null) g_ranch.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
        if(g_explosion != null) g_explosion.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
        if(g_grenade != null) g_grenade.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

    }

    public void listenForDirection()
    {
        listeningForDirection = true;
        boostTime = 90;
        listenForGrounded("Jump_End");
    }

    public void cancelWUp()
    {
        listeningForDirection = false;
    }

    public void SairExplosion()
    {
        SpawnExplosion(new Vector3(1.9f * movement.Facing,3.3f,0),1);
    }


    public void SideWExplosion()
    {
        SpawnExplosion(new Vector3(-1.5f * movement.Facing,2.7f,0), -1);
    }

    public void UairExplosion()
    {
        SpawnExplosion(new Vector3(-.4f* movement.Facing,5.5f,0),1,90 );
    }

    void SpawnExplosion(Vector3 pos, int flip, int direction = 0)
    {
        g_explosion = GameController.Instance.CreatePrefab("Explosion", transform.position + pos, Quaternion.Euler(0,0,movement.Facing *direction));
        g_explosion.transform.localScale = new Vector3(flip * 10f * movement.Facing, 10f , 1f);

        foreach (HitboxCollision hitbox in g_explosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    public void listenForWallBounce()
    {
        listeningForWallbounce = true;
    }

    public override void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        listeningForWallbounce = false;
        listeningForDirection = false;
    } 

    public void SpawnGrenade()
    {
        
        Vector3 pos = new Vector3(.5f * movement.Facing,3.7f,0);
        
        g_grenade = GameController.Instance.CreatePrefab("DCGenade", transform.position + pos, transform.rotation);
        g_grenade.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        g_grenade.GetComponent<Rigidbody2D>().velocity = new Vector2(20* movement.Facing,25);

        foreach (HitboxCollision hitbox in g_grenade.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    //W_Neutral

    public void handleRanchStartup()
    {
    	//sets all special inputs to true to "clear" it

    	foreach(PlayerInputData input in drifter.input)
    		input.Special = true;
    	listenForSpecialTapped("W_Neutral_Fire");
        if(Empowered) drifter.PlayAnimation("W_Neutral_Fire");
    	else if(charge > 1) drifter.PlayAnimation("W_Neutral_" + charge);
    }

    public void SetCharge(int charge)
    {
    	this.charge = charge;
    	Empowered = (charge == 3);

        drifter.Sparkle(Empowered);

        drifter.SetAnimationOverride(Empowered?1:0);

    }

    public void SpawnRanch()
    {
        
        Vector3 pos = new Vector3(1f * movement.Facing,2.7f,0);
        
        g_ranch = GameController.Instance.CreatePrefab("Ranch" + charge, transform.position + pos, transform.rotation);
        g_ranch.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);

        rb.velocity = new Vector2((charge - 1) * -15f* movement.Facing,0);
        
        if(charge < 3)g_ranch.GetComponent<Rigidbody2D>().velocity = new Vector2((charge == 1?55f:25f)* movement.Facing,0);

        SetCharge(1);

        foreach (HitboxCollision hitbox in g_ranch.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }
    }

    //Rollback
    //=========================================

    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
        baseFrame.CharacterFrame= new DCRollbackFrame() 
        {
            Ranch = g_ranch != null ? g_ranch.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
            Explosion = g_explosion != null ? g_explosion.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
            Grenade = g_grenade != null ? g_grenade.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
            
            BoostTime = boostTime,
            Charge = charge,
            JumpGranted = jumpGranted,
            ListeningForWallbounce = listeningForWallbounce,
            ListeningForDirection = listeningForDirection
        };

        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);

        DCRollbackFrame dc_frame = (DCRollbackFrame)p_frame.CharacterFrame;

        boostTime = dc_frame.BoostTime;
        charge = dc_frame.Charge;
        jumpGranted = dc_frame.JumpGranted;
        listeningForWallbounce = dc_frame.ListeningForWallbounce;
        listeningForDirection = dc_frame.ListeningForDirection;

        if(dc_frame.Ranch != null)
        {
            if(g_ranch == null)SpawnRanch();
            g_ranch.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(dc_frame.Ranch);
        }
        else
        {
            Destroy(g_ranch);
            g_ranch = null;
        }  

        if(dc_frame.Explosion != null)
        {
            if(g_explosion == null)SpawnExplosion(Vector3.zero,0);
            g_explosion.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(dc_frame.Explosion);
        }
        else
        {
            Destroy(g_explosion);
            g_explosion = null;
        } 

        if(dc_frame.Grenade != null)
        {
            if(g_grenade == null)SpawnGrenade();
            g_grenade.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(dc_frame.Grenade);
        }
        else
        {
            Destroy(g_grenade);
            g_grenade = null;
        } 
    }

}

public class DCRollbackFrame: ICharacterRollbackFrame
{
    public string Type { get; set; }

    public BasicProjectileRollbackFrame Ranch;
    public BasicProjectileRollbackFrame Explosion;
    public BasicProjectileRollbackFrame Grenade;
    public int BoostTime;
    public int Charge;
    public bool JumpGranted;
    public bool ListeningForWallbounce;
    public bool ListeningForDirection;
    
}


