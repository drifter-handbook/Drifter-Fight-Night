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

    public float returnSpeed = 17.5f;

    SyncAnimatorStateHost anim;
    PlayerAttacks attacks;
    GameObject Orro;
    BeanState targetPos;
    public Rigidbody2D rb;
    bool following = true;
    float beancountdown = 1f;

    public bool canAct = false;
    public bool alive = false;

    //Queue<BeanState> states = new Queue<BeanState>();

    BeanState state;

    public class BeanState
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

        if(state!= null && HitstunDuration <=0)
        {
            //Get the next state for bean to move towards
            targetPos = state;

            //Flip bena to the targeted direction, if he can act
            if(canAct)
                facing = targetPos.Facing;

            //If bean is currently following orro,
            if(following && canAct)
            {

                if(!alive)
                {
                    //Heal bean if he is dead
                    percentage -= 4f * Time.deltaTime;
                    if(percentage <= 0)
                    {
                        percentage = 0;
                        alive = true;
                        anim.SetState("Bean_Spawn");
                    }
                }

                //Return to orro
                //If bean is too far away (more than 3 stage lengths, he will immediately teleport to orro.
                if(Vector3.Distance(rb.position,targetPos.Pos) > 100f)
                {
                    rb.position = targetPos.Pos;
                    transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z);
                }

                //If bean is returning to orro, he will move at a slower speed and not heal
                if(Vector3.Distance(rb.position,targetPos.Pos) > 2.8f)
                {
                    rb.position =  Vector3.MoveTowards(rb.position,targetPos.Pos,returnSpeed * Time.deltaTime);
                    transform.localScale = new Vector3((targetPos.Pos.x > rb.position.x ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                        beancountdown = .5f;
                }
                //Follow orro while attatched
                //Bean follows more closely while attatched to not get left behind
                else
                {
                    //Tick down beans damage when he is attatched to orro
                    if(percentage > 0)percentage -= 2f * Time.deltaTime;
                    // if(!alive && percentage <= 0)
                    // {
                    //     anim.SetState("Bean_Spawn");
                    //     canAct = false;
                    //     percentage = 0;
                    //     alive = true;
                    // }

                    //Follow Logic
                    rb.position =  Vector3.Lerp(rb.position,targetPos.Pos,.25f * beancountdown);
                    transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                    if(beancountdown < 1f)beancountdown += 2.5f * Time.deltaTime;
                }
            }

        }
        //If bean is in hitstun, tick down his hitstun counter and remove all unused states
        else if(HitstunDuration > 0)
        {
            state = null;
            HitstunDuration -= Time.deltaTime;
            if(HitstunDuration <=0)returnToNeutral();
        }
    }


    //Enqueus a state for bean to mimic after a short delay
    public void addBeanState(Vector3 pos,int facingDir)
    {
        if(!GameController.Instance.IsHost)return;
        state = new BeanState(pos,facingDir);
    }

    //Tells bean to start returning to orro. 
    public void recallBean(Vector3 pos,int facingDir)
    {
        if(!GameController.Instance.IsHost)return;

        if(!following)
        {
            state = null;
            targetPos = new BeanState(pos, facingDir);
            following = true; 
        }
        else
        {
            state = null;
            addBeanState(rb.position,facing);
            following = false;
        }
        
    }

    //BEAN IS GONE :Crab:
    public void die()
    {
        if(!GameController.Instance.IsHost || !alive)return;
        canAct = false;
        alive = false;
        rb.velocity = Vector3.zero;
        anim.SetState("Bean_True_Death");
    }

    //Sends bean out at a set speed.
    public void setBean(float speed)
    {
        if(!GameController.Instance.IsHost || HitstunDuration > 0f || !alive)return;
        state = null;
        following = false;
        transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
        if(speed > 0 && Vector3.Distance(rb.position,targetPos.Pos) < 2.8f)
            rb.velocity = new Vector3(facing * speed,0,0);
    }

    //Spawns a mutlihit razor projectile for Orro's jab
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


    //Spawns a side special projectile for Bean, scaling more slowly than Orro
    public void SpawnBeanSideW()
    {
        UnityEngine.Debug.Log("BEAN!");
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

    //Returns bean to his neutral state, clearing all previous states and variables.
    public void returnToNeutral()
    {
        if(!GameController.Instance.IsHost)return;
        state = null;
        canAct = true;
        anim.SetState("Bean_Idle");
    }


    //Use this at the end of beans death animation
    public void setCanAct()
    {
        if(!GameController.Instance.IsHost)return;
        state = null;
        rb.position = targetPos.Pos;
        canAct = true;
        following = true;
        //alive = true;
    }

    //Plays a follow up state, ignoring if bean "can act" or not. 
    //Still will not play if he is in hitstun
    public void playFollowState(String stateName)
    {
        if(!GameController.Instance.IsHost || !alive || HitstunDuration >0) return;
        canAct = false;
        transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y, transform.localScale.z); 
        anim.SetState(stateName);
    }


    //Plays an animation for bean, if he can act and is alive
    public void playState(String stateName)
    {
        if(!GameController.Instance.IsHost || !canAct || !alive)return;

        canAct = false;

        transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y, transform.localScale.z); 
        anim.SetState(stateName);
    }
 
    //Refreshes beans hitboxes so he can multihit
    public void multihit()
    {
        if(!GameController.Instance.IsHost)return;
        attacks.SetMultiHitAttackID();
    }

    //Registers a hit on bean, and handles his counter.
    //If bean has taken over 40%, he becomes inactive untill he can heal
    public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {

        int returnCode = -3;

        if(GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && !oldAttacks.ContainsKey(attackID))
        {
            if(following && Vector3.Distance(rb.position,targetPos.Pos) <= 2.8f) return -3;

                returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackType,attackData);

                if(returnCode >= 0)anim.SetState("Hitstun");

            if(percentage > 40f)
            {
                    alive = false;
                    canAct = false;
                    anim.SetState("Bean_Death");
                    HitstunDuration = 0f;
                    rb.velocity = Vector3.zero;
                }
        }

        return returnCode;

    }
}
