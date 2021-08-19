using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tombstone : NonplayerHurtboxHandler
{

	public int Uses = 3;
	public Rigidbody2D rb;
	public int tombstoneType = 0;

	// Start is called before the first frame update
    void Awake()
    {
    	rb = GetComponent<Rigidbody2D>();
    }

    // // Update is called once per frame
    void Update()
    {
    	if(Uses <=0)Destroy(gameObject);
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


}