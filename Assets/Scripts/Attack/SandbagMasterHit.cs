using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandbagMasterHit : MasterHit
{
	bool dust = false;
	InstantiatedEntityCleanup sandblast;
	InstantiatedEntityCleanup sandspear1;
	InstantiatedEntityCleanup sandspear2;

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

	public void Neutral_Special()
	{

		if(g_Sandblast!= null) Destroy(g_Sandblast);
		g_Sandblast = GameController.Instance.CreatePrefab("Sandblast", transform.position + new Vector3(1.5f * movement.Facing, 2.5f), transform.rotation);
		g_Sandblast.transform.localScale = new Vector3(10f * movement.Facing, 10f , 1f);
        foreach (HitboxCollision hitbox in g_Sandblast.GetComponentsInChildren<HitboxCollision>(true))
        {
            hitbox.parent = drifter.gameObject;
            hitbox.AttackID = attacks.AttackID;
            hitbox.Facing = movement.Facing;
        }
		g_Sandblast.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);
		g_Sandblast.GetComponent<Rigidbody2D>().velocity = new Vector3(movement.Facing * 25f,0,0);
		sandblast = g_Sandblast.GetComponent<InstantiatedEntityCleanup>();
	}

	public void Ground_Down()
	{

		GameObject g_Sandspear1 = GameController.Instance.CreatePrefab("Sandspear", transform.position + new Vector3(1.2f * movement.Facing, 1.3f,1), transform.rotation);
		GameObject g_Sandspear2 = GameController.Instance.CreatePrefab("Sandspear", transform.position + new Vector3(-1.5f * movement.Facing, 1.3f,-1), transform.rotation);
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
		g_Sandspear1.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);
		g_Sandspear1.GetComponent<SpriteRenderer>().material.SetColor(Shader.PropertyToID("_OutlineColor"),CharacterMenu.ColorFromEnum[(PlayerColor)drifter.GetColor()]);
		g_Sandspear1.transform.SetParent(drifter.gameObject.transform);
		g_Sandspear2.transform.SetParent(drifter.gameObject.transform);

		sandspear1 = g_Sandspear1.GetComponent<InstantiatedEntityCleanup>();
		sandspear2 = g_Sandspear2.GetComponent<InstantiatedEntityCleanup>();
	}

	//Rollback
	//=========================================

	//Takes a snapshot of the current frame to rollback to
    public override MasterhitRollbackFrame SerializeFrame()
    {
    	MasterhitRollbackFrame baseFrame = SerializeBaseFrame();
    	baseFrame.CharacterFrame= new SandbagRollbackFrame() 
        {
            Sandblast = g_Sandblast != null ? sandblast.SerializeFrame(): null,
			Sandspear1 = g_Sandspear1 != null ? sandspear1.SerializeFrame(): null,
			Sandspear2 = g_Sandspear2 != null ? sandspear2.SerializeFrame(): null,     
        };

        return baseFrame;
    }

    //Rolls back the entity to a given frame state
    public override void DeserializeFrame(MasterhitRollbackFrame p_frame)
    {
    	DeserializeBaseFrame(p_frame);

    	SandbagRollbackFrame sb_frame = (SandbagRollbackFrame)p_frame.CharacterFrame;
    	// if(sb_frame.Sandblast == null)
    	// {
    	// 	Destroy(g_Sandblast);
    	// 	sandblast = null;
    	// }
    	// else if(g_Sandblast == null)
    	// {

    	// }

    }

}

public class SandbagRollbackFrame: ICharacterRollbackFrame
{
	public string Type { get; set; }
    
	public BasicProjectileRollbackFrame Sandblast;
	public BasicProjectileRollbackFrame Sandspear1;
	public BasicProjectileRollbackFrame Sandspear2;
	
}
