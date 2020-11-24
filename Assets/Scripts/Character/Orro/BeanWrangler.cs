﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BeanWrangler : MonoBehaviour
{
    // Start is called before the first frame update
    public Animator anim;
    public bool Up = false;
    public bool Down = false;
    public bool Side = false;
    public bool Neutral = false;
    public bool Hide = false;

    public int facing = 1;

    PlayerAttacks attacks;
    GameObject Orro;
    BeanState targetPos;
    Rigidbody2D rb;
    float beanUpdateTimer = 0f;
    float timeSinceState = 0f;

    Queue<BeanState> states = new Queue<BeanState>();

    public struct BeanState
    {
        public BeanState(Vector3 pos, int facing)
        {
            Pos = pos;
            Facing = facing;
        }

        public Vector3 Pos { get; set;}
        public int Facing { get; set;}
    }

    void Start()
    {

        //Movement Stuff
        rb = GetComponent<Rigidbody2D>();
        targetPos = new BeanState(rb.position, facing);
        Orro = gameObject.GetComponentInChildren<HitboxCollision>().parent;
        attacks = gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponent<PlayerAttacks>();

        
    }

    void Update(){

        if(states.Count == 0 && timeSinceState >= 5f && beanUpdateTimer >=0) Destroy(this.gameObject);

        if (Side) anim.SetTrigger("Side");
        if (Down) anim.SetTrigger("Down");
        if (Up) anim.SetTrigger("Up");
        if (Neutral) anim.SetTrigger("Neutral");

        timeSinceState += Time.deltaTime;
        if(beanUpdateTimer >= 0) beanUpdateTimer += Time.deltaTime;

        if(beanUpdateTimer >= 1f && !(beanUpdateTimer < 0))
        {
            if(states.Count >0){
                targetPos = states.Dequeue();
            }
            beanUpdateTimer = 1f;
        }

        rb.position =  Vector3.Lerp(rb.position,targetPos.Pos,.1f);

        facing = targetPos.Facing;

        transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
            transform.localScale.y, transform.localScale.z);
    }


    public void addBeanState(Vector3 pos,int facing)
    {
        timeSinceState =  0;
        states.Enqueue(new BeanState(pos,facing));
    }


    public void recallBean(Vector3 pos,int facing){
        states.Clear();
        targetPos = new BeanState(pos, facing);

        beanUpdateTimer = 0f;
    }

    public void setBean(){
        states.Clear();
        beanUpdateTimer = -1f;
    }

    public void beanSpit()
    {
        Vector3 flip = new Vector3(facing *8,8,0f);
        Vector3 pos = new Vector3(facing *.7f,3.5f,1f);
        if (GameController.Instance.IsHost)
        {
            GameObject spit = GameController.Instance.host.CreateNetworkObject("BeanSpit", transform.position + pos, transform.rotation);
            spit.transform.localScale = flip;
            try{

                attacks.SetMultiHitAttackID();
                spit.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * 30 - rb.velocity.x, 0);
                foreach (HitboxCollision hitbox in spit.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = Orro;
                    hitbox.AttackID = attacks.AttackID;
                    hitbox.AttackType = attacks.AttackType;
                    hitbox.Active = true;
                    hitbox.Facing = facing;
                }
            }
            //TODO: Remove after network rework
            catch(NullReferenceException E){
                //Eventually
            }
        }
    }

 
    public void multihit(){
        try{
            attacks.SetMultiHitAttackID();    
        }
        //TODO: Remove after network rework
        catch (NullReferenceException E){
            //Eventually
        }
        
    }
}
