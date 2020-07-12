using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public int numberOfJumps = 2;
    public float delayedJumpDuration = 0.05f; // 3 seconds you can change this to whatever you want
    public float walkSpeed = 15f;
    public float jumpSpeed = 32f;
    public bool flipSprite = false;

    SpriteRenderer sprite;
    CapsuleCollider2D capsule;
    private Vector3 origTransform;
    private Vector3 flippedTransform;

    public PlayerInputData input { get; set; } = new PlayerInputData();
    PlayerInputData prevInput = new PlayerInputData();

    Animator animator;
    public PlayerAnimatorState animatorState { get; set; } = new PlayerAnimatorState();

    // stuns the character for several frames if stunCount > 0
    int stunCount = 0;

    Rigidbody2D rb;
    BoxCollider2D col;

    [NonSerialized]
    public bool IsClient;

    Coroutine varyJumpHeight;
    public float varyJumpHeightDuration = 0.5f;
    public float varyJumpHeightForce = 10f;

    // attack effects are used to impact player movement during an attack
    // for example, recovery makes the player go up
    IPlayerAttackEffect attackEffect;

    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        capsule = GetComponentInChildren<CapsuleCollider2D>();
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
        attackEffect = GetComponent<IPlayerAttackEffect>();
    }

    void Update()
    {
        if (IsClient)
        {
            return;
        }

        // get input
        bool jumpPressed = !prevInput.Jump && input.Jump;
        bool lightPressed = !prevInput.Light && input.Light;
        bool specialPressed = !prevInput.Special && input.Special;
        bool grabPressed = !prevInput.Grab && input.Grab;
        // TODO: spawn hitboxes
        bool canAct = stunCount == 0 && !animator.GetBool("Guarding");
        bool canGuard = stunCount == 0;
        bool moving = input.MoveX != 0;
        SetAnimatorBool("Grounded", IsGrounded());

        if (moving && canAct)
        {
            sprite.flipX = flipSprite ^ (input.MoveX > 0);
        }

        if (grabPressed && canAct)
        {
            SetAnimatorTrigger("Grab");
            StartMovementEffect(attackEffect?.Grab(), 0f);
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
        if (lightPressed && canAct)
        {
            if (animator.GetBool("Grounded"))
            {
                SetAnimatorTrigger("Attack");
                StartMovementEffect(attackEffect?.Light(), 0f);
                StartCoroutine(StunFor(0.1f));
            }
            else
            {
                SetAnimatorTrigger("Aerial");
                StartMovementEffect(attackEffect?.Aerial(), 0f);
                StartCoroutine(StunFor(0.5f));
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

        if (specialPressed && input.MoveY > 0 && canAct)
        {
            // recovery
            SetAnimatorTrigger("Recovery");
            StartMovementEffect(attackEffect?.Recovery(), 0f);
            StartCoroutine(StunFor(0.25f));
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
                SetAnimatorTrigger("Jump");
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump
                StartCoroutine(DelayedJump());
            }
        }

        // set previous player input
        prevInput.CopyFrom(input);
    }

    public bool Walking = false;
    // used by clients
    public void SyncAnimatorState(PlayerAnimatorState state)
    {
        int ID = GetComponent<INetworkSync>().ID;
        animator.SetBool("Grounded", state.Grounded);
        animator.SetBool("Walking", state.Walking);
        Walking = state.Walking;
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
        if (!IsClient)
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
        if (!IsClient)
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
            if (!animator.GetBool("Grounded") && input.Jump)
            {
                rb.AddForce(Vector2.up * -Physics2D.gravity * varyJumpHeightForce);
            }
        }
        varyJumpHeight = null;
    }

    public IEnumerator StunFor(float time)
    {
        stunCount++;
        yield return new WaitForSeconds(time);
        stunCount--;
    }

    // Super-armor logic
    private class AttackEffect
    {
        public Coroutine Effect;
        public float SuperArmor;
        public float Damage;
    }
    
    List<AttackEffect> movementEffects = new List<AttackEffect>();
    private void StartMovementEffect(IEnumerator ef, float superArmor)
    {
        if (ef != null)
        {
            movementEffects.Add(new AttackEffect() {
                Effect = StartCoroutine(ef),
                SuperArmor = superArmor,
                Damage = 0
            });
        }
    }

    // call this when launched to damage a movement effect
    public void DamageSuperArmor(float damage)
    {
        for (int i = 0; i < movementEffects.Count; i++)
        {
            AttackEffect ef = movementEffects[i];
            ef.Damage += damage;
            if (ef.Damage > ef.SuperArmor)
            {
                StopCoroutine(ef.Effect);
                movementEffects.RemoveAt(i);
                i--;
            }
        }
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
        return new PlayerAnimatorState()
        {
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
