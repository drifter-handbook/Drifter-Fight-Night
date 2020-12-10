using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;


public class LucillePortal : MonoBehaviour
{

	public GameObject drifter;

	void OnTriggerEnter2D(Collider2D collider)
	{
		if(!GameController.Instance.IsHost)return;



		HitboxCollision hitbox = collider.gameObject.GetComponent<HitboxCollision>();

		try
		{
			if(hitbox != null && hitbox.parent == drifter && collider.gameObject.tag == "Lucille_Portal_Contact")
			{
				GetComponent<Rigidbody2D>().velocity = new Vector3(hitbox.Facing * ((hitbox.OverrideData.AngleOfImpact < 45f && hitbox.OverrideData.AngleOfImpact > -30f)?35f:0f),(hitbox.OverrideData.AngleOfImpact > 45f ?35f:(hitbox.OverrideData.AngleOfImpact > 20?0:-35f )),0);


				if((hitbox.OverrideData.AngleOfImpact > 45f && hitbox.OverrideData.AngleOfImpact < 135f))GetComponent<SyncAnimatorStateHost>().SetState("Move_up");
				else if(hitbox.Facing > 0)GetComponent<SyncAnimatorStateHost>().SetState("Move_Right");
				else GetComponent<SyncAnimatorStateHost>().SetState("Move_Left");

				foreach (HitboxCollision portalHitbox in GetComponentsInChildren<HitboxCollision>(true))
				{
					portalHitbox.AttackID -= 3;
					portalHitbox.Facing = hitbox.Facing;
				}

				GraphicalEffectManager.Instance.CreateHitSparks(HitSpark.LUCILLE,  Vector3.Lerp(transform.position, hitbox.parent.transform.position, 0.1f), 0, new Vector2(6f, 6f));
			}

		}

		catch(NullReferenceException)
		{
			return;
		}
	}

	public void playState(string state)
	{
		if(!GameController.Instance.IsHost)return;
		GetComponent<SyncAnimatorStateHost>().SetState(state);

	}

}
