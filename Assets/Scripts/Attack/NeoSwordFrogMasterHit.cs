using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeoSwordFrogMasterHit : MasterHit
{
    Coroutine kunaiShoot;

    GameObject tongue;
    Tether tongueTether;

    public int W_Down_Projectiles = 3;

    bool listeningForDirection = false;
    int delaytime = 0;

    Vector2 HeldDirection = Vector2.zero;    

    override protected void UpdateMasterHit()
    {
        base.UpdateMasterHit();

        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
            Empowered = false;
            drifter.sparkle.SetState("Hide");
            if(kunaiShoot != null)StopCoroutine(kunaiShoot);
        }

        if(movement.ledgeHanging || status.HasEnemyStunEffect())
        {
            if(tongue != null)deleteTongue();
            listeningForDirection = false;
        }

        //Handle neutral special attacks
        if(listeningForDirection)
        {
            if(!drifter.input[0].Special) delaytime++;
            HeldDirection += new Vector2(drifter.input[0].MoveX,drifter.input[0].MoveY);
            if(HeldDirection != Vector2.zero || delaytime > 5) NeutralSpecialSlash();
        }

    }

    public void listenForDirection()
    {
        listeningForDirection = true;
        delaytime = 0;
    }

    public new void clearMasterhitVars()
    {
        base.clearMasterhitVars();
        listeningForDirection = false;
        deleteTongue();
    }

    public new void returnToIdle()
    {
        base.returnToIdle();
        if(kunaiShoot != null)StopCoroutine(kunaiShoot);
        deleteTongue();
    }

    public void NeutralSpecialSlash()
    {
        if(!isHost)return;

        listeningForDirection = false;
        movement.updateFacing();
        facing = movement.Facing;
        if(HeldDirection.y <0 && movement.grounded) playState("W_Neutral_GD");
        else if(HeldDirection.y <0) playState("W_Neutral_D");
        else if(HeldDirection.y >0) playState("W_Neutral_U");
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
                
                hitbox.Facing = facing;
            }

            projnum--;

            refreshHitboxID();

        }


    }

}


