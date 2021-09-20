using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //Character Properties
    public int numberOfJumps;
    public float delayedJumpDuration = 0.05f;
    public float walkSpeed = 15f;
    public float groundAccelerationTime = .6f;
    public float airAccelerationTime = .8f;
    public float airSpeed = 15f;
    public float terminalVelocity = 25f;
    public bool flipSprite = false;
    public float jumpHeight = 20f;
    public float jumpTime = 1f;
    public int Weight = 90;
    public float ledgeOffset = 1f;
    public float ledgeClimbOffset = 0f;
    public Vector3 particleOffset =  Vector3.zero;
    public float varyJumpHeightDuration = 0.5f;
    public float varyJumpHeightForce = 10f;


    protected static float framerateScalar =.0833333333f;

    protected float baseWalkSpeed= 0;

    //Calculated character properties
    float jumpSpeed;
    float baseGravity;

    //Animator State Fields
    public int Facing { get; set; } = 1;
    [NonSerialized]
    public int currentJumps;
    //[NonSerialized]
    public bool grounded = true;
    [NonSerialized]
    public bool hitstun = false;
    [NonSerialized]
    public bool canLandingCancel = false;
    [NonSerialized]
    public bool jumping = false;
    [NonSerialized]
    public bool gravityPaused = false;
    [NonSerialized]
    public bool ledgeHanging = false;
    [NonSerialized]
    public Vector3 wallSliding = Vector3.zero;
    bool strongLedgeGrab = true;
    [NonSerialized]
    public float techWindowElapsed = 0;

    public float accelerationPercent = .9f;

    float dashLock = 0;

    //Access to main camera for screen darkening
    ScreenShake mainCamera;

    // public float activeFriction = .1f;
    // public float inactiveFriction = .4f;
    PolygonCollider2D frictionCollider;
    BoxCollider2D BodyCollider; 


    //Component Fields
    Animator animator;
    PlayerAttacks attacks;
    PlayerStatus status;
    SpriteRenderer sprite;
    Rigidbody2D rb;
    Drifter drifter;
    GameObjectShake shake;

    
    //Jump Coroutines
    Coroutine jumpCoroutine;
    Coroutine varyJumpHeight;

    //Situational Iteration variables
    float dropThroughTime;
    int ringTime = 6;
    float walkTime = 0;
    Vector2 prevVelocity;

    float currentSpeed;

    void Awake()
    {
        //Aggregate componenents
        rb = GetComponent<Rigidbody2D>();
        drifter = GetComponent<Drifter>();
        sprite = GetComponentInChildren<SpriteRenderer>();
        attacks = GetComponent<PlayerAttacks>();
        shake = gameObject.GetComponentInChildren<GameObjectShake>();

        //Do this better
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<ScreenShake>();

        status = drifter.status;
        animator = drifter.animator;

        BodyCollider = GetComponent<BoxCollider2D>();
        frictionCollider = GetComponent<PolygonCollider2D>();

        baseWalkSpeed = walkSpeed;
        
    }
    void Start(){
        
        baseGravity = rb.gravityScale;
        jumpSpeed = (jumpHeight / jumpTime + .5f*(rb.gravityScale * jumpTime));
        if (!GameController.Instance.IsHost)
        {
            rb.isKinematic = true;
        }
    }

    //Restitution
    //TODO Redo this whole mess
    void OnCollisionEnter2D(Collision2D col)
    {

        if(!status.HasGroundFriction() && ((prevVelocity.y < 0 || col.gameObject.tag !=  "Platform" ) && prevVelocity.magnitude > 35f))
        {

            if(techWindowElapsed <= framerateScalar * 2)UnityEngine.Debug.Log("COULD HAVE TECHED");
            else UnityEngine.Debug.Log("COULD NOT HAVE TECHED");

            if(drifter.input[0].Guard && techWindowElapsed <= framerateScalar * 2)
            {
                rb.velocity = Vector3.zero;
                status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,.01f);
                status.ApplyStatusEffect(PlayerStatusEffect.KNOCKBACK,.01f);
        
                hitstun = false;
                
                drifter.returnToIdle();

                techParticle();

                //PARTICLE EFFECT HERE

            }
            else
            {
                //status.bounce();
                Vector3 normal = col.contacts[0].normal;
                
                rb.velocity = Vector2.Reflect(prevVelocity,normal) *.8f;
                //status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE, Mathf.Min(rb.velocity.magnitude * .005f,.3f));
                spawnJuiceParticle(col.contacts[0].point, MovementParticleMode.Restitution, Quaternion.Euler(0f,0f, ( (rb.velocity.x < 0)?1:-1 ) * Vector3.Angle(Vector3.up,normal)),false);
                techWindowElapsed = 0;
            }
        }
    }

    void Update()
    {
        if(dashLock >0)dashLock -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!GameController.Instance.IsHost || GameController.Instance.IsPaused)
            return;


        if(drifter.input[0].Guard) techWindowElapsed += Time.deltaTime;
        else if(status.HasGroundFriction()) techWindowElapsed = 0;

        bool moving = drifter.input[0].MoveX != 0;

        //Unpause gravity when hit
        if(!status.HasGroundFriction())gravityPaused=false;

        //pause attacker during hitpause, and apply hurt animation to defender
        if(status.HasStatusEffect(PlayerStatusEffect.HITPAUSE))
        {
            
            if(drifter.guardBreaking && status.HasEnemyStunEffect())
            {
                drifter.PlayAnimation("Guard_Break");
                StartCoroutine(shake.Shake(.3f,.7f));
            }
            else if(status.HasEnemyStunEffect() && !drifter.guarding)
            {
                drifter.PlayAnimation("HitStun");
                StartCoroutine(shake.Shake(.2f,.7f));
            }
            else if(status.HasEnemyStunEffect())
            {
                drifter.PlayAnimation("BlockStun");
                StartCoroutine(shake.Shake(.1f,.7f));
            }
            else{
                animator.enabled = false;
            }
            
        }
        //Reactivate attacker when hitpause removed
        else
        {
            animator.enabled = true;
        }

        //Cancel aerials on landing + landing animation
        if(!grounded && IsGrounded() && !status.HasEnemyStunEffect() && !drifter.guarding && !drifter.guardBreaking && (!status.HasStatusEffect(PlayerStatusEffect.END_LAG) || canLandingCancel))drifter.PlayAnimation(drifter.JumpEndStateName);

        //Handles jumps
        if(grounded && !jumping)
        {
            //Resets jumps if player is on the ground
            currentJumps = numberOfJumps;
            strongLedgeGrab = true;
        
            //If the player walked off a ledge, remove their grounded jump
            if(!IsGrounded())
            {
                currentJumps--;
            }            
        }
        //TODO make sure this doesnt shit particles everywhere 
        else if(IsGrounded() && !status.HasStunEffect() && !jumping)
        {
            //drifter.PlayAnimation("Jump_End");
            spawnJuiceParticle(transform.position + particleOffset + new Vector3(0,-1,0), MovementParticleMode.Land);
        }

        grounded = IsGrounded();
        wallSliding = IsWallSliding();

        //if(status.HasStatusEffect(PlayerStatusEffect.PLANTED) && !grounded)status.ApplyStatusEffect(PlayerStatusEffect.PLANTED,0f);
       
        //Sets hitstun state when applicable

        if(status.HasEnemyStunEffect() && drifter.guardBreaking)
        {
            drifter.PlayAnimation("Guard_Break");
            hitstun = true;
        }

        else if(status.HasEnemyStunEffect() && !drifter.guarding)
        {
            hitstun = true;
            drifter.PlayAnimation("HitStun");
            DropLedge();
        }

        else if(status.HasEnemyStunEffect() && drifter.guarding)
        {
            drifter.PlayAnimation("BlockStun");
            hitstun = true;
        }  
        

        if(hitstun && !status.HasEnemyStunEffect() && !drifter.guarding)
        {
            hitstun = false;
            drifter.guardBreaking = false;
            drifter.returnToIdle();
            ringTime = 6;
        }

        else if(hitstun && !status.HasEnemyStunEffect() && drifter.guarding)
        {
            hitstun = false;
            drifter.PlayAnimation("Guard");
        }

        //Smoke Trail
        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && rb.velocity.magnitude > 45f){
            spawnJuiceParticle(transform.position, MovementParticleMode.SmokeTrail, Quaternion.Euler(0,0,UnityEngine.Random.Range(0,180)),false);
        }

        //Sonic Boom Trail
        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && rb.velocity.magnitude > 75f){
            
            if(ringTime>= 6){
                particleOffset = new Vector3(particleOffset.x * Facing * (flipSprite?-1:1),particleOffset.y,0);

                GameObject launchRing = GameController.Instance.host.CreateNetworkObject("LaunchRing", transform.position + particleOffset,  Quaternion.Euler(0,0,((rb.velocity.y>0)?1:-1) * Vector3.Angle(rb.velocity, new Vector3(1f,0,0))));

                launchRing.transform.localScale = new Vector3(  7.5f* Facing * (flipSprite?-1:1),7.5f,1);

                ringTime = 0;

            }
            else{
                ringTime++;
            }

        }

        //Inverts controls if revered
        if(status.HasStatusEffect(PlayerStatusEffect.REVERSED)){
            drifter.input[0].MoveX *= -1;
        }

        //Pauses you in place if you have a corresponding status effect.
        if(status.HasStatusEffect(PlayerStatusEffect.STUNNED)
         || status.HasStatusEffect(PlayerStatusEffect.PLANTED)
         || status.HasStatusEffect(PlayerStatusEffect.DEAD) 
         || status.HasStatusEffect(PlayerStatusEffect.HITPAUSE) 
         || status.HasStatusEffect(PlayerStatusEffect.GRABBED)
         || status.HasStatusEffect(PlayerStatusEffect.CRINGE)
        )
        {
            //cancelJump();
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;                       
        }

        //makes sure gavity is always reset after using a move
        //TODO make sure this is still necessary
        else if((!status.HasStatusEffect(PlayerStatusEffect.END_LAG) || !gravityPaused) && !ledgeHanging){
            rb.gravityScale = baseGravity;
        }

        //Saves previpus vleocity for resitution. REMOVE IF NOT NEEDED
        if(rb.velocity != Vector2.zero)prevVelocity = rb.velocity;

    }


    public void UpdateInput()
    {
        if (!GameController.Instance.IsHost || GameController.Instance.IsPaused)
            return;


        bool jumpPressed = !drifter.input[1].Jump && drifter.input[0].Jump;
        // TODO: spawn hitboxes
        bool canAct = !status.HasStunEffect() && !drifter.guarding;
        bool canGuard = !status.HasStunEffect() && !jumping;
        bool moving = drifter.input[0].MoveX != 0;
       
       //Platform dropthrough
        if(gameObject.layer != 8 && Time.time - dropThroughTime > framerateScalar *3)
            gameObject.layer = 8;
        

        ContactPoint2D[] contacts = new ContactPoint2D[1];
        bool groundFrictionPosition = frictionCollider.GetContacts(contacts) >0;

        if(!moving)accelerationPercent = .9f;
        
        //Normal walking logic
        if (moving && canAct && ! ledgeHanging)
        {

            updateFacing();
        	//UnityEngine.Debug.Log("BEFORE velocity: " + rb.velocity.x);

            //If just started moving or switched directions
            if((rb.velocity.x == 0 || rb.velocity.x * drifter.input[0].MoveX < 0) && IsGrounded()){

                if(groundFrictionPosition) spawnJuiceParticle(new Vector2(-Facing * (flipSprite?-1:1)* 1.5f,0) + contacts[0].point, MovementParticleMode.KickOff);
            }

            
            if(IsGrounded())
            {

                if(!jumping)
                {
                    drifter.PlayAnimation(drifter.WalkStateName);
                    status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0);
                    if(groundFrictionPosition)
                    {
                        if(walkTime > .2f + (30f -walkSpeed)/100f)
                        {
                            spawnJuiceParticle(new Vector2(-Facing * (flipSprite?-1:1)* 1.5f,0) + contacts[0].point, MovementParticleMode.WalkDust);
                            walkTime = 0;
                        }
                        else walkTime += Time.deltaTime;
                        

                    }

                }
                if(accelerationPercent > 0) accelerationPercent -= Time.deltaTime/groundAccelerationTime;
                else accelerationPercent = 0;

                currentSpeed = walkSpeed * (status.HasStatusEffect(PlayerStatusEffect.SLOWED) ? .6f: 1f) * (status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f) * (drifter.input[0].MoveX > 0 ? 1 : -1);

            }
            else
            {
                if(!jumping)drifter.PlayAnimation(drifter.AirIdleStateName);
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,0);

                if(accelerationPercent >0) accelerationPercent -= Time.deltaTime/airAccelerationTime;
                else accelerationPercent = 0;

                currentSpeed = airSpeed * (status.HasStatusEffect(PlayerStatusEffect.SLOWED) ? .6f: 1f) * (status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f) * (drifter.input[0].MoveX > 0 ? 1 : -1);

            	
            }
            rb.velocity = new Vector2(Mathf.Lerp(currentSpeed,rb.velocity.x,accelerationPercent), rb.velocity.y);

        }


        //Ledgegrabs Stuff
        else if(canAct && ledgeHanging)
        {
            rb.velocity = Vector2.zero;
            //Roll Onto Ledge
            if(drifter.input[0].Guard)
            {
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,framerateScalar * 2);
                drifter.PlayAnimation(drifter.LedgeRollStateName);
            }

            //Jump away from ledge
            else if((drifter.input[0].MoveX * (flipSprite?-1:1) * Facing < 0)){
                DropLedge();
                drifter.returnToIdle();

                rb.velocity = new Vector3(Facing * (flipSprite?-1:1) * -25f,25f);
            }
            
            //Neutral Getup
            else if((drifter.input[0].MoveX * (flipSprite?-1:1) * Facing > 0)  || drifter.input[0].MoveY > 0){
                DropLedge();
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,framerateScalar * 2);
                drifter.PlayAnimation(drifter.LedgeClimbStateName);

                rb.position = new Vector3(rb.position.x + (rb.position.x > 0 ? -1 :1) *2f, rb.position.y + 5f - ledgeClimbOffset);
            }

            //Drop down from ledge
            else if(drifter.input[0].MoveY < 0 && drifter.input[1].MoveY < 0 && ledgeHanging){
                DropLedge();
                drifter.returnToIdle();
            }

        }

        //Player is not trying to move, and is not in hitstun
        else if (!moving && status.HasGroundFriction())
        {
            //TODO Make sure this isnt eating inputs
            if(canAct && !jumping)drifter.returnToIdle();
            //standing ground friction (When button is not held)
            if(!grounded)rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 20f * Time.deltaTime), rb.velocity.y);
            else rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 80f * Time.deltaTime), rb.velocity.y);
        }


        //Slowdown on the ground
        else if(IsGrounded())
        {
            //Moving Ground Friction
            rb.velocity = new Vector2(Mathf.MoveTowards(rb.velocity.x, 0f, 40f * Time.deltaTime), rb.velocity.y);
        }

        //Drop through platforms
        if(drifter.doubleTappedY() && drifter.input[0].MoveY < 0)
        {
            gameObject.layer = 13;
            rb.velocity = new Vector2(rb.velocity.x,-terminalVelocity /2f);
            dropThroughTime = Time.time;
        }


        //Guard
        if(drifter.input[0].Guard && canGuard && !ledgeHanging && !status.HasStatusEffect(PlayerStatusEffect.GUARDBROKEN))
        {
            //shift is guard
            if(!drifter.guarding)drifter.PlayAnimation("Guard_Start");
            drifter.guarding = true;
            updateFacing();
        }
      
        //Disable Guarding
        else if(!drifter.input[0].Guard && !status.HasStunEffect() && drifter.guarding && !status.HasStatusEffect(PlayerStatusEffect.GUARDBROKEN))
        {
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,framerateScalar * 3);
            drifter.guarding = false;
            drifter.parrying = true;
            drifter.PlayAnimation("Guard_Drop");
        }

        //Terminal velocity

        if(rb.velocity.y < -terminalVelocity && !status.HasEnemyStunEffect()){
            rb.velocity = new Vector2(rb.velocity.x,-terminalVelocity);
        }

        //Jump
        if (jumpPressed && canAct)
        {
            jump();
        }


        else if(canAct && drifter.doubleTappedX())
        {
            dash();
        }

        //mashout effects
        if((status.HasStatusEffect(PlayerStatusEffect.PLANTED) || status.HasStatusEffect(PlayerStatusEffect.AMBERED) || status.HasStatusEffect(PlayerStatusEffect.PARALYZED) || status.HasStatusEffect(PlayerStatusEffect.GRABBED))&& drifter.input[1].MoveX != drifter.input[0].MoveX){
            status.mashOut();

            StartCoroutine(shake.Shake(.2f,.7f));

            spawnJuiceParticle( transform.position + particleOffset + new Vector3(.5f,UnityEngine.Random.Range(1f,3f),0), MovementParticleMode.Mash);
        }

        //Pause movement for relevent effects.
        
    }

    //Moves the character left or right, based on the speed provided
    public void move(float speed)
    {
        if(accelerationPercent >0) accelerationPercent -= Time.deltaTime/airAccelerationTime;
        else accelerationPercent = 0;

        updateFacing();

        if(drifter.input[0].MoveX != 0)
        {
            currentSpeed = speed * (status.HasStatusEffect(PlayerStatusEffect.SLOWED) ? .6f: 1f) * (status.HasStatusEffect(PlayerStatusEffect.SPEEDUP) ? 1.5f: 1f) * (drifter.input[0].MoveX > 0 ? 1 : -1);
            rb.velocity = new Vector2(Mathf.Lerp(currentSpeed,rb.velocity.x,accelerationPercent), rb.velocity.y);
        }
        
    }
    

    //Made it public for treamlining channeled attack cancels
    public void techParticle()
    {
        spawnJuiceParticle(BodyCollider.bounds.center, MovementParticleMode.Tech, Quaternion.Euler(0f,0f,0f),false);
    }

    //Updates the direction the player is facing
    public void updateFacing()
    {

        if(Facing != drifter.input[0].MoveX)accelerationPercent =.9f;
        if(flipSprite ^ drifter.input[0].MoveX > 0)
            Facing = 1;
        
        else if(flipSprite ^ drifter.input[0].MoveX < 0)
            Facing = -1;


        attacks.Facing = Facing * (flipSprite?-1:1);
        drifter.SetIndicatorDirection(Facing);
        transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),
        transform.localScale.y, transform.localScale.z);
    }

    //Used to forcibly invert the players direction
    //TODO This seems like it could be lumped in somewhere else
    public void flipFacing(){
        Facing *= -1;
        drifter.SetIndicatorDirection(Facing);
        transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y, transform.localScale.z);
    }

    public void setFacing(int dir){
        Facing = dir;
        drifter.SetIndicatorDirection(Facing);
        transform.localScale = new Vector3(Facing * Mathf.Abs(transform.localScale.x),transform.localScale.y, transform.localScale.z);
    }


    //Kills jump coroutines if they exist, for paused gravity attacks
    public void cancelJump()
    {
        if(jumpCoroutine!= null)StopCoroutine(jumpCoroutine);
        if(varyJumpHeight!= null)StopCoroutine(varyJumpHeight);
    }

    public void updatePosition (Vector3 position){
      transform.position = position;
    }

    RaycastHit2D[] hits = new RaycastHit2D[10];
    private bool IsGrounded()
    {
        int count = Physics2D.RaycastNonAlloc(frictionCollider.bounds.center + frictionCollider.bounds.extents.y * Vector3.down, Vector3.down, hits, 0.2f);

        for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform" && status.HasGroundFriction())) return rb.velocity.y <=.1f;

        return false;
    }

    RaycastHit2D[] wallHits = new RaycastHit2D[10];
    private Vector3 IsWallSliding()
    {
        int count = Physics2D.RaycastNonAlloc(BodyCollider.bounds.center + new Vector3( BodyCollider.bounds.extents.x * (( Facing > 0)^flipSprite?1:-1),BodyCollider.bounds.extents.y,0), ((Facing > 0)^flipSprite?Vector3.right:Vector3.left),wallHits, 0.35f);

        for (int i = 0; i < count; i++)if (wallHits[i].collider.gameObject.tag == "Ground" && status.HasGroundFriction())return wallHits[i].normal;

        return Vector3.zero;
    }

    //Sets many movement flags to specific vlaues to allow for ledge hanging
    public void GrabLedge(Vector3 pos)
    {
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,framerateScalar * 2);
        cancelJump();
        gravityPaused = false;
        jumping = false;
        attacks.ledgeHanging = true;
        drifter.clearGuardFlags();
        ledgeHanging = true;
        rb.gravityScale = 0f;
        if(strongLedgeGrab)drifter.PlayAnimation(drifter.StrongLedgeGrabStateName);
        else drifter.PlayAnimation(drifter.WeakLedgeGrabStateName);

        setFacing(flipSprite ^ rb.position.x > 0 ? -1 :1);

        rb.position = new Vector3(pos.x - (rb.position.x > 0 ? -1 :1) *1.5f, pos.y - 1.75f - ledgeOffset,pos.z);
 
        attacks.resetRecovery();      
        
        currentJumps = numberOfJumps;

        rb.velocity = Vector2.zero;
    }

    //Manages all the things that need to happen when a ledge is released
    public void DropLedge(){
        ledgeHanging = false;
        rb.gravityScale = baseGravity;
        strongLedgeGrab = false;
        attacks.ledgeHanging = false;
    }

    //Wrapper for spawning particles at the character's feet
    public void spawnKickoffDust()
    {
        ContactPoint2D[] contacts = new ContactPoint2D[1];
        bool groundFrictionPosition = frictionCollider.GetContacts(contacts) >0;
        
        if(groundFrictionPosition) spawnJuiceParticle(new Vector2(-Facing * (flipSprite?-1:1)* 1.5f,0) + contacts[0].point, MovementParticleMode.KickOff);
    }


    //Public jump method allows for forced jumps from attacks
    public void jump()
    {
        jumping = true;
        if(ledgeHanging)DropLedge();
            //jump
            if (currentJumps > 0)
            {
                gravityPaused = false;
                currentJumps--;
                if(!grounded)drifter.PlayAnimation("Air_Jump_Start");
                else drifter.PlayAnimation(drifter.JumpStartStateName);
                //Particles
                if(IsGrounded()){
                    spawnJuiceParticle(transform.position + particleOffset + new Vector3(0,-1,0), MovementParticleMode.Jump);
                }
                else{
                    spawnJuiceParticle(transform.position + particleOffset +new Vector3(0,-1,0), MovementParticleMode.DoubleJump);
                }
                //jump needs a little delay so character animations can spend
                //a frame of two preparing to jump
                jumpCoroutine = StartCoroutine(DelayedJump());
            }
    }

    public bool dash()
    {
        if(currentJumps > 0 && dashLock <=0)
        {
            accelerationPercent = 0;
            dashLock = .5f;
            spawnJuiceParticle(BodyCollider.bounds.center + new Vector3(Facing * (flipSprite?-1:1)* 1.5f,0), MovementParticleMode.Dash_Ring, Quaternion.Euler(0f,0f,0f), false);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,4);
            drifter.PlayAnimation("Dash");
            jumping = false;
            currentJumps--;
            return true;
        }
        return false;
    }

    //Public wrapper for movement particle spawning
    public void spawnJuiceParticle(Vector3 pos, MovementParticleMode mode)
    {
        spawnJuiceParticle(pos, mode, transform.rotation, false);
    }

    public void spawnJuiceParticle(Vector3 pos, MovementParticleMode mode, bool flip)
    {
         spawnJuiceParticle(pos, mode, transform.rotation, flip);
    }

    //Creates a movement particle at the designated location
    private void spawnJuiceParticle(Vector3 pos, MovementParticleMode mode, Quaternion angle, bool flip){

        particleOffset = new Vector3(particleOffset.x * Facing * (flipSprite?-1:1),particleOffset.y,0);
    	GraphicalEffectManager.Instance.CreateMovementParticle(mode, pos, angle.eulerAngles.z, new Vector2(Facing * (flipSprite ? -1 : 1) * (flip ? -1 : 1), 1));
    }


    public void superCancel()
    {

        if(!GameController.Instance.IsHost || drifter.superCharge < 1f || status.HasStatusEffect(PlayerStatusEffect.DEAD) || !drifter.canSuper)return;

        //Hyperguard
        if(status.HasStatusEffect(PlayerStatusEffect.KNOCKBACK) && drifter.guarding && !drifter.guardBreaking  && drifter.superCharge > 1f)
        {
            animator.enabled = true;
            hitstun = false;
            status.clearStunStatus();

            spawnSuperParticle("Hyper_Guard_Burst",1f,8f);
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,10f * framerateScalar);
            drifter.PlayAnimation("Burst");
             //status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,3f * framerateScalar);
        }
        
        //Offensive Cancel
        else if(status.HasStatusEffect(PlayerStatusEffect.END_LAG) && drifter.superCharge > 1f)
        {
            if(drifter.superCharge > 2f && !drifter.canFeint)
            {
                spawnSuperParticle("Offensive_Cancel",2f,20f);
                drifter.PlayAnimation("Burst");
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,10f * framerateScalar);
            }
            else if(drifter.canFeint)
            {
                spawnSuperParticle("Feint_Cancel",1f,8f);
                drifter.PlayAnimation("Burst");
                status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,10f * framerateScalar);
            }
            
        }

        //Burst/Defensive Cancel
        else if(!drifter.guarding && drifter.superCharge > 2f && status.HasEnemyStunEffect() && !status.HasStatusEffect(PlayerStatusEffect.GRABBED))
        {
            animator.enabled = true;
            hitstun = false;
            status.clearStunStatus();
            status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,10f * framerateScalar);

            spawnSuperParticle("Defensive_Cancel",2f,8f);
            if(currentJumps+1 < numberOfJumps) currentJumps++;
            drifter.PlayAnimation("Burst");
            //status.ApplyStatusEffect(PlayerStatusEffect.HITPAUSE,3f * framerateScalar);
        }

    }

    private void spawnSuperParticle(string mode,float cost,float darkentime)
    {

        StartCoroutine(mainCamera.darkenScreen(darkentime * framerateScalar));
        drifter.canSuper = false;
        attacks.SetupAttackID(DrifterAttackType.Super_Cancel);
        Vector3 flip = new Vector3(Facing * 10f, 10f, 0f);
        //Vector3 pos = new Vector3(Facing * 3f, 3.5f, 1f);
        
        drifter.superCharge -= cost;

        GameObject cancel = GameController.Instance.host.CreateNetworkObject("SuperEffect", transform.position , transform.rotation);
        foreach (HitboxCollision hitbox in cancel.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = Facing;
        }
        cancel.GetComponent<SyncAnimatorStateHost>().SetState(mode);
        
    }

    //delays jump to allow for jump squant and move queuing
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

    //Varries the jup heing based on how long the button is held
    private IEnumerator VaryJumpHeight()
    {
        float time = 0f;
        while (time < varyJumpHeightDuration)
        {
            yield return new WaitForFixedUpdate();
            time += Time.fixedDeltaTime;
            if (!status.HasStunEffect() && drifter.input[0].Jump)
            {
                //rb.AddForce(Vector2.up * -Physics2D.gravity * varyJumpHeightForce);
                rb.velocity = new Vector2(rb.velocity.x, jumpSpeed);
            }
        }
        varyJumpHeight = null;
    }
}
