using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public bool isGrounded = true;
    public int numberOfJumps = 2;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey("d"))
        {
            transform.Translate(.3f,0,0);
        }
        if (Input.GetKey("a"))
        {
            transform.Translate(-.3f, 0, 0);
        }
        if (Input.GetKeyDown("w"))
        {
            if (numberOfJumps > 0)
            {
                numberOfJumps--;
                Vector3 v = GetComponent<Rigidbody2D>().velocity;
                v.y = 0.0f;
                GetComponent<Rigidbody2D>().velocity = v;

                GetComponent<Rigidbody2D>().AddForce(Vector3.up * 2000);
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
       if (other.gameObject.tag == "Ground" && GetComponent<Rigidbody2D>().velocity.y <= 0)
        {
            //UnityEngine.Debug.Log("GroundedEnter");
            isGrounded = true;
            numberOfJumps = 2;
      }
    }

    void OnCollisionExit2D(Collision2D other)
    {
       if(other.gameObject.tag == "Ground")
       {
            //UnityEngine.Debug.Log("GroundedLeave");
            isGrounded = false;
       }
    }
   // */
}
