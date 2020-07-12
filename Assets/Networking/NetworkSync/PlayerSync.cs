using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSync : MonoBehaviour, INetworkSync
{
    bool active;
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string NetworkSyncType = "";

    public string Type { get { return NetworkSyncType; } }
    public int ID { get; set; } = NetworkEntityList.NextID;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!active)
        {
            return;
        }
        // lerp
        time = Mathf.MoveTowards(time, latency, Time.deltaTime);
        float t = 0;
        if (latency > 0)
        {
            t = time / latency;
        }
        transform.position = Vector3.Lerp(oldPos, targetPos, t);
    }

    public class PlayerData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public bool facing = false;
        public PlayerAnimatorState animatorState = new PlayerAnimatorState();
    }

    public void Deserialize(INetworkEntityData data)
    {
        PlayerData playerData = data as PlayerData;
        if (playerData != null)
        {
            if (!active)
            {
                transform.position = new Vector3(playerData.x, playerData.y, playerData.z);
            }
            active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            GetComponentInChildren<SpriteRenderer>().flipX = playerData.facing;
            oldPos = transform.position;
            targetPos = new Vector3(playerData.x, playerData.y, playerData.z);
            GetComponent<playerMovement>().SyncAnimatorState(playerData.animatorState);
            GetComponent<playerMovement>().IsClient = true;
        }
    }

    public INetworkEntityData Serialize()
    {
        PlayerData data = new PlayerData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            facing = GetComponentInChildren<SpriteRenderer>().flipX,
            animatorState = (PlayerAnimatorState)GetComponent<playerMovement>().animatorState.Clone()
        };
        GetComponent<playerMovement>().ResetAnimatorTriggers();
        return data;
    }
}
