using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class LucillePortalBounce : MonoBehaviour
{

    public GameObject sprite;

    void OnCollisionEnter2D(Collision2D collider)
    {
        if(!GameController.Instance.IsHost)return;

        try
        {
            if(collider.gameObject.tag == "Ground")
            {

                GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.DarkRestitution,collider.contacts[0].point,0,Vector3.one);
                //Play bouncing states here

            }
        }
        catch(NullReferenceException)
        {
            return;
        }
    }    
}
