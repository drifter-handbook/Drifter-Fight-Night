using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeMasterHit : MasterHit
{

	public SyncAnimatorStateHost sparkle;


	//0 Up stone
	//1 Side Stone
	//2 Down Stone
	Tombstone[] tombstones = new Tombstone[] {null,null,null};
	int tombstoneIndex = 0;
	int nearbyStone = -1;

	Vector2 stoneOffset = new Vector2(0,2);

	new void Update()
    {
        if(!isHost)return;

        base.Update();
        if(status.HasStatusEffect(PlayerStatusEffect.DEAD))
        {
       		for(int i = 0; i <3; i++)
       			Destroy(tombstones[i]);
        }

        isNearStone();
    }


	//Creates a tombstone projectile
    public void SpawnTombstone()
    {
        if(!isHost)return;
        facing = movement.Facing;

        bool stonesFull = true;
        
        GameObject stone = host.CreateNetworkObject("Tombstone", transform.position, transform.rotation);
        stone.transform.localScale = new Vector3(10f * facing, 10f , 1f);
        foreach (HitboxCollision hitbox in stone.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.AttackType = attacks.AttackType;
            hitbox.Active = true;
            hitbox.Facing = facing;
       }

       foreach (HurtboxCollision hurtbox in stone.GetComponentsInChildren<HurtboxCollision>(true))
            hurtbox.owner = drifter.gameObject;

       for(int i = 0; i <3; i++)
       {
       		if(tombstones[i] == null)
       		{
       			tombstones[i] = stone.GetComponent<Tombstone>();
       			stone.GetComponent<SyncAnimatorStateHost>().SetState(i + "_Idle");
       			tombstoneIndex = i;
       			tombstones[i].tombstoneType = i;
       			i = 3;
       			stonesFull = false;
       		}
       }

       if(stonesFull)
       {
       		tombstoneIndex++;
       		if(tombstoneIndex >2)tombstoneIndex = 0;
       		stone.GetComponent<SyncAnimatorStateHost>().SetState(tombstoneIndex + "_Idle");
       		Destroy(tombstones[tombstoneIndex].gameObject);
       		tombstones[tombstoneIndex] = stone.GetComponent<Tombstone>();
       		tombstones[tombstoneIndex].tombstoneType = tombstoneIndex;
       }

       stone.GetComponent<SyncProjectileColorDataHost>().setColor(drifter.GetColor());
       
    }

    public void decrementStoneUses()
    {
    	if(Empowered && nearbyStone >=0)tombstones[nearbyStone].Uses--;
    }


    void isNearStone()
    {
    	for(int i = 0; i <3; i++)
    	{
        	if(tombstones[i] != null && Vector3.Distance(rb.position,(tombstones[i].rb.position + stoneOffset)) < 4.5f)
        	{
        		sparkle.SetState("ChargeIndicator");
        		nearbyStone = i;
        		Empowered= true;
        		return;
        	}
    	}
    	Empowered = false;
    	nearbyStone = -1;
    	sparkle.SetState("Hide");
    }

    //Inhereted Roll Methods

    public override void roll()
    {
        if(!isHost)return;
        facing = movement.Facing;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.6f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 40f,0f);
    }


    public override void rollGetupStart()
    {
        if(!isHost)return;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.5f);
        rb.velocity = new Vector3(0,75f,0);
    }


    public override void rollGetupEnd()
    {
        if(!isHost)return;
        facing = movement.Facing;
        movement.gravityPaused = false;
        rb.gravityScale = gravityScale;
        status.ApplyStatusEffect(PlayerStatusEffect.END_LAG,.42f);
        status.ApplyStatusEffect(PlayerStatusEffect.INVULN,.3f);
        rb.velocity = new Vector2(facing * 25f,5f);
    }

}
