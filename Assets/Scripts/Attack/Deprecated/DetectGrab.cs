﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectGrab : MonoBehaviour
{
    public Drifter drifter;

    public string GrabState = "";

    public GameObject victim;

    public bool playState = true;

    public bool applyVelocity = false;

    public float xVelocity = 0;

    public float yVelocity = 0;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(!GameController.Instance.IsHost)return;
        if(col.gameObject.layer == 10
         && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>() != drifter
         && !col.GetComponent<HurtboxCollision>().parent.GetComponent<PlayerStatus>().HasStatusEffect(PlayerStatusEffect.INVULN)
         && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<PlayerHurtboxHandler>().CanHit(gameObject.GetComponent<HitboxCollision>().AttackID))
        {

            victim = col.gameObject.GetComponent<HurtboxCollision>().parent;

            if(playState)
            {
                StartCoroutine(delayState(col));
            }
            if(applyVelocity)
            {
                StartCoroutine(delayVelocity(col));
            }
            
        }
    }

    IEnumerator delayState(Collider2D col)
    {
        yield return new WaitForSeconds(.0833333333f / 5f );
        //If a state is provided, and the hitbox ACUALLY hit, play the success state
        if(GrabState != "" && !col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<PlayerHurtboxHandler>().CanHit(gameObject.GetComponent<HitboxCollision>().AttackID))drifter.PlayAnimation(GrabState);
        yield break;

    }

    IEnumerator delayVelocity(Collider2D col)
    {
    	yield return new WaitForSeconds(.0833333333f / 5f );
    	Rigidbody2D rb = drifter.gameObject.GetComponent<Rigidbody2D>();
                if(!col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<PlayerHurtboxHandler>().CanHit(gameObject.GetComponent<HitboxCollision>().AttackID))
                {
                    rb.velocity = new Vector3(xVelocity == 0?rb.velocity.x:xVelocity * gameObject.GetComponent<HitboxCollision>().Facing,
                                        yVelocity == 0?rb.velocity.y:yVelocity);
                }
        yield break;

    }
}
