using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public int numberOfJumps = 2;
    public float delayedJumpDuration = 0.05f; // 3 seconds you can change this to whatever you want
    public float walkSpeed = 15f;

    SpriteRenderer sprite;

    private Vector3 origTransform;
    private Vector3 flippedTransform;

    public PlayerInputData input { get; set; } = new PlayerInputData();

    Animator animator;
    public PlayerAnimatorState animatorState { get; set; } = new PlayerAnimatorState();

    // stuns the character for several frames if stunCount > 0
    int stunCount = 0;

    Rigidbody2D rb;

    [NonSerialized]
    public GameSyncManager sync;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // TODO: spawn hitboxes
        bool canAct = stunCount == 0 && !animator.GetBool("Guarding");
        bool canGuard = stunCount == 0;
        bool moving = input.MoveX != 0;
        SetAnimatorBool("Grounded", IsGrounded());
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
            SetAnimatorTrigger("Grab");
            StartCoroutine(StunFor(0.5f));
        }
        else if (moving && canAct)
        {
            SetAnimatorBool("Walking", true);
            rb.velocity = new Vector2(input.MoveX > 0 ? walkSpeed : -walkSpeed, rb.velocity.y);
        }
        else
        {
            SetAnimatorBool("Walking", false);
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 40f * Time.deltaTime), rb.velocity.y);
        }

        //attack  //neutral aerial
        if (input.Light && canAct)
        {
            if (animator.GetBool("Grounded"))
            {
                SetAnimatorTrigger("Attack");
                StartCoroutine(StunFor(0.1f));
            } 
            else
            {
                SetAnimatorTrigger("Aerial");
                StartCoroutine(StunFor(0.7f));
            }
        }
        if (input.Guard && canGuard) 
        {
            //shift is guard 
            if (!animator.GetBool("Guarding"))
            {
                SetAnimatorBool("Guarding", true);
            }
        }
        else
        {
            SetAnimatorBool("Guarding", false);
        }

        if (input.Jump && canAct)
        {
            if (input.MoveY > 0)
            {
                // +up, recovery
                SetAnimatorTrigger("Recovery");
                StartCoroutine(StunFor(0.25f));
            }
            //jump 
            if (numberOfJumps > 0)
            {
                SetAnimatorTrigger("Jump");
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump
                StartCoroutine(DelayedJump());
            }
        }
        // we processed key presses for host
        if (sync != null && sync.GetIsHost())
        {
            sync.GetComponent<PlayerInput>()?.ResetKeyDowns();
        }
    }

    // used by clients
    public void SyncAnimatorState(PlayerAnimatorState state)
    {
        animator.SetBool("Grounded", state.Grounded);
        animator.SetBool("Walking", state.Walking);
        animator.SetBool("Guarding", state.Guarding);
        if (state.Attack) animator.SetTrigger("Attack");
        if (state.Grab) animator.SetTrigger("Grab");
        if (state.Jump) animator.SetTrigger("Jump");
        if (state.Recovery) animator.SetTrigger("Recovery");
        if (state.Aerial) animator.SetTrigger("Aerial");
    }
    // used by host
    private void SetAnimatorTrigger(string s)
    {
        if (sync != null && sync.GetIsHost())
        {
            animator.SetTrigger(s);
        }
        switch (s)
        {
            case "Attack":
                animatorState.Attack = true;
                break;
            case "Grab":
                animatorState.Grab = true;
                break;
            case "Jump":
                animatorState.Jump = true;
                break;
            case "Recovery":
                animatorState.Recovery = true;
                break;
            case "Aerial":
                animatorState.Aerial = true;
                break;
        }
    }
    public void ResetAnimatorTriggers()
    {
        animatorState.Attack = false;
        animatorState.Grab = false;
        animatorState.Jump = false;
        animatorState.Recovery = false;
        animatorState.Aerial = false;
    }
    private void SetAnimatorBool(string s, bool value)
    {
        if (sync != null && sync.GetIsHost())
        {
            animator.SetBool(s, value);
        }
        switch (s)
        {
            case "Grounded":
                animatorState.Grounded = value;
                break;
            case "Walking":
                animatorState.Walking = value;
                break;
            case "Guarding":
                animatorState.Guarding = value;
                break;
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
        Vector3 v = rb.velocity;
        v.y = 0.0f;
        rb.velocity = v;
        rb.AddForce(Vector3.up * 2500);
    }

    private IEnumerator StunFor(float time)
    {
        stunCount++;
        yield return new WaitForSeconds(time);
        stunCount--;
    }
}

public class PlayerAnimatorState : ICloneable
{
    public bool Grounded = false;
    public bool Walking = false;
    public bool Guarding = false;
    public bool Attack = false;
    public bool Grab = false;
    public bool Jump = false;
    public bool Recovery = false;
    public bool Aerial = false;

    public object Clone()
    {
        return new PlayerAnimatorState() {
            Grounded = Grounded,
            Walking = Walking,
            Guarding = Guarding,
            Attack = Attack,
            Grab = Grab,
            Jump = Jump,
            Recovery = Recovery,
            Aerial = Aerial
        };
    }
}
