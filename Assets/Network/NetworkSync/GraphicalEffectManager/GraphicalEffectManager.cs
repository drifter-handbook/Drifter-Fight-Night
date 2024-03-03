using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GraphicalEffectType
{
	HitSpark, MovementParticle
}

public class GraphicalEffectManager : MonoBehaviour
{
	public static GraphicalEffectManager Instance => GameObject.FindGameObjectWithTag("GraphicalEffectManager").GetComponent<GraphicalEffectManager>();

	public GameObject hitSparksPrefab;
	public GameObject movementParticlePrefab;
	public GameObject specialCancelEffet;
	public GameObject movementCancelEffect;

	public void CreateMovementCancel(GameObject source) {
		var parent = source.transform.Find("Sprite");
		var sprite = parent.GetComponent<SpriteRenderer>();

		var fx = Instantiate(movementCancelEffect, parent);
		fx.GetComponent<AfterimageSpawner>().Init(sprite);
	}

	public void CreateSpecialCancel(GameObject source) {
		var parent = source.transform.Find("Sprite");
		var sprite = parent.GetComponent<SpriteRenderer>();

		var fx = Instantiate(specialCancelEffet, parent);
		fx.GetComponent<AfterimageSpawner>().Init(sprite);
	}

	public void CreateHitSparks(HitSpark mode, Vector3 pos, float angle, Vector2 scale,Color color)	{
		SpawnHitSparks(mode, pos, angle, scale, color);
	}


	public void CreateHitSparks(HitSpark mode, Vector3 pos, float angle, Vector2 scale)	{
	   CreateHitSparks(mode,pos,angle,scale,Color.white);
	}

	public void CreateMovementParticle(MovementParticleMode mode, Vector3 pos, float angle, Vector2 scale)	{
		SpawnMovementParticle(mode, pos, angle, scale);
	}

	private void SpawnHitSparks(HitSpark mode, Vector3 pos, float angle, Vector2 scale, Color color)	{
		GameObject hitSpark = Instantiate(hitSparksPrefab, pos, Quaternion.Euler(0, 0, angle));
		hitSpark.GetComponent<HitSparks>().SetAnimation(mode);
		hitSpark.GetComponent<SpriteRenderer>().color = color;
		hitSpark.transform.localScale = new Vector3(scale.x, scale.y, 1);
	}

	private void SpawnHitSparks(HitSpark mode, Vector3 pos, float angle, Vector2 scale)	{
		SpawnHitSparks(mode,pos,angle,scale, Color.white);
	}

	private void SpawnMovementParticle(MovementParticleMode mode, Vector3 pos, float angle, Vector2 scale)	{
		GameObject juiceParticle = Instantiate(movementParticlePrefab, pos, Quaternion.Euler(0, 0, angle));
		juiceParticle.GetComponent<JuiceParticle>().mode = mode;
		juiceParticle.transform.localScale = new Vector3(juiceParticle.transform.localScale.x * scale.x, juiceParticle.transform.localScale.y * scale.y, 1);
	}

	// public void ReceiveNetworkMessage(NetworkMessage message)
	// {
	//     if (!GameController.Instance.IsHost)
	//     {
	//         GraphicalEffectPacket effect = NetworkUtils.GetNetworkData<GraphicalEffectPacket>(message.contents);
	//         if (effect != null)
	//         {
	//             switch ((GraphicalEffectType)effect.effect)
	//             {
	//                 case GraphicalEffectType.HitSpark:
	//                     SpawnHitSparks((HitSpark)effect.mode, effect.pos.ToVector3(), effect.angle, effect.scale.ToVector2(),effect.color.ToColor());
	//                     break;
	//                 case GraphicalEffectType.MovementParticle:
	//                     SpawnMovementParticle((MovementParticleMode)effect.mode, effect.pos.ToVector3(), effect.angle, effect.scale.ToVector2());
	//                     break;
	//             }
	//         }
	//     }
	// }
}