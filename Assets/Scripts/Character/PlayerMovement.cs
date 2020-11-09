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
    public float terminalVelocity = 25f;
    public float fastFallTerminalVelocity = 55f;
    public bool flipSprite = false;

    public float jumpHeight = 20f;
    public float jumpTime = 1f;

    public int Weight = 90;

    public int currentJumps;
    public float ledgeOffset = 1f;
    public float ledgeClimbOffset = 0f;
    float jumpSpeed;
    float baseGravity;

    

    Vector2 prevVelocity;

    SpriteRenderer sprite;
    public int Facing { get; set; } = 1;
    public bool grounded = true;
    public bool gravityPaused = false;
    bool ledgeHanging = false;
    bool strongLedgeGrab = true;

    Animator animator;

    NetworkEntityList entities;

    PlayerAttacks attacks;

    Rigidbody2D rb;
    PolygonCollider2D col;

    Coroutine varyJumpHeight;
    public float varyJumpHeightDuration = 0.5f;
    public float varyJumpHeightForce = 10f;

    int count = 0;

    PlayerStatus status;

    Drifter drifter;

    float dropThroughTime;

    int prevMoveX = 0;
    int prevMoveY = 0;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        entities = GameObject.FindGameObjectWithTag("NetworkEntityList").GetComponent<NetworkEntityList>();

        drifter = GetComponent<Drifter>();
        animator = drifter.animator;
        sprite = GetComponentInChildren<SpriteRenderer>();

        attacks = GetComponent<PlayerAttacks>();

        col = GetComponent<PolygonCollider2D>();
        status = GetComponent<PlayerStatus>();


    }
    void Start(){
        baseGravity = rb.gravityScale;
        jumpSpeed = (float)(jumpHeight / jumpTime + .5f*(rb.gravityScale * jumpTime));

    }

    //Restitution

    void OnCollisionEnter2D(Collision2D col){
        if(!status.HasGroundFriction() && (prevVelocity.y < 0 || col.gameObject.tag !=  "Platform") && (-65f >= (Mathf.Atan2(prevVelocity.y, prevVelocity.x)*180f / Mathf.PI) &&  (Mathf.Atan2(prevVelocity.y, prevVelocity.x)*180f / Mathf.PI) >= -115f)){
            
            if(prevVelocity.y < -5f ){
                status.bounce();
                Vector3 normal = col.contacts[0].normal;
                rb.velocity = Vector2.Reflect(prevVelocity,normal) *.8f;
                status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.2f);
                spawnJuiceParticle(new Vector3(0,-2.5f,0),7,Quaternion.Euler(0f,0f,Vector3.Angle(Vector3.down,normal)));
            }

        }
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
       
        if(gameObject.layer != 8 && Time.time - dropThroughTime > .55f){
            gameObject.layer = 8;
        }

        //Handle jump resets
        if(animator.GetBool("Grounded"))
        {
            currentJumps = numberOfJumps;
            strongLedgeGrab = true;
            
            if(!IsGrounded())
            {
                currentJumps--;
            }            
        }
        else if(IsGrounded() && !status.HasStunEffect())
        {
            spawnJuiceParticle(new Vector3(0,-1,0),2);
        }

        drifter.SetAnimatorBool("Grounded", IsGrounded());
        grounded = IsGrounded();

        //Sets hitstun state when applicable
        if(status.HasEnemyStunEffect() && !animator.GetBool("HitStun")){
            drifter.SetAnimatorBool("HitStun",true);
        }
        else if(!status.HasEnemyStunEffect() && animator.GetBool("HitStun"))
        {
            drifter.SetAnimatorBool("HitStun",false);
        }

        //Pause all animations while in hitpause
        if(status.HasStatusEffect(PlayerStatusEffect.HITPAUSE))
        {
            animator.enabled = false;
        }
        else{
            animator.enabled = true;
        }

        //Smoke trail
        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && rb.velocity.magnitude > 45f){
            spawnJuiceParticle(Vector3.zero,1,Quaternion.Euler(0,0,UnityEngine.Random.Range(0,180)));
        }

        //Reversed controls
        if(status.HasStatusEffect(PlayerStatusEffect.REVERSED)){
            drifter.input.MoveX *= -1;
        }

        //Normal walking logic
        if (moving && canAct && ! ledgeHanging)
        {
        	//UnityEngine.Debug.Log("BEFORE velocity: " + rb.velocity.x);
        	updateFacing();

            drifter.SetAnimatorBool("Walking", true);

            //If just started moving or switched directions
            if((rb.velocity.x == 0 || rb.velocity.x * drifter.input.MoveX < 0) && IsGrounded()){
                spawnJuiceParticle(new Vector3(-Facing * (flipSprite?-1:1)* 1.5f,-1.3f,0),5);
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
        else if(canAct && ledgeHanging)
        {
            if((drifter.input.MoveX * (flipSprite?-1:1) * Facing < 0)){
                DropLedge();
                drifter.SetAnimatorTrigger("Ledge_Drop");
                rb.velocity = new Vector3(Facing * (flipSprite?-1:1) * -25f,25f);
            }
            
            else if((drifter.input.MoveX * (flipSprite?-1:1) * Facing > 0)  || drifter.input.MoveY > 0){
                DropLedge();
                drifter.SetAnimatorTrigger("Ledge_Climb_Basic");
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.2f);
                rb.position = new Vector3(rb.position.x + (rb.position.x > 0 ? -1 :1) *2f, rb.position.y + 5f - ledgeClimbOffset);
            }

        }
        //Turn walking animation off
        else if (!moving && status.HasGroundFriction())
        {
            drifter.SetAnimatorBool("Walking", false);
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 80f * Time.deltaTime), rb.velocity.y);
        }
        //Tunrs hang animation on
        else
        {
            drifter.SetAnimatorBool("Grounded", false);
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 40f * Time.deltaTime), rb.velocity.y);
        }

        //fastfall
        //if(canAct && !IsGrounded() && drifter.input.MoveY < 0 && prevMoveY < 0) rb.velocity = new Vector2(rb.velocity.x,-fastFallTerminalVelocity);

        //Drop throuhg platforms
        if(canGuard && drifter.input.MoveY <-1){
            gameObject.layer = 13;
            dropThroughTime = Time.time;

        }

        //Roll
        if(drifter.input.Guard && canGuard && moving && IsGrounded()){

            drifter.SetAnimatorTrigger("Roll");
            updateFacing();
        }

        //Guard
        else if (drifter.input.Guard && canGuard && !ledgeHanging)
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

        if(drifter.input.Guard && canGuard && ledgeHanging)
        {
            drifter.SetAnimatorTrigger("Ledge_Climb");
        }

        //Terminal velocity

        if(rb.velocity.y < -terminalVelocity && !status.HasEnemyStunEffect()){
            rb.velocity = new Vector2(rb.velocity.x,(drifter.input.MoveY < 0 && prevMoveY < 0 ?-fastFallTerminalVelocity:-terminalVelocity));
        }

        if(drifter.input.MoveY < 0 && prevMoveY < 0 && ledgeHanging){
            DropLedge();
            drifter.SetAnimatorTrigger("Ledge_Drop");
        }

        //Jump
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

        //mashout effects
        if((status.HasStatusEffect(PlayerStatusEffect.PLANTED) || status.HasStatusEffect(PlayerStatusEffect.AMBERED)) && prevMoveX != drifter.input.MoveX){
            status.mashOut();

            spawnJuiceParticle(new Vector3(.5f,UnityEngine.Random.Range(1f,3f),0),6);
        }
        prevMoveX = drifter.input.MoveX;
        prevMoveY = drifter.input.MoveY;

        //Pause movement for relevent effects.
        if(status.HasStatusEffect(PlayerStatusEffect.STUNNED) || status.HasStatusEffect(PlayerStatusEffect.PLANTED) || status.HasStatusEffect(PlayerStatusEffect.DEAD) || status.HasStatusEffect(PlayerStatusEffect.HITPAUSE))
        {
            if(status.HasStatusEffect(PlayerStatusEffect.PLANTED) && !IsGrounded())
            {
            	status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,.4f);
            }
            else{
                rb.velocity = Vector2.zero;
                rb.gravityScale = 0;
            }
            
        }
        //makes sure gavity is always reset after using a move
        else if(!status.HasStatusEffect(PlayerStatusEffect.END_LAG) && !gravityPaused && !ledgeHanging){
            rb.gravityScale = baseGravity;
        }

        if(rb.velocity != Vector2.zero)prevVelocity = rb.velocity;
        
    }
    void updateFacing()
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

    public void flipFacing(){
        Facing *= -1;
        transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y, transform.localScale.z);
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
            if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform" && status.HasGroundFriction()))
            {
                return true;
            }
        }
        return false;
    }

    public void GrabLedge(Vector3 pos){
        gravityPaused = false;
        attacks.ledgeHanging = true;
        if(strongLedgeGrab)drifter.SetAnimatorTrigger("Ledge_Grab_Strong");
        else drifter.SetAnimatorTrigger("Ledge_Grab_Weak");
        Facing = flipSprite ^ rb.position.x > 0 ? -1 :1;
        transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),
                transform.localScale.y, transform.localScale.z);

        rb.position = new Vector3(pos.x - (rb.position.x > 0 ? -1 :1) *2f, pos.y-ledgeOffset,pos.z);
 
        attacks.resetRecovery();

        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.2f);
        ledgeHanging = true;
        rb.gravityScale = 0f;
        currentJumps = numberOfJumps - 1;

        rb.velocity = Vector2.zero;
    }

    public void DropLedge(){
        ledgeHanging = false;
        rb.gravityScale = baseGravity;
        strongLedgeGrab = false;
        attacks.ledgeHanging = false;
    }


    public void spawnJuiceParticle(Vector3 pos, int mode)
    {
        spawnJuiceParticle(pos, mode, transform.rotation);
    }

    private void spawnJuiceParticle(Vector3 pos, int mode, Quaternion angle){

    	GameObject juiceParticle = Instantiate(entities.GetEntityPrefab("MovementParticle"), transform.position + pos,  angle);
        juiceParticle.GetComponent<JuiceParticle>().mode = mode;
        juiceParticle.transform.localScale = new Vector3( juiceParticle.transform.localScale.x * Facing * (flipSprite?-1:1),juiceParticle.transform.localScale.y,1);

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
            if (!animator.GetBool("Grounded") && !status.HasStunEffect() && drifter.input.Jump)
            {
                //rb.AddForce(Vector2.up * -Physics2D.gravity * varyJumpHeightForce);
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            }
        }
        varyJumpHeight = null;
    }
}
