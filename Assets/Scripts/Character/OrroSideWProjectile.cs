using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrroSideWProjectile : MonoBehaviour
{
    // Start is called before the first frame update
    public float duration;
    public int facing;
    public Animator anim;
    public Rigidbody2D rb;
    public bool empowered = false;
    void Start()
    {

        StartCoroutine(Empower());
        StartCoroutine(delete(duration));
        
    }
    void Update(){
        if(empowered)
        {
            anim.SetTrigger("Empower");
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
         if(col.gameObject.name == "Reflector"){
            rb.velocity =  rb.velocity * -2.5f;
            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.GetComponentInChildren<HitboxCollision>().parent;
                    //Mkae this not suck later
                    hitbox.AttackID = 10000;
                }
        }
        else if(col.gameObject.name == "Hurtboxes" && !empowered)
        {
            StartCoroutine(delete(.1f));
        }
    }

    IEnumerator Empower(){
        yield return new WaitForSeconds(duration * .30f);
        anim.SetTrigger("Empower"); 
        empowered = true;
        rb.velocity += new Vector2(facing *15f,0f);

        yield break;
    }

    IEnumerator delete(float time){
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
        yield break;
    }
}
