using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public bool isGrounded = false;
    public bool isWalking = false;
    public int numberOfJumps = 2;

    public SpriteRenderer sprite;

    private Vector3 origTransform;
    private Vector3 flippedTransform;
    public float jumpTimeCounter;
    public float jumpTime;

    public Animator animator;
    // Start is called before the first frame update
    void Start()
    {
       //Nothing... for now
    }

    // Update is called once per frame
    void Update()
    {
        if (animator.GetBool("Grounded") != isGrounded)
        {
            animator.SetBool("Grounded", isGrounded);
        }

        if (animator.GetBool("Walking") != isWalking)
        {
            animator.SetBool("Walking", isWalking);
        }


        if (Input.GetKey("d"))
        {
            sprite.flipX = true;
        } else if (Input.GetKey("a")){
            sprite.flipX = true;
        }


        if (Input.GetKeyDown("g"))
        {
            animator.SetTrigger("Grab");
        }
        else if (Input.GetKey("d"))
        {
            if (!isWalking)
            {
                animator.SetTrigger("Walk");
            }
            isWalking = true;
            transform.Translate(.6f, 0, 0);
        }
       else if (Input.GetKey("a"))
        {
            sprite.flipX = false;
                if (!isWalking)
                {
                    animator.SetTrigger("Walk");
                }
                isWalking = true;
                transform.Translate(-.6f, 0, 0);

        } else {
            isWalking = false;
        }


        if (Input.GetKeyDown("q"))
        {
            //attack  //neutral aeriel
            if (isGrounded)
            {
                animator.SetTrigger("Attack");
            } else
            {
                animator.SetTrigger("Aeriel");
            }
        }

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            //shift is guard
            if (!animator.GetBool("Guarding"))
            {
                animator.SetTrigger("Guard");
                animator.SetBool("Guarding", true);
            }

        } else
        {
            animator.SetBool("Guarding", false);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (Input.GetKey("w"))
            {
                // +up, recovery
                animator.SetTrigger("Recovery");
            }


            //jump
            if (numberOfJumps > 0)
            {
                jumpTimeCounter = jumpTime;
                animator.SetTrigger("Jump");
                StartCoroutine(DelayedJump());
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump\
            }
        }
        if (Input.GetKey(KeyCode.Space)){
          if(jumpTimeCounter>0){
            GetComponent<Rigidbody2D>().AddForce(Vector3.up * 80);
            jumpTimeCounter -= Time.deltaTime;
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

    private IEnumerator DelayedJump()
    {
        float duration = 0.05f; // 3 seconds you can change this
                             //to whatever you want
        float normalizedTime = 0;
        while (normalizedTime <= 1f)
        {
            normalizedTime += Time.deltaTime / duration;
            yield return null;
        }
        numberOfJumps--;
        Vector3 v = GetComponent<Rigidbody2D>().velocity;
        v.y = 0.0f;
        GetComponent<Rigidbody2D>().velocity = v;
        GetComponent<Rigidbody2D>().AddForce(Vector3.up * 1500);
    }
}
