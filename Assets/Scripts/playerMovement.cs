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

    SpriteRenderer sprite;

    private Vector3 origTransform;
    private Vector3 flippedTransform;

    public PlayerInputData input { get; set; } = new PlayerInputData();

    Animator animator;

    // stuns the character for several frames if stunCount > 0
    int stunCount = 0;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // TODO: spawn hitboxes
        bool canAct = stunCount == 0 && !animator.GetBool("Guarding");
        bool canGuard = stunCount == 0;
        bool moving = input.MoveX != 0;
        animator.SetBool("Grounded", IsGrounded());
        if (animator.GetBool("Grounded"))
        {
            numberOfJumps = 2;
        }

        if (moving && canAct)
        {
            sprite.flipX = input.MoveX > 0;
        }

        if (input.Grab && canAct)
        {
            animator.SetTrigger("Grab");
            StartCoroutine(StunFor(0.5f));
        }
        else if (moving && canAct)
        {
            if (!animator.GetBool("Walking")) { animator.SetTrigger("Walk"); }
            animator.SetBool("Walking", true);
            transform.Translate((input.MoveX > 0 ? walkSpeed : -walkSpeed), 0, 0);
        }
        else {
            animator.SetBool("Walking", false);
        }

        //attack  //neutral aerial
        if (input.Light && canAct)
        {
            if (animator.GetBool("Grounded"))
            {
                animator.SetTrigger("Attack");
                StartCoroutine(StunFor(0.1f));
            } 
            else
            {
                animator.SetTrigger("Aerial");
                StartCoroutine(StunFor(0.5f));
            }
        }
        if (input.Guard && canGuard) 
        {
            //shift is guard 
            if (!animator.GetBool("Guarding"))
            {
                animator.SetBool("Guarding", true);
            }
        }
        else
        {
            animator.SetBool("Guarding", false);
        }

        if (input.Jump && canAct)
        {
            if (input.MoveY > 0)
            {
                // +up, recovery
                animator.SetTrigger("Recovery");
                StartCoroutine(StunFor(0.25f));
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

    RaycastHit2D[] hits = new RaycastHit2D[10];
    private bool IsGrounded()
    {
        int count = Physics2D.RaycastNonAlloc(transform.position + 3.9f * Vector3.down, Vector3.down, hits, 0.2f);
        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.gameObject.tag == "Ground")
            {
                return true;
            }
        }
        return false;
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

    private IEnumerator StunFor(float time)
    {
        stunCount++;
        yield return new WaitForSeconds(time);
        stunCount--;
    }
}
