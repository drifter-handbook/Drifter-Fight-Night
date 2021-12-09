﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
    Coroutine kunaiShoot;

    bool listeningForDirection = false;

    Vector2 HeldDirection = Vector2.zero;

    static float maxFloatTime = 1f;
    bool floating = false;
    float floatTime = maxFloatTime;

    GameObject tongue;
    Tether tongueTether;

    public int W_Down_Projectiles = 3;

    void Update()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            drifter.sparkle.SetState("Hide");
            if(kunaiShoot != null)StopCoroutine(kunaiShoot);
        }
        // each frame, if SF is in his up special, tick down remaining time
        if(listeningForDirection && floating && floatTime >=0)
        {
            floatTime -= Time.deltaTime;
        }

        if(movement.ledgeHanging || status.HasEnemyStunEffect())
        {
            clearFloat();
            if(tongue != null)deleteTongue();
        }

    }


    new void FixedUpdate()
    {
        if(!isHost)return;

        base.FixedUpdate();

        //Handle neutral special attacks
        if(listeningForDirection && !floating)
        {
            HeldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
            if(HeldDirection != Vector2.zero) NeutralSpecialSlash();
        }
        //Handle floating movement
        else if(listeningForDirection && floating)
        {
            movement.move(11f,false);
            if(floatTime <=0 && floating)
            {
                playState("W_Up_End");
                clearFloat();
            }
        }
    }

    public void listenForDirection()
    {
        listeningForDirection = true;
    }

    public void balloonFloat()
    {
        floatTime = maxFloatTime;
        listeningForDirection = true;
        floating = true;
        listenForJumpCancel();
        setTerminalVelocity(1);
        setLandingCancel();
    }

    public void clearFloat()
    {
        floating = false;
        floatTime = maxFloatTime;
        listeningForDirection = false;
    }

    public new void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        clearFloat();
    }

    public new void returnToIdle()
    {
        base.returnToIdle();
        if(kunaiShoot != null)StopCoroutine(kunaiShoot);
        clearFloat();
    }

    public void NeutralSpecialSlash()
    {
        if(!isHost)return;

        listeningForDirection = false;
        if(HeldDirection.y <0 && movement.grounded) playState("W_Neutral_GD");
        else if(HeldDirection.y <0) playState("W_Neutral_D");
        else if(HeldDirection.y >0) playState("W_Neutral_U");
        else if(HeldDirection.x * movement.Facing <0)playState("W_Neutral_B");
        else playState("W_Neutral_S");

        HeldDirection = Vector2.zero;

    }

     //Flips the direction the charactr is facing mid move)
    public void invertDirection()
    {
        if(!isHost)return;
        movement.flipFacing();
    }




    //Grab Methods
    public void SpawnTongue()
    {
        if(!isHost)return;
        facing = movement.Facing;

        if(tongue != null)deleteTongue();

        tongue = host.CreateNetworkObject("SF_Tongue", transform.position + new Vector3(2.3f * facing,1.6f), transform.rotation);
        tongue.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in tongue.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        tongue.transform.SetParent(drifter.gameObject.transform);
        tongue.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());

        tongueTether = tongue.GetComponentInChildren<Tether>();
        tongueTether.setTargetLength(.64f);
        tongueTether.setSpeed(4f);
    }

     public void deleteTongue()
    {
        if(tongue != null)Destroy(tongue);
        tongue = null;
    }

    public void disableTongueHitbox()
    {
        tongueTether.togglehitbox(0);
    }

    public void setTongueLen(float len)
    {
        if(!isHost || tongue == null)return;
        tongueTether.setTargetLength(len);
    }

    public void freezeTether()
    {
        if(!isHost || tongue == null)return;
        tongueTether.freezeLen();
    }



    public void downSpecialProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        //Fire an arrow if Swordfrog has a charge
        kunaiShoot = StartCoroutine(fireKunaiGroundLine());

    }

    public void downSpecialProjectileAir()
    {
        if(!isHost)return;
        facing = movement.Facing;
        //Fire an arrow if Swordfrog has a charge
        kunaiShoot = StartCoroutine(fireKunaiAirLine());

    }

    IEnumerator fireKunaiGround()
    {
        int baseCharge = 2;
        int projnum = 5;
        float radians;

        while(projnum >= 0)
        {
            yield return new WaitForSeconds(framerateScalar/7f);
            radians = (baseCharge* 25 - projnum * 15) * Mathf.PI/180f ;
            GameObject arrow = host.CreateNetworkObject("Kunai", transform.position + new Vector3(1.5f * facing, 2.8f  + (baseCharge - projnum) * .6f, 0), Quaternion.Euler(0,0,movement.Facing * ((baseCharge - projnum) *5f + 10f)));
            arrow.transform.localScale = new Vector3(10f * facing, 10f, 1f);

           

            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * (50f +  Mathf.Cos(radians) * 5), Mathf.Sin(radians) * 5 + 5f);
            foreach (HitboxCollision hitbox in arrow.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            projnum--;

            refreshHitboxID();
        }
        yield break;

    }


    IEnumerator fireKunaiGroundLine()
    {
        int projnum = W_Down_Projectiles;

        Vector3 size = new Vector3(10f * facing, 10f, 1f);
        Vector3 pos = new Vector3(.2f * facing, 2.7f, 1f);

        while(projnum > 0)
        {
            yield return new WaitForSeconds(framerateScalar/3f);

            GameObject arrowA = host.CreateNetworkObject("Kunai", transform.position + new Vector3(1.5f * facing, 1.5f + W_Down_Projectiles/5f + (W_Down_Projectiles - projnum) * .6f, 0), transform.rotation);

            arrowA.transform.localScale = size;
            arrowA.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + 50f * facing, 0);

            foreach (HitboxCollision hitbox in arrowA.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            projnum--;

            refreshHitboxID();

        }

    }


    IEnumerator fireKunaiAirLine()
    {
        int projnum = W_Down_Projectiles;

        Vector3 size = new Vector3(10f, 10f, 1f);
        Vector3 pos = new Vector3(.2f * facing, 2.7f, 1f);

        while(projnum > 0)
        {

            float degreesA = facing >0 ? (335f  - projnum * 4f) : (215f  + projnum * 4f);
            float radiansA = degreesA * Mathf.PI/180f;
            float posDegrees = (facing >0 ? 335f  : 215f);
            float posRadians = posDegrees * Mathf.PI/180f;

            yield return new WaitForSeconds(framerateScalar/3f);

            GameObject arrowA = host.CreateNetworkObject("Kunai", transform.position + new Vector3(facing * (-.5f + projnum/2f), projnum/2f -.9f)
                                                                 + pos, 
                                                                 Quaternion.Euler(0,0,posDegrees));


            arrowA.transform.localScale = size;
            arrowA.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + (Mathf.Cos(posRadians) *50f), Mathf.Sin(posRadians)*50f);

            foreach (HitboxCollision hitbox in arrowA.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            projnum--;

            refreshHitboxID();

        }


    }

    //Creates two arcs of projectiles for the air version of down special
    IEnumerator fireKunaiAir()
    {
        //Upper arc
        float degreesA;
        float radiansA;
        //lower arc
        float degreesB;
        float radiansB;

        int projnum = 3;
        Vector3 size = new Vector3(10f, 10f, 1f);
        Vector3 pos = new Vector3(.2f * facing, 2.7f, 1f);

        while(projnum > 0)
        {
            yield return new WaitForSeconds(framerateScalar/7f);
            degreesA = facing >0 ? (330f  - projnum * 5) : (210f  + projnum * 5f);
            degreesB = facing >0 ? (300f  + projnum * 5) : (240f  - projnum * 5f);
            radiansA = degreesA * Mathf.PI/180f;
            radiansB = degreesB* Mathf.PI/180f;


            GameObject arrowA = host.CreateNetworkObject("Kunai", transform.position
                                                                 + new Vector3((Mathf.Cos(radiansA)), Mathf.Sin(radiansA))
                                                                 + pos, 
                                                                 Quaternion.Euler(0,0,degreesA));
            arrowA.transform.localScale = size;
            arrowA.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + (Mathf.Cos(radiansA) *50f), Mathf.Sin(radiansA)*50f);
            
            foreach (HitboxCollision hitbox in arrowA.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            // if(projnum !=1)
            // {
                GameObject arrowB = host.CreateNetworkObject("Kunai", transform.position
                                                                 + new Vector3((Mathf.Cos(radiansB)), Mathf.Sin(radiansB))
                                                                 + pos,
                                                                 Quaternion.Euler(0,0,degreesB));
                arrowB.transform.localScale = size;
                arrowB.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + (Mathf.Cos(radiansB) *50f), Mathf.Sin(radiansB)*50f);
                foreach (HitboxCollision hitbox in arrowB.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = drifter.gameObject;
                    hitbox.AttackID = attacks.AttackID + 100;
                    hitbox.AttackType = attacks.AttackType;
                    hitbox.Active = true;
                    hitbox.Facing = facing;
                }
            // }

            projnum--;

            refreshHitboxID();
        }
        yield break;

    }

    //Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.2f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0f,35f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        rb.velocity = new Vector2(facing * 30f,5f);
    }
}


