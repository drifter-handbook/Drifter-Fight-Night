using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{

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
        facing = movement.Facing;
        GameObject ring = GameController.Instance.host.CreateNetworkObject("LaunchRing", transform.position + new Vector3(0,1.4f),  transform.rotation);
    }

}
