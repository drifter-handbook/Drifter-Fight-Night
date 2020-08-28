using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RyykeTombstoneSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    public string Name = "RyykeTombstone";
    float time = 0f;
    Vector3 oldPos;
    Vector3 oldScale;
    Vector3 targetPos;
    bool oldGrounded = false;

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
        transform.position = Vector3.Lerp(oldPos, targetPos, t);
        GetComponent<RyykeTombstone>().grounded = oldGrounded;
        transform.localScale = oldScale;
    }

    public class TombstoneData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float xScale = 1f;
        public float yScale = 1f;
        public bool grounded = false;
    }

    public void Deserialize(INetworkEntityData data)
    {
        TombstoneData projData = data as TombstoneData;
        if (projData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(projData.x, projData.y, projData.z);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            oldPos = transform.position;
            gameObject.GetComponent<RyykeTombstone>().grounded = projData.grounded;
            targetPos = new Vector3(projData.x, projData.y, projData.z);
            oldScale = new Vector3(projData.xScale,projData.yScale,1);
            oldGrounded = projData.grounded;

        }
    }

    public INetworkEntityData Serialize()
    {
        TombstoneData data = new TombstoneData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            xScale = gameObject.transform.localScale.x,
            yScale = gameObject.transform.localScale.y,
            grounded = gameObject.GetComponent<RyykeTombstone>().grounded

        };
        return data;
    }
}
