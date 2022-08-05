using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{

    bool listeningForDirection = false;
    int delaytime = 0;
    Vector2 HeldDirection = Vector2.zero;

    GameObject bird;


    override protected void UpdateMasterHit()
    {
        base.UpdateMasterHit();

        //Handle neutral special attacks
        if(listeningForDirection)
        {
            if(!drifter.input[0].Special) delaytime++;
            HeldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
            if(HeldDirection != Vector2.zero || delaytime > 5) NeutralSpecialPortal();
        }

    }

    public void listenForDirection()
    {
        listeningForDirection = true;
        delaytime = 0;
    }

    public void NeutralSpecialPortal()
    {
        listeningForDirection = false;
        movement.updateFacing();
        
        if(HeldDirection.y <0) playState("W_Neutral_D");
        else if(HeldDirection.y >0) playState("W_Neutral_U");
        else playState("W_Neutral_S");

        HeldDirection = Vector2.zero;

    }

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
