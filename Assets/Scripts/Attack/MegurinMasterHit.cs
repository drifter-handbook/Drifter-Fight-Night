using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;

    public int facing;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
    }

    public override void callTheRecovery()
    {
        Debug.Log("Recovery start!");
    }
    public void RecoveryPauseMidair()
    {
        // pause in air
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
    }
    public void RecoveryWarp()
    {
         rb.position += new Vector2(0,20);
    }

    public void resetGravity(){
        rb.gravityScale = gravityScale;
    }

    public void spawnStorm(){

        Vector3 pos = new Vector3(0f,8f,0f);
        GameObject MegurinStorm = Instantiate(entities.GetEntityPrefab("MegurinStorm"), transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in MegurinStorm.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(MegurinStorm);
    }

    public void spawnOrb(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *4f,5,1f);
        GameObject MegurinOrb = Instantiate(entities.GetEntityPrefab("ChromaticOrb"), transform.position + pos, transform.rotation);
        MegurinOrb.transform.localScale = flip;
        MegurinOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing *10, 0);
        MegurinOrb.GetComponent<Animator>().SetInteger("Mode",drifter.Charge);
        foreach (HitboxCollision hitbox in MegurinOrb.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(MegurinOrb);
    }

    public void spawnSmallBolt(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *1f,1f,1f);
        GameObject smallBolt = Instantiate(entities.GetEntityPrefab("StrongBolt"), transform.position, transform.rotation);
        smallBolt.transform.localScale += flip;
        foreach (HitboxCollision hitbox in smallBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(smallBolt);
    }
    public void spawnLargeBolt(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *1f,1f,1f);
        GameObject largeBolt = Instantiate(entities.GetEntityPrefab("WeakBolt"), transform.position, transform.rotation);
        largeBolt.transform.localScale += flip;
        foreach (HitboxCollision hitbox in largeBolt.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(largeBolt);
    }

    public void setLightning(){
        drifter.Charge = -1;
    }
    public void setIce(){
        drifter.Charge = 1;
    }
    public void setWind(){
        drifter.Charge = 0;
    }



    public override void cancelTheNeutralW()
    {
        rb.gravityScale = gravityScale;
    }
}
