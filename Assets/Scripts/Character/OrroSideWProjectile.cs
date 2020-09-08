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
            rb.velocity =  rb.velocity * -1.5f;

            gameObject.transform.localScale = new Vector3(gameObject.transform.localScale.x * -1,gameObject.transform.localScale.y,gameObject.transform.localScale.z);

            foreach (HitboxCollision hitbox in gameObject.GetComponentsInChildren<HitboxCollision>(true))
                {
                    hitbox.parent = col.gameObject.transform.parent.GetComponentInChildren<HitboxCollision>().parent;
                    //Mkae this not suck laters
                    hitbox.AttackID = 300 + Random.Range(0,25);
                }

        }
        else if(col.gameObject.name == "Hurtboxes" && !empowered && col.GetComponent<HurtboxCollision>() != this.gameObject.GetComponentInChildren<HitboxCollision>().parent.GetComponentInChildren<HurtboxCollision>())
        {
            StartCoroutine(delete(.1f));
        }
    }

    IEnumerator Empower(){
        yield return new WaitForSeconds(duration * .15f);
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
