using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;
    public int facing;
    PlayerStatus status;
    public Animator anim;

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

    public void dodgeRoll(){
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.4f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 30f,0f);
    }

    public void grabDash(){
        facing = movement.Facing;
        rb.velocity = new Vector2(facing * 35f,0f);
    }

    public void RecoveryThrowSpear()
    {
        // jump upwards and create spear projectile
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * 35f);
        rb.gravityScale = gravityScale;
        GameObject neroSpear = Instantiate(entities.GetEntityPrefab("NeroSpear"), transform.position, transform.rotation);
        foreach (HitboxCollision hitbox in neroSpear.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
        }
        entities.AddEntity(neroSpear);
    }
    public override void hitTheRecovery(GameObject target)
    {
        Debug.Log("Recovery hit!");
    }
    public override void cancelTheRecovery()
    {
        Debug.Log("Recovery end!");
        rb.gravityScale = gravityScale;
    }

    public void neutralWCharge(){
         rb.gravityScale = .1f;
         facing = movement.Facing;
         status.ApplyStatusEffect(PlayerStatusEffect.ARMOUR,.4f);
         rb.velocity = new Vector3(facing * 50, 0);
    }

    public void counter(){
        if(!status.HasInulvernability()){
            status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.1f);
        }
        if(status.HasHit()){
            anim.SetBool("Empowered",true);
        }

    }
    public void hitCounter(){
        anim.SetBool("Empowered",false);
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.45f);
        
    }
    public void whiffCounter(){
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,1f);
    }
    public override void cancelTheNeutralW()
    {
        rb.gravityScale = gravityScale;
    }
}
