using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroMasterHit : MasterHit
{
    BeanWrangler bean;
    GameObject beanObject;

    

    void Start()
    {
        spawnBean();
        Empowered = true;
    }

    void Update()
    {
        if(!isHost)return;

        if(Empowered)bean.addBeanState(rb.position - new Vector2(-2f * movement.Facing, 4f), movement.Facing);
    }

    //Projectiles

    public void fireball()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing * 12f, 12f, 0f);
        Vector3 pos = new Vector3(facing * 3f, 3.5f, 1f);
        
        GameObject orroOrb = host.CreateNetworkObject("OrroSideW", transform.position + pos, transform.rotation);
        orroOrb.transform.localScale = flip;
        orroOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 25, 0);
        foreach (HitboxCollision hitbox in orroOrb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        orroOrb.GetComponent<OrroSideWProjectile>().facing = facing;

    }

    public void downwardsExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x, 35f);
        
        GameObject orroSplosion = host.CreateNetworkObject("DairExplosion", transform.position, transform.rotation);
        orroSplosion.transform.localScale = new Vector3(7.5f * facing, 7.5f, 1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;

        }
    }

    public void marble()
    {
        if(!isHost)return;
        facing = movement.Facing;

        GameObject marble = host.CreateNetworkObject("Marble", transform.position + new Vector3(0, 3f, 0), transform.rotation);
        foreach (HitboxCollision hitbox in marble.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        marble.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 35f, 0);

    }


    public void upwardsExplosion()
    {
        if(!isHost)return;
        facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x,10f);

        GameObject orroSplosion = host.CreateNetworkObject("UairExplosion", transform.position + new Vector3(0, .5f, 0), transform.rotation);
        orroSplosion.transform.localScale = new Vector3(7.5f * facing, 7.5f, 1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;

        }
    }

    //Bean!

    public void BeanSide()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playeState("BEAN_SIDE");
    }
    public void BeanDown()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playeState("BEAN_DOWN");
    }
    public void BeanUp()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playeState("BEAN_UP");
    }
    public void BeanNeutral()
    {
        if(!isHost)return;
        refreshBeanHitboxes();
        bean.playeState("BEAN_NEUTRAL");
    }

    public void spawnBean()
    {
        if(!isHost)return;
        facing = movement.Facing;

        beanObject = host.CreateNetworkObject("Bean", rb.position - new Vector2(-2f * movement.Facing, 4f), transform.rotation);
        foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }

        bean = beanObject.GetComponent<BeanWrangler>();
        bean.facing = facing;

    }

    private void refreshBeanHitboxes(){
        if(!isHost)return;
        foreach (HitboxCollision hitbox in beanObject.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = DrifterAttackType.W_Neutral;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
    }

    public void BeanRecall()
    {
        if(!isHost)return;

        if(Empowered)bean.setBean();
        else bean.recallBean(rb.position - new Vector2(-2f * movement.Facing,4f),movement.Facing);
        Empowered =!Empowered;

    }

    //Inhereted Roll Methods
    public override void roll()
    {
        //unused
    }

    public override void rollGetupStart()
    {
        //unused
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        rb.position += new Vector2(7f* facing,5.5f);
    }

}

