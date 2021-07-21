using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
    Coroutine kunaiShoot;

    Vector2 HeldDirection;

    int charge = 0;

    void Update()
    {
        if(!isHost)return;
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            if(kunaiShoot != null)StopCoroutine(kunaiShoot);
            if(charge > 0)charge = 0;
        }
    }

    public void charge_W_Neutral(int grantCharge)
    {
         if(!isHost)return;
         if(chargeAttackPesistent("W_Neutral_Fire") !=0)return;
         else if(grantCharge >=1)
         {
            charge++;
            if(charge>=3)
            {
                Empowered = true;
                returnToIdle();
            }
        }
    }

    public void backdash()
    {
        if(!isHost)return;
        facing = movement.Facing;

        if(drifter.input[0].MoveX == facing) rb.velocity = new Vector2(30 * facing,movement.grounded?22:rb.velocity.y+10f);
        else rb.velocity = new Vector2(15 * facing,movement.grounded?30:rb.velocity.y+10f);
        

    }

    //Causes a non-aerial move to cancle on htiing the ground
    public void landingCancel()
    {
        if(!isHost)return;
        movement.canLandingCancel = true;
    }

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
            radians = (baseCharge* 25 - projnum * 15) * Mathf.PI/180f ;
            GameObject arrow = host.CreateNetworkObject("Arrow", transform.position + new Vector3((- (baseCharge - projnum) * .6f )* facing, 2.8f  - (baseCharge - projnum) * .6f, 0), Quaternion.Euler(0,0,movement.Facing * ((baseCharge - projnum) *-5f - 70f)));
            arrow.transform.localScale = new Vector3(10f * facing, 10f, 1f);

           

            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * (35f +  Mathf.Cos(radians) * 15), Mathf.Sin(radians) * -20 - 70f);
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

    public void dance(int speed)
    {

        if(!isHost)return;
        movement.gravityPaused = true;
        rb.gravityScale = 5;
        rb.velocity = speed * Vector3.Normalize(HeldDirection);

    }

    public void dendrobate()
    {
        if(!isHost)return;
        if(HeldDirection.x == 0 && HeldDirection.y  < 0)drifter.PlayAnimation("Dendro_Down");
        else if(HeldDirection.x == 0 && HeldDirection.y > 0)drifter.PlayAnimation("Dendro_Up");
    }

    public void saveDirection()
    {
        if(!isHost)return;
        movement.updateFacing();
        Vector2 TestDirection = new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
        HeldDirection = TestDirection == Vector2.zero? HeldDirection: TestDirection;
    }



    public void neutralSpecialProjectile()
    {
        if(!isHost)return;
        facing = movement.Facing;
        Empowered = false;
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
            GameObject arrow = host.CreateNetworkObject("Arrow", transform.position + new Vector3(2.4f * facing, 2.8f  + (baseCharge - projnum) * .6f, 0), Quaternion.Euler(0,0,movement.Facing * ((baseCharge - projnum) *5f + 10f)));
            arrow.transform.localScale = new Vector3(10f * facing, 10f, 1f);

           

            arrow.GetComponent<Rigidbody2D>().velocity = new Vector2(rb.velocity.x + facing * (70f +  Mathf.Cos(radians) * 15), Mathf.Sin(radians) * 20 + 10f);
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


