using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroMasterHit : MasterHit
{
    public Animator anim;
    BeanWrangler bean;
    GameObject beanObject;

    void Start()
    {
        spawnBean();
    }

    void Update(){
        // if(beanObject == null){
        //     spawnBean();
        // }
        if(anim.GetBool("Empowered"))bean.addBeanState(rb.position - new Vector2(-2f * movement.Facing,4f),movement.Facing);

    }

    public override void rollGetupEnd(){
        facing = movement.Facing;
        rb.position += new Vector2(7f* facing,5.5f);
    }

    // public void ledgeClimbLag(){
    //     status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.9f);
    // }

    public void spawnFireball()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *3f,3.5f,1f);
        GameObject orroOrb = Instantiate(entities.GetEntityPrefab("OrroSideW"), transform.position + pos, transform.rotation);
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
        orroOrb.GetComponent<OrroSideWProjectile>().facing=facing;
        entities.AddEntity(orroOrb);
    }

    // public void dodgeRoll()
    // {
    //     facing = movement.Facing;
    //     status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.8f);
    //     status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.4f);
    // }

    public override void roll()
    {
        facing = movement.Facing;
        rb.position += new Vector2(facing* 10,0);
    }

    public override void rollGetupStart()
    {
        //unused
    }


	public void dair()
    {
    	facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x,35f);
        GameObject orroSplosion = Instantiate(entities.GetEntityPrefab("DairExplosion"), transform.position, transform.rotation);
        orroSplosion.transform.localScale = new Vector3(7.5f * facing,7.5f,1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        entities.AddEntity(orroSplosion);
    }

    // public void backdash(){
    // 	facing = movement.Facing;

    // 	rb.velocity = new Vector2(facing * -25f,23f);
    // }


    public void sair()
    {
    	facing = movement.Facing;
        GameObject marble = Instantiate(entities.GetEntityPrefab("Marble"), transform.position + new Vector3(0,3f,0), transform.rotation);
        foreach (HitboxCollision hitbox in marble.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        marble.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 35f, 0);
        entities.AddEntity(marble);
    }


    public void uair()
    {
    	facing = movement.Facing;
        rb.velocity = new Vector2(rb.velocity.x,10f);
        GameObject orroSplosion = Instantiate(entities.GetEntityPrefab("UairExplosion"), transform.position + new Vector3(0,.5f,0), transform.rotation);
        orroSplosion.transform.localScale = new Vector3(7.5f * facing,7.5f,1f);
        foreach (HitboxCollision hitbox in orroSplosion.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        entities.AddEntity(orroSplosion);
    }

     public void cancelGravity(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        movement.gravityPaused= true;
        rb.gravityScale = 0;
    }
    public void inTheHole(){
        
        rb.velocity = new Vector2(0f,225f);
    }

    public void resetGravity(){
        movement.gravityPaused= false;
        rb.gravityScale = gravityScale;
    }

    public void grabEndlag(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.55f);
    }

    

    public void BeanSide()
    {
        refreshBeanHitboxes();
        bean.Side = true;
    }
    public void BeanDown()
    {
        refreshBeanHitboxes();
        bean.Down = true;
    }
    public void BeanUp()
    {
        refreshBeanHitboxes();
        bean.Up = true;
    }
    public void BeanNeutral()
    {
        refreshBeanHitboxes();
        bean.Neutral = true;
    }

    public void spawnBean()
    {
            facing = movement.Facing;
            beanObject = Instantiate(entities.GetEntityPrefab("Bean"), rb.position - new Vector2(-2f * movement.Facing,4f), transform.rotation);

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

            entities.AddEntity(beanObject);

    }

    private void refreshBeanHitboxes(){
        bean.resetAnimatorTriggers();
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
        if(anim.GetBool("Empowered"))
        {
            UnityEngine.Debug.Log("SENT OUT");
            drifter.SetAnimatorBool("Empowered",false);
            bean.setBean();
        }
        else{
            UnityEngine.Debug.Log("COME BACK");
            drifter.SetAnimatorBool("Empowered",true);
            bean.recallBean(rb.position - new Vector2(-2f * movement.Facing,4f),movement.Facing);

        }
       
    }

}
