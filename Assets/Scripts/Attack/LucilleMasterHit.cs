using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class LucilleMasterHit : MasterHit
{

    Queue<GameObject> rifts = new Queue<GameObject>();

    //Queue<GameObject> rifts = new Queue<GameObject>(new GameObject[3]);

    bool jumpGranted = false;

    GameObject bomb;

    public GrabHitboxCollision bombGrab;

    //Coroutine riftDetonation;

    void Update()
    {
        if(!isHost)return;
        if(movement.terminalVelocity !=  terminalVelocity && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminal();
            jumpGranted = false;
        }
        if(jumpGranted && movement.grounded)jumpGranted = false;

        if(status.HasStatusEffect(PlayerStatusEffect.DEAD) && rifts.Count >0) collapseAllPortals(0);
    }

    public void setTerminalVelocity()
    {
        if(!isHost)return;
        movement.canLandingCancel = false;  
        movement.terminalVelocity = 75;
    }

    public void resetTerminal()
    {
        if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }

    public void setBombTarget()
    {
        if(!isHost)return;
        if(bomb != null)
        {
            bomb.GetComponent<StickToTarget>().victim = bombGrab.victim;
            bombGrab.victim = null;
            bomb = null;
        } 
    }


    public void Side_Grab_Bomb()
    {
        // jump upwards and create spear projectile
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(facing * 3.7f,3.3f,0);

        bomb = GameController.Instance.host.CreateNetworkObject("Lucille_Bomb", transform.position + pos, transform.rotation);

        foreach (HitboxCollision hitbox in bomb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
            
        }
    }

    public void SpawnRift()
    {
        // jump upwards and create spear projectile
        if(!isHost || drifter.superCharge < 1)return;

        facing = movement.Facing;

        //remove the oldest portal if size limit would be exceeded.
        //if(getTotalPortalSize() >= 9) rifts.Dequeue().GetComponent<LucillePortal>().decay();
       
        GameObject rift = GameController.Instance.host.CreateNetworkObject("Lucille_Rift", transform.position + new Vector3(0,3.5f,0), transform.rotation);

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
       
        drifter.superCharge -= 1f;
        rifts.Enqueue(rift);
        
        //drifter.SetCharge(3 + getTotalPortalSize());
    }

    public void warpToNearestRift()
    {
        if(!isHost)return;
        GameObject[] riftarray = rifts.ToArray();

        float shortestDistance = 8000f;
        GameObject targetPortal = null;

        foreach(GameObject rift in riftarray)
        {
            if(shortestDistance > Vector3.Distance(rift.transform.position, drifter.transform.position))
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

            targetPortal.GetComponent<LucillePortal>().detonate();
        }
        else
        {
            UnityEngine.Debug.Log("NO PORTALS");
        }
    }


    public void breakRift(GameObject self,bool pauseOnHit = false)
    {
        rifts = new Queue<GameObject>(rifts.Where<GameObject>(x => x != self));

        if(pauseOnHit)status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.1f * self.GetComponent<LucillePortal>().size + .1f);

        // if(getTotalPortalSize() == 0) drifter.SetCharge(0);
        // else drifter.SetCharge(3 + getTotalPortalSize());
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

    int getTotalPortalSize()
    {
        int totalPortalSize = 0;

        //Lucille can have up to 3 total portal size active
        //Calculates the current size before a new poeral is added

        if(rifts.Count ==0)return 0;

        rifts = new Queue<GameObject>(rifts.Where<GameObject>(x => x != null));

        foreach(GameObject riftObj in rifts) totalPortalSize += riftObj.GetComponent<LucillePortal>().size;

        if(totalPortalSize >9)
        {
            UnityEngine.Debug.Log("TOO MUCH GIRTH");
            return 9;
        }

        return totalPortalSize;
    }


    //Can gain 1 extra jump by bouncing on a portal. Only works once per airtime.
    public void grantJump()
    {
        if(!isHost || movement.currentJumps >= movement.numberOfJumps - 1 || jumpGranted)return;
        movement.currentJumps++;
        jumpGranted = true;
    }



    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,4f * framerateScalar);
    }

    public override void rollGetupStart()
    {
        //unused
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        rb.position += new Vector2(8.5f* facing,5.8f);
    }
}
