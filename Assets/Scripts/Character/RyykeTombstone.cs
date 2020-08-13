 using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
    public HitboxCollision hitbox;
	public Vector2 velocity;
    Rigidbody2D rb;
    Animator anim;
    bool armed = false;

    public bool grounded { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {

    	rb = GetComponent<Rigidbody2D>();
    	rb.velocity=velocity;
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
   		if (!grounded)
        {
        }
            //rb.velocity = velocity;
    }

    IEnumerator Delete()
    {
        yield return new WaitForSeconds(0.8f);
        Destroy(gameObject);
        yield break;
    }

 	IEnumerator Arm()
    {
        yield return new WaitForSeconds(1.5f);
        armed = true;
        yield break;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        GameObject hitboxParent = hitbox.parent;

        if (col.gameObject.tag == "Ground" && !grounded)
        {
        	grounded = true;
			anim.SetBool("Grounded",true);
			grounded = true;
			rb.velocity=Vector2.zero;
			StartCoroutine(Arm());
		}
        else if(col.gameObject.tag != "Ground"  && col.gameObject != hitboxParent && !grounded){
        	//anim.SetBool("Grounded",true);
        	grounded = true;
        	rb.velocity=Vector2.zero;
        	anim.SetTrigger("Delete");
        	StartCoroutine(Delete());
        }
        else if(armed  && col.gameObject != hitboxParent){
        	anim.SetTrigger("Activate");
   			StartCoroutine(Delete());
        }
    }

}