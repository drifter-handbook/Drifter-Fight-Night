using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeroMasterHit : MasterHit
{
    Rigidbody2D rb;
    PlayerAttacks attacks;
    float gravityScale;
    PlayerMovement movement;

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
    public void RecoveryThrowSpear()
    {
        // jump upwards and create spear projectile
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * movement.jumpSpeed);
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
}
