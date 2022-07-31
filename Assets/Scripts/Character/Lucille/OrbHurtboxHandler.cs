using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrbHurtboxHandler : NonplayerHurtboxHandler
{

	Collider2D grabPoint;
	public Vector3 direction;

    public override void UpdateFrame()
    {
        base.UpdateFrame();

        if(grabPoint !=null && grabPoint.enabled)
            transform.position = grabPoint.bounds.center;
     
        else if (grabPoint !=null && !grabPoint.enabled)
        {
            grabPoint = null;
            if(direction != Vector3.zero)
            {
                foreach (HitboxCollision hitbox in GetComponentsInChildren<HitboxCollision>(true))
                    hitbox.Facing = direction.x != 0 ? (int)direction.x : hitbox.Facing;

                rb.velocity = Vector3.Normalize(direction) * 45;
                direction = Vector3.zero;
            }
        }
        else
        {
            grabPoint = null;
        }

    }

	public void setDirection(Vector3 p_dir)
	{
		direction = p_dir;
	}

	public override int RegisterAttackHit(HitboxCollision hitbox, HurtboxCollision hurtbox, int attackID, SingleAttackData attackData)
    {

        if (GameController.Instance.IsHost && hurtbox.owner == hitbox.parent && CanHit(attackID) && hitbox.gameObject.tag == "Lucille_Portal_Grab")
        {
        	GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
        	grabPoint = hitbox.gameObject.GetComponent<Collider2D>();
            oldAttacks[attackID] = MAX_ATTACK_DURATION;
        	return 1; 

        }

        return -3;
    }
}

