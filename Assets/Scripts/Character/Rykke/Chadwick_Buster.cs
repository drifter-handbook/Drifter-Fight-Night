using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chadwick_Buster : Chadwick_Basic
{

    public Animator anim;
    bool reflected = false;

    void OnTriggerEnter2D(Collider2D col)
    {
        if(col.gameObject.name == "Reflector" && !reflected)
        {
                reflected = true;
                rb.velocity =  rb.velocity * -1.5f;

                gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,gameObject.transform.localScale.y,gameObject.transform.localScale.z);

                drifter = col.gameObject.transform.parent.GetComponentInChildren<HitboxCollision>().parent.GetComponent<Drifter>();

                foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.transform.parent.GetComponentInChildren<HitboxCollision>().parent;
                    //Mkae this not suck laters
                    hitbox.Facing*=-1;
                    hitbox.AttackID = 300 + Random.Range(0,25);
                }
        }

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
