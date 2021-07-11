using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BeanWrangler : MonoBehaviour
{
    // Start is called before the first frame update
    

    public int facing = 1;
    public int color = 0;

    SyncAnimatorStateHost anim;
    PlayerAttacks attacks;
    GameObject Orro;
    BeanState targetPos;
    Rigidbody2D rb;
    bool following = true;
    float beancountdown = 1f;
    bool canAct = true;

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

        if(!GameController.Instance.IsHost)return;

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

        if(states.Count > 0)
        {

            targetPos = states.Dequeue();

            if(canAct)
                facing = targetPos.Facing;

            if(following && canAct)
            {
                
                rb.position =  Vector3.Lerp(rb.position,targetPos.Pos,beancountdown *.15f);

                if(beancountdown < 1f)
                {
                    beancountdown += Time.deltaTime/2f;
                    transform.localScale = new Vector3((targetPos.Pos.x > rb.position.x ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                }
                else
                {
                    transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                }
            }
        
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
        beancountdown = 0f;
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
        beancountdown = 0f;
        anim.SetState("Bean_Idle");
    }

    public void playState(String stateName)
    {
        if(!GameController.Instance.IsHost || !canAct)return;

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
}
