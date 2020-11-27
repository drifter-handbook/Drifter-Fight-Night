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

      if(col.gameObject.name == "Hurtboxes" && col.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>() != drifter)
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
