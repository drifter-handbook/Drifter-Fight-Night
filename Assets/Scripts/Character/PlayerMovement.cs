using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int numberOfJumps = 2;
    public float delayedJumpDuration = 0.05f;
    public float walkSpeed = 15f;
    public float jumpSpeed = 32f;
    public bool flipSprite = false;

    SpriteRenderer sprite;
    public int Facing { get; private set; } = 1;

    Animator animator;

    Rigidbody2D rb;
    BoxCollider2D col;

    Coroutine varyJumpHeight;
    public float varyJumpHeightDuration = 0.5f;
    public float varyJumpHeightForce = 10f;

    PlayerStatus status;

    Drifter drifter;

    void Awake()
    {
        drifter = GetComponent<Drifter>();
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        status = GetComponent<PlayerStatus>();
    }

    void Update()
    {
        if (!GameController.Instance.IsHost)
        {
            return;
        }

        bool jumpPressed = !drifter.prevInput.Jump && drifter.input.Jump;
        // TODO: spawn hitboxes
        bool canAct = !status.HasStunEffect() && !animator.GetBool("Guarding");
        bool canGuard = !status.HasStunEffect();
        bool moving = drifter.input.MoveX != 0;
        drifter.SetAnimatorBool("Grounded", IsGrounded());

        if (moving && canAct)
        {
            sprite.flipX = flipSprite;
            Facing = (drifter.input.MoveX > 0) ? 1 : -1;
            transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),
                transform.localScale.y, transform.localScale.z);
        }

        if (moving && canAct)
        {
            drifter.SetAnimatorBool("Walking", true);
            rb.velocity = new Vector2(drifter.input.MoveX > 0 ? walkSpeed : -walkSpeed, rb.velocity.y);
        }
        else if (!moving && status.HasGroundFriction())
        {
            drifter.SetAnimatorBool("Walking", false);
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 80f * Time.deltaTime), rb.velocity.y);
        }
        else
        {
            drifter.SetAnimatorBool("Grounded", false);
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 40f * Time.deltaTime), rb.velocity.y);
        }

        if (drifter.input.Guard && canGuard)
        {
            //shift is guard
            if (!animator.GetBool("Guarding"))
            {
                drifter.SetAnimatorBool("Guarding", true);
            }
        }
        else
        {
            drifter.SetAnimatorBool("Guarding", false);
        }

        if (jumpPressed && canAct && rb.velocity.y < 0.8f * jumpSpeed)
        {
            //jump
            if (animator.GetBool("Grounded"))
            {
                numberOfJumps = 2;
            }
            if (numberOfJumps > 0)
            {
                numberOfJumps--;
                drifter.SetAnimatorTrigger("Jump");
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump
                StartCoroutine(DelayedJump());
            }
        }
    }

    RaycastHit2D[] hits = new RaycastHit2D[10];
    private bool IsGrounded()
    {
        int count = Physics2D.RaycastNonAlloc(col.bounds.center + col.bounds.extents.y * Vector3.down, Vector3.down, hits, 0.2f);
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
        if (varyJumpHeight != null)
        {
            StopCoroutine(varyJumpHeight);
        }
        rb.velocity = new Vector2(rb.velocity.x, 0f);
        float time = 0;
        while (time <= delayedJumpDuration)
        {
            time += Time.deltaTime;
            yield return null;
        }
        rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
        varyJumpHeight = StartCoroutine(VaryJumpHeight());
    }

    private IEnumerator VaryJumpHeight()
    {
        float time = 0f;
        while (time < varyJumpHeightDuration)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            if (!animator.GetBool("Grounded") && drifter.input.Jump)
            {
                rb.AddForce(Vector2.up * -Physics2D.gravity * varyJumpHeightForce);
            }
        }
        varyJumpHeight = null;
    }
}
