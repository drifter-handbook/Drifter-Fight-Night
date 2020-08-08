using System;
using System.Collections.Generic;
using UnityEngine;

public class LadyParhelionAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    PlayerMovement movement;
    Rigidbody2D rb;
    float gravityScale;

    NetworkEntityList Entities;
    PlayerHurtboxHandler attacks;

    public void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        attacks = GetComponent<PlayerHurtboxHandler>();
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
        // move diagonally
        rb.gravityScale = 0f;
        rb.velocity = (Vector2.right * movement.Facing * 3f + Vector2.up).normalized * 75f;
        yield return new WaitForSeconds(0.6f);
        // jump upwards and create spear projectile
        rb.velocity = Vector2.zero;
        rb.gravityScale = gravityScale;
        yield break;
    }

    public void Reset()
    {
        rb.gravityScale = gravityScale;
    }
}