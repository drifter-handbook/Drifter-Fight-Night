using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChromaticOrbSync : MonoBehaviour, INetworkSync
{
    public bool Active { get; private set; }
    public float latency = 0.025f;
    float time = 0f;
    Vector3 oldPos;
    Vector3 targetPos;
    Vector3 oldScale;

    public string Type { get; private set; } = "ChromaticOrb";
    public int ID { get; set; } = NetworkEntityList.NextID;

    SpriteRenderer sr;

    void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
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
        transform.localScale = oldScale;
        transform.position = Vector3.Lerp(oldPos, targetPos, t);
    }

    public class ChromaticData : INetworkEntityData
    {
        public string Type { get; set; } = "";
        public int ID { get; set; }
        public string name = "";
        public float x = 0f;
        public float y = 0f;
        public float z = 0f;
        public float xScale = 1f;
        public float yScale = 1f;
        public int mode = 0;
    }

    public void Deserialize(INetworkEntityData data)
    {
        ChromaticData chromData = data as ChromaticData;
        if (chromData != null)
        {
            if (!Active)
            {
                transform.position = new Vector3(chromData.x, chromData.y, chromData.z);
            }
            Active = true;
            // move from current position to final position in latency seconds
            time = 0f;
            GetComponent<Animator>().SetInteger("Mode", chromData.mode);
            oldPos = transform.position;
            oldScale = new Vector3(chromData.xScale,chromData.yScale,1);
            targetPos = new Vector3(chromData.x, chromData.y, chromData.z);
        }
    }

    public INetworkEntityData Serialize()
    {
        ChromaticData data = new ChromaticData()
        {
            name = gameObject.name,
            ID = ID,
            x = transform.position.x,
            y = transform.position.y,
            z = transform.position.z,
            xScale = gameObject.transform.localScale.x,
            yScale = gameObject.transform.localScale.y,
            mode = GetComponent<Animator>().GetInteger("Mode"),
        };
        return data;
    }
}
