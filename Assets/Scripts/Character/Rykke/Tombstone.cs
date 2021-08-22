using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tombstone : NonplayerHurtboxHandler
{

	public int Uses = 3;
	
	public int tombstoneType = 0;
	public bool canAct = false;
	public bool active = false;
	public PlayerAttacks attacks;
	public int facing;


	public WalkOff ledgeDetector;

	bool isHost;

	Collider2D physicsCollider; 

	Rigidbody2D rb;

	SyncAnimatorStateHost anim;

	bool listeningForGrounded = false;
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
    	physicsCollider = GetComponent<PolygonCollider2D>();
    	
    }

    // // Update is called once per frame
    void FixedUpdate()
    {
    	if(!isHost)return;
    	if(Uses <=0 && canAct)Destroy(gameObject);

    	if(listeningForGrounded && IsGrounded())
    	{
    		listeningForGrounded = false;
    		anim.SetState("Land");
    		canAct = false;

    	}
    }

    //Registers a hit on bean, and handles his counter.
    //If bean has taken over 40%, he becomes inactive untill he can heal
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

    public void updateDirection(int p_facing)
    {
    	if(!isHost)return;
    	facing = p_facing;
    	transform.localScale = new Vector3(facing * Mathf.Abs(transform.localScale.x),
        transform.localScale.y, transform.localScale.z);

    }

    public void returnToIdle()
    {
    	if(!isHost)return;

    	if(active && distanceFromParent < 4.5f)
			anim.SetState("Active_Idle");
	
		else if(!canAct && !active)
			anim.SetState("Deactivate");
		else
			anim.SetState(tombstoneType + "_Idle");

		//else
			
		listeningForGrounded = false;
		ledgeDetector.setPreventWalkoff(false);
		canAct = true;

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

        for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform")) return true;

        return false;
    }


}