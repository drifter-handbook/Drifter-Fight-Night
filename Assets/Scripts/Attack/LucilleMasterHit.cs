using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LucilleMasterHit : MasterHit
{

    // Queue<GameObject> rifts = new Queue<GameObject>();

    // bool jumpGranted = false;

    public float warpSpeed = 200;

    bool listeningForDirection = false;
    bool listeningForThrow = false;

    GameObject mark;
    GameObject orb;
    GameObject wave;

    GrabHitboxCollision[] grabBoxes;

    Vector2 HeldDirection;

    //Coroutine riftDetonation;

    void Start()
    {
        grabBoxes = drifter.GetComponentsInChildren<GrabHitboxCollision>(true);
    }

    new void FixedUpdate()
    {
        if(!isHost)return;

        base.FixedUpdate();

        if(movement.terminalVelocity !=  terminalVelocity && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminalVelocity();
        }
    
        //Handle neutral special attacks
        if(listeningForDirection)
        {
            if(drifter.input[0].MoveX != 0 || drifter.input[0].MoveY!= 0)
            {
                HeldDirection = new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
                if(listeningForThrow)
                {
                    if(orb !=null)orb.GetComponent<OrbHurtboxHandler>().setDirection(HeldDirection != Vector2.zero ? HeldDirection : Vector2.right * facing);
                    W_Neutral_Throw();
                }
            }                
        }
    }

    public new void returnToIdle()
    {
        base.returnToIdle();
        listeningForDirection = false;
        listeningForThrow = false; 
        foreach(GrabHitboxCollision hitbox in grabBoxes)
            hitbox.victim = null;

    }

    public void W_Neutral_Throw()
    {
        if(!isHost)return;
        facing = movement.Facing;

        if(HeldDirection.y < 0)drifter.PlayAnimation("W_Side_Down");
        else if (HeldDirection.y > 0) drifter.PlayAnimation("W_Side_Up");
        else if(HeldDirection.x * facing < 0) drifter.PlayAnimation("W_Side_Back");
        else drifter.PlayAnimation("W_Side_Forward");

        HeldDirection = Vector2.zero;

        listeningForDirection = false;
    }

    public void listenForDirection()
    {
       if(!isHost)return;
       listeningForDirection = true; 
    }

    public void listenForThrow()
    {
       if(!isHost)return;
       listenForDirection();
       listeningForThrow = true; 
    }

    public void infect()
    {
        foreach(GrabHitboxCollision infector in grabBoxes)
        {
            if(infector.victim != null)
            {
                infect(infector.victim);
                infector.victim = null;  
                return;
            }
        }

    }


    public void infect(GameObject victim)
    {
        if(!isHost)return;
        facing = movement.Facing;

        if(mark != null)
        {
            // mark.GetComponent<LucillePortal>().decay();
            // breakRift(mark);
            Destroy(mark);
            mark = null;
        }

        mark = GameController.Instance.host.CreateNetworkObject("Lucille_Disk", victim.GetComponent<Rigidbody2D>().position, transform.rotation);
        foreach (HitboxCollision hitbox in mark.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = facing;
        }
        mark.GetComponent<StickToTarget>().victim = victim;

    }


    public void spawnOrb()
    {
        if(!isHost)return;
        facing = movement.Facing;

        if(orb != null)
        {
            orb.GetComponent<SyncAnimatorStateHost>().SetState("Detonate");
            orb = null;
        }

        orb = GameController.Instance.host.CreateNetworkObject("Lucille_Orb", transform.position + new Vector3(facing * 1f,1.5f,0), transform.rotation);
        orb.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in orb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = facing;
        }
        orb.GetComponent<Infector>().Lucille = this;
        orb.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());

        foreach (HurtboxCollision hurtbox in orb.GetComponentsInChildren<HurtboxCollision>(true))
            hurtbox.owner = drifter.gameObject;

        return;
    }

    public void spawnWave()
    {
        if(!isHost)return;
        facing = movement.Facing;

        wave = GameController.Instance.host.CreateNetworkObject("Lucille_Wave", transform.position + new Vector3(facing * 1f,3.5f,0), transform.rotation);
        wave.GetComponent<Rigidbody2D>().velocity = new Vector3(facing*45f,0f);
        wave.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in wave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = facing;
        }
        wave.GetComponent<Infector>().Lucille = this;
        wave.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        return;
    }
    

    public void warpToNearestRift()
    {
        if(!isHost)return;
        if( mark != null)
        {
            rb.transform.position = mark.transform.position;
            Destroy(mark);
            mark = null;
            attacks.currentUpRecoveries = attacks.maxRecoveries;
        }
        else if( orb != null)
        {
            rb.transform.position = orb.transform.position;
            Destroy(orb);
            orb = null;
        }
        else
        {
            if(HeldDirection == Vector2.zero) HeldDirection = Vector2.up;
            rb.velocity = Vector3.Normalize(HeldDirection) * warpSpeed;
            HeldDirection = Vector2.zero;
            listeningForDirection = false;
        }
        
    }


    // public void SpawnRift()
    // {
    //     if(!isHost || drifter.superCharge < 1)return;

    //     SpawnRift(transform.position + new Vector3(0,3.5f,0));
    //     drifter.superCharge -= 1f;
        
    // }


    // public void SpawnRift(Vector3 pos)
    // {

    //     if(!isHost)return;

    //     facing = movement.Facing;
       
    //     GameObject rift = GameController.Instance.host.CreateNetworkObject("Lucille_Rift", pos, transform.rotation);

    //     foreach (HitboxCollision hitbox in rift.GetComponentsInChildren<HitboxCollision>(true))
    //     {
    //         hitbox.parent = drifter.gameObject;
    //         hitbox.AttackID = attacks.AttackID;
    //         hitbox.AttackType = attacks.AttackType;
    //         hitbox.Facing = facing;
            
    //     }
    //     rift.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    //     rift.GetComponent<LucillePortal>().drifter = drifter.gameObject;
       
    //     rift.GetComponent<HurtboxCollision>().owner = drifter.gameObject;

    //     rifts.Enqueue(rift);
    // }

    

    // public void breakRift(GameObject self,bool pauseOnHit = false)
    // {
    //     rifts = new Queue<GameObject>(rifts.Where<GameObject>(x => x != self));

    //     if(pauseOnHit)status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.1f * self.GetComponent<LucillePortal>().size + .1f);
    // }


    // public void collapseAllPortals(int explosiveDelete = 1)
    // {
    //     if(!isHost)return;
    //     GameObject rift;
    //     facing = movement.Facing;
    //     while(rifts.Count >0)
    //     {
    //         rift = rifts.Dequeue();

    //         if(rift == null)continue;

    //         //Gain half of the meter value of the portal back when all are collapsed
    //         drifter.gainSuperMeter(.33f * rift.GetComponent<LucillePortal>().size);
    //         foreach (HitboxCollision hitbox in rift.GetComponentsInChildren<HitboxCollision>(true))
    //         {
    //             hitbox.AttackID -=3;
    //             hitbox.Facing = facing;
    //         }
    //         if(explosiveDelete != 0)rift.GetComponent<LucillePortal>().detonate();
    //         else rift.GetComponent<LucillePortal>().decay();
    //     }
    //     rifts = new Queue<GameObject>();
    // }

    // //Can gain 1 extra jump by bouncing on a portal. Only works once per airtime.
    // public void grantJump()
    // {
    //     if(!isHost || movement.currentJumps >= movement.numberOfJumps - 1 || jumpGranted)return;
    //     movement.currentJumps++;
    //     jumpGranted = true;
    // }
}
