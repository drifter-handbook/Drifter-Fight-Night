using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeetHeal : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collider)
	{
		if(!GameController.Instance.IsHost)return;

		float damage = collider.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>().DamageTaken;

		collider.gameObject.GetComponent<HurtboxCollision>().parent.GetComponent<Drifter>().DamageTaken = Mathf.Max(0f,damage - 6f);

		for(int i = 0; i < 5;i++)
    	{
    		GraphicalEffectManager.Instance.CreateMovementParticle(MovementParticleMode.Heal, collider.transform.position + new Vector3(UnityEngine.Random.Range(2f,-2f), UnityEngine.Random.Range(0f,5f)), 0, new Vector2(1, 1));
    	}

    	Destroy(gameObject.transform.parent.gameObject);

	}
}
