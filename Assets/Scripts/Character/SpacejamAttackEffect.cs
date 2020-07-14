using System;
using System.Collections.Generic;
using UnityEngine;

public class SpacejamAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    playerMovement movement;
    Rigidbody2D rb;
    SpriteRenderer sr;
    NetworkEntityList Entities;
    PlayerKnockback knockback;

    public void Start()
    {
        movement = GetComponent<playerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        knockback = GetComponent<PlayerKnockback>();
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
        float range = 6f;
        Vector3 pos = movement.Facing * Vector3.right * range + 1f * Vector3.down;
        GameObject spacejamBell = Instantiate(Entities.GetEntityPrefab("SpacejamBell"), transform.position + pos, transform.rotation);
        foreach (HitboxCollision hitbox in spacejamBell.GetComponentsInChildren<HitboxCollision>())
        {
            hitbox.parent = gameObject;
            hitbox.AttackID = knockback.AttackID;
            hitbox.AttackType = knockback.AttackType;
        }
        Entities.AddEntity(spacejamBell);
        yield break;
    }

    public IEnumerator<object> Recovery()
    {
        yield return new WaitForSeconds(0.12f);
        // jump upwards
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * movement.jumpSpeed);
        yield break;
    }
}