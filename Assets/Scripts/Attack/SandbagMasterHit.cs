using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandbagMasterHit : MasterHit
{
	bool rolling = false;

	void Update()
	{
		if(status.HasEnemyStunEffect() || movement.ledgeHanging)
			rolling = false;
		else if(rolling && movement.grounded)
		{
			rb.velocity = new Vector2(movement.Facing * 33f * (status.HasStatusEffect(PlayerStatusEffect.SLOWMOTION) ? .4f : 1f),rb.velocity.y);
        	status.saveXVelocity(movement.Facing *33f);
		}
	}

	public void SetRolling()
	{
		rolling = true;
	}

	new public void clearMasterhitVars()
	{
		base.clearMasterhitVars();
		rolling = false;
	}

}
