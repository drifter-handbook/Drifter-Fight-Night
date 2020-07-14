using System;
using System.Collections.Generic;
using UnityEngine;

public class SpacejamAttackEffect : MonoBehaviour, IPlayerAttackEffect
{
    playerMovement movement;
    Rigidbody2D rb;
    SpriteRenderer sr;
    NetworkEntityList Entities;

    public void Start()
    {
        movement = GetComponent<playerMovement>();
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponentInChildren<SpriteRenderer>();
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
        float range = 6f;
        Vector3 pos = (sr.flipX ^ movement.flipSprite ? 1f : -1f) * Vector3.right * range + 1f * Vector3.down;
        GameObject spacejamBell = Instantiate(Entities.GetEntityPrefab("SpacejamBell"), transform.position + pos, transform.rotation);
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