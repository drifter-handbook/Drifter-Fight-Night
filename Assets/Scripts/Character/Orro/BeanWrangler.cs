using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class BeanWrangler : NonplayerHurtboxHandler
{
    // Start is called before the first frame update
    public int color = 0;

    public int charge = 0;

    public float returnSpeed = 25f;

    public int Bean_Respawn_Delay = 180;

    Animator animator;
    PlayerAttacks attacks;
    GameObject Orro;
    BeanState targetPos;
    bool following = true;
    float beancountdown = 1f;

    public bool canAct = false;
    public bool alive = false;
    float prevHitstunDuration;

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

        base.Start();
        animator = GetComponent<Animator>();
        //Movement Stuff
        targetPos = new BeanState(rb.position, facing);
        Orro = gameObject.GetComponentInChildren<HitboxCollision>().parent;
        attacks = gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponent<PlayerAttacks>();

    }

    public override void UpdateFrame()
    {

        base.UpdateFrame();
        prevHitstunDuration = HitstunDuration;
        
        if(HitstunDuration >0) 
            return;
        else if(prevHitstunDuration != HitstunDuration && HitstunDuration <=0 && alive)
            returnToNeutral();
        else
        {
            //Get the next state for bean to move towards
            targetPos = state;

            //Flip bena to the targeted direction, if he can act

            //If bean is currently following orro,
            if(following && canAct)
            {
                facing = targetPos.Facing;

                if(!alive)
                {
                    //Heal bean if he is dead
                    if(percentage > 0) percentage -= 4f * Time.fixedDeltaTime;
                    if(percentage <= 0)
                    {
                        percentage = 0;
                        alive = true;
                        PlayAnimation("Bean_Spawn");
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
                if(Vector3.Distance(rb.position,targetPos.Pos) > 3.8f)
                {
                    rb.position =  Vector3.MoveTowards(rb.position,targetPos.Pos,returnSpeed * Time.fixedDeltaTime);
                    transform.localScale = new Vector3((targetPos.Pos.x > rb.position.x ? 1f : -1f) * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                        beancountdown = .5f;
                }
                //Follow orro while attatched
                //Bean follows more closely while attatched to not get left behind
                else
                {
                    //Tick down beans damage when he is attatched to orro
                    if(percentage > 0) percentage -= 2f * Time.fixedDeltaTime;

                    //Follow Logic
                    rb.position =  Vector3.Lerp(rb.position,targetPos.Pos,.25f * beancountdown);
                    transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
                    if(beancountdown < 1f)beancountdown += 2.5f * Time.fixedDeltaTime;
                }
            }

        }
    }

    public void PlayAnimation(string p_state, float p_normalizedTime = -1)
    {
        animator.Play(Animator.StringToHash(p_state),0,p_normalizedTime < 0 ? 0: p_normalizedTime);
    }


    //Enqueus a state for bean to mimic after a short delay
    public void addBeanState(Vector3 pos,int facingDir)
    {
        state = new BeanState(pos,facingDir);
    }

    //Enqueus a state for bean to mimic after a short delay
    public void setBeanDirection(int facingDir)
    {
        facing = facingDir;
        transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
    }

    //Tells bean to start returning to orro. 
    public void recallBean(Vector3 pos,int facingDir)
    {

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
        PlayAnimation("Bean_True_Death");
    }

    //Sends bean out at a set speed.
    public void setBean(float speed)
    {
        if(!GameController.Instance.IsHost || HitstunDuration > 0f || !alive)return;
        state = null;
        following = false;
        transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
                        transform.localScale.y, transform.localScale.z); 
        if(speed > 0 && Vector3.Distance(rb.position,targetPos.Pos) < 3.8f)
            rb.velocity = new Vector3(facing * speed,0,0);
    }

    //Spawns a mutlihit razor projectile for Orro's jab
    public void bean_ground_Neutral()
    {

        GameObject razor = GameController.Instance.CreatePrefab("SpaceRazor", transform.position , transform.rotation);
        razor.transform.localScale = new Vector3(facing *10,10,1f);
        attacks.SetMultiHitAttackID();
        foreach (HitboxCollision hitbox in razor.GetComponentsInChildren<HitboxCollision>(true))
        {
                hitbox.parent = Orro;
                hitbox.AttackID = attacks.AttackID;
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

        GameObject rip = GameController.Instance.CreatePrefab("BeanWSide", transform.position + pos, transform.rotation);
        rip.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in rip.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = Orro;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = facing;
       }

       rip.GetComponent<SyncProjectileColorDataHost>().setColor(color);
       charge = 0;
    }

    //Returns bean to his neutral state, clearing all previous states and variables.
    public void returnToNeutral()
    {
        state = null;
        canAct = true;
        PlayAnimation("Bean_Idle");
    }


    //Use this at the end of beans death animation
    public void setCanAct()
    {
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
        PlayAnimation(stateName);
    }


    //Plays an animation for bean, if he can act and is alive
    public void playState(String stateName)
    {
        if(!GameController.Instance.IsHost || !canAct || !alive)return;

        canAct = false;

        transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y, transform.localScale.z); 
        PlayAnimation(stateName);
    }


    //Plays an animation for bean, if he can act and is alive
    public void playChargeState(String stateName)
    {
        if(!GameController.Instance.IsHost || HitstunDuration >0  || !canAct || !alive)return;

        transform.localScale = new Vector3(targetPos.Facing * Mathf.Abs(transform.localScale.x),
                    transform.localScale.y, transform.localScale.z); 
        PlayAnimation(stateName);
    }
 
    //Refreshes beans hitboxes so he can multihit
    public void multihit()
    {
        attacks.SetMultiHitAttackID();
    }

    //Registers a hit on bean, and handles his counter.
    //If bean has taken over 40%, he becomes inactive untill he can heal
    public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, SingleAttackData attackData)
    {

        int returnCode = -3;

        if(GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && CanHit(attackID))
        {
            if(following && Vector3.Distance(rb.position,targetPos.Pos) <= 3.8f) return -3;

                returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackData);
                oldAttacks[attackID] = MAX_ATTACK_DURATION;

                if(returnCode >= 0)PlayAnimation("Hitstun");

            if(percentage > maxPercentage)
            {
                    alive = false;
                    canAct = false;
                    PlayAnimation("Bean_Death");
                    HitstunDuration = 0;
                    //Delay before bean begins recharging
                    HitPauseDuration = Bean_Respawn_Delay;
                    rb.velocity = Vector3.zero;
                    delayedVelocity = Vector3.zero;
            }
        }

        return returnCode;

    }
}
