using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LucilleMasterHit : MasterHit
{

    Queue<GameObject> rifts = new Queue<GameObject>();

    //Queue<GameObject> rifts = new Queue<GameObject>(new GameObject[3]);

    bool jumpGranted = false;

    bool listeningForDirection = false;

    GameObject accretionDisk;

    GrabHitboxCollision[] grabBoxes;

    //Coroutine riftDetonation;

    void Start()
    {
        grabBoxes = drifter.GetComponentsInChildren<GrabHitboxCollision>(true);
    }

    void Update()
    {
        if(!isHost)return;
        if(movement.terminalVelocity !=  terminalVelocity && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminalVelocity();
            jumpGranted = false;
        }
        if(jumpGranted && movement.grounded)jumpGranted = false;

        if(status.HasStatusEffect(PlayerStatusEffect.DEAD) && rifts.Count >0) collapseAllPortals(0);
    }

    new void FixedUpdate()
    {
        if(!isHost)return;

        base.FixedUpdate();

        //Handle neutral special attacks
        if(listeningForDirection)
        {
            movement.move(14f,false);
        }
    }

    public new void returnToIdle()
    {
        base.returnToIdle();
        cancelListeningForDirection();
        foreach(GrabHitboxCollision hitbox in grabBoxes)
            hitbox.victim = null;

    }

    public void setListeningForDirection()
    {
       if(!isHost)return;
       listeningForDirection = true; 
    }

    public void cancelListeningForDirection()
    {
       if(!isHost)return;
       listeningForDirection = false; 
    }


    public void infectWithRift()
    {
        if(!isHost)return;
        facing = movement.Facing;

        if(accretionDisk != null)
        {
            accretionDisk.GetComponent<LucillePortal>().decay();
            breakRift(accretionDisk);
        }

        foreach(GrabHitboxCollision infector in grabBoxes)
        {
            if(infector.victim != null)
            {
                accretionDisk = GameController.Instance.host.CreateNetworkObject("Lucille_Disk", infector.victim.GetComponent<Rigidbody2D>().position, transform.rotation);
                foreach (HitboxCollision hitbox in accretionDisk.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = drifter.gameObject;
                    hitbox.AttackID = attacks.AttackID;
                    hitbox.AttackType = attacks.AttackType;
                    hitbox.Active = true;
                    hitbox.Facing = facing;
                }
                accretionDisk.GetComponent<StickToTarget>().victim = infector.victim;
                infector.victim = null;  
                accretionDisk.GetComponent<LucillePortal>().drifter = drifter.gameObject;
                rifts.Enqueue(accretionDisk);
                return;
            }
        }
    }

    public void SpawnRift()
    {
        if(!isHost || drifter.superCharge < 1)return;

        SpawnRift(transform.position + new Vector3(0,3.5f,0));
        drifter.superCharge -= 1f;
        
    }


    public void SpawnRift(Vector3 pos)
    {

        if(!isHost)return;

        facing = movement.Facing;
       
        GameObject rift = GameController.Instance.host.CreateNetworkObject("Lucille_Rift", pos, transform.rotation);

        foreach (HitboxCollision hitbox in rift.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
            
        }
        rift.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        rift.GetComponent<LucillePortal>().drifter = drifter.gameObject;
       
        rifts.Enqueue(rift);
    }

    public void warpToNearestRift()
    {
        if(!isHost)return;
        GameObject[] riftarray = rifts.ToArray();

        float shortestDistance = 8000f;
        GameObject targetPortal = null;

        foreach(GameObject rift in riftarray)
        {
            if(rift != null && shortestDistance > Vector3.Distance(rift.transform.position, drifter.transform.position))
            {
                shortestDistance = Vector3.Distance(rift.transform.position, drifter.transform.position);
                targetPortal = rift;
            }
        }

        if(targetPortal != null)
        {

            foreach (HitboxCollision hitbox in targetPortal.GetComponentsInChildren<HitboxCollision>(true)) hitbox.AttackID -=3;

            rb.transform.position = targetPortal.transform.position;

            breakRift(targetPortal);

            attacks.resetRecovery();

            targetPortal.GetComponent<LucillePortal>().detonate();
        }
        else
        {   
            infectWithRift();
            UnityEngine.Debug.Log("NO PORTALS");
        }
    }


    public void breakRift(GameObject self,bool pauseOnHit = false)
    {
        rifts = new Queue<GameObject>(rifts.Where<GameObject>(x => x != self));

        if(pauseOnHit)status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.1f * self.GetComponent<LucillePortal>().size + .1f);
    }


    public void collapseAllPortals(int explosiveDelete = 1)
    {
        if(!isHost)return;
        GameObject rift;
        facing = movement.Facing;
        while(rifts.Count >0)
        {
            rift = rifts.Dequeue();

            if(rift == null)continue;

            //Gain half of the meter value of the portal back when all are collapsed
            drifter.gainSuperMeter(.33f * rift.GetComponent<LucillePortal>().size);
            foreach (HitboxCollision hitbox in rift.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.AttackID -=3;
                hitbox.Facing = facing;
            }
            if(explosiveDelete != 0)rift.GetComponent<LucillePortal>().detonate();
            else rift.GetComponent<LucillePortal>().decay();
        }
        rifts = new Queue<GameObject>();
    }

    //Can gain 1 extra jump by bouncing on a portal. Only works once per airtime.
    public void grantJump()
    {
        if(!isHost || movement.currentJumps >= movement.numberOfJumps - 1 || jumpGranted)return;
        movement.currentJumps++;
        jumpGranted = true;
    }
}
