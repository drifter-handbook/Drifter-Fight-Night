using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RykkeMasterHit : MasterHit
{
    public TetherRange playerRange;
    public TetherRange ledgeRange;
    GameObject activeStone;
    public AudioSource audioSource;
    public AudioClip[] audioClips;

    bool tethering = false;
    bool tetheredPlayer = false;
    Vector2 tetherTarget = Vector3.zero;

    int recoveryReset =2;

    void Update()
    {

        if(!isHost)return;
        if(status.HasEnemyStunEffect() && tethering)tethering = false;

        if(((Vector2.Distance(tetherTarget,rb.position) < 4.5f && tetheredPlayer) || movement.ledgeHanging)){
            cancelTethering();
        }

        if(movement.grounded){
            recoveryReset = 2;
        }

        if(tethering){
            rb.position =  Vector3.Lerp(rb.position,tetherTarget,.15f);
        }

        if(drifter.Charge <=0)Empowered = false;
    }


    //Tether Recovery Logic

    public void enableTetherBox()
    {
        playerRange.gameObject.transform.parent.gameObject.SetActive(true);
        
    }

    public void selectTetherTarget()
    {
        if(!isHost)return;
        facing = movement.Facing;
        GameObject arms = host.CreateNetworkObject("LongArmOfTheLaw", transform.position + new Vector3(facing * 2, 5, 0), transform.rotation);
        foreach (HitboxCollision hitbox in arms.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
        }
        float length = 15f;

        if (ledgeRange.TetherPoint != Vector3.zero)
        {
            arms.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(arms.transform.position.x - ledgeRange.TetherPoint.x, -arms.transform.position.y + ledgeRange.TetherPoint.y) * 180 / Mathf.PI));
            length = Vector2.Distance(ledgeRange.TetherPoint, arms.transform.position);
            tetherTarget = ledgeRange.TetherPoint;
            tetheredPlayer = false;
        }
        else if (playerRange.TetherPoint != Vector3.zero)
        {
            arms.transform.rotation = Quaternion.Euler(0, 0, (Mathf.Atan2(arms.transform.position.x - (playerRange.TetherPoint.x + playerRange.enemyVelocity.x * .15f), -arms.transform.position.y + (playerRange.TetherPoint.y + playerRange.enemyVelocity.y * .15f)) * 180 / Mathf.PI));
            length = Vector2.Distance(playerRange.TetherPoint + playerRange.enemyVelocity * .1f, arms.transform.position);
                tetherTarget = playerRange.TetherPoint + (playerRange.enemyVelocity *.1f);
                tetheredPlayer = true;
            }

            else
            {
                arms.transform.rotation = Quaternion.Euler(0, 0, 45 * -facing);
                tetherTarget = Vector2.zero;
                tetheredPlayer = false;
            }
            arms.transform.localScale = new Vector3(13, length / 1.3f, 1);
        }

        public void moveToTetherTarget()
        {
            if(!isHost)return;
            facing = movement.Facing;
            
            if(tetherTarget != Vector2.zero)
            {
                tethering = true;
            }

            else if (recoveryReset > 0){
                attacks.currentRecoveries = 1;
                recoveryReset--;
            }
            playerRange.gameObject.transform.parent.gameObject.SetActive(false);
            rb.gravityScale = gravityScale;
            movement.gravityPaused = false;
        //tetheredPlayer = false;
        }

        public void cancelTethering()
        {
            if(!isHost)return;
            if(tethering){
                tethering = false;
                if(tetheredPlayer){
                    tetherTarget = Vector2.zero;
                    drifter.PlayAnimation("W_Up_Attack");
                    rb.velocity = new Vector3(facing*35,30,0);
                }
                else if (!movement.ledgeHanging){
                    rb.position =  Vector3.Lerp(rb.position,tetherTarget,.55f);
                    tetherTarget = Vector2.zero;
                }
                
            }
            
        }

    //Side Grab "Projectile"

        public void sideGrab()
        {
            if(!isHost)return;
            facing = movement.Facing;
            Vector3 flip = new Vector3(facing *8f,8f,1f);
            Vector3 loc = new Vector3(facing *4f,0f,0f);
            
            GameObject HoldPerson = host.CreateNetworkObject("HoldPerson", transform.position + loc, transform.rotation);
            HoldPerson.transform.localScale = flip;
            foreach (HitboxCollision hitbox in HoldPerson.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            HoldPerson.GetComponentInChildren<DetectGrab>().drifter = drifter;
            HoldPerson.GetComponentInChildren<DetectGrab>().GrabState = Empowered?"Grab_Empowered":"";
        }


    public void sideWEmpowered()
        {
            if(!isHost)return;
            conmsumeStack();
            facing = movement.Facing;
            Vector3 flip = new Vector3(facing *10f,10f,1f);
            Vector3 loc = new Vector3(facing *-1f,0f,0f);
            
            GameObject ChadPunch = host.CreateNetworkObject("ChadwickPunch", transform.position + loc, transform.rotation);
            ChadPunch.transform.localScale = flip;
            foreach (HitboxCollision hitbox in ChadPunch.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            ChadPunch.GetComponentInChildren<Chadwick_Basic>().speed = new Vector2(facing * 65f,0);
            ChadPunch.GetComponentInChildren<Chadwick_Basic>().drifter = drifter;
        }

    public void Buster_Wolf()
        {
            if(!isHost || !Empowered)return;
            conmsumeStack();
            facing = movement.Facing;
            Vector3 flip = new Vector3(facing *10f,10f,1f);
            Vector3 loc = new Vector3(facing *-1f,0f,0f);
            
            GameObject ChadPunch = host.CreateNetworkObject("Chadwick_Buster", transform.position + loc, transform.rotation);
            ChadPunch.transform.localScale = flip;
            foreach (HitboxCollision hitbox in ChadPunch.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }
            ChadPunch.GetComponentInChildren<Chadwick_Buster>().speed = new Vector2(facing * 65f,0);
            ChadPunch.GetComponentInChildren<Chadwick_Buster>().drifter = drifter;
        }

    //Down W
        public void plantGravestone()
        {
            if(!isHost)return;
            if(activeStone){
                activeStone.GetComponent<RyykeTombstone>().Break();
            }  
            facing = movement.Facing;
            if(!movement.grounded)rb.velocity = new Vector2(rb.velocity.x *.5f,10);
            Vector3 flip = new Vector3(facing *8f,8f,1f);
            Vector3 loc = new Vector3(facing *1f,.8f,0f);

            GameObject tombstone = host.CreateNetworkObject("RyykeTombstone", transform.position + loc, transform.rotation);
            tombstone.transform.localScale = flip;
            foreach (HitboxCollision hitbox in tombstone.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            tombstone.GetComponent<RyykeTombstone>().facing = facing;
            activeStone = tombstone;
        }

    //Neutral W
        public void awaken(){
            if(!isHost)return;
            facing = movement.Facing;
            Vector3 flip = new Vector3(facing *8f,8f,1f);
            Vector3 loc = new Vector3(facing *3.5f,0f,0f);
            
            GameObject tombstone = host.CreateNetworkObject("RyykeTombstone", transform.position + loc, transform.rotation);
            tombstone.transform.localScale = flip;
            foreach (HitboxCollision hitbox in tombstone.GetComponentsInChildren<HitboxCollision>(true))
            {
                hitbox.parent = drifter.gameObject;
                hitbox.AttackID = attacks.AttackID;
                hitbox.AttackType = attacks.AttackType;
                hitbox.Active = true;
                hitbox.Facing = facing;
            }

            tombstone.GetComponent<RyykeTombstone>().facing = facing;
            tombstone.GetComponent<RyykeTombstone>().awakenActivate();
            

        }

        public void grantStack()
        {
            if(!isHost)return;
            if(drifter.Charge < 3){
                audioSource.PlayOneShot(audioClips[0]);
                drifter.Charge++;
                Empowered = true;
                drifter.GuardStateName = "Guard_Strong";
                drifter.BlockReduction = .75f;
            }

        }

        public void conmsumeStack()
        {
            if(!isHost)return;
            if(drifter.Charge > 0){
              drifter.Charge--;
              if(drifter.Charge == 0){
    			Empowered = false;
                drifter.GuardStateName = "Guard";
                drifter.BlockReduction = .25f;
            }
        }
    }



    //Inhereted Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        applyEndLag(1);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }

    public override void rollGetupStart()
    {
        if(!isHost)return;
        applyEndLag(1);
        rb.velocity = new Vector3(0,78f,0);
    }

    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 45f,5f);
    }
}
