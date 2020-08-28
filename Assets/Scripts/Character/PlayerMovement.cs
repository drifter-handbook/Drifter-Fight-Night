using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public int numberOfJumps;
    public float delayedJumpDuration = 0.05f;
    public float walkSpeed = 15f;
    public float groundAccelerationTime = .6f;
    public float airAccelerationTime = .8f;
    public float airSpeed = 15f;
    public float terminalVelocity = 80f;
    public bool flipSprite = false;

    public float jumpHeight = 20f;
    public float jumpTime = 1f;

    public int currentJumps;
    float jumpSpeed;
    float baseGravity;

    SpriteRenderer sprite;
    public int Facing { get; private set; } = 1;

    Animator animator;

    NetworkEntityList entities;

    Rigidbody2D rb;
    BoxCollider2D col;

    Coroutine varyJumpHeight;
    public float varyJumpHeightDuration = 0.5f;
    public float varyJumpHeightForce = 10f;

    PlayerStatus status;

    Drifter drifter;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();

        drifter = GetComponent<Drifter>();
        animator = drifter.animator;
        sprite = GetComponentInChildren<SpriteRenderer>();

        col = GetComponent<BoxCollider2D>();
        status = GetComponent<PlayerStatus>();
    }
    void Start(){
        baseGravity = rb.gravityScale;
        jumpSpeed = (float)(jumpHeight / jumpTime + .5f*(rb.gravityScale * jumpTime));

    }

    void Update()
    {
        if (!GameController.Instance.IsHost || GameController.Instance.IsPaused)
        {
            return;
        }

        bool jumpPressed = !drifter.prevInput.Jump && drifter.input.Jump;
        // TODO: spawn hitboxes
        bool canAct = !status.HasStunEffect() && !animator.GetBool("Guarding");
        bool canGuard = !status.HasStunEffect();
        bool moving = drifter.input.MoveX != 0;

        //Handle jump resets
        if(animator.GetBool("Grounded"))
        {
            currentJumps = numberOfJumps;
            if(!IsGrounded())
            {
                currentJumps--;
            }

        }
        else if(IsGrounded()){
            //landing
            //spawnJuiceParticle(new Vector3(0,-1,0),2);
        }
        drifter.SetAnimatorBool("Grounded", IsGrounded());

        if(status.HasEnemyStunEffect() && !animator.GetBool("HitStun")){
            drifter.SetAnimatorBool("HitStun",true);
        }
        else if(!status.HasEnemyStunEffect() && animator.GetBool("HitStun"))
        {
            drifter.SetAnimatorBool("HitStun",false);
        }

        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && rb.velocity.magnitude > 45f){
            spawnJuiceParticle(Vector3.zero,1);
        }

        if(status.HasStatusEffect(PlayerStatusEffect.REVERSED)){
            drifter.input.MoveX *= -1;
        }

        if (moving && canAct)
        {
        	//UnityEngine.Debug.Log("BEFORE velocity: " + rb.velocity.x);
        	updateFacing();

            drifter.SetAnimatorBool("Walking", true);

            //If just started moving or switched directions
            if((rb.velocity.x == 0 || rb.velocity.x * drifter.input.MoveX < 0) && IsGrounded()){
                spawnJuiceParticle(new Vector3(-Facing * 1.5f,-1.3f,0),5);
            }

            if(IsGrounded())
            {
            	//UnityEngine.Debug.Log("Ground Accell");
            	rb.velocity = new Vector2(drifter.input.MoveX > 0 ? 
                    Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)?walkSpeed:(.6f*walkSpeed)),rb.velocity.x,groundAccelerationTime) :
                    Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)?-walkSpeed:(-.6f*walkSpeed)),rb.velocity.x,groundAccelerationTime), rb.velocity.y);
            }
            else
            {
            	rb.velocity = new Vector2(drifter.input.MoveX > 0 ? 
                    Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)?airSpeed:(.6f*airSpeed)),rb.velocity.x,airAccelerationTime) : 
                    Mathf.Lerp((!status.HasStatusEffect(PlayerStatusEffect.SLOWED)?-airSpeed:(-.6f*airSpeed)),rb.velocity.x,airAccelerationTime), rb.velocity.y);
            }

            //UnityEngine.Debug.Log("AFTER velocity: " + rb.velocity.x);
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

        if(drifter.input.Guard && canGuard && moving){

            drifter.SetAnimatorTrigger("Roll");
            updateFacing();
        }

        else if (drifter.input.Guard && canGuard)
        {
            //shift is guard
            if (!animator.GetBool("Guarding"))
            {
                drifter.SetAnimatorBool("Guarding", true);
            }
            updateFacing();

        }
        else
        {
            drifter.SetAnimatorBool("Guarding", false);
        }

        //Terminal velocity

        if(rb.velocity.y < -terminalVelocity && !status.HasEnemyStunEffect()){
            rb.velocity = new Vector2(rb.velocity.x,-terminalVelocity);
        }


        if (jumpPressed && canAct) //&& rb.velocity.y < 0.8f * jumpSpeed)
        {
            //jump
            if (currentJumps > 0)
            {
                currentJumps--;
                drifter.SetAnimatorTrigger("Jump");
                //Particles
                if(IsGrounded()){
                    spawnJuiceParticle(new Vector3(0,-1,0),3);
                }
                else{
                    spawnJuiceParticle(new Vector3(0,-1,0),4);
                }
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump
                StartCoroutine(DelayedJump());
            }
        }

        if(status.HasStatusEffect(PlayerStatusEffect.STUNNED) || status.HasStatusEffect(PlayerStatusEffect.PLANTED))
        {
            if(status.HasStatusEffect(PlayerStatusEffect.PLANTED) && !IsGrounded())
            {
                status.ApplyStatusEffect(PlayerStatusEffect.PLANTED,0f);
            }
            else{
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;
            }
            
        }
        else if(!status.HasStatusEffect(PlayerStatusEffect.END_LAG)){
            rb.gravityScale = baseGravity;
        }
    }
    public void updateFacing()
    {
        if(flipSprite ^ drifter.input.MoveX > 0){
                Facing = 1;
            }
            else if(flipSprite ^ drifter.input.MoveX < 0){
                Facing = -1;
            }
            transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),
                transform.localScale.y, transform.localScale.z);
    }

    public void updatePosition (Vector3 position){
      transform.position = position;
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

    private void spawnJuiceParticle(Vector3 pos, int mode)
    {
        GameObject juiceParticle = Instantiate(entities.GetEntityPrefab("MovementParticle"), transform.position + pos, transform.rotation);
        juiceParticle.GetComponent<JuiceParticle>().mode = mode;
        juiceParticle.transform.localScale = new Vector3( juiceParticle.transform.localScale.x * Facing,juiceParticle.transform.localScale.y,1);
        entities.AddEntity(juiceParticle);
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
                //rb.AddForce(Vector2.up * -Physics2D.gravity * varyJumpHeightForce);
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            }
        }
        varyJumpHeight = null;
    }
}
