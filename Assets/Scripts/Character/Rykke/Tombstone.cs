using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tombstone : NonplayerHurtboxHandler
{

	public int Uses = 3;
	public Rigidbody2D rb;
	public int tombstoneType = 0;
	public bool canAct = false;
	public bool active = false;
	public PlayerAttacks attacks;
	public int facing;

	public Collider2D physicsCollider; 

	SyncAnimatorStateHost anim;

	public bool listeningForGrounded = false;


	bool isHost;

	// Start is called before the first frame update
    void Awake()
    {
    	isHost = GameController.Instance.IsHost;
    	if(!isHost)return;
    	rb = GetComponent<Rigidbody2D>();
    	anim = GetComponent<SyncAnimatorStateHost>();
    	
    }

    // // Update is called once per frame
    void Update()
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
    	canAct = true;
    	if(active)
			anim.SetState("Active_Idle");
		else
			anim.SetState(tombstoneType + "_Idle");

    }

    //Refreshes the tombstone's hitboxes so it can multihit
    public void multihit()
    {
        if(!isHost)return;
        attacks.SetMultiHitAttackID();
    }

    public void listenForGrounded()
    {
    	if(!isHost)return;
    	listeningForGrounded = true;
    }


    private bool IsGrounded()
    {
    	RaycastHit2D[] hits = new RaycastHit2D[10];
        int count = Physics2D.RaycastNonAlloc(physicsCollider.bounds.center + physicsCollider.bounds.extents.y * Vector3.down, Vector3.down, hits, 0.2f);

        for (int i = 0; i < count; i++) if (hits[i].collider.gameObject.tag == "Ground" || (hits[i].collider.gameObject.tag == "Platform")) return true;

        return false;
    }


}