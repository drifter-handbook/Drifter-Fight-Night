using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoBojoMasterHit : MasterHit
{

    GameObject centaur;
    int power = 0;

    public void SpawnDownTiltWave()
    {
        if(!isHost)return;
        facing = movement.Facing;
        //Vector3 pos = new Vector3(2f * facing,2.7f,0);
        
        GameObject wave = host.CreateNetworkObject("BojoDTiltWave", transform.position , transform.rotation);
        wave.transform.localScale = new Vector3(10f * facing, 10f , 1f);

        wave.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 33,-22);
        foreach (HitboxCollision hitbox in wave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = -attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
       }

       wave.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    }

    public void whirl()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Bojo_Whirl, false);
    }


    public void SpawnCentaur()
    {
        if(!isHost || centaur != null)return;

        facing = movement.Facing;
        //Vector3 pos = new Vector3(2f * facing,2.7f,0);
        
        centaur = host.CreateNetworkObject("Centaur", transform.position , transform.rotation);
        centaur.transform.localScale = new Vector3(10f * facing, 10f , 1f);


        centaur.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 15,0);
        foreach (HitboxCollision hitbox in centaur.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = -attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
       }

       centaur.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());

       centaur.GetComponent<SyncAnimatorStateHost>().SetState("Centaur_" + power);

    }

    public void setCentaurPower(int pow)
    {
        if(!isHost)return;
        power = pow;
    }


    //Inhereted Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,75f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
