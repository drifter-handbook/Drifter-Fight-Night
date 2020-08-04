using System;
using System.Collections.Generic;
using UnityEngine;

public class RyykeAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    PlayerMovement movement;
    Rigidbody2D rb;
    SpriteRenderer sr;
    NetworkEntityList Entities;
    PlayerAttacking attacks;

    public void Start()
    {
        movement = GetComponent<PlayerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
        attacks = GetComponent<PlayerAttacking>();
    }

    public IEnumerator<object> Aerial()
    {
        float t = 0;
        while (t < 0.5f)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0.9f * rb.velocity.y);
            t += Time.deltaTime;
            yield return null;
        }
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
        // jump upwards
        rb.velocity = new Vector2(rb.velocity.x, 1.5f * movement.jumpSpeed);
        yield break;
    }

    public void Reset()
    {
    }
}