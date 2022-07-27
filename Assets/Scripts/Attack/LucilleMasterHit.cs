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
    int delaytime = 0;

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

    override protected void UpdateMasterHit()
    {
        base.UpdateMasterHit();

        if(movement.ledgeHanging || status.HasEnemyStunEffect())
        {
            resetTerminalVelocity();
        }
    
        //Handle neutral special attacks
        if(listeningForDirection)
        {
            if(!drifter.input[0].Special) delaytime++;
            if(delaytime >5 && listeningForThrow) W_Neutral_Throw();

            if(drifter.input[0].MoveX != 0 || drifter.input[0].MoveY!= 0)
            {
                HeldDirection = new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);

                if(listeningForThrow)
                {
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
        

        if(HeldDirection.y < 0)drifter.PlayAnimation("W_Side_Down");
        else if (HeldDirection.y > 0) drifter.PlayAnimation("W_Side_Up");
        else if(HeldDirection.x * movement.Facing < 0) drifter.PlayAnimation("W_Side_Back");
        else drifter.PlayAnimation("W_Side_Forward");
        
        if(orb !=null)orb.GetComponent<OrbHurtboxHandler>().setDirection(HeldDirection != Vector2.zero ? HeldDirection : Vector2.right * movement.Facing);

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
       delaytime = 0;
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
            hitbox.Facing = movement.Facing;
        }
        mark.GetComponent<StickToTarget>().victim = victim;

    }


    public void spawnOrb()
    {
        if(!isHost)return;
        

        if(orb != null)
        {
            orb.GetComponent<SyncAnimatorStateHost>().SetState("Detonate");
            orb = null;
        }

        orb = GameController.Instance.host.CreateNetworkObject("Lucille_Orb", transform.position + new Vector3(movement.Facing * 1f,1.5f,0), transform.rotation);
        orb.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in orb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = movement.Facing;
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
        

        wave = GameController.Instance.host.CreateNetworkObject("Lucille_Wave", transform.position + new Vector3(movement.Facing * 1f,3.5f,0), transform.rotation);
        wave.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing*45f,0f);
        wave.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in wave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = movement.Facing;
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
            orb.GetComponent<SyncAnimatorStateHost>().SetState("Detonate");
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
}
