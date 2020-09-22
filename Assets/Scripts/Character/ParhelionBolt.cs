using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParhelionBolt : MonoBehaviour
{

    public bool destroy = false;
    public float speed = -55;
    // Start is called before the first frame update
    public float duration;
    Rigidbody2D rb;
    Animator anim;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(decay());
        anim = GetComponent<Animator>();
        rb.velocity = new Vector2(0,speed);
    }

    IEnumerator decay(){
        yield return new WaitForSeconds(duration);
        Destroy(gameObject);
    }

    IEnumerator hitGround(){
        destroy = true;
        anim.SetTrigger("Break");
        yield return new WaitForSeconds(.2f);
         Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if(destroy){
            anim.SetTrigger("Break");
        }
    }

     void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ground" || col.gameObject.tag == "Platform")
        {
            rb.velocity = Vector2.zero;
           StartCoroutine(hitGround());
        }
    }
}
