using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BeanWrangler : NonplayerHurtboxHandler
{
    // Start is called before the first frame update
    

    public int facing = 1;
    public int color = 0;

    public float returnSpeed = 15f;

    SyncAnimatorStateHost anim;
    PlayerAttacks attacks;
    GameObject Orro;
    BeanState targetPos;
    Rigidbody2D rb;
    bool following = true;
    float beancountdown = 1f;
    bool canAct = true;
    bool alive = false;

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

    new void Start()
    {

        if(!GameController.Instance.IsHost)return;

        base.Start();

        anim = GetComponent<SyncAnimatorStateHost>();
        //Movement Stuff
        rb = GetComponent<Rigidbody2D>();
        targetPos = new BeanState(rb.position, facing);
        Orro = gameObject.GetComponentInChildren<HitboxCollision>().parent;
        attacks = gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponent<PlayerAttacks>();

    }

    void Update()
    {

        if(!GameController.Instance.IsHost)return;

        //if(states.Count == 0)Destroy(gameObject);

        if(states.Count > 0 && HitstunDuration <=0)
        {

            targetPos = states.Dequeue();

            if(canAct)
                facing = targetPos.Facing;

            if(following && canAct)
            {

                if(!alive)
                {
                    percentage -= Time.deltaTime;
                    if(percentage <= 0)
                    {
                        percentage = 0;
                        alive = true;

                        //play spawn animation
                    }
                }

                //Return to orro
                if(Vector3.Distance(rb.position,targetPos.Pos) > 100f)
                {
                    rb.position = targetPos.Pos;
                    transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z);
                }

                if(Vector3.Distance(rb.position,targetPos.Pos) > 2.8f)
                {
                    rb.position =  Vector3.MoveTowards(rb.position,targetPos.Pos,returnSpeed * Time.deltaTime);
                    transform.localScale = new Vector3((targetPos.Pos.x > rb.position.x ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                        beancountdown = .5f;
                }
                //Follow orro while attatched
                else
                {
                    //Tick down beans damage when he is attatched to orro
                    percentage -= Time.deltaTime;
                    //Follow Logic
                    rb.position =  Vector3.Lerp(rb.position,targetPos.Pos,.25f * beancountdown);
                    transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                    if(beancountdown < 1f)beancountdown += 2.5f * Time.deltaTime;
                }
            }

        }
        else if(HitstunDuration >0)
        {
            states.Clear();
            HitstunDuration -= Time.deltaTime;
        }
    }


    public void addBeanState(Vector3 pos,int facingDir)
    {
        if(!GameController.Instance.IsHost)return;
        states.Enqueue(new BeanState(pos,facingDir));
    }


    public void recallBean(Vector3 pos,int facingDir)
    {
        if(!GameController.Instance.IsHost)return;
        states.Clear();
        targetPos = new BeanState(pos, facingDir);
        following = true;
    }

    public void setBean(float speed)
    {
        if(!GameController.Instance.IsHost)return;
        states.Clear();
        following = false;
        if(speed >0)
            rb.velocity = new Vector3(facing * speed,0,0);
    }

    public void bean_ground_Neutral()
    {
        if(!GameController.Instance.IsHost)return;
        Vector3 flip = new Vector3(facing *10,10,0f);
        GameObject razor = GameController.Instance.host.CreateNetworkObject("SpaceRazor", transform.position , transform.rotation);
        razor.transform.localScale = flip;
        attacks.SetMultiHitAttackID();
        foreach (HitboxCollision hitbox in razor.GetComponentsInChildren<HitboxCollision>(true))
        {
                hitbox.parent = Orro;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
        }

        razor.GetComponent<SyncProjectileColorDataHost>().setColor(color);
    }

    public void returnToNeutral()
    {
        if(!GameController.Instance.IsHost)return;
        states.Clear();
        canAct = true;
        anim.SetState("Bean_Idle");
    }


    //Use this at the end of beans death animation
    public void setCanAct()
    {
        if(!GameController.Instance.IsHost)return;
        states.Clear();
        canAct = true;
    }

    public void playState(String stateName)
    {
        if(!GameController.Instance.IsHost || !canAct || !alive)return;

        canAct = false;

        transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y, transform.localScale.z); 
        anim.SetState(stateName);
    }
 
    public void multihit()
    {
        if(!GameController.Instance.IsHost)return;
        attacks.SetMultiHitAttackID();    
    }

    public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {

        int returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackType,attackData);

        if(percentage > 40f)
        {
            alive = false;
            canAct = false;
            //Play bean death animation
        }

        return returnCode;

    }
}
