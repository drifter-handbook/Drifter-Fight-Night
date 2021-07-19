using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroReworkMasterHit : MasterHit
{


    BeanWrangler bean;
    GameObject beanObject;
    float neutralSpecialCharge = 0;

    void Start()
    {
        spawnBean();
        Empowered = false;
    }

    void Update()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            bean.die();
            beanObject = null;
            Empowered = false;
        }
        else if(beanObject == null)
        {
            
            spawnBean();
        }
        else bean.addBeanState(rb.position - new Vector2(-1f * movement.Facing, 3f), movement.Facing);
    }


    //Roll Methods

    public void WNeutralCharge()
    {
        if(!isHost)return;
        applyEndLag(1);
        if(neutralSpecialCharge > 8)
        {
            playState("W_Neutral_Fire");
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

    public void page()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position + new Vector3(0,1,0),MovementParticleMode.Orro_Page, false);
    }


    public void boost()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position + new Vector3(0,2,0),MovementParticleMode.Orro_Boost, false);
    }

    public void pageFlip()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.spawnJuiceParticle(transform.position + new Vector3(facing * 1.5f,1,0),MovementParticleMode.Orro_Page, true);
    }

    public void setTerminalVelocity(float vel)
    {
        if(!isHost)return;
        movement.canLandingCancel = false;  
        movement.terminalVelocity = vel;
    }

    public void resetTerminalVelocity()
    {
        if(!isHost)return; 
        movement.terminalVelocity = terminalVelocity;
    }

    //Bean!
    public void BeanSide()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playState("Bean_Side");
    }
    public void BeanDown()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playState("Bean_Down");
    }
    public void BeanUp()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playState("Bean_Up");
    }
    public void BeanNeutral()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playState("Bean_Neutral");
    }

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
        beanObject.GetComponentInChildren<HurtboxCollision>().owner = drifter.gameObject;
        bean.facing = facing;
        bean.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        bean.color = drifter.GetColor();

    }

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

    public new void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        neutralSpecialCharge = 0;

    }

    public void WNeutralFire()
    {
        if(!isHost)return;

        if(!Empowered)bean.setBean(neutralSpecialCharge * 4f  + 8f);
        else bean.recallBean(rb.position - new Vector2(-2f * movement.Facing,4f),movement.Facing);
        Empowered =!Empowered;
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


