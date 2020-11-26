using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chadwick_Basic : MonoBehaviour
{

    protected Rigidbody2D rb;
    public Vector2 speed;
    public Drifter drifter;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
    }

    // Update is called once per frame


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
        else if(col.gameObject.name == "Hurtboxes" && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>() != drifter)
        {
            rb.velocity = rb.velocity*.4f;
        }
    }

    
    public void destroy(){
        Destroy(gameObject);
    }

    public void lunge(){
        rb.velocity = speed;
    }

}
