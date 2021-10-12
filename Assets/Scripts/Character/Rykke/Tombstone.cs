using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tombstone : NonplayerHurtboxHandler
{

	public int Uses = 3;
	
	
	public bool canAct = false;
	public bool active = false;
	public int facing;
	public WalkOff ledgeDetector;

	PlayerAttacks attacks;
	int tombstoneType = 0;
	
	GameObject drifter;
	bool projectile = true;

	float zombieRadius = 4.5f;

	bool isHost;

	Collider2D physicsCollider; 

	Rigidbody2D rb;

	SyncAnimatorStateHost anim;

	bool listeningForGrounded = true;
	float distanceFromParent = 0;

	//Const Vector offset
	Vector2 offset = new Vector2(0,2);

	// Start is called before the first frame update
    void Awake()
    {
    	isHost = GameController.Instance.IsHost;
    	if(!isHost)return;
    	rb = GetComponent<Rigidbody2D>();
    	anim = GetComponent<SyncAnimatorStateHost>();
    	physicsCollider = GetComponentInChildren<PolygonCollider2D>();
    }

    // // Update is called once per frame
    void FixedUpdate()
    {
    	if(!isHost)return;
    	if(Uses <=0 && canAct)Destroy(gameObject);

    	if(listeningForGrounded && IsGrounded())
    	{
    		listeningForGrounded = false;
    		if(projectile) returnToIdle();
    		else 
    		{
    			anim.SetState("Land");
    			canAct = false;
    		}

    	}
    }

    new void Start()
    {
    	base.Start();
    	playAnimationEvent(tombstoneType + "_Spin");
    }

    //sets necessary fields to make spawning cleaner
    public Tombstone setup(int p_tombstoneIndex, int p_facing,PlayerAttacks p_attacks,GameObject p_drifter,float p_radius)
    {
       tombstoneType = p_tombstoneIndex;
       facing = p_facing;
       attacks = p_attacks;
       drifter = p_drifter;
       zombieRadius = p_radius;
       return this;
    }

    //Registers a hit on bean, and handles his counter.
    //
    public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, DrifterAttackType attackType, SingleAttackData attackData)
    {

        int returnCode = -3;

        if(takesKnockback)takesKnockback = false;

        if(GameController.Instance.IsHost && hitbox.parent != hurtbox.parent && hurtbox.owner != hitbox.parent && !oldAttacks.ContainsKey(attackID))
        {
   			returnCode =  base.RegisterAttackHit(hitbox,hurtbox,attackID,attackType,attackData);

            if(returnCode >= 0)
              	Uses--;

            if(Uses <=0)Destroy(gameObject);
            
        }

        return returnCode;

    }

    void OnTriggerEnter2D(Collider2D collider)
	{
		if(!GameController.Instance.IsHost)return;

		if(collider.gameObject.tag == "BounceObject" && collider.GetComponent<Tombstone>().drifter == drifter)
			if(projectile)
            {
                rb.velocity = new Vector3(facing * -10f,20f);
                foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
                    hitbox.Facing = -facing;
            }
			//else if(!active && ! canAct)rb.velocity = new Vector3(collider.gameObject.transform.position.x > transform.position.x ? -5f:5f,0);
	}	
			

    public void playAnimation(string state, bool actionable = false, bool gated = false)
    {
    	if(!isHost || (!canAct && gated))return;
    	canAct = actionable;
    	anim.SetState(state);

    }

    public void playAnimationEvent(string state)
    {
    	if(!isHost)return;
    	canAct = false;
    	anim.SetState(state);

    }

    //Sets the "Active" flag
    public void activate()
    {
    	if(!isHost)return;
    	active = true;
    }


    //Launches stone with set velocity from an external object
    public void throwStone(int mode)
    {
    	if(!isHost)return;
    	//canAct = false;
    	switch(mode)
    	{
    		case 0:
    			rb.velocity = new Vector3(facing *5f,-40f);
    			break;
    		case 1:
    			rb.velocity = new Vector3(facing * 30f,20f);
    			break;
    		case 2:
    			rb.velocity = new Vector3(0,45f);
    			break;
    		default:
    			break;
    	}
    	

    }

    //Switches the direction the object is facing when recieving a command;
    public void updateDirection(int p_facing)
    {
    	if(!isHost)return;
    	facing = p_facing;
    	transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
        transform.localScale.y, transform.localScale.z);

    }

    //Returns to the idle state and resets most flags
    public void returnToIdle()
    {
    	if(!isHost)return;

    	if(projectile)
    		anim.SetState(tombstoneType + "_Idle");
    	else if(active && distanceFromParent < zombieRadius)
			anim.SetState(IsGrounded() ? "Active_Idle" : "Hang");
		else if(!canAct && !active)
			anim.SetState(IsGrounded() ? "Deactivate" : "Hang");
		else
			anim.SetState(tombstoneType + "_Idle");

		//else
			
		listeningForGrounded = false;
		ledgeDetector.setPreventWalkoff(false);
		canAct = true;
		projectile = false;

    }

    public void listenForGrounded()
    {
    	if(!isHost)return;
    	listeningForGrounded = true;
    }

    public void listenForLedge()
    {
    	if(!isHost)return;

    	ledgeDetector.togglePreventWalkoff();
    }
    
    public float getDistance(Vector3 parent)
    {
    	if(!isHost) return 99;

    	distanceFromParent = Vector3.Distance(parent,rb.position + offset);

    	return distanceFromParent;
    }


    private bool IsGrounded()
    {
    	RaycastHit2D[] hits = new RaycastHit2D[10];
        int count = Physics2D.RaycastNonAlloc(physicsCollider.bounds.center + physicsCollider.bounds.extents.y * Vector3.down, Vector3.down, hits, 0.2f);

        for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform")) return rb.velocity.y <=.1f;

        return false;
    }


}