using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LucilleMasterHit : MasterHit
{

    Queue<GameObject> rifts = new Queue<GameObject>();
    float terminalVelocity;

    //Coroutine riftDetonation;

    void Start()
    {
        if(!isHost)return;
        terminalVelocity = movement.terminalVelocity;
    }

    void Update()
    {
        if(!isHost)return;
        if(movement.terminalVelocity !=  terminalVelocity && (movement.ledgeHanging || status.HasEnemyStunEffect()))
        {
            resetTerminal();
        }
    }

    public void setTerminalVelocity()
    {
        if(!isHost)return;
        movement.terminalVelocity = 75;
    }

    public void resetTerminal()
    {
        if(!isHost)return;
        movement.terminalVelocity = terminalVelocity;
    }


    public void Side_Attack_Fireball()
    {
        // jump upwards and create spear projectile
        if(!isHost)return;
        facing = movement.Facing;
        Vector3 pos = new Vector3(facing * 4.3f,3.7f,0);

        GameObject bolt = GameController.Instance.host.CreateNetworkObject("Lucille_Side_Fireball", transform.position + pos, transform.rotation);

         bolt.transform.localScale = new Vector3(facing * 10f,10f,0);

        bolt.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 10f,0f,0);
        foreach (HitboxCollision hitbox in bolt.GetComponentsInChildren<HitboxCollision>(true))
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
        if(!isHost)return;

        facing = movement.Facing;

        if(rifts.Count == 3)
        {
            //Play animation here
            rifts.Dequeue().GetComponent<LucillePortal>().playState("SoftDelete");
        }
       
        GameObject rift = GameController.Instance.host.CreateNetworkObject("Lucille_Rift", transform.position + new Vector3(0,3.5f,0), transform.rotation);

        foreach (HitboxCollision hitbox in rift.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
            
        }
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
            if(shortestDistance > Vector3.Distance(rift.transform.position, drifter.transform.position))
            {
                shortestDistance = Vector3.Distance(rift.transform.position, drifter.transform.position);
                targetPortal = rift;
            }
        }

        if(targetPortal != null)
        {
            rb.transform.position = targetPortal.transform.position;
            rifts.Clear(); 
            foreach(GameObject rift in riftarray)
            {
                if(rift != targetPortal)rifts.Enqueue(rift);
            }

            foreach (HitboxCollision hitbox in targetPortal.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.AttackID -=3;
            }

            targetPortal.GetComponent<LucillePortal>().playState("HardDelete");
        }
        else
        {
            UnityEngine.Debug.Log("NO PORTALS");
        }
    }

    public void collapseAllPortals()
    {
        if(!isHost)return;
        GameObject rift;
        facing = movement.Facing;
        while(rifts.Count >0)
        {

            rift = rifts.Dequeue();

            foreach (HitboxCollision hitbox in rift.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.AttackID -=3;
                hitbox.Facing = facing;
            }
            rift.GetComponent<LucillePortal>().playState("HardDelete");
        }
    }



    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -30f,0f);
    }

    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0,70f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * -35f,0f);
    }
}
