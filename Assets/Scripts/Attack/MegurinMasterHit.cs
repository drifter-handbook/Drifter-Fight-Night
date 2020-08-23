using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MegurinMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    PlayerStatus status;
    float gravityScale;
    PlayerMovement movement;
    public Animator anim;
    public SpriteRenderer sprite;
    GameObject activeStorm;
    Vector2 HeldDirection;

    int neutralWCharge = 0;

    public int facing;

    void Start()
    {
        rb = drifter.GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        attacks = drifter.GetComponent<PlayerAttacks>();
        movement = drifter.GetComponent<PlayerMovement>();
        status = drifter.GetComponent<PlayerStatus>();
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
    public void saveDirection(){
        Vector2 TestDirection = new Vector2(drifter.input.MoveX,drifter.input.MoveY);
        HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
    }
    public void RecoveryWarp()
    {
        saveDirection();
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.25f);

        HeldDirection.Normalize();

        rb.position += HeldDirection*20f;

        HeldDirection = Vector2.zero;

    }

    public void Nair(){
        attacks.SetMultiHitAttackID();
    }

    public void resetGravity(){
        rb.gravityScale = gravityScale;
    }

    public void dodgeRoll(){
        
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.7f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
    }

    public void warpRoll(){
        facing = movement.Facing;
        rb.position += new Vector2(6f* facing,0f);
    }

    public void spawnStorm(){

        Vector3 pos = new Vector3(0f,6.5f,0f);
        GameObject MegurinStorm = Instantiate(entities.GetEntityPrefab("MegurinStorm"), transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in MegurinStorm.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }

        if(activeStorm){
            StartCoroutine(activeStorm.GetComponent<MegurinStorm>().Fade(0));
        }
        MegurinStorm.GetComponent<MegurinStorm>().attacks = attacks;
        activeStorm = MegurinStorm;

        entities.AddEntity(MegurinStorm);
    }

    public void spawnOrb(){

        facing = movement.Facing;
        Vector3 flip = new Vector3(facing *12f,12f,0f);
        Vector3 pos = new Vector3(facing *4f,5,1f);
        GameObject MegurinOrb = Instantiate(entities.GetEntityPrefab("ChromaticOrb"), transform.position + pos, transform.rotation);
        MegurinOrb.transform.localScale = flip;
        MegurinOrb.GetComponent<Rigidbody2D>().velocity = new Vector2(facing *25, 0);
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
        Vector3 flip = new Vector3(facing *10f,10f,1f);
        Vector3 pos = new Vector3(facing *3f,4,1f);
        GameObject smallBolt = Instantiate(entities.GetEntityPrefab("WeakBolt"), transform.position + pos, transform.rotation);
        smallBolt.transform.localScale = flip;
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
        Vector3 flip = new Vector3(facing *10f,10f,1f);
        Vector3 pos = new Vector3(facing *3f,4,1f);
        GameObject largeBolt = Instantiate(entities.GetEntityPrefab("StrongBolt"), transform.position + pos, transform.rotation);
        largeBolt.transform.localScale = flip;
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


    public void chargeNeutralW(){
        if(neutralWCharge < 10){
            neutralWCharge +=1;
        }
        else{
            sprite.color = new Color(0,60,255);
            anim.SetBool("Empowered",true);
        }
    }

    public void beginLightningbolt(){
        sprite.color = Color.white;
        if(anim.GetBool("Empowered") == true){
            drifter.SetAnimatorBool("Empowered",false);
            drifter.SetAnimatorBool("HasCharge",true);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,1.8f);
        }
        else{
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,1f);
        }
          
    }
    public void fireLightningbolt(){
        neutralWCharge = 0;
        if(anim.GetBool("HasCharge") == true){
            spawnLargeBolt();
        }
        else{
            spawnSmallBolt();
        }
          
    }
    public void removeBoltStored(){
         drifter.SetAnimatorBool("HasCharge",false);
    }


    public override void cancelTheNeutralW()
    {
        rb.gravityScale = gravityScale;
    }
}
