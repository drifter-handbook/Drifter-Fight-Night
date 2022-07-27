using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{

    GameObject bird;

    public void warpStart()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Myth_Warp_Start,false);
    }

    public void warpEnd()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Myth_Warp_End,false);
    }

    public void ring()
    {
        GameObject ring = GameController.Instance.host.CreateNetworkObject("LaunchRing", transform.position + new Vector3(0,1.4f),  transform.rotation);
    }

    public void Spawn_Bird()
    {
        if(!isHost)return;

        if(bird != null)
        {
            Destroy(bird);
        }
        
        bird = host.CreateNetworkObject("Mytharius_Bird", transform.position + new Vector3(movement.Facing * 1.4f,3f), transform.rotation);
        bird.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in bird.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Facing = movement.Facing;
       }

       bird.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
       bird.GetComponent<Bird_Wrangler>().setup(drifter.gameObject,attacks,movement.Facing,drifter.GetColor());

    }


}
