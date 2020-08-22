﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public int facing;
    public Animator anim;
    GameObject beanRemote;
    public GameObject localBean;
    Animator localBeanAnim;
    PlayerStatus status;
    float beanSpeed = 10f;

    void Start()
    {

        localBeanAnim = localBean.GetComponent<Animator>();
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
    }

    public void spawnFireball()
    {
        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *3f,5.5f,1f);
        GameObject orroOrb = Instantiate(entities.GetEntityPrefab("OrroSideW"), transform.position + pos, transform.rotation);
        orroOrb.transform.localScale = flip;
        orroOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 10, 0);
        foreach (HitboxCollision hitbox in orroOrb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        orroOrb.GetComponent<OrroSideWProjectile>().facing=facing;
        entities.AddEntity(orroOrb);
    }

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,0f);
    }

     public void cancelGravity(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        rb.gravityScale = 0;
    }
    public void inTheHole(){
        facing = movement.Facing;
        rb.velocity = Vector2.zero;
        rb.position += new Vector2(0,20);
    }

    public void resetGravity(){
        rb.gravityScale = gravityScale;
    } 

    //Bean

    public void chargebean()
    {
        beanSpeed+=10f;
        if(beanSpeed >= 50){
            anim.SetTrigger("Bean");
        }
    }

    public void fireBean(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.4f);
        if(anim.GetBool("Empowered")){
            facing = movement.Facing;
            Vector3 flip = new Vector3(facing *6.7f,6.7f,0f);
            Vector3 pos = new Vector3(facing *1.3f,2f,1f);
            GameObject BeanProj = Instantiate(entities.GetEntityPrefab("Bean"), transform.position + pos, transform.rotation);
            BeanProj.transform.localScale = flip;
            BeanProj.GetComponent<Rigidbody2D>().simulated = true;
            BeanProj.GetComponent<Rigidbody2D>().velocity = new Vector2(facing *beanSpeed, 0f);
            foreach (HitboxCollision hitbox in BeanProj.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
            }
            if(beanRemote){
                Destroy(beanRemote);
            }
            beanRemote = BeanProj;
            localBeanAnim.SetBool("Hide", true);
            drifter.SetAnimatorBool("Empowered",false);
            entities.AddEntity(BeanProj);
            beanSpeed = 20f;
        }
        else{
            BeanRecall();
        }
       
    }

    public void jabCombo(){
        attacks.SetupAttackID(DrifterAttackType.Ground_Q_Neutral);
    }

    public void BeanSide()
    {
        if(beanRemote){
            beanRemote.GetComponent<BeanWrangler>().anim.SetTrigger("Side");
        }
        else{
           localBeanAnim.SetTrigger("Side");
        }
    }
    public void BeanDown()
    {
        if(beanRemote){
            beanRemote.GetComponent<BeanWrangler>().anim.SetTrigger("Down");
        }
        else{
           localBeanAnim.SetTrigger("Down");
        }
    }
    public void BeanUp()
    {
        if(beanRemote){
            beanRemote.GetComponent<BeanWrangler>().anim.SetTrigger("Up");
        }
        else{
           localBeanAnim.SetTrigger("Up");
        }
    }
    public void BeanNeutral()
    {
        if(beanRemote){
            beanRemote.GetComponent<BeanWrangler>().anim.SetTrigger("Neutral");
        }
        else{
           localBeanAnim.SetTrigger("Neutral");
        }
    }
    public void BeanRecall()
    {
        Destroy(beanRemote);
        localBeanAnim.SetBool("Hide",false);
        drifter.SetAnimatorBool("Empowered",true);
    }


}
