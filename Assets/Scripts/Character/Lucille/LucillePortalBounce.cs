using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LucillePortalBounce : MonoBehaviour
{

    public GameObject sprite;

    Rigidbody2D rb;

    void Start()
    {
        if(!GameController.Instance.IsHost)return;
        rb = gameObject.transform.parent.GetComponent<Rigidbody2D>();
    }

    void OnCollisionEnter2D(Collision2D collider)
    {
        if(!GameController.Instance.IsHost)return;

        try
        {
            if(collider.gameObject.tag == "Ground") GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.DarkRestitution,collider.contacts[0].point,((rb.velocity.x < 0)?1:-1 ) * Vector3.Angle(Vector3.up,collider.contacts[0].normal),Vector3.one);
        }
        catch(NullReferenceException)
        {
            return;
        }
    }

     void OnTriggerExit2D(Collider2D collider)
     {
        if(!GameController.Instance.IsHost)return;
        if(collider.gameObject.tag == "Killzone")sprite.GetComponent<SyncAnimatorStateHost>().SetState("SoftDelete");

     }    
}
