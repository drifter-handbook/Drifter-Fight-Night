using System;
using System.Collections.Generic;
using UnityEngine;

public class NeroAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    PlayerMovement movement;
    Rigidbody2D rb;
    float gravityScale;

    NetworkEntityList Entities;
    PlayerAttacking attacks;

    public void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        attacks = GetComponent<PlayerAttacking>();
    }

    public IEnumerator<object> Aerial()
    {
        yield break;
    }

    public IEnumerator<object> Grab()
    {
        yield break;
    }

    public IEnumerator<object> Light()
    {
        yield break;
    }

    public IEnumerator<object> Recovery()
    {
        yield return new WaitForSeconds(0.12f);
        // pause in air for half a second
        rb.gravityScale = 0f;
        rb.velocity = Vector2.zero;
        yield return new WaitForSeconds(0.12f);
        // jump upwards and create spear projectile
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * movement.jumpSpeed);
        rb.gravityScale = gravityScale;
        GameObject neroSpear = Instantiate(Entities.GetEntityPrefab("NeroSpear"), transform.position, transform.rotation);
        foreach (HitboxCollision hitbox in neroSpear.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
        }
        Entities.AddEntity(neroSpear);
        yield break;
    }

    public void Reset()
    {
        rb.gravityScale = gravityScale;
    }
}
