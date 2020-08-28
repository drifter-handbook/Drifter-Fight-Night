using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatusSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    public string Name = "MovementParticle";
    float time = 0f;
    Vector3 oldPos;
    Vector3 oldScale;
    Vector3 targetPos;

    public string Type { get; private set; }
    public int ID { get; set; } = NetworkEntityList.NextID;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        Type = Name;
    }

    // Update is called once per frame
    void Update()
    {
        if (!Active)
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


    }

    public class StatusData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public int mode = 0;
    }

    public void Deserialize(INetworkEntityData data)
    {
        StatusData projData = data as StatusData;
        if (projData != null)
        {
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            gameObject.GetComponent<PlayerStatusController>().mode = projData.mode;
            
        }
    }

    public INetworkEntityData Serialize()
    {
        StatusData data = new StatusData()
        {
            name = gameObject.name,
            ID = ID,
            mode = GetComponent<PlayerStatusController>().mode,
        };
        return data;
    }
}
