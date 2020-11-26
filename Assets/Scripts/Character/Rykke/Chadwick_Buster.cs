using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chadwick_Buster : Chadwick_Basic
{

    public Animator anim;
    public Drifter drifter;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name == "Hurtboxes" && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>() != drifter){
            rb.velocity = Vector2.zero;

            anim.Play("Busta_Wolf");

        }
        
    }

    public void refreshHitboxes(){
        foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>())
            {
                hitbox.AttackID = hitbox.AttackID-=55;
            }
    }

}
