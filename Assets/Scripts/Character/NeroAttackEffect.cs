using System;
using System.Collections.Generic;
using UnityEngine;

public class NeroAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    playerMovement movement;
    Rigidbody2D rb;
    float gravityScale;

    NetworkEntityList Entities;

    public void Start()
    {
        movement = GetComponent<playerMovement>();
        rb = GetComponent<Rigidbody2D>();
        gravityScale = rb.gravityScale;
        Entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();
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
        Entities.AddEntity(neroSpear);
        yield break;
    }
}