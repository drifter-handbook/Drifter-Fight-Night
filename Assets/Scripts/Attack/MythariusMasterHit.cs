using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MythariusMasterHit : MasterHit
{

    bool listeningForDirection = false;
    int delaytime = 0;
    Vector2 HeldDirection = Vector2.zero;

    GameObject bird;

    //Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
        MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
        DeserializeBaseFrame(p_frame);
    }

    public void warpStart()
    {

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Myth_Warp_Start,false);
    }

    public void warpEnd()
    {

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Myth_Warp_End,false);
    }

    //Move to particle system
    public void ring()
    {
        GameObject ring = GameController.Instance.CreatePrefab("LaunchRing", transform.position + new Vector3(0,1.4f),  transform.rotation);
    }

    public void Spawn_Bird()
    {

        if(bird != null)
        {
            Destroy(bird);
        }
        
        bird = GameController.Instance.CreatePrefab("Mytharius_Bird", transform.position + new Vector3(movement.Facing * 1.4f,3f), transform.rotation);
        bird.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in bird.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
       }

       bird.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
       bird.GetComponent<Bird_Wrangler>().setup(drifter.gameObject,attacks,movement.Facing,drifter.GetColor());

    }


}
