using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BeanWrangler : NonplayerHurtboxHandler
{
    // Start is called before the first frame update
    

    public int facing = 1;
    public int color = 0;

    public int charge = 0;

    public float returnSpeed = 15f;

    SyncAnimatorStateHost anim;
    PlayerAttacks attacks;
    GameObject Orro;
    BeanState targetPos;
    Rigidbody2D rb;
    bool following = true;
    float beancountdown = 1f;

    public bool canAct = false;
    public bool alive = false;

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
                    percentage -= 4f * Time.deltaTime;
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
                    if(percentage > 0)percentage -= 2f * Time.deltaTime;
                    if(!alive && percentage <= 0)
                    {
                        anim.SetState("Bean_Spawn");
                        canAct = false;
                        percentage = 0;
                        alive = true;
                        

                    }

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

    public void die()
    {
        if(!GameController.Instance.IsHost || !alive)return;
        canAct = false;
        alive = false;
        rb.velocity = Vector3.zero;
        anim.SetState("Bean_True_Death");
    }

    public void setBean(float speed)
    {
        if(!GameController.Instance.IsHost)return;
        states.Clear();
        following = false;
        transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
        if(speed >0)
            rb.velocity = new Vector3(facing * speed,0,0);
    }

    public void bean_ground_Neutral()
    {
        if(!GameController.Instance.IsHost)return;

        GameObject razor = GameController.Instance.host.CreateNetworkObject("SpaceRazor", transform.position , transform.rotation);
        razor.transform.localScale = new Vector3(facing *10,10,1f);
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


    public void SpawnBeanSideW()
    {

        UnityEngine.Debug.Log("BEAN!");

        //if(!GameController.Instance.IsHost || charge <=3)return;
        Vector3 pos = new Vector3(2.4f * facing,3.4f,0);

        multihit();

        GameObject rip = host.CreateNetworkObject("BeanWSide", transform.position + pos, transform.rotation);
        rip.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in rip.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = Orro;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
            hitbox.OverrideData.StatusDuration = Mathf.Max((charge-3)/3,1);
       }

       rip.GetComponent<SyncProjectileColorDataHost>().setColor(color);
       charge = 0;
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
        rb.position = targetPos.Pos;
        canAct = true;
    }

    public void playFollowState(String stateName)
    {
        if(!GameController.Instance.IsHost || !alive || HitstunDuration >0) return;
        anim.SetState(stateName);
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

        if(following && Vector3.Distance(rb.position,targetPos.Pos) <= 2.8f) return -3;

        int returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackType,attackData);

        if(percentage > 40f)
        {
            alive = false;
            canAct = false;
            anim.SetState("Bean_Death");
            HitstunDuration = 0f;
            rb.velocity = Vector3.zero;
            //Play bean death animation
        }

        return returnCode;

    }
}
