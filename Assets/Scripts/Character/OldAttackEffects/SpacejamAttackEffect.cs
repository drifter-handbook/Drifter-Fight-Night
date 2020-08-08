using System;
using System.Collections.Generic;
using UnityEngine;

public class SpacejamAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    PlayerMovement movement;
    Rigidbody2D rb;
    SpriteRenderer sr;
    NetworkEntityList Entities;
    PlayerAttacks attacks;

    public void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        attacks = GetComponent<PlayerAttacks>();
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
        foreach (HitboxCollision hitbox in spacejamBell.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
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

    public void Reset()
    {
    }
}