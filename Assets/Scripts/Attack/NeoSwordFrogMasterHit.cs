using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
    Coroutine kunaiShoot;

    bool listeningForDirection = false;

    Vector2 HeldDirection = Vector2.zero;

    int charge = 0;

    static float maxFloatTime = 1f;
    bool floating = false;
    float floatTime = maxFloatTime;

    void Update()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            drifter.sparkle.SetState("Hide");
            if(kunaiShoot != null)StopCoroutine(kunaiShoot);
            if(charge > 0)charge = 0;
        }
        // each frame, if SF is in his up special, tick down remaining time
        if(listeningForDirection && floatTime >=0)
        {
            floatTime -= Time.deltaTime;
        }

        if(movement.ledgeHanging || status.HasEnemyStunEffect())
            clearFloat();
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
            if(floatTime <=0)
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

    public new void returnToIdle()
    {
        base.returnToIdle();
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

    // public void charge_W_Neutral(int grantCharge)
    // {
    //      if(!isHost)return;
    //      if(chargeAttackPesistent("W_Neutral_Fire") !=0)return;
    //      else if(grantCharge >=1)
    //      {
    //         charge++;
    //         if(charge>=3)
    //         {
    //             Empowered = true;
    //             returnToIdle();
    //         }
    //     }
    // }


    // if(Empowered)drifter.sparkle.SetState("ChargeIndicator");
    //     else drifter.sparkle.SetState("Hide");

    // public void backdash()
    // {
    //     if(!isHost)return;
    //     facing = movement.Facing;

    //     if(drifter.input[0].MoveX == facing) rb.velocity = new Vector2(30 * facing,movement.grounded?22:rb.velocity.y+10f);
    //     else rb.velocity = new Vector2(15 * facing,movement.grounded?30:rb.velocity.y+10f);
        

    // }

    // //Causes a non-aerial move to cancle on htiing the ground
    // public void landingCancel()
    // {
    //     if(!isHost)return;
    //     movement.canLandingCancel = true;
    // }

    public void downSpecialProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Empowered = false;
        //Fire an arrow if Swordfrog has a charge
        kunaiShoot = StartCoroutine(fireKunaiDown());

    }

     //Flips the direction the charactr is facing mid move)
    public void invertDirection()
    {
        if(!isHost)return;
        movement.flipFacing();
    }

    IEnumerator fireKunaiDown()
    {

        int baseCharge = charge;
        int projnum = charge * 2;
        float radians;

        while(projnum >= 0)
        {
            yield return new WaitForSeconds(framerateScalar/7f);
            radians = (baseCharge* 110 - projnum * 15) * Mathf.PI/180f ;
            GameObject arrow = host.CreateNetworkObject("Kunai", transform.position + new Vector3((- (baseCharge - projnum) * .6f )* facing, 2.8f  - (baseCharge - projnum) * .6f, 0), Quaternion.Euler(0,0,movement.Facing * ((baseCharge - projnum) *-5f - 70f)));
            arrow.transform.localScale = new Vector3(10f * -facing, 10f, 1f);

           

            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * (-35f +  Mathf.Cos(radians) * -15), Mathf.Sin(radians) * -20 - 70f);
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
            if(projnum%2 ==0)charge--;
        }
        if(charge < 0)charge = 0;
        yield break;

    }

    // public void dance(int speed)
    // {

    //     if(!isHost)return;
    //     movement.gravityPaused = true;
    //     rb.gravityScale = 5;
    //     rb.velocity = speed * Vector3.Normalize(HeldDirection);

    // }

    // public void dendrobate()
    // {
    //     if(!isHost)return;
    //     if(HeldDirection.x == 0 && HeldDirection.y  < 0)drifter.PlayAnimation("Dendro_Down");
    //     else if(HeldDirection.x == 0 && HeldDirection.y > 0)drifter.PlayAnimation("Dendro_Up");
    // }

    // public void saveDirection()
    // {
    //     if(!isHost)return;
    //     movement.updateFacing();
    //     Vector2 TestDirection = new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
    //     HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
    // }



    public void neutralSpecialProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Empowered = false;
        charge = 2;
        //Fire an arrow if Swordfrog has a charge
        kunaiShoot = StartCoroutine(fireKunaiNeutral());

    }

    IEnumerator fireKunaiNeutral()
    {

        int baseCharge = charge;
        int projnum = charge * 2;
        float radians;

        while(projnum >= 0)
        {
            yield return new WaitForSeconds(framerateScalar/7f);
            radians = (baseCharge* 25 - projnum * 15) * Mathf.PI/180f ;
            GameObject arrow = host.CreateNetworkObject("Kunai", transform.position + new Vector3(1.5f * facing, 2.8f  + (baseCharge - projnum) * .6f, 0), Quaternion.Euler(0,0,movement.Facing * ((baseCharge - projnum) *5f + 10f)));
            arrow.transform.localScale = new Vector3(10f * facing, 10f, 1f);

           

            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * (50f +  Mathf.Cos(radians) * 15), Mathf.Sin(radians) * 20 + 10f);
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
            if(projnum%2 ==0)charge--;
        }
        if(charge < 0)charge = 0;
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


