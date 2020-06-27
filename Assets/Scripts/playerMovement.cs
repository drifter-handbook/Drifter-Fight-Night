using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public int numberOfJumps = 2;
    public float delayedJumpDuration = 0.05f; // 3 seconds you can change this to whatever you want
    public float walkSpeed = 0.1f;

    public CustomControls keyBindings;
    public SpriteRenderer sprite;

    private Vector3 origTransform;
    private Vector3 flippedTransform;

    public PlayerInputData input { get; set; } = new PlayerInputData();

    public Animator animator;

    void Update()
    {
        bool moving = Input.GetKey(keyBindings.leftKey) 
            || Input.GetKey(keyBindings.rightKey);

        if (moving) sprite.flipX = Input.GetKey(keyBindings.rightKey);

        if (Input.GetKeyDown(keyBindings.grabKey)) animator.SetTrigger("Grab");
        else if (moving)
        {
            if (!animator.GetBool("Walking")) { animator.SetTrigger("Walk"); }
            animator.SetBool("Walking", true);
            transform.Translate((sprite.flipX ? walkSpeed : -walkSpeed), 0, 0);
        }
       else {
            animator.SetBool("Walking", false);
        }

        //attack  //neutral aerial
        if (Input.GetKeyDown(keyBindings.lightKey))
        {
            if (animator.GetBool("Grounded"))
            {
                animator.SetTrigger("Attack");
            } 
            else
            {
                animator.SetTrigger("Aerial");
            }
        }

        if (Input.GetKey(keyBindings.guard1Key) || Input.GetKey(keyBindings.guard2Key)) 
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

        if (Input.GetKeyDown(keyBindings.jumpKey))
        {

            if (Input.GetKey(keyBindings.upKey))
            {
                // +up, recovery
                animator.SetTrigger("Recovery");
            }


            //jump 
            if (numberOfJumps > 0)
            {
                animator.SetTrigger("Jump");
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump
                StartCoroutine(DelayedJump());
            }
        }
    }
    
    void OnCollisionEnter2D(Collision2D other)
    {
       if (other.gameObject.tag == "Ground" && GetComponent<Rigidbody2D>().velocity.y <= 0)
        {
            //UnityEngine.Debug.Log("GroundedEnter");
            animator.SetBool("Grounded", true);
            numberOfJumps = 2;
      }
    }

    void OnCollisionExit2D(Collision2D other)
    {
       if(other.gameObject.tag == "Ground")
       {
            //UnityEngine.Debug.Log("GroundedLeave");
            animator.SetBool("Grounded", false);
       }
    }


    private IEnumerator DelayedJump()
    {
        
        float normalizedTime = 0;
        while (normalizedTime <= 1f)
        {
            normalizedTime += Time.deltaTime / delayedJumpDuration;
            yield return null;
        }
        numberOfJumps--;
        Vector3 v = GetComponent<Rigidbody2D>().velocity;
        v.y = 0.0f;
        GetComponent<Rigidbody2D>().velocity = v;
        GetComponent<Rigidbody2D>().AddForce(Vector3.up * 2500);
    }
}
