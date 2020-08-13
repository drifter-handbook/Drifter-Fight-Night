using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstoneSync : MonoBehaviour, INetworkSync
{
    public bool Grounded { get; private set; }
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;

    public string Type { get; private set; } = "RyykeTombstone";
    public int ID { get; set; } = NetworkEntityList.NextID;

    void Awake()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!Grounded)
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

    public class StoneData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public bool grounded = true;
    }

    public void Deserialize(INetworkEntityData data)
    {
        StoneData projData = data as StoneData;
        if (projData != null)
        {
            if (!Grounded)
            {
                transform.position = new Vector3(projData.x, projData.y, projData.z);
            }
            Grounded = true;
            // move from current position to final position in latency seconds
            time = 0f;
            GetComponent<Animator>().SetBool("Grounded", projData.grounded);
            oldPos = transform.position;
            targetPos = new Vector3(projData.x, projData.y, projData.z);
        }
    }

    public INetworkEntityData Serialize()
    {
        StoneData data = new StoneData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            grounded = GetComponent<RyykeTombstone>().grounded
        };
        return data;
    }
}
