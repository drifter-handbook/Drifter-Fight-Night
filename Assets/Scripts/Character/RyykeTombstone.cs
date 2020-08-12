using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstone : MonoBehaviour
{
	public Vector2 velocity;

    Rigidbody2D rb;
    Animator anim;

    public bool grounded { get; set; } = false;

    // Start is called before the first frame update
    void Start()
    {

    	rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {

    }


    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Ground" && !grounded)
        {
        	grounded = true;
			anim.SetBool("Grounded",true);
		}
        
    }
}
