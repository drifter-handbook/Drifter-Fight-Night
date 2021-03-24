using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GraphicalEffectType
{
    HitSpark, MovementParticle
}

public class GraphicalEffectManager : MonoBehaviour, INetworkMessageReceiver
{
    public static GraphicalEffectManager Instance => GameObject.FindGameObjectWithTag("GraphicalEffectManager").GetComponent<GraphicalEffectManager>();

    NetworkSync sync;

    public GameObject hitSparksPrefab;
    public GameObject movementParticlePrefab;

    // Start is called before the first frame update
    void Start()
    {
        sync = GetComponent<NetworkSync>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateHitSparks(HitSpark mode, Vector3 pos, float angle, Vector2 scale)
    {
        if (GameController.Instance.IsHost)
        {
            SpawnHitSparks(mode, pos, angle, scale);
            sync.SendNetworkMessage(new GraphicalEffectPacket()
            {
                effect = (int)GraphicalEffectType.HitSpark,
                mode = (int)mode,
                pos = new SyncableVector3(pos),
                angle = angle,
                scale = new SyncableVector2(scale)
            }, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }

    public void CreateMovementParticle(MovementParticleMode mode, Vector3 pos, float angle, Vector2 scale)
    {
        if (GameController.Instance.IsHost)
        {
            SpawnMovementParticle(mode, pos, angle, scale);
            sync.SendNetworkMessage(new GraphicalEffectPacket()
            {
                effect = (int)GraphicalEffectType.MovementParticle,
                mode = (int)mode,
                pos = new SyncableVector3(pos),
                angle = angle,
                scale = new SyncableVector2(scale)
            }, LiteNetLib.DeliveryMethod.Unreliable);
        }
    }

    private void SpawnHitSparks(HitSpark mode, Vector3 pos, float angle, Vector2 scale)
    {
        GameObject hitSpark = Instantiate(hitSparksPrefab, pos, Quaternion.Euler(0, 0, angle));
        hitSpark.GetComponent<HitSparks>().SetAnimation(mode);
        hitSpark.transform.localScale = new Vector3(scale.x, scale.y, 1);
    }

    private void SpawnMovementParticle(MovementParticleMode mode, Vector3 pos, float angle, Vector2 scale)
    {
        GameObject juiceParticle = Instantiate(movementParticlePrefab, pos, Quaternion.Euler(0, 0, angle));
        juiceParticle.GetComponent<JuiceParticle>().mode = mode;
        juiceParticle.transform.localScale = new Vector3(juiceParticle.transform.localScale.x * scale.x, juiceParticle.transform.localScale.y * scale.y, 1);
    }

    public void ReceiveNetworkMessage(NetworkMessage message)
    {
        if (!GameController.Instance.IsHost)
        {
            GraphicalEffectPacket effect = NetworkUtils.GetNetworkData<GraphicalEffectPacket>(message.contents);
            if (effect != null)
            {
                switch ((GraphicalEffectType)effect.effect)
                {
                    case GraphicalEffectType.HitSpark:
                        SpawnHitSparks((HitSpark)effect.mode, effect.pos.ToVector3(), effect.angle, effect.scale.ToVector2());
                        break;
                    case GraphicalEffectType.MovementParticle:
                        SpawnMovementParticle((MovementParticleMode)effect.mode, effect.pos.ToVector3(), effect.angle, effect.scale.ToVector2());
                        break;
                }
            }
        }
    }
}

public class GraphicalEffectPacket : INetworkData
{
    public string Type { get; set; }
    public int effect;
    public int mode;
    public SyncableVector3 pos;
    public float angle;
    public SyncableVector2 scale;
}