﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoBojoMasterHit : MasterHit
{

    GameObject centaur;
    GameObject soundwave;
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
            hitbox.Facing = facing;
       }

       wave.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
    }

    public void whirl()
    {
        if(!isHost)return;

        movement.spawnJuiceParticle(transform.position ,MovementParticleMode.Bojo_Whirl, false);
    }

    public void Neutral_Special()
    {
        if(!isHost)return;

        facing = movement.Facing;
        if(soundwave!= null) Destroy(soundwave);
        soundwave = host.CreateNetworkObject("Bojo_Note", transform.position + new Vector3(1.5f * movement.Facing, 4f), transform.rotation);
        soundwave.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in soundwave.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            
            hitbox.Facing = facing;
        }
        soundwave.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
        soundwave.GetComponent<Rigidbody2D>().velocity = new Vector3(facing * 20f,0,0);
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
}
