using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandbagMasterHit : MasterHit
{
	bool dust = false;

	GameObject g_Sandblast;
	GameObject g_Sandspear1;
	GameObject g_Sandspear2;

	int dustCount = 7;

	override public void UpdateFrame()
    {
        base.UpdateFrame();

        if(status.HasEnemyStunEffect() || movement.ledgeHanging)
			dust = false;
		else if(dust)
		{
			if(dustCount > 3f)
			{
				dustCount = 0;
				movement.spawnJuiceParticle(transform.position + new Vector3(movement.Facing * -1.4f,2.7f,0), MovementParticleMode.SmokeTrail);
			}
        	dustCount +=1;
		}

		if(g_Sandblast != null) g_Sandblast.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
		if(g_Sandspear1 != null) g_Sandspear1.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();
		if(g_Sandspear2 != null) g_Sandspear2.GetComponent<InstantiatedEntityCleanup>().UpdateFrame();

    }

    //Particle system
	public void Setdust()
	{
		dust = true;
		
		GameObject ring = GameController.Instance.CreatePrefab("LaunchRing", transform.position,  transform.rotation);
		ring.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		dustCount = 3;
	}
	public void disableDust()
	{
		dust = false;
	}

	public new void returnToIdle()
    {
        base.returnToIdle();
        dust = false;
    }

	public override void clearMasterhitVars()
	{
		base.clearMasterhitVars();
		dust = false;
	}

	public void clear()
	{
		Destroy(g_Sandspear1);
		Destroy(g_Sandspear2);
		g_Sandspear1 = null;
		g_Sandspear2 = null;
		dust = false;
	}

	public void Neutral_Special()
	{

		if(g_Sandblast!= null) Destroy(g_Sandblast);
		CreateSandblast();
	}

	void CreateSandblast()
	{
		g_Sandblast = GameController.Instance.CreatePrefab("Sandblast", transform.position + new Vector3(1.5f * movement.Facing, 2.5f), transform.rotation);
		g_Sandblast.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in g_Sandblast.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }

        SetObjectColor(g_Sandblast);
		g_Sandblast.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 25f,0,0);
	}

	public void Ground_Down()
	{

		CreateSandSpears();

		//sandspear1 = g_Sandspear1.GetComponent<InstantiatedEntityCleanup>();
		//sandspear2 = g_Sandspear2.GetComponent<InstantiatedEntityCleanup>();
	}

	void CreateSandSpears()
	{
		g_Sandspear1 = GameController.Instance.CreatePrefab("Sandspear", transform.position + new Vector3(1.2f * movement.Facing, 1.3f,1), transform.rotation);
		g_Sandspear2 = GameController.Instance.CreatePrefab("Sandspear", transform.position + new Vector3(-1.5f * movement.Facing, 1.3f,-1), transform.rotation);
		g_Sandspear1.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
		g_Sandspear2.transform.localScale = new Vector3(-10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in g_Sandspear1.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }
        foreach (HitboxCollision hitbox in g_Sandspear2.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }
		SetObjectColor(g_Sandspear1);
		SetObjectColor(g_Sandspear2);
		g_Sandspear1.transform.SetParent(drifter.gameObject.transform);
		g_Sandspear2.transform.SetParent(drifter.gameObject.transform);
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
    	MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
    	baseFrame.CharacterFrame = new SandbagRollbackFrame() 
        {
            Sandblast = g_Sandblast != null ? g_Sandblast.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			Sandspear1 = g_Sandspear1 != null ? g_Sandspear1.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,
			Sandspear2 = g_Sandspear2 != null ? g_Sandspear2.GetComponent<InstantiatedEntityCleanup>().SerializeFrame(): null,     
        };

        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
    	DeserializeBaseFrame(p_frame);

    	SandbagRollbackFrame sb_frame = (SandbagRollbackFrame)p_frame.CharacterFrame;

    	//Sandblast reset
    	if(sb_frame.Sandblast != null)
    	{
    		if(g_Sandblast == null)CreateSandblast();
    		g_Sandblast.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(sb_frame.Sandblast);
    	}
    	//Projectile does not exist in rollback frame
    	else
    	{
    		Destroy(g_Sandblast);
    		g_Sandblast = null;
    	}  

    	//Sandspears reset
    	if(sb_frame.Sandspear1 != null)
    	{
    		if(g_Sandspear1 == null)CreateSandSpears();
    		g_Sandspear1.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(sb_frame.Sandspear1);
    		g_Sandspear2.GetComponent<InstantiatedEntityCleanup>().DeserializeFrame(sb_frame.Sandspear2);

    	}
    	//Projectile does not exist in rollback frame
    	else
    	{
    		Destroy(g_Sandspear1);
    		Destroy(g_Sandspear2);
    		g_Sandspear1 = null;
    		g_Sandspear2 = null;
    	}  

    }

}

public class SandbagRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
    
	public BasicProjectileRollbackFrame Sandblast;
	public BasicProjectileRollbackFrame Sandspear1;
	public BasicProjectileRollbackFrame Sandspear2;
	
}
